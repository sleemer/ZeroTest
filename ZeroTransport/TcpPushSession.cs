using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace ZeroTransport
{
    public class TcpPushSession<T> : IPushSession<T>, IDisposable
    {
        private readonly TcpClient _client;
        private readonly string _host;
        private readonly int _port;
        private readonly Subject<T> _data = new Subject<T>();
        private IDisposable _subscription;

        public TcpPushSession(string address, int port)
        {
            _client = new TcpClient();
            _host = address;
            _port = port;
        }

        public IObservable<T> Data { get { return _data.AsObservable(); } }
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
					Trace.WriteLine(string.Format("new state: {0}",_state));
                }
                finally {
                    _stateLock.ExitWriteLock();
                }
            }
        }
        public void Connect()
        {
			if (State == SessionState.Connected||State == SessionState.Connecting) {
				throw new InvalidOperationException ();
			}
			State = SessionState.Connecting;
            _subscription = Observable.FromAsync(() => _client.ConnectAsync(_host, _port))
                .ObserveOn(NewThreadScheduler.Default)
                .Subscribe(_ => {
                    State = SessionState.Connected;
					Trace.WriteLine(string.Format("Getting data from network on {0} thread.", Thread.CurrentThread.ManagedThreadId));
                    var stream = _client.GetStream();
					while (State == SessionState.Connected) {
                        try {
                            var item = Serializer.DeserializeWithLengthPrefix<T>(stream, PrefixStyle.Fixed32);
                            if (item == null) {
                                _data.OnCompleted();
                                break;
                            }
                            _data.OnNext(item);
                        }
                        catch (Exception ex) {
							Trace.WriteLine(ex.Message);
                            _data.OnError(ex);
                            break;
                        }
                    }
                });
        }
        public void Disconnect()
        {
			if (State == SessionState.Disconnected) {
				throw new InvalidOperationException ();
			}
            _subscription.Dispose();
            _subscription = null;
            State = SessionState.Disconnected;
        }

        public void Dispose()
        {
            if (_subscription != null) {
                Disconnect();
            }
        }
    }
}
