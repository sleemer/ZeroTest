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
        public static ZeroSender<T> CreateOutPin<T>(string address, IObservable<T> source)
        {
            return new ZeroSender<T>(source, address, _mqContext);
        }

        public static TcpReceiver<T> CreateInPin<T>(string address, int port)
        {
            return new TcpReceiver<T>(address, port);
        }
        public static TcpSender<T> CreateOutPin<T>(string address, int port, IObservable<T> source)
        {
            return new TcpSender<T>(source, address, port);
        }
    }
}

