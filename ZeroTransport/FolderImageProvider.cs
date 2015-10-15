using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroTransport
{
    public class FolderImageProvider : IDisposable
    {
        private List<ImagePacket> _images = new List<ImagePacket>();
        private int _currentImageIndex = -1;
        private IObservable<ImagePacket> _imageStream;
        private readonly string _folder;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private static string[] _supportedExts = new string[] { ".BMP", ".PNG", ".JPG" };

        public FolderImageProvider(string folder)
        {
            _folder = folder;
            _imageStream = InitAsync(_cts.Token)
                .ToObservable()
				.SelectMany(_ => GenerateImageStream());
        }

        public IObservable<ImagePacket> GetImageStream()
        {
            return _imageStream;
        }

        private async Task InitAsync(CancellationToken token)
        {
            _images.Clear();
            foreach (var filePath in Directory.GetFiles(_folder).Where(file => _supportedExts.Contains(Path.GetExtension(file).ToUpper()))) {
                var image = await FileUtils.ReadAllBytesAsync(filePath, token)
                                           .ConfigureAwait(false);
                _images.Add(new ImagePacket { Image = image, Timestamp = DateTime.Now });
            }
        }
        private IObservable<ImagePacket> GenerateImageStream()
        {
            return Observable.Interval(TimeSpan.FromMilliseconds(1000 / 35))
                             .Select(x => GetNextImagePacket())
                             .Publish()
                             .RefCount();
        }
        private ImagePacket GetNextImagePacket()
        {
            _currentImageIndex++;
            if (_currentImageIndex >= _images.Count)
                _currentImageIndex = 0;
            var img = _images[_currentImageIndex];
            img.Timestamp = DateTime.Now;
            return img;
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            _cts.Dispose();
        }

        #endregion
    }
}
