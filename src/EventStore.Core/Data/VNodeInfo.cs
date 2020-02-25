using System;
using System.Net;
using EventStore.Common.Utils;

namespace EventStore.Core.Data {
	public class VNodeInfo {
		public readonly Guid InstanceId;
		public readonly int DebugIndex;
		public readonly IPEndPoint InternalTcp;
		public readonly IPEndPoint InternalSecureTcp;
		public readonly IPEndPoint ExternalTcp;
		public readonly IPEndPoint ExternalSecureTcp;
		public readonly IPEndPoint ExternalHttp;
		public readonly bool IsReadOnlyReplica;

		public VNodeInfo(Guid instanceId, int debugIndex,
			IPEndPoint internalTcp, IPEndPoint internalSecureTcp,
			IPEndPoint externalTcp, IPEndPoint externalSecureTcp,
			IPEndPoint externalHttp,
			bool isReadOnlyReplica) {
			Ensure.NotEmptyGuid(instanceId, "instanceId");
			Ensure.NotNull(internalTcp, "internalTcp");
			Ensure.NotNull(externalTcp, "externalTcp");
			Ensure.NotNull(externalHttp, "externalHttp");

			DebugIndex = debugIndex;
			InstanceId = instanceId;
			InternalTcp = internalTcp;
			InternalSecureTcp = internalSecureTcp;
			ExternalTcp = externalTcp;
			ExternalSecureTcp = externalSecureTcp;
			ExternalHttp = externalHttp;
			IsReadOnlyReplica = isReadOnlyReplica;
		}

		public bool Is(IPEndPoint endPoint) {
			return endPoint != null
			       && (ExternalHttp.Equals(endPoint)
			           || InternalTcp.Equals(endPoint)
			           || (InternalSecureTcp != null && InternalSecureTcp.Equals(endPoint))
			           || ExternalTcp.Equals(endPoint)
			           || (ExternalSecureTcp != null && ExternalSecureTcp.Equals(endPoint)));
		}

		public override string ToString() {
			return $"InstanceId: {InstanceId:B}, InternalTcp: {InternalTcp}, InternalSecureTcp: {InternalSecureTcp}, " +
			       $"ExternalTcp: {ExternalTcp}, ExternalSecureTcp: {ExternalSecureTcp}, ExternalHttp: {ExternalHttp}," +
			       $"IsReadOnlyReplica: {IsReadOnlyReplica}";
		}
	}
}
