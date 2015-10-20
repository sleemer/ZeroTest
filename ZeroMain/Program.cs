using System;
using ZeroTransport;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Diagnostics;
using System.Threading;
using System.IO;
using ZeroCore.Contracts;
using ZeroCore;

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
            using (var inPin = (new TcpSessionFactory<ImagePacket>()).CreateSubSession(address)) {
                inPin.Data.ObserveOn(TaskPoolScheduler.Default).Subscribe(i => {
                    Trace.TraceInformation(i.Timestamp.ToLocalTime().ToString());
                    fps++;
                    if (fpsTimer.ElapsedMilliseconds >= 1000) {
						Trace.TraceInformation("{0:0.##} fps", fps);
                        fps = 0;
                        fpsTimer.Restart();
                    }
                });
                inPin.Start();
                Trace.TraceInformation("Started getting images from camera at {0}.", DateTimeOffset.Now.LocalDateTime);
                while (Console.ReadKey().Key != ConsoleKey.Escape) {
                    if (inPin.State == SessionState.Connected) {
                        inPin.Stop();
						Trace.TraceInformation("stopped");
                    } else {
                        inPin.Start();
                        Trace.TraceInformation("Started getting images from camera at {0}.", DateTimeOffset.Now.LocalDateTime);
                    }
                }
            }
        }
    }
}
