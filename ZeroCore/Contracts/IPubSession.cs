using System;

namespace ZeroCore.Contracts
{
    public interface IPubSession : IDisposable
    {
        void Start();
        void Stop();
        SessionState State { get; }
    }
}
