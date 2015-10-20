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
        private readonly IObservable<byte[]> _source;
        private FixedConcurrentQueue<byte[]> _buffer = new FixedConcurrentQueue<byte[]>(10);
        private CancellationDisposable _subscription;
        private TcpListener _listener;        

        public TcpPubSession(IObservable<T> source, string address, int port)
        {
            _source = source.Select(item => {
                using (var ms = new MemoryStream()) {
                    Serializer.SerializeWithLengthPrefix(ms, item, PrefixStyle.Fixed32);
                    return ms.ToArray();
                }
            });
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
            
            _subscription = new CancellationDisposable();
            var token = _subscription.Token;
            _source.Subscribe(item => _buffer.Enqueue(item), token);
            _listener.Start();
            Observable.FromAsync(() => _listener.AcceptTcpClientAsync())
                .Repeat()
                .Subscribe(
                    client => SendDataToClient(client, token),
                    token);

            State = SessionState.Connected;
            Trace.TraceInformation("TcpPubSession started on {0}.", _listener.LocalEndpoint.ToString());
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
            Trace.TraceInformation("TcpPubSession stopped on {0}.", _listener.LocalEndpoint.ToString());
        }

        private void SendDataToClient(TcpClient client, CancellationToken subscriptionToken)
        {
            var clientInfo = client.Client.RemoteEndPoint.ToString();
            int fps = 0;
            var stream = client.GetStream();
            var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(subscriptionToken);
            Stopwatch fpsTimer = Stopwatch.StartNew();
            tokenSource.Token.Register(() => client.Close());
            Trace.TraceInformation("new client from {0} connected", clientInfo);
            _source.StartWith(_buffer.ToArray()).Subscribe(
                async item => {
                    try {
                        await stream.WriteAsync(item, 0, item.Length);
                    }
                    catch (Exception ex) {
                        if (ex is IOException && ex.InnerException != null && ex.InnerException is SocketException) {
                            Trace.TraceInformation("client from {0} disconnected", clientInfo);
                        } else {
                            Trace.TraceError(ex.Message);
                        }
                        tokenSource.Cancel();
                    }
                    fps++;
                    if (fpsTimer.ElapsedMilliseconds >= 1000) {
                        Trace.TraceInformation("sending fps {0} to client {1}", fps, clientInfo);
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
