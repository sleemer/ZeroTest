using System;
using NetMQ;
using NetMQ.Sockets;
using System.Reactive.Linq;
using System.IO;
using ProtoBuf;
using System.Threading.Tasks;

namespace ZeroTransport
{
    public class ZeroReceiver<T> : IDisposable
    {
        private readonly Poller _poller = new Poller();
        private readonly PullSocket _socket;
        private readonly string _address;

        internal ZeroReceiver(string address, NetMQContext mqContext)
        {
            _address = address;
            _socket = mqContext.CreatePullSocket();
            _poller.AddSocket(_socket);
            Data = Observable.FromEvent<EventHandler<NetMQSocketEventArgs>, NetMQSocketEventArgs>(
                handler => (s, e) => {
                    handler(e);
                },
                handler => _socket.ReceiveReady += handler,
                handler => _socket.ReceiveReady -= handler)
                .Select(_ => {
                    using (var ms = new MemoryStream(_socket.ReceiveFrameBytes())) {
                        return Serializer.DeserializeWithLengthPrefix<T>(ms, PrefixStyle.Fixed32);
                    }
                })
                .Publish()
                .RefCount();
        }

        public IObservable<T> Data { get; private set; }

        public void Start()
        {
            _poller.PollTillCancelledNonBlocking();
            _socket.Connect(_address);
        }

        public void Stop()
        {
            _poller.CancelAndJoin();
            _socket.Disconnect(_address);
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            _poller.Dispose();
            _socket.Dispose();
        }

        #endregion
    }
}

