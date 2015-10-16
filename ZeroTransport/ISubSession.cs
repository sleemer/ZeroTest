using System;

namespace ZeroTransport
{
    public interface ISubSession<T> : IDisposable
    {
        void Start();
        void Stop();
        IObservable<T> Data { get; }
        SessionState State { get; }
    }
}
