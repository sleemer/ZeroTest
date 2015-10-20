using System;
using System.Reactive.Linq;
using ZeroCore.Contracts;

namespace ZeroCore
{
    public class FakeImageProvider : IImageProvider
    {
        public FakeImageProvider()
        {
        }

        private static ImagePacket _fakeImage;

        static FakeImageProvider()
        {
            var image = new byte[1920 * 1080 * 4];
            var rnd = new Random();
            for (int i = 0; i < image.Length; i++) {
                image[i] = (byte)rnd.Next(0, 255);
            }
            _fakeImage = new ImagePacket { Image = image };
        }

        public IObservable<ImagePacket> GetImageStream()
        {
            return Observable.Interval(TimeSpan.FromMilliseconds(1000 / 35))
                .Select(_ => {
                    _fakeImage.Timestamp = DateTime.Now;
                    return _fakeImage;
                });
        }

        public void Dispose()
        {
        }
    }
}

