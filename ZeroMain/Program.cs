using System;
using ZeroTransport;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Diagnostics;
using System.Threading;

namespace ZeroMain
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            //string address = "ipc:///pictures-from-camera";
            //string address = "tcp://localhost:9000";
            string address = "127.0.0.1";
            int port = 9000;
            int counter = 0;
            var watcher = Stopwatch.StartNew();
            DateTime begin = DateTime.Now;
			Console.WriteLine("Getting images from camera on {0} thread.", Thread.CurrentThread.ManagedThreadId);
            using (var inPin = PinFactory.CreateInPin<ImagePacket>(address, port)) {
                inPin.Data.ObserveOn(TaskPoolScheduler.Default).Subscribe(i => {
                    counter++;
                    if (counter == 100) {
                        counter = 0;
                        watcher.Stop();
						Console.WriteLine("{0:D2} - {1:hh:mm:ss} - {2:hh:mm:ss}: {3}", Thread.CurrentThread.ManagedThreadId, begin, DateTime.Now, 1000.0 / (watcher.ElapsedMilliseconds / 100));
                        watcher = Stopwatch.StartNew();
                        begin = DateTime.Now;
                    }
                });
                inPin.Start();
                Console.ReadKey();
            }
        }
    }
}
