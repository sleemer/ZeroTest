using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroCore.Contracts;

namespace ZeroTransport.Zero
{
    public sealed class ZeroPubSession<T> : IPubSession
    {
        private readonly PublisherSocket _socket;

        public ZeroPubSession(IObservable<T> source, string address, NetMQContext context)
        {
            _socket = context.CreatePublisherSocket();
        }

        public SessionState State
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
