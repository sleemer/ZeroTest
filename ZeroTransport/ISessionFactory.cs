using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeroTransport
{
    interface ISessionFactory<T>
    {
        IPushSession<T> CreatePushSession(string address);
        IPullSession CreatePullSession(string address, IObservable<T> source);
        ISubSession<T> CreateSubSession(string address);
        IPubSession CreatePubSession(string address, IObservable<T> source);
    }
}
