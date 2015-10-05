using System;
using ProtoBuf;

namespace ReactTransport
{
	[ProtoContract]
	public class DataTicket
	{
		private DataTicket()
		{
		}
		public DataTicket (bool isRequired, int count = 1)
		{
			IsRequired = isRequired;
			Count = count;
		}
		[ProtoMember(1)]
		public bool IsRequired{ get; private set;}
		[ProtoMember(2)]
		public int Count{ get; private set;}

		static DataTicket(){}
		private static DataTicket _dropOne = new DataTicket(false);
		private static DataTicket _sendOne = new DataTicket(true);
		public static DataTicket DropOne{get{ return _dropOne; }}
		public static DataTicket SendOne{get{ return _sendOne; }}
	}
}

