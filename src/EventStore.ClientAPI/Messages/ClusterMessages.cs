using System;

namespace EventStore.ClientAPI.Messages {
	internal class ClusterMessages {
		public class ClusterInfoDto {
			public MemberInfoDto[] Members { get; set; }

			public ClusterInfoDto() {
			}

			public ClusterInfoDto(MemberInfoDto[] members) {
				Members = members;
			}
		}

		public class MemberInfoDto {
			public Guid InstanceId { get; set; }

			public DateTime TimeStamp { get; set; }
			public VNodeState State { get; set; }
			public bool IsAlive { get; set; }

			public string InternalTcpIp { get; set; }
			public int InternalTcpPort { get; set; }
			public int InternalSecureTcpPort { get; set; }

			public string ExternalTcpIp { get; set; }
			public int ExternalTcpPort { get; set; }
			public int ExternalSecureTcpPort { get; set; }

			public string ExternalHttpIp { get; set; }
			public int ExternalHttpPort { get; set; }

			public long LastCommitPosition { get; set; }
			public long WriterCheckpoint { get; set; }
			public long ChaserCheckpoint { get; set; }

			public long EpochPosition { get; set; }
			public int EpochNumber { get; set; }
			public Guid EpochId { get; set; }

			public int NodePriority { get; set; }

			public override string ToString() {
				if (State == VNodeState.Manager)
					return
						$"MAN {InstanceId:B} <{(IsAlive ? "LIVE" : "DEAD")}> [{State}, " +
						$"{ExternalHttpIp}:{ExternalHttpPort}] | {TimeStamp:yyyy-MM-dd HH:mm:ss.fff}";
				return
					$"VND {InstanceId:B} <{(IsAlive ? "LIVE" : "DEAD")}> [{State}, {InternalTcpIp}:{InternalTcpPort}, " +
					$"{(InternalSecureTcpPort > 0 ? $"{InternalTcpIp}:{InternalSecureTcpPort}" : "n/a")}, " +
					$"{ExternalTcpIp}:{ExternalTcpPort}, {(ExternalSecureTcpPort > 0 ? $"{ExternalTcpIp}:{ExternalSecureTcpPort}" : "n/a")}, " +
					$"{ExternalHttpIp}:{ExternalHttpPort}] {LastCommitPosition}/{WriterCheckpoint}/{ChaserCheckpoint}/E{EpochNumber}@{EpochPosition}:{EpochId:B} | {TimeStamp:yyyy-MM-dd HH:mm:ss.fff}";
			}
		}

		public enum VNodeState {
			Initializing,
			Unknown,
			PreReplica,
			CatchingUp,
			Clone,
			Follower,
			PreLeader,
			Leader,
			Manager,
			ShuttingDown,
			Shutdown,
			ReadOnlyLeaderless,
			PreReadOnlyReplica,
			ReadOnlyReplica
		}
	}
}
