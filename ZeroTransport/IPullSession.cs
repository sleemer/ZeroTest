using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeroTransport
{
    public interface IPullSession : IDisposable
    {
        void Bind();
        void Unbind();
        SessionState State { get; }
    }
}
