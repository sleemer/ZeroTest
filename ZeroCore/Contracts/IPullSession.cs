using System;

namespace ZeroCore.Contracts
{
    public interface IPullSession : IDisposable
    {
        void Bind();
        void Unbind();
        SessionState State { get; }
    }
}
