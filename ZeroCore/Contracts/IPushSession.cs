using System;

namespace ZeroCore.Contracts
{
    public interface IPushSession<out T> : IDisposable
    {
        void Connect();
        void Disconnect();
        IObservable<T> Data { get; }
        SessionState State { get; }
    }
}
