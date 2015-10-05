using System;
using NetMQ;
using NetMQ.Sockets;
using System.Reactive.Linq;
using System.IO;
using ProtoBuf;

namespace ZeroTransport
{
    public class ZeroSender<T> : IDisposable
    {
        private readonly IObservable<T> _source;
        private readonly string _address;
        private readonly PushSocket _socket;
        private IDisposable _subscription;

        public ZeroSender(IObservable<T> source, string address, NetMQContext context)
        {
            _source = source;
            _address = address;
            _socket = context.CreatePushSocket();
        }
        public void Start()
        {
            _socket.Bind(_address);
            _subscription = _source.Subscribe(item => {
                using (var ms = new MemoryStream()) {
                    Serializer.SerializeWithLengthPrefix(ms, item, PrefixStyle.Fixed32);
                    _socket.SendFrame(ms.ToArray());
                }
            });
        }
        public void Stop()
        {
            _subscription.Dispose();
            _subscription = null;
            _socket.Unbind(_address);
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            if (_subscription != null) {
                Stop();
            }
            _socket.Dispose();
        }

        #endregion
    }
}

