using System;
using System.Reactive.Subjects;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using ProtoBuf;
using System.Reactive.Linq;

namespace ReactTransport
{
	public class Client<T>
	{
		private readonly Subject<bool> _notifications = new Subject<bool> ();
		private readonly Subject<T> _data = new Subject<T> ();
		private TcpClient _client;
		private CancellationTokenSource _cts;

		public Client (Guid id, string name)
		{
			Id = id;
			Name = name;

			_client = new TcpClient ();
		}

		public Guid Id{ get; private set; }

		public string Name{ get; private set; }

		public bool IsConnected{ get { return _client.Connected; } }

		public IObservable<T> Data {
			get {
				return _data.AsObservable ();
			}
		}
		public IObservable<bool> Notifications{
			get{
				return _notifications.AsObservable ();
			}
		}

		public void Connect (string ip, int port)
		{
			if (IsConnected) {
				throw new InvalidOperationException ("Already connected.");
			}

			var ipAddress = IPAddress.Parse (ip);

			_cts = new CancellationTokenSource ();
			var token = _cts.Token;

			Task.Run (async () => {
				try {
					await _client.ConnectAsync (ipAddress, port).ConfigureAwait (false);
					Serializer.SerializeWithLengthPrefix(_client.GetStream(), new Handshake(Id, Name), PrefixStyle.Fixed32);
					while (!token.IsCancellationRequested) {
						var packet = Serializer.DeserializeWithLengthPrefix<DataPacket<T>> (_client.GetStream (), PrefixStyle.Fixed32);

						if (packet == null) {
							_notifications.OnCompleted ();
							_data.OnCompleted ();
							break;
						}
						if (packet.IsNotificationTicket) {
							_notifications.OnNext (true);
						} else {
							_data.OnNext (packet.Item);
						}
					}
				} catch (Exception ex) {
					_data.OnError (ex);
					_notifications.OnError (ex);
				}
			}, token);

		}

		public void GetNextPacket()
		{
			Serializer.SerializeWithLengthPrefix (_client.GetStream (), DataTicket.SendOne, PrefixStyle.Fixed32);
		}
		public void DropNextPacket()
		{
			Serializer.SerializeWithLengthPrefix (_client.GetStream (), DataTicket.DropOne, PrefixStyle.Fixed32);
		}

		public void Disconnect ()
		{
			if (!IsConnected) {
				throw new InvalidOperationException ("Connect to server first!");
			}


			_cts.Cancel ();
			_cts = null;
			_client.Close ();
		}
	}
}

