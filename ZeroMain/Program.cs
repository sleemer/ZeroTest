using System;
using ZeroTransport;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace ZeroMain
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
			int fps = 0;
            var fpsTimer = Stopwatch.StartNew();
            Console.WriteLine("Getting images from camera on {0} thread.", Thread.CurrentThread.ManagedThreadId);
            using (var inPin = (new TcpSessionFactory<ImagePacket>()).CreatePushSession(address)) {
                inPin.Data.ObserveOn(TaskPoolScheduler.Default).Subscribe(i => {
                    fps++;
                    if (fpsTimer.ElapsedMilliseconds >= 1000) {
                        Console.WriteLine("{0:0.##} fps", fps);
                        fps = 0;
                        fpsTimer.Restart();
                    }
                });
                inPin.Connect();
                Console.WriteLine("started");
                while (Console.ReadKey().Key != ConsoleKey.Escape) {
                    if (inPin.State == SessionState.Connected) {
                        inPin.Disconnect();
                        Console.WriteLine("stopped");
                    } else {
                        inPin.Connect();
                        Console.WriteLine("started");
                    }
                }
            }
        }
    }
}
