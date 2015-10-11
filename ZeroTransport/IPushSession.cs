using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeroTransport
{
    public interface IPushSession<out T> : IDisposable
    {
        void Connect();
        void Disconnect();
        IObservable<T> Data { get; }
        SessionState State { get; }
    }
}
