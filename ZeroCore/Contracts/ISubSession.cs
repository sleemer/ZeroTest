using System;

namespace ZeroCore.Contracts
{
    public interface ISubSession<T> : IDisposable
    {
        void Start();
        void Stop();
        IObservable<T> Data { get; }
        SessionState State { get; }
    }
}
