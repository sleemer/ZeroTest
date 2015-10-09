using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeroTransport
{
    public interface IOutPin : IDisposable
    {
        void Bind();
        void Unbind();
        PinState State { get; }
    }
}
