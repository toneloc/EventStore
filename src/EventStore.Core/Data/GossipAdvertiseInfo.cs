using System.Net;

namespace EventStore.Core.Data {
	public class GossipAdvertiseInfo {
		public IPEndPoint InternalTcp { get; set; }
		public IPEndPoint InternalSecureTcp { get; set; }
		public IPEndPoint ExternalTcp { get; set; }
		public IPEndPoint ExternalSecureTcp { get; set; }
		public IPEndPoint ExternalHttp { get; set; }
		public IPAddress AdvertiseExternalIPAs { get; set; }
		public int AdvertiseExternalHttpPortAs { get; set; }

		public GossipAdvertiseInfo(IPEndPoint internalTcp, IPEndPoint internalSecureTcp,
			IPEndPoint externalTcp, IPEndPoint externalSecureTcp, IPEndPoint externalHttp,
			IPAddress advertiseExternalIPAs, int advertiseExternalHttpPortAs) {
			InternalTcp = internalTcp;
			InternalSecureTcp = internalSecureTcp;
			ExternalTcp = externalTcp;
			ExternalSecureTcp = externalSecureTcp;
			ExternalHttp = externalHttp;
			AdvertiseExternalIPAs = advertiseExternalIPAs;
			AdvertiseExternalHttpPortAs = advertiseExternalHttpPortAs;
		}

		public override string ToString() {
			return
				$"IntTcp: {InternalTcp}, IntSecureTcp: {InternalSecureTcp}\nExtTcp: {ExternalTcp}, ExtSecureTcp: {ExternalSecureTcp}, ExtHttp: {ExternalHttp}, ExtAdvertiseAs: {AdvertiseExternalIPAs}:{AdvertiseExternalHttpPortAs}";
		}
	}
}
