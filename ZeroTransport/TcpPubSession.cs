using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroTransport
{
    public sealed class TcpPubSession<T> : IPubSession, IDisposable
    {
        private readonly IObservable<T> _source;
        private CancellationDisposable _subscription;
        private TcpListener _listener;

        public TcpPubSession(IObservable<T> source, string address, int port)
        {
            _source = source;
            _listener = new TcpListener(IPAddress.Parse(address), port);
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
        public void Start()
        {
            if (State == SessionState.Connected) {
                throw new InvalidOperationException();
            }
            State = SessionState.Connected;
            Console.WriteLine("Sender started on {0} thread.", Thread.CurrentThread.ManagedThreadId);
            _subscription = new CancellationDisposable();
            _listener.Start();
            Observable.FromAsync(() => _listener.AcceptTcpClientAsync())
                .Repeat()
                .Subscribe(
                    client => SendDataToClient(client),
                    _subscription.Token
                );
        }
        public void Stop()
        {
            if (State == SessionState.Disconnected) {
                throw new InvalidOperationException();
            }
            _listener.Stop();
            _subscription.Dispose();
            _subscription = null;
            State = SessionState.Disconnected;
        }

        private void SendDataToClient(TcpClient client)
        {
            var subscription = _subscription;
            if(subscription == null) {
                return;
            }

            var clientInfo = client.Client.RemoteEndPoint.ToString();
            int fps = 0;
            var stream = client.GetStream();
            var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(_subscription.Token);
            Stopwatch fpsTimer = Stopwatch.StartNew();
            tokenSource.Token.Register(() => client.Close());
            Trace.WriteLine(string.Format("new client from {0} connected", clientInfo));            
            _source.Subscribe(
                item => {
                    try {
                        Serializer.SerializeWithLengthPrefix(stream, item, PrefixStyle.Fixed32);
                    }
                    catch (Exception ex) {
                        if (ex is IOException && ex.InnerException != null && ex.InnerException is SocketException) {
                            Trace.WriteLine(string.Format("client from {0} disconnected", clientInfo));
                        } else {
                            Trace.TraceError(ex.Message);
                        }
                        tokenSource.Cancel();
                    }
                    fps++;
                    if (fpsTimer.ElapsedMilliseconds >= 1000) {
                        Trace.WriteLine(string.Format("sending fps {0} to client {1}", fps, clientInfo));
                        fps = 0;
                        fpsTimer.Restart();
                    }
                },
                ex => tokenSource.Cancel(),
                tokenSource.Token);
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
