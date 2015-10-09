using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeroTransport
{
    interface IPinFactory<T>
    {
        IInPin<T> CreateInPin(string address);
        IOutPin CreateOutPin(string address, IObservable<T> source);
    }
}
