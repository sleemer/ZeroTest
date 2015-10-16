using ProtoBuf;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;

namespace ZeroTransport
{
    public sealed class TcpSubSession<T> : ISubSession<T>, IDisposable
    {
        private readonly string _host;
        private readonly int _port;
        private readonly Subject<T> _data = new Subject<T>();
        private TcpClient _client;
        private IDisposable _subscription;

        public TcpSubSession(string address, int port)
        {
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
                }
                finally {
                    _stateLock.ExitWriteLock();
                }
            }
        }
        public void Start()
        {
            if (State != SessionState.Disconnected) {
                throw new InvalidOperationException();
            }

            StartInternal();
        }

        private void StartInternal()
        {
            State = SessionState.Connecting;
            _client = new TcpClient();
            _subscription = Observable.FromAsync(() => _client.ConnectAsync(_host, _port))
                .Catch<Unit, Exception>(ex => Observable.FromAsync(() => _client.ConnectAsync(_host, _port)).Delay(TimeSpan.FromSeconds(3)))
                .ObserveOn(NewThreadScheduler.Default)
                .Subscribe(_ => {
                    State = SessionState.Connected;
                    var stream = _client.GetStream();
                    while (true) {
                        try {
                            var item = Serializer.DeserializeWithLengthPrefix<T>(stream, PrefixStyle.Fixed32);
                            if (item == null) {
                                if (State != SessionState.Disconnected) {
                                    _subscription.Dispose();
                                    StartInternal();
                                }
                                break;
                            }
                            _data.OnNext(item);
                        }
                        catch (Exception ex) {
                            if (State != SessionState.Disconnected) {
                                Trace.WriteLine(ex.Message);
                                _subscription.Dispose();
                                StartInternal();
                            }
                            break;
                        }
                    }
                },
                ex => {
                    Trace.WriteLine(ex.Message);
                });
        }

        public void Stop()
        {
            if (State == SessionState.Disconnected) {
                throw new InvalidOperationException();
            }

            _client.Close();
            _subscription.Dispose();
            State = SessionState.Disconnected;
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            if (_subscription != null) {
                Stop();
            }
        }

        #endregion
    }
}
