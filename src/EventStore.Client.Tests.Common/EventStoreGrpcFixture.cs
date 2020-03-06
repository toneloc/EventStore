using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EventStore.Core;
using EventStore.Core.Messages;
using EventStore.Core.Messaging;
using EventStore.Core.Services.UserManagement;
using EventStore.Core.TransactionLog.Chunks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Display;
using Xunit;
using Xunit.Abstractions;

namespace EventStore.Client {
	public abstract class EventStoreGrpcFixture : IAsyncLifetime {
		public const string TestEventType = "-";

		private static readonly Subject<LogEvent> s_logEventSubject = new Subject<LogEvent>();
		private readonly TFChunkDb _db;
		private readonly IList<IDisposable> _disposables;

		public ClusterVNode Node { get; }
		public EventStoreClient Client { get; }
		private readonly IWebHost _host;
		protected readonly Uri _serverUri;

		static EventStoreGrpcFixture() {
			var loggerConfiguration = new LoggerConfiguration()
				.Enrich.FromLogContext()
				.MinimumLevel.Is(LogEventLevel.Verbose)
				.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
				.MinimumLevel.Override("Grpc", LogEventLevel.Warning)
				.WriteTo.Observers(observable => observable.Subscribe(s_logEventSubject.OnNext))
				.WriteTo.Seq("http://localhost:5341/", period: TimeSpan.FromMilliseconds(1));
			Log.Logger = loggerConfiguration.CreateLogger();

			AppDomain.CurrentDomain.DomainUnload += (_, e) => Log.CloseAndFlush();
		}

		protected EventStoreGrpcFixture(
			Action<VNodeBuilder> configureVNode = default,
			Action<IWebHostBuilder> configureWebHost = default,
			EventStoreClientSettings clientSettings = default) {
			var webHostBuilder = new WebHostBuilder();
			configureWebHost?.Invoke(webHostBuilder);

			var vNodeBuilder = new TestVNodeBuilder();
			vNodeBuilder.RunInMemory().WithTfChunkSize(1024 * 1024);
			configureVNode?.Invoke(vNodeBuilder);

			Node = vNodeBuilder.Build();
			_db = vNodeBuilder.GetDb();
			_disposables = new List<IDisposable>();

			using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
			var port = ((IPEndPoint)socket.LocalEndPoint).Port;

			_host = webHostBuilder
				.UseKestrel(serverOptions => {
					serverOptions.Listen(IPAddress.Loopback, port, listenOptions => {
						if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
							listenOptions.Protocols = HttpProtocols.Http2;
						} else {
							listenOptions.UseHttps();
						}
					});
				})
				.UseSerilog()
				.UseStartup(Node.Startup)
				.Build();

			var settings = clientSettings ?? new EventStoreClientSettings {
				OperationOptions = { TimeoutAfter = Debugger.IsAttached ? null : (TimeSpan?)TimeSpan.FromSeconds(30) },
				ConnectivitySettings = new EventStoreClientConnectivitySettings {
					Address = new UriBuilder {
						Port = port,
						Scheme = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
							? Uri.UriSchemeHttp
							: Uri.UriSchemeHttps
					}.Uri,
				},
				CreateHttpMessageHandler = () => new SocketsHttpHandler {
					SslOptions = new SslClientAuthenticationOptions {
						RemoteCertificateValidationCallback = delegate { return true; }
					}
				},
				LoggerFactory = _host.Services.GetRequiredService<ILoggerFactory>()
			};
			_serverUri = settings.ConnectivitySettings.Address;
			Client = new EventStoreClient(settings);
		}

		protected abstract Task Given();
		protected abstract Task When();

		public IEnumerable<EventData> CreateTestEvents(int count = 1, string type = default)
			=> Enumerable.Range(0, count).Select(index => CreateTestEvent(index, type ?? TestEventType));

		protected static EventData CreateTestEvent(int index) => CreateTestEvent(index, TestEventType);

		protected static EventData CreateTestEvent(int index, string type)
			=> new EventData(Uuid.NewUuid(), type, Encoding.UTF8.GetBytes($@"{{""x"":{index}}}"));

		public virtual async Task InitializeAsync() {
			await _host.StartAsync();
			await Node.StartAsync(true);

			var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			var envelope  = new CallbackEnvelope(m => {
				if (m is UserManagementMessage.ResponseMessage rm) {
					if (rm.Success) tcs.TrySetResult(true);
					else tcs.TrySetException(new Exception($"Create user failed {rm.Error}"));
				} else {
					tcs.TrySetException(new Exception($"Wrong expected message type {m.GetType().FullName}"));
				}
			});
			Node.MainQueue.Publish(new UserManagementMessage.Create(envelope, SystemAccounts.System, TestCredentials.TestUser1.Username, "test", Array.Empty<string>(), TestCredentials.TestUser1.Password));
			await tcs.Task;
			
			await Given().WithTimeout(TimeSpan.FromMinutes(5));
			await When().WithTimeout(TimeSpan.FromMinutes(5));
		}

		public virtual async Task DisposeAsync() {
			await Node.StopAsync();
			_db.Dispose();
			await _host.StopAsync();
			_host.Dispose();
			Client?.Dispose();
			foreach (var disposable in _disposables) {
				disposable.Dispose();
			}
		}

		public string GetStreamName([CallerMemberName] string testMethod = default) {
			var type = GetType();

			return $"{type.DeclaringType.Name}.{testMethod ?? "unknown"}";
		}

		public void CaptureLogs(ITestOutputHelper testOutputHelper) {
			const string captureCorrelationId = nameof(captureCorrelationId);

			MessageTemplateTextFormatter formatter = new MessageTemplateTextFormatter(
				"{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{SourceContext}] {Message}");

			MessageTemplateTextFormatter formatterWithException =
				new MessageTemplateTextFormatter(
					"{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{SourceContext}] {Message}{NewLine}{Exception}");

			var subscription = s_logEventSubject.Subscribe(logEvent => {
				using var writer = new StringWriter();
				if (logEvent.Exception != null) {
					formatterWithException.Format(logEvent, writer);
				} else {
					formatter.Format(logEvent, writer);
				}

				testOutputHelper.WriteLine(writer.ToString());
			});

			_disposables.Add(subscription);
		}


		protected class ResponseVersionHandler : DelegatingHandler {
			protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
				CancellationToken cancellationToken) {
				var response = await base.SendAsync(request, cancellationToken);
				response.Version = request.Version;
				return response;
			}
		}

		protected class DelayedHandler : HttpClientHandler {
			private readonly int _delay;

			public DelayedHandler(int delay) {
				_delay = delay;
			}

			protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
				CancellationToken cancellationToken) {
				await Task.Delay(_delay, cancellationToken);
				return await base.SendAsync(request, cancellationToken);
			}
		}

		public class TestClusterVNodeStartup : IStartup {
			private readonly ClusterVNode _node;

			public TestClusterVNodeStartup(ClusterVNode node) {
				if (node == null)
					throw new ArgumentNullException(nameof(node));
				_node = node;
			}

			public IServiceProvider ConfigureServices(IServiceCollection services) =>
				_node.Startup.ConfigureServices(services);

			public void Configure(IApplicationBuilder app) => _node.Startup.Configure(app.Use(CompleteResponse));

			private static RequestDelegate CompleteResponse(RequestDelegate next) => context =>
				next(context).ContinueWith(_ => context.Response.Body.FlushAsync());
		}
	}
}
