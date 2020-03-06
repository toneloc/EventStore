using System;
using System.Net.Http;
using System.Net.Security;

namespace EventStore.Client.Operations {
	public abstract class EventStoreOperationsGrpcFixture : EventStoreGrpcFixture {
		public EventStoreOperationsClient OperationsClient { get; }

		protected EventStoreOperationsGrpcFixture() {
			OperationsClient = new EventStoreOperationsClient(_serverUri,() => new SocketsHttpHandler {
				SslOptions = new SslClientAuthenticationOptions {
					RemoteCertificateValidationCallback = delegate { return true; }
				}
			});
		}
	}
}
