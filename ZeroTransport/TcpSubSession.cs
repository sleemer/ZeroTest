using ProtoBuf;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Disposables;

namespace ZeroTransport
{
	public sealed class TcpSubSession<T> : ISubSession<T>, IDisposable
	{
		private readonly string _host;
		private readonly int _port;
		private readonly Subject<T> _data = new Subject<T> ();
		private IDisposable _subscription;

		public TcpSubSession (string address, int port)
		{
			_host = address;
			_port = port;
		}

		public IObservable<T> Data { get { return _data.AsObservable (); } }

		private SessionState _state = SessionState.Disconnected;
		private ReaderWriterLockSlim _stateLock = new ReaderWriterLockSlim ();

		public SessionState State {
			get {
				_stateLock.EnterReadLock ();
				try {
					return _state;
				} finally {
					_stateLock.ExitReadLock ();
				}
			}
			private set {
				_stateLock.EnterWriteLock ();
				try {
					_state = value;
				} finally {
					_stateLock.ExitWriteLock ();
				}
			}
		}

		public void Start ()
		{
			if (State != SessionState.Disconnected) {
				throw new InvalidOperationException ();
			}

			StartInternal ();
		}

		private void StartInternal ()
		{
			var cancellationDisposable = new CancellationDisposable ();
			var token = cancellationDisposable.Token;
			Observable.FromAsync (() => GetActiveConnectionAsync (_host, _port, token))
				.SelectMany (client => {
					token.Register (() => client.Close ());
					return GetDataFromConnection (client, token);
				})
				.Repeat ()
                .ObserveOn (NewThreadScheduler.Default)
				.Subscribe (
					item => _data.OnNext (item),
					ex => Trace.WriteLine (ex.Message),
					token);
			_subscription = cancellationDisposable;
		}

		private async Task<TcpClient> GetActiveConnectionAsync (string host, int port, CancellationToken token)
		{
			State = SessionState.Connecting;
			var client = new TcpClient ();
			while (!token.IsCancellationRequested) {
				try {
					await client.ConnectAsync (host, port);
					State = SessionState.Connected;
					return client;
				} catch (Exception) {
				}
				try {
					await Task.Delay (TimeSpan.FromSeconds (3), token);
				} catch (OperationCanceledException) {
					break;
				}
			}
			return null;
		}

		private static IObservable<T> GetDataFromConnection (TcpClient client, CancellationToken externalToken)
		{
			return Observable.Create<T> (o => {
				var stream = client.GetStream ();
				var cts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
				var token = cts.Token;
				var cancellationDisposable = new CancellationDisposable(cts);
				Task.Run(()=>{
					while (!token.IsCancellationRequested) {
						try {
							var item = Serializer.DeserializeWithLengthPrefix<T> (stream, PrefixStyle.Fixed32);
							if (item == null) {
								o.OnCompleted ();
								break;
							}
							o.OnNext (item);
						} catch (Exception ex) {
							Trace.WriteLine (ex.Message);
							o.OnCompleted ();
							break;
						}
					}
				}, token);
				return cancellationDisposable;
			});
		}

		public void Stop ()
		{
			if (State == SessionState.Disconnected) {
				throw new InvalidOperationException ();
			}
				
			_subscription.Dispose ();
			State = SessionState.Disconnected;
		}

		#region Implementation of IDisposable

		public void Dispose ()
		{
			if (_subscription != null) {
				Stop ();
			}
		}

		#endregion
	}
}
