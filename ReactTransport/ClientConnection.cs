using System;
using System.Net.Sockets;
using System.Collections.Concurrent;
using ProtoBuf;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Reactive.Disposables;
using System.Reactive.Concurrency;

namespace ReactTransport
{
	public class ClientConnection<T>:IDisposable
	{
		private readonly CompositeDisposable _subscriptions = new CompositeDisposable ();
		private TcpClient _client;
		private BlockingCollection<T> _buffer = new BlockingCollection<T> (30);

		public ClientConnection (Guid id, string name, IObservable<T> data, TcpClient client)
		{
			_client = client;
			Id = id;
			Name = name;

			_subscriptions.Add (data.ObserveOn (TaskPoolScheduler.Default).Subscribe (OnData));
			_subscriptions.Add (GetDataTickets (client).ObserveOn (TaskPoolScheduler.Default).Subscribe (OnDataTicket));
		}

		public Guid Id{ get; private set; }

		public string Name{ get; private set; }

		private void OnDataTicket (DataTicket dataTicket)
		{
			T data;
			if (_buffer.TryTake (out data) && dataTicket.IsRequired) {
				Serializer.SerializeWithLengthPrefix (_client.GetStream (), data, PrefixStyle.Fixed32);
			}
		}

		private void OnData (T item)
		{
			_buffer.Add (item);
			Serializer.SerializeWithLengthPrefix (_client.GetStream (), DataPacket<T>.NotificationTicket, PrefixStyle.Fixed32);
		}

		private static IObservable<DataTicket> GetDataTickets (TcpClient client)
		{
			return Observable.Create<DataTicket> (o => {
				var cancellationDisposable = new CancellationDisposable ();
				var token = cancellationDisposable.Token;
				Task.Run (() => {
					try {
						while (!token.IsCancellationRequested) {
							var dataTicket = Serializer.DeserializeWithLengthPrefix<DataTicket> (client.GetStream (), PrefixStyle.Fixed32);
							if (dataTicket == null) {
								o.OnCompleted ();
								break;
							} else {
								o.OnNext (dataTicket);
							}
						}
					} catch (Exception ex) {
						o.OnError (ex);
					}
				});
				return cancellationDisposable;
			});
		}

		#region IDisposable implementation

		public void Dispose ()
		{
			_subscriptions.Dispose ();
		}

		#endregion
	}
}

