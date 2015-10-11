using System;
using NetMQ;

namespace ZeroTransport
{
    public class ZeroSessionFactory<T> : ISessionFactory<T>
    {
        private static NetMQContext _mqContext = NetMQContext.Create();
        static ZeroSessionFactory()
        {
        }

        public IPushSession<T> CreatePushSession(string address)
        {
            return new ZeroPushSession<T>(address, _mqContext);
        }
        public IPullSession CreatePullSession(string address, IObservable<T> source)
        {
            return new ZeroPullSession<T>(source, address, _mqContext);
        }
    }
}

