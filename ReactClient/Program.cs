using System;
using ReactTransport;

namespace ReactClient
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			var id = Guid.NewGuid ();
			var name = "client#1";
			var client = new Client<long> (id, name);
			client.Connect ("127.0.0.1", 9800);
			client.Data.Subscribe (i => Console.WriteLine (i));
			client.Notifications.Subscribe (_ => client.GetNextPacket ());
			Console.WriteLine ("Client started.");
			Console.ReadKey ();
		}
	}
}
