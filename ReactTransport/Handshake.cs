using System;
using ProtoBuf;

namespace ReactTransport
{
	[ProtoContract]
	public class Handshake
	{
		private Handshake ()
		{
		}
		public Handshake(Guid clientId, string clientName = null)
		{
			ClientId = clientId;
			ClientName = clientName;
		}

		[ProtoMember(1)]
		public Guid ClientId{ get; private set;}
		[ProtoMember(2)]
		public string ClientName{ get; private set;}
	}
}

