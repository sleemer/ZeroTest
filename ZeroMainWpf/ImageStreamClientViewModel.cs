using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZeroMainWpf.UI;
using ZeroTransport;

namespace ZeroMainWpf
{
    public sealed class ImageStreamClientViewModel : NotificationObject
    {
        #region Fields

        private IDisposable _imageStreamSubscription;
        private byte[] _staleImage;
        private readonly string _staleImageFilePath;

        #endregion

        #region Constructors

        public ImageStreamClientViewModel()
        {
            _staleImageFilePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Resources",
                "no-img.gif");

            _connectCommand = new DelegateCommand(() => Connect(), CanConnect);
            _disconnectCommand = new DelegateCommand(() => Disconnect(), CanDisconnect);
        }

        #endregion

        #region Properties

        private bool _isBusy = false;
        private string _serverIP = "127.0.0.1";
        private int _serverPort = 9000;
        private WriteableBitmap _currentImageFrame;
        private int _fps = 0;

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                RaisePropertyChanged(() => IsBusy);
            }
        }
        public string ServerIP
        {
            get { return _serverIP; }
            set
            {
                _serverIP = value;
                RaisePropertyChanged(() => ServerIP);
                _connectCommand.RaiseCanExecute();
            }
        }
        public int ServerPort
        {
            get { return _serverPort; }
            set
            {
                _serverPort = value;
                RaisePropertyChanged(() => ServerPort);
                _connectCommand.RaiseCanExecute();
            }
        }
        public WriteableBitmap CurrentImageFrame
        {
            get { return _currentImageFrame; }
            set
            {
                _currentImageFrame = value;
                RaisePropertyChanged(() => CurrentImageFrame);
            }
        }
        public int FPS
        {
            get { return _fps; }
            private set
            {
                _fps = value;
                RaisePropertyChanged(() => FPS);
            }
        }

        #endregion

        #region Commands

        private DelegateCommand _connectCommand;
        public ICommand ConnectCommand { get { return _connectCommand; } }
        private DelegateCommand _disconnectCommand;
        public ICommand DisconnectCommand { get { return _disconnectCommand; } }

        #endregion

        #region Private methods

        private bool CanConnect()
        {
            IPAddress _;
            return _imageStreamSubscription == null
                && IPAddress.TryParse(ServerIP, out _)
                && ServerPort != 0;
        }
        private void Connect()
        {
            if (!CanConnect()) {
                return;
            }

            IsBusy = true;
            int counter = 0;
            int count = 0;
            var timer = Stopwatch.StartNew();
            try {
                CompositeDisposable disposable = new CompositeDisposable();
                var pin = (new TcpSessionFactory<ImagePacket>()).CreateSubSession(string.Format("tcp://{0}:{1}", ServerIP, ServerPort));
                disposable.Add(pin.Data
                    .Sample(TimeSpan.FromMilliseconds(1000 / 35))
                    .Select(packet => new { Packet = packet, Buffer = UpdateBuffer(packet.Image) })
                    .ObserveOnDispatcher()
                    .Subscribe(
                        img => {
                            if (CurrentImageFrame == null) {
                                CurrentImageFrame = new WriteableBitmap(CreateBitmapFrame(img.Packet.Image));
                            } else {
                                RenderBitmap(img.Buffer, CurrentImageFrame);
                            }
                            counter++;
                            if (timer.ElapsedMilliseconds >= 1000) {
                                FPS = counter;
                                counter = 0;
                                timer.Restart();
                            }
                            Debug.WriteLine("timestamp={0}, total images={1}", img.Packet.Timestamp, count++);
                        },
                        ex => {
                            MessageBox.Show(ex.Message, "Error");
                            Disconnect();
                        }));
                disposable.Add(Disposable.Create(() => pin.Stop()));
                pin.Start();
                _imageStreamSubscription = disposable;
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message, "Error");
            }
            finally {
                IsBusy = false;
                _connectCommand.RaiseCanExecute();
                _disconnectCommand.RaiseCanExecute();
            }
        }
        private bool CanDisconnect()
        {
            return _imageStreamSubscription != null;
        }
        private void Disconnect()
        {
            if (!CanDisconnect()) {
                return;
            }

            _imageStreamSubscription.Dispose();
            _imageStreamSubscription = null;
            _connectCommand.RaiseCanExecute();
            _disconnectCommand.RaiseCanExecute();
        }

        private static BitmapFrame CreateBitmapFrame(byte[] image)
        {
            using (var ms = new MemoryStream(image)) {
                var decoder = BitmapDecoder.Create(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                return decoder.Frames[0];
            }
        }
        private static byte[] _buffer;
        private static Int32Rect _rect;
        private static int _stride;
        private static void RenderBitmap(byte[] image, WriteableBitmap bitmap)
        {
            bitmap.WritePixels(_rect, image, _rect.Width * _stride, 0, 0);
        }
        private static byte[] UpdateBuffer(byte[] image)
        {
            using (var ms = new MemoryStream(image)) {
                using (var img = new Bitmap(ms)) {
                    _stride = Image.GetPixelFormatSize(img.PixelFormat) / 8;
                    if (_buffer == null) {
                        _buffer = new byte[img.Width * img.Height * _stride];
                        _rect = new Int32Rect(0, 0, img.Width, img.Height);
                    }
                    BitmapData bitmapData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadWrite, img.PixelFormat);
                    IntPtr intPtr = bitmapData.Scan0;
                    Marshal.Copy(intPtr, _buffer, 0, _buffer.Length);
                    img.UnlockBits(bitmapData);
                    return _buffer;
                }
            }
        }

        #endregion
    }
}
