using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EventStore.Core.Services;
using Xunit;

namespace EventStore.Client.Streams {
	public class read_all_events_backward_filtered
		: IClassFixture<read_all_events_backward_filtered.Fixture> {
		private readonly Fixture _fixture;

		public read_all_events_backward_filtered(Fixture fixture) {
			_fixture = fixture;
		}

		[Fact]
		public async Task regular_expression_stream_name() {
			var streamPrefix = _fixture.GetStreamName();
			var events = _fixture.CreateTestEvents(10).ToArray();

			foreach (var e in events) {
				await _fixture.Client.AppendToStreamAsync($"{streamPrefix}_{Guid.NewGuid():n}",
					AnyStreamRevision.NoStream, new[] {e});
			}

			var result = await _fixture.Client.ReadAllAsync(Direction.Backwards, Position.End, 20,
					filterOptions: new FilterOptions(StreamFilter.RegularExpression($"^{streamPrefix}")))
				.ToArrayAsync();

			Assert.Equal(events.Select(x => x.EventId), result.Reverse().Select(x => x.OriginalEvent.EventId));
		}

		[Fact]
		public async Task prefix_stream_name() {
			var streamPrefix = _fixture.GetStreamName();
			var events = _fixture.CreateTestEvents(10).ToArray();

			foreach (var e in events) {
				await _fixture.Client.AppendToStreamAsync($"{streamPrefix}_{Guid.NewGuid():n}",
					AnyStreamRevision.NoStream, new[] {e});
			}

			var result = await _fixture.Client.ReadAllAsync(Direction.Backwards, Position.End, 20,
					filterOptions: new FilterOptions(StreamFilter.Prefix(streamPrefix)))
				.ToArrayAsync();

			Assert.Equal(events.Select(x => x.EventId), result.Reverse().Select(x => x.OriginalEvent.EventId));
		}

		[Fact]
		public async Task regular_expression_event_type() {
			const string eventTypePrefix = nameof(regular_expression_event_type);
			var streamPrefix = _fixture.GetStreamName();
			var events = _fixture.CreateTestEvents(10)
				.Select(e =>
					new EventData(e.EventId, $"{eventTypePrefix}-{Guid.NewGuid():n}", e.Data, e.Metadata, e.ContentType))
				.ToArray();

			foreach (var e in events) {
				await _fixture.Client.AppendToStreamAsync($"{streamPrefix}_{Guid.NewGuid():n}",
					AnyStreamRevision.NoStream, new[] {e});
			}

			var result = await _fixture.Client.ReadAllAsync(Direction.Backwards, Position.End, 20,
					filterOptions: new FilterOptions(EventTypeFilter.RegularExpression(new Regex($"^{eventTypePrefix}"))))
				.ToArrayAsync();

			Assert.Equal(events.Select(x => x.EventId), result.Reverse().Select(x => x.OriginalEvent.EventId));
		}

		[Fact]
		public async Task prefix_event_type() {
			const string eventTypePrefix = nameof(prefix_event_type);
			var streamPrefix = _fixture.GetStreamName();
			var events = _fixture.CreateTestEvents(10)
				.Select(e =>
					new EventData(e.EventId, $"{eventTypePrefix}-{Guid.NewGuid():n}", e.Data, e.Metadata, e.ContentType))
				.ToArray();

			foreach (var e in events) {
				await _fixture.Client.AppendToStreamAsync($"{streamPrefix}_{Guid.NewGuid():n}",
					AnyStreamRevision.NoStream, new[] {e});
			}

			var result = await _fixture.Client.ReadAllAsync(Direction.Backwards, Position.End, 20,
					filterOptions: new FilterOptions(EventTypeFilter.Prefix(eventTypePrefix)))
				.ToArrayAsync();

			Assert.Equal(events.Select(x => x.EventId), result.Reverse().Select(x => x.OriginalEvent.EventId));
		}

		[Fact]
		public async Task max_count_is_respected() {
			var streamName = _fixture.GetStreamName();
			const int count = 20;
			const ulong maxCount = (ulong)count / 2;

			await _fixture.Client.AppendToStreamAsync(streamName, AnyStreamRevision.NoStream,
				_fixture.CreateTestEvents(count));

			var events = await _fixture.Client.ReadAllAsync(Direction.Backwards, Position.End, maxCount,
					filterOptions: new FilterOptions(EventTypeFilter.ExcludeSystemEvents()))
				.Take(count)
				.ToArrayAsync();

			Assert.Equal(maxCount, (ulong)events.Length);
		}

		public class Fixture : EventStoreGrpcFixture {
			public const string FilteredOutStream = nameof(FilteredOutStream);

			protected override Task Given() => Client.SetStreamMetadataAsync("$all", AnyStreamRevision.Any,
				new StreamMetadata(acl: new StreamAcl(SystemRoles.All)), userCredentials: TestCredentials.Root);

			protected override Task When() =>
				Client.AppendToStreamAsync(FilteredOutStream, AnyStreamRevision.NoStream, CreateTestEvents(10));
		}
	}
}
