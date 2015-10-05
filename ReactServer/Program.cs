using System;
using System.Reactive.Linq;
using ReactTransport;

namespace ReactServer
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			var data = Observable.Interval (TimeSpan.FromSeconds (1))
				.Publish()
				.RefCount();
			var server = new BroadcastServer<long> ("127.0.0.1", 9800, data);
			server.Start ();
			Console.WriteLine ("Server started. Press any key to stop.");
			Console.ReadKey ();
			server.Stop ();
		}
	}
}
