using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using ZeroCore;
using ZeroCore.Contracts;
using ZeroTransport;

namespace ZeroCamera
{
    class MainClass
    {
        public static void Main(string[] args)
        {
			Trace.Listeners.Add (new ConsoleTraceListener ());

            //string address = "ipc:///pictures-from-camera";
            string address = "tcp://127.0.0.1:9000";
            //string address = "127.0.0.1";
            //int port = 9000;
			Trace.WriteLine("Camera working...");
			var pictures = new FolderImageProvider(@"C:\Users\v_kovalev\Pictures").GetImageStream();
            using (var outPin = (new TcpSessionFactory<ImagePacket>()).CreatePubSession(address, pictures)) {
                outPin.Start();
				Trace.TraceInformation("started");
                while (Console.ReadKey().Key != ConsoleKey.Escape) {
                    if (outPin.State == SessionState.Connected) {
                        outPin.Stop();
						Trace.TraceInformation("stopped");
                    } else {
                        outPin.Start();
						Trace.TraceInformation("started");
                    }
                }
            }
        }
    }
}
