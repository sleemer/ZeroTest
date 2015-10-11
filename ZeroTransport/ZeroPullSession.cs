using System;
using NetMQ;
using NetMQ.Sockets;
using System.Reactive.Linq;
using System.IO;
using ProtoBuf;
using System.Threading;
using System.Diagnostics;

namespace ZeroTransport
{
    public class ZeroPullSession<T> : IPullSession, IDisposable
    {
        private readonly IObservable<T> _source;
        private readonly string _address;
        private readonly PushSocket _socket;
        private IDisposable _subscription;

        public ZeroPullSession(IObservable<T> source, string address, NetMQContext context)
        {
            _source = source;
            _address = address;
            _socket = context.CreatePushSocket();
            _socket.Options.SendHighWatermark = 0;
        }

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

        public void Bind()
        {
            if (State == SessionState.Connected) {
                throw new InvalidOperationException();
            }
            _socket.Bind(_address);
            int count = 0;
            var fpsTimer = Stopwatch.StartNew();
            _subscription = _source.Subscribe(item => {
                try {
                    using (var ms = new MemoryStream()) {
                        Serializer.Serialize(ms, item);
                        _socket.SendFrame(ms.ToArray());                        
                    }
                    count++;
                    if (fpsTimer.ElapsedMilliseconds >= 1000) {
                        Debug.WriteLine("fps: {0}", count);
                        count = 0;
                        fpsTimer.Restart();
                    }
                }
                catch (Exception) {
                    Unbind();
                }
            });
            State = SessionState.Connected;
        }
        public void Unbind()
        {
            if (State == SessionState.Disconnected) {
                throw new InvalidOperationException();
            }
            _subscription.Dispose();
            _subscription = null;
            _socket.Unbind(_address);
            State = SessionState.Disconnected;
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            if (_subscription != null) {
                Unbind();
            }
            _socket.Dispose();
        }

        #endregion
    }
}

