﻿using System;
using EventStore.Core.Bus;
using EventStore.Core.Data;
using EventStore.Core.Messages;
using EventStore.Core.Messaging;

namespace EventStore.Core.Services.RequestManager.Managers {
	public class WriteEvents : RequestManagerBase {
		private readonly string _streamId;
		private readonly bool _betterOrdering;
		private readonly Event[] _events;

		public WriteEvents(
					IPublisher publisher,
					TimeSpan timeout,
					IEnvelope clientResponseEnvelope,
					Guid internalCorrId,
					Guid clientCorrId,
					string streamId,
					bool betterOrdering,
					long expectedVersion,
					Event[] events,
					CommitSource commitSource)
			: base(
					 publisher,
					 timeout,
					 clientResponseEnvelope,
					 internalCorrId,
					 clientCorrId,
					 expectedVersion,
					 commitSource,
					 prepareCount: 0,
					 waitForCommit: true) {
			_streamId = streamId;
			_betterOrdering = betterOrdering;
			_events = events;
		}

		protected override Message WriteRequestMsg =>
			new StorageMessage.WritePrepares(
					InternalCorrId,
					WriteReplyEnvelope,
					_streamId,
					ExpectedVersion,
					_events,
					LiveUntil);


		protected override Message ClientSuccessMsg =>
			 new ClientMessage.WriteEventsCompleted(
				 ClientCorrId,
				 FirstEventNumber,
				 LastEventNumber,
				 CommitPosition,  //not technically correct, but matches current behavior correctly
				 CommitPosition);

		protected override Message ClientFailMsg =>
			 new ClientMessage.WriteEventsCompleted(
				 ClientCorrId,
				 Result,
				 FailureMessage,
				 FailureCurrentVersion);
	}
}
