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
                }
                finally {
                    _stateLock.ExitWriteLock();
                }
            }
        }
        public void Connect()
        {
            _subscription = Observable.FromAsync(() => _client.ConnectAsync(_host, _port))
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
        public void Disconnect()
        {
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
