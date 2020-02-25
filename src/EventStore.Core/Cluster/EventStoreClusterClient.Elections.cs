using System;
using System.Net;
using System.Threading.Tasks;
using EventStore.Client;
using EventStore.Cluster;
using EventStore.Core.Messages;
using EndPoint = EventStore.Cluster.EndPoint;

namespace EventStore.Core.Cluster {
	public partial class EventStoreClusterClient {
		public void SendViewChange(ElectionMessage.ViewChange msg, IPEndPoint destinationEndpoint, DateTime deadline) {
			SendViewChangeAsync(msg.ServerId, msg.ServerExternalHttp, msg.AttemptedView, deadline).ContinueWith(r => {
				if (r.Exception != null) {
					Log.Error(r.Exception, "View Change Send Failed to {Server}", destinationEndpoint);
				}
			});
		}

		public void SendViewChangeProof(ElectionMessage.ViewChangeProof msg, IPEndPoint destinationEndpoint,
			DateTime deadline) {
			SendViewChangeProofAsync(msg.ServerId, msg.ServerExternalHttp, msg.InstalledView, deadline).ContinueWith(
				r => {
					if (r.Exception != null) {
						Log.Error(r.Exception, "View Change Proof Send Failed to {Server}",
							destinationEndpoint);
					}
				});
		}

		public void SendPrepare(ElectionMessage.Prepare msg, IPEndPoint destinationEndpoint, DateTime deadline) {
			SendPrepareAsync(msg.ServerId, msg.ServerExternalHttp, msg.View, deadline).ContinueWith(r => {
				if (r.Exception != null) {
					Log.Error(r.Exception, "Prepare Send Failed to {Server}", destinationEndpoint);
				}
			});
		}

		public void SendPrepareOk(ElectionMessage.PrepareOk prepareOk, IPEndPoint destinationEndpoint,
			DateTime deadline) {
			SendPrepareOkAsync(prepareOk.View, prepareOk.ServerId, prepareOk.ServerExternalHttp, prepareOk.EpochNumber,
					prepareOk.EpochPosition, prepareOk.EpochId, prepareOk.LastCommitPosition,
					prepareOk.WriterCheckpoint,
					prepareOk.ChaserCheckpoint, prepareOk.NodePriority, deadline)
				.ContinueWith(r => {
					if (r.Exception != null) {
						Log.Error(r.Exception, "Prepare OK Send Failed to {Server}",
							destinationEndpoint);
					}
				});
		}

		public void SendProposal(ElectionMessage.Proposal proposal, IPEndPoint destinationEndpoint, DateTime deadline) {
			SendProposalAsync(proposal.ServerId, proposal.ServerExternalHttp, proposal.LeaderId,
					proposal.LeaderExternalHttp,
					proposal.View, proposal.EpochNumber, proposal.EpochPosition, proposal.EpochId,
					proposal.LastCommitPosition, proposal.WriterCheckpoint, proposal.ChaserCheckpoint,
					proposal.NodePriority,
					deadline)
				.ContinueWith(r => {
					if (r.Exception != null) {
						Log.Error(r.Exception, "Proposal Send Failed to {Server}",
							destinationEndpoint);
					}
				});
		}

		public void SendAccept(ElectionMessage.Accept accept, IPEndPoint destinationEndpoint, DateTime deadline) {
			SendAcceptAsync(accept.ServerId, accept.ServerExternalHttp, accept.LeaderId, accept.LeaderExternalHttp,
					accept.View, deadline)
				.ContinueWith(r => {
					if (r.Exception != null) {
						Log.Error(r.Exception, "Accept Send Failed to {Server}", destinationEndpoint);
					}
				});
		}

		public void SendLeaderIsResigning(ElectionMessage.LeaderIsResigning resigning, IPEndPoint destinationEndpoint,
			DateTime deadline) {
			SendLeaderIsResigningAsync(resigning.LeaderId, resigning.LeaderExternalHttp, deadline).ContinueWith(r => {
				if (r.Exception != null) {
					Log.Error(r.Exception, "Leader is Resigning Send Failed to {Server}", destinationEndpoint);
				}
			});
		}

		public void SendLeaderIsResigningOk(ElectionMessage.LeaderIsResigningOk resigningOk,
			IPEndPoint destinationEndpoint, DateTime deadline) {
			SendLeaderIsResigningOkAsync(resigningOk.LeaderId, resigningOk.LeaderExternalHttp,
				resigningOk.ServerId, resigningOk.ServerExternalHttp, deadline).ContinueWith(r => {
				if (r.Exception != null) {
					Log.Error(r.Exception, "Leader is Resigning Ok Send Failed to {Server}", destinationEndpoint);
				}
			});
		}

		private async Task SendViewChangeAsync(Guid serverId, IPEndPoint serverExternalHttp, int attemptedView,
			DateTime deadline) {
			var request = new ViewChangeRequest {
				ServerId = Uuid.FromGuid(serverId).ToDto(),
				ServerExternalHttp = new EndPoint(serverExternalHttp.Address.ToString(), (uint)serverExternalHttp.Port),
				AttemptedView = attemptedView
			};
			await _electionsClient.ViewChangeAsync(request, deadline: deadline.ToUniversalTime());
		}

		private async Task SendViewChangeProofAsync(Guid serverId, IPEndPoint serverExternalHttp, int installedView,
			DateTime deadline) {
			var request = new ViewChangeProofRequest {
				ServerId = Uuid.FromGuid(serverId).ToDto(),
				ServerExternalHttp = new EndPoint(serverExternalHttp.Address.ToString(), (uint)serverExternalHttp.Port),
				InstalledView = installedView
			};
			await _electionsClient.ViewChangeProofAsync(request, deadline: deadline.ToUniversalTime());
		}

		private async Task SendPrepareAsync(Guid serverId, IPEndPoint serverExternalHttp, int view, DateTime deadline) {
			var request = new PrepareRequest {
				ServerId = Uuid.FromGuid(serverId).ToDto(),
				ServerExternalHttp = new EndPoint(serverExternalHttp.Address.ToString(), (uint)serverExternalHttp.Port),
				View = view
			};
			await _electionsClient.PrepareAsync(request, deadline: deadline.ToUniversalTime());
		}

		private async Task SendPrepareOkAsync(int view, Guid serverId, IPEndPoint serverExternalHttp, int epochNumber,
			long epochPosition, Guid epochId, long lastCommitPosition, long writerCheckpoint, long chaserCheckpoint,
			int nodePriority, DateTime deadline) {
			var request = new PrepareOkRequest {
				View = view,
				ServerId = Uuid.FromGuid(serverId).ToDto(),
				ServerExternalHttp = new EndPoint(serverExternalHttp.Address.ToString(), (uint)serverExternalHttp.Port),
				EpochNumber = epochNumber,
				EpochPosition = epochPosition,
				EpochId = Uuid.FromGuid(epochId).ToDto(),
				LastCommitPosition = lastCommitPosition,
				WriterCheckpoint = writerCheckpoint,
				ChaserCheckpoint = chaserCheckpoint,
				NodePriority = nodePriority
			};
			await _electionsClient.PrepareOkAsync(request, deadline: deadline.ToUniversalTime());
		}

		private async Task SendProposalAsync(Guid serverId, IPEndPoint serverExternalHttp, Guid leaderId,
			IPEndPoint leaderExternalHttp, int view, int epochNumber, long epochPosition, Guid epochId,
			long lastCommitPosition, long writerCheckpoint, long chaserCheckpoint, int nodePriority,
			DateTime deadline) {
			var request = new ProposalRequest {
				ServerId = Uuid.FromGuid(serverId).ToDto(),
				ServerExternalHttp = new EndPoint(serverExternalHttp.Address.ToString(), (uint)serverExternalHttp.Port),
				LeaderId = Uuid.FromGuid(leaderId).ToDto(),
				LeaderExternalHttp = new EndPoint(leaderExternalHttp.Address.ToString(), (uint)leaderExternalHttp.Port),
				View = view,
				EpochNumber = epochNumber,
				EpochPosition = epochPosition,
				EpochId = Uuid.FromGuid(epochId).ToDto(),
				LastCommitPosition = lastCommitPosition,
				WriterCheckpoint = writerCheckpoint,
				ChaserCheckpoint = chaserCheckpoint,
				NodePriority = nodePriority
			};
			await _electionsClient.ProposalAsync(request, deadline: deadline.ToUniversalTime());
		}

		private async Task SendAcceptAsync(Guid serverId, IPEndPoint serverExternalHttp, Guid leaderId,
			IPEndPoint leaderExternalHttp, int view, DateTime deadline) {
			var request = new AcceptRequest {
				ServerId = Uuid.FromGuid(serverId).ToDto(),
				ServerExternalHttp = new EndPoint(serverExternalHttp.Address.ToString(), (uint)serverExternalHttp.Port),
				LeaderId = Uuid.FromGuid(leaderId).ToDto(),
				LeaderExternalHttp = new EndPoint(leaderExternalHttp.Address.ToString(), (uint)leaderExternalHttp.Port),
				View = view
			};
			await _electionsClient.AcceptAsync(request);
			_electionsClient.Accept(request, deadline: deadline.ToUniversalTime());
		}

		private async Task SendLeaderIsResigningAsync(Guid leaderId, IPEndPoint leaderExternalHttp, DateTime deadline) {
			var request = new LeaderIsResigningRequest {
				LeaderId = Uuid.FromGuid(leaderId).ToDto(),
				LeaderExternalHttp = new EndPoint(leaderExternalHttp.Address.ToString(), (uint)leaderExternalHttp.Port),
			};
			await _electionsClient.LeaderIsResigningAsync(request, deadline: deadline.ToUniversalTime());
		}

		private async Task SendLeaderIsResigningOkAsync(Guid leaderId, IPEndPoint leaderExternalHttp,
			Guid serverId, IPEndPoint serverExternalHttp, DateTime deadline) {
			var request = new LeaderIsResigningOkRequest {
				LeaderId = Uuid.FromGuid(leaderId).ToDto(),
				LeaderExternalHttp = new EndPoint(leaderExternalHttp.Address.ToString(), (uint)leaderExternalHttp.Port),
				ServerId = Uuid.FromGuid(serverId).ToDto(),
				ServerExternalHttp = new EndPoint(serverExternalHttp.Address.ToString(), (uint)serverExternalHttp.Port),
			};
			await _electionsClient.LeaderIsResigningOkAsync(request, deadline: deadline.ToUniversalTime());
		}
	}
}
