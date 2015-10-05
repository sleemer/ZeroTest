using System;
using ProtoBuf;

namespace ReactTransport
{
	[ProtoContract]
	public class DataPacket<T>
	{
		private DataPacket ()
		{
		}

		public DataPacket (T item)
		{
			Item = item;
		}

		[ProtoMember (1)]
		public T Item {
			get;
			private set;
		}

		[ProtoMember (2)]
		public bool IsNotificationTicket { 
			get; 
			private set;
		}

		static DataPacket ()
		{
		}

		private static DataPacket<T> _notificationTicket = new DataPacket<T> { IsNotificationTicket = true };

		public static DataPacket<T> NotificationTicket{ get { return _notificationTicket; } }
	}
}

