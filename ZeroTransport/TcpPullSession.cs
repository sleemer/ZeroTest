using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ZeroTransport
{
    public class TcpPullSession<T> : IPullSession, IDisposable
    {
        private readonly IObservable<T> _source;
        private CancellationDisposable _subscription;
        private TcpListener _listener;

        public TcpPullSession(IObservable<T> source, string address, int port)
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
        public void Bind()
        {
            if (State == SessionState.Connected) {
                throw new InvalidOperationException();
            }
            State = SessionState.Connected;
			Console.WriteLine("Sender started on {0} thread.", Thread.CurrentThread.ManagedThreadId);
            _subscription = new CancellationDisposable();
            _listener.Start();
            Observable.FromAsync(() => _listener.AcceptTcpClientAsync())
                .Subscribe(client => {
                    var stream = client.GetStream();
                    _source.Subscribe(item => SendPacket(stream, item), _subscription.Token);
                }, _subscription.Token);
        }
        public void Unbind()
        {
            if (State == SessionState.Disconnected) {
                throw new InvalidOperationException();
            }
            _listener.Stop();
            _subscription.Dispose();
            _subscription = null;
            State = SessionState.Disconnected;
        }

        private void SendPacket(Stream stream, T packet)
        {
            try {
                var watch = Stopwatch.StartNew();
                Serializer.SerializeWithLengthPrefix(stream, packet, PrefixStyle.Fixed32);
				Console.WriteLine("sent packet on {0:D2} it took {1}",Thread.CurrentThread.ManagedThreadId, watch.ElapsedMilliseconds);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            if (_subscription != null) {
                Unbind();
            }
        }

        #endregion
    }
}
