using System;

namespace ZeroTransport
{
    public interface IPubSession : IDisposable
    {
        void Start();
        void Stop();
        SessionState State { get; }
    }
}
