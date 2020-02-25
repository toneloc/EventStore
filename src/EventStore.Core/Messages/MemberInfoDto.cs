using System;
using EventStore.Core.Cluster;
using EventStore.Core.Data;

namespace EventStore.Core.Messages {
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
		public bool IsReadOnlyReplica { get; set; }

		public MemberInfoDto() {
		}

		public MemberInfoDto(MemberInfo member) {
			InstanceId = member.InstanceId;

			TimeStamp = member.TimeStamp;
			State = member.State;
			IsAlive = member.IsAlive;

			InternalTcpIp = member.InternalTcpEndPoint.Address.ToString();
			InternalTcpPort = member.InternalTcpEndPoint.Port;
			InternalSecureTcpPort =
				member.InternalSecureTcpEndPoint == null ? 0 : member.InternalSecureTcpEndPoint.Port;

			ExternalTcpIp = member.ExternalTcpEndPoint.Address.ToString();
			ExternalTcpPort = member.ExternalTcpEndPoint.Port;
			ExternalSecureTcpPort =
				member.ExternalSecureTcpEndPoint == null ? 0 : member.ExternalSecureTcpEndPoint.Port;

			ExternalHttpIp = member.ExternalHttpEndPoint.Address.ToString();
			ExternalHttpPort = member.ExternalHttpEndPoint.Port;

			LastCommitPosition = member.LastCommitPosition;
			WriterCheckpoint = member.WriterCheckpoint;
			ChaserCheckpoint = member.ChaserCheckpoint;

			EpochPosition = member.EpochPosition;
			EpochNumber = member.EpochNumber;
			EpochId = member.EpochId;

			NodePriority = member.NodePriority;
			IsReadOnlyReplica = member.IsReadOnlyReplica;
		}

		public override string ToString() {
			return
				$"InstanceId: {InstanceId:B}, TimeStamp: {TimeStamp:yyyy-MM-dd HH:mm:ss.fff}, State: {State}, IsAlive: {IsAlive}, " +
				$"InternalTcpIp: {InternalTcpIp}, InternalTcpPort: {InternalTcpPort}, InternalSecureTcpPort: {InternalSecureTcpPort}, " +
				$"ExternalTcpIp: {ExternalTcpIp}, ExternalTcpPort: {ExternalTcpPort}, ExternalSecureTcpPort: {ExternalSecureTcpPort}, " +
				$"ExternalHttpIp: {ExternalHttpIp}, ExternalHttpPort: {ExternalHttpPort}, " +
				$"LastCommitPosition: {LastCommitPosition}, WriterCheckpoint: {WriterCheckpoint}, ChaserCheckpoint: {ChaserCheckpoint}, " +
				$"EpochPosition: {EpochPosition}, EpochNumber: {EpochNumber}, EpochId: {EpochId:B}, NodePriority: {NodePriority}, " +
				$"IsReadOnlyReplica: {IsReadOnlyReplica}";
		}
	}
}
