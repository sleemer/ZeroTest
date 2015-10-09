using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeroTransport
{
    public interface IInPin<out T> : IDisposable
    {
        void Connect();
        void Disconnect();
        IObservable<T> Data { get; }
        PinState State { get; }
    }
}
