using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeroTransport
{
    public interface IPubSession
    {
        void Start();
        void Stop();
        SessionState State { get; }
    }
}
