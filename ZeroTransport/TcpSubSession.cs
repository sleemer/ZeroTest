using ProtoBuf;
using System;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;

namespace ZeroTransport
{
    public sealed class TcpSubSession<T>:ISubSession<T>, IDisposable
    {
        private readonly TcpClient _client;
        private readonly string _host;
        private readonly int _port;
        private readonly Subject<T> _data = new Subject<T>();
        private IDisposable _subscription;

        public TcpSubSession(string address, int port)
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
                }
                finally {
                    _stateLock.ExitWriteLock();
                }
            }
        }
        public void Start()
        {
            var connectionStream = Observable.FromAsync(() => _client.ConnectAsync(_host, _port));
            _subscription = connectionStream
                .Catch<Unit, Exception>(ex=>connectionStream.Delay(TimeSpan.FromSeconds(3)))
                .ObserveOn(NewThreadScheduler.Default)
                .Subscribe(_ => {
                    State = SessionState.Connected;
                    Console.WriteLine("Getting data from network on {0} thread.", Thread.CurrentThread.ManagedThreadId);
                    var stream = _client.GetStream();
                    while (true) {
                        try {
                            var item = Serializer.DeserializeWithLengthPrefix<T>(stream, PrefixStyle.Fixed32);
                            if (item == null) {
                                _data.OnCompleted();
                                break;
                            }
                            _data.OnNext(item);
                        }
                        catch (Exception ex) {
                            Console.WriteLine(ex.Message);
                            _data.OnError(ex);
                            break;
                        }
                    }
                });
        }
        public void Stop()
        {
            _subscription.Dispose();
            _subscription = null;
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
