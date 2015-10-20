using System;

namespace ZeroCore.Contracts
{
    public interface IImageProvider : IDisposable
    {
        IObservable<ImagePacket> GetImageStream();
    }
}
