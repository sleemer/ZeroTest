using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using ZeroTransport;

namespace ZeroCamera
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            //string address = "ipc:///pictures-from-camera";
            string address = "tcp://localhost:9000";
            //string address = "127.0.0.1";
            //int port = 9000;
            Console.WriteLine("Camera working...");
            var pictures = new FolderImageProvider(@"C:\Users\v_kovalev\Pictures").GetImageStream();
            using (var outPin = (new ZeroFactory<ImagePacket>()).CreateOutPin(address, pictures)) {
                outPin.Bind();
                Console.WriteLine("started");
                while (Console.ReadKey().Key != ConsoleKey.Escape) {
                    if (outPin.State == PinState.Connected) {
                        outPin.Unbind();
                        Console.WriteLine("stopped");
                    } else {
                        outPin.Bind();
                        Console.WriteLine("started");
                    }
                }
            }
        }
    }
}
