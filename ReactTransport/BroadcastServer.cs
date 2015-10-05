using System;
using System.Net.Sockets;
using System.Net;
using System.Collections.Concurrent;
using System.Threading;
using System.Reactive.Linq;
using ProtoBuf;
using System.Threading.Tasks;

namespace ReactTransport
{
	public class BroadcastServer<T>:IDisposable
	{
		private IDisposable _clientsSubscription;
		private ConcurrentDictionary<Guid, ClientConnection<T>> _clients = new ConcurrentDictionary<Guid, ClientConnection<T>> ();
		private readonly TcpListener _listener;
		private readonly IObservable<T> _dataStream;

		public BroadcastServer (string ip, int port, IObservable<T> dataStream)
		{
			var ipAddress = IPAddress.Parse (ip);
			_listener = new TcpListener (ipAddress, port);
			_dataStream = dataStream;
		}

		public bool IsRunning{ get; private set; }

		public void Start ()
		{
			if (IsRunning) {
				throw new InvalidOperationException ("Server's already started.");
			}

			_listener.Start ();
			_clientsSubscription = Observable.FromAsync (_listener.AcceptTcpClientAsync)
				.Repeat ()
				.Subscribe (OnClientConnected);
			IsRunning = true;
		}

		public void Stop ()
		{
			if (!IsRunning) {
				throw new InvalidOperationException ("Server has not been started yet.");
			}

			var sub = _clientsSubscription;
			if (sub != null) {
				sub.Dispose();
				foreach (var clientConnection in _clients.Values) {
					clientConnection.Dispose ();
				}
			}
			IsRunning = false;
		}

		private void OnClientConnected (TcpClient client)
		{
			Task.Run (() => {
				var handshake = Serializer.DeserializeWithLengthPrefix<Handshake> (client.GetStream (), PrefixStyle.Fixed32);
				if (handshake == null) {
					return;
				}
				_clients.TryAdd (
					handshake.ClientId,
					new ClientConnection<T> (handshake.ClientId, handshake.ClientName, _dataStream, client)
				);
			});
		}

		#region IDisposable implementation

		public void Dispose ()
		{
			_clientsSubscription.Dispose ();
		}

		#endregion
	}
}

