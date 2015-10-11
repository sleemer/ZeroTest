using System;
using NetMQ;
using NetMQ.Sockets;
using System.Reactive.Linq;
using System.IO;
using ProtoBuf;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace ZeroTransport
{
    public class ZeroPushSession<T> : IPushSession<T>, IDisposable
    {
        private readonly Poller _poller = new Poller();
        private readonly PullSocket _socket;
        private readonly string _address;
        private readonly NetMQTimer _timeout;

        internal ZeroPushSession(string address, NetMQContext mqContext)
        {
            _address = address;
            _socket = mqContext.CreatePullSocket();
            _socket.Options.ReceiveHighWatermark = 0;
            _timeout = new NetMQTimer(TimeSpan.FromSeconds(5));
            _timeout.Elapsed += (s, e) => {
                if (State == SessionState.Connected) {
                    _socket.Disconnect(_address);
                    _socket.Connect(_address);
                }
            };
            _poller.AddSocket(_socket);
            _poller.AddTimer(_timeout);
            _poller.PollTillCancelledNonBlocking();
            int count = 0;
            Data = Observable.FromEvent<EventHandler<NetMQSocketEventArgs>, NetMQSocketEventArgs>(
                    handler => (s, e) => handler(e),
                    handler => _socket.ReceiveReady += handler,
                    handler => _socket.ReceiveReady -= handler)
                .Select(_ => {
                    Debug.WriteLine("received {0} packet", count++);
                    _timeout.Enable = false;
                    _timeout.Enable = true;
                    using (var ms = new MemoryStream(_socket.ReceiveFrameBytes())) {
                        return Serializer.Deserialize<T>(ms);
                    }
                })
                .Publish()
                .RefCount();
        }

        public IObservable<T> Data { get; private set; }
        private SessionState _state = SessionState.Disconnected;
        private ReaderWriterLockSlim _stateLock = new ReaderWriterLockSlim();
        public SessionState State
        {
            get
            {
                _stateLock.EnterReadLock();
                try {
                    return _state;
                }
                finally {
                    _stateLock.ExitReadLock();
                }
            }
            private set
            {
                _stateLock.EnterWriteLock();
                try {
                    _state = value;
                }
                finally {
                    _stateLock.ExitWriteLock();
                }
            }
        }
        public void Connect()
        {
            if (State == SessionState.Connected) {
                throw new InvalidOperationException();
            }
            _timeout.Enable = true;
            _socket.Connect(_address);
            State = SessionState.Connected;
        }

        public void Disconnect()
        {
            if (State == SessionState.Disconnected) {
                throw new InvalidOperationException();
            }
            _timeout.Enable = false;
            _socket.Disconnect(_address);
            State = SessionState.Disconnected;
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            _poller.CancelAndJoin();
            _poller.Dispose();
            _socket.Dispose();
        }

        #endregion
    }
}

