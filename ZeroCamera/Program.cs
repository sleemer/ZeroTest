﻿using System;
using System.Diagnostics;
using System.Reactive.Linq;
using ZeroTransport;

namespace ZeroCamera
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            //string address = "ipc:///pictures-from-camera";
            //string address = "tcp://localhost:9000";
            string address = "127.0.0.1";
            int port = 9000;
			var img = new ImagePacket { Image = new byte[(2048 * 1080 * 24)/8], Timestamp = DateTime.Now };
            Console.WriteLine("Camera working...");
            var pictures = Observable.Interval(TimeSpan.FromMilliseconds(1000 / 60)).Select(_ => img);
            using (var outPin = PinFactory.CreateOutPin(address, port, pictures)) {
                outPin.Start();
                Console.ReadKey();
            }
        }
    }
}
