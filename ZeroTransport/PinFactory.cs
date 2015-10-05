using System;
using NetMQ;

namespace ZeroTransport
{
	public static class PinFactory
	{
		private static NetMQContext _mqContext = NetMQContext.Create();
		static PinFactory ()
		{
		}

		public static ZeroReceiver<T> CreateInPin<T>(string address)
		{
			return new ZeroReceiver<T> (address, _mqContext);
		}
	}
}

