using System;
using NetMQ;

namespace ZeroTransport
{
    public class ZeroFactory<T> : IPinFactory<T>
    {
        private static NetMQContext _mqContext = NetMQContext.Create();
        static ZeroFactory()
        {
        }

        public IInPin<T> CreateInPin(string address)
        {
            return new ZeroReceiver<T>(address, _mqContext);
        }
        public IOutPin CreateOutPin(string address, IObservable<T> source)
        {
            return new ZeroSender<T>(source, address, _mqContext);
        }

        //public static TcpReceiver<T> CreateInPin<T>(string address, int port)
        //{
        //    return new TcpReceiver<T>(address, port);
        //}
        //public static TcpSender<T> CreateOutPin<T>(string address, int port, IObservable<T> source)
        //{
        //    return new TcpSender<T>(source, address, port);
        //}
    }
}

