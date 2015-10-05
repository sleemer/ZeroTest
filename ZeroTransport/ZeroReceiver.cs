using System;
using NetMQ;
using NetMQ.Sockets;
using System.Reactive.Linq;
using System.IO;

namespace ZeroTransport
{
	public class ZeroReceiver<T>
	{
		private readonly PullSocket _socket;
		private readonly string _address;

		internal ZeroReceiver (string address, NetMQContext mqContext)
		{
			_address = address;
			_socket = mqContext.CreatePullSocket ();
			Data = Observable.FromEvent<EventHandler<NetMQSocketEventArgs>, NetMQSocketEventArgs> (
				handler => (s, e) => {
					handler (e);
				},
				handler => _socket.ReceiveReady += handler,
				handler => _socket.ReceiveReady -= handler)
				.Select (_ => {
				using (var ms = new MemoryStream (_socket.ReceiveFrameBytes ())) { 
					return ProtoBuf.Serializer.Deserialize<T> (ms);
				}
			})
				.Publish ()
				.RefCount ();

		}

		public IObservable<T> Data{ get; private set; }

		public void Start ()
		{
			_socket.Connect (_address);

		}

		public void Stop ()
		{
			_socket.Disconnect (_address);
		}

		private void OnReceive (object sender, NetMQSocketEventArgs e)
		{
			
		}
	}
}

