using System;

namespace ZeroCore.Contracts
{
    public interface ISessionFactory<T>
    {
        IPushSession<T> CreatePushSession(string address);
        IPullSession CreatePullSession(string address, IObservable<T> source);
        ISubSession<T> CreateSubSession(string address);
        IPubSession CreatePubSession(string address, IObservable<T> source);
    }
}
