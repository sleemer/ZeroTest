using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeroTransport
{
    public class TcpSessionFactory<T> : ISessionFactory<T>
    {
        public IPushSession<T> CreatePushSession(string address)
        {
            if (string.IsNullOrWhiteSpace(address)) {
                throw new ArgumentNullException("address");
            }
            var parts = address.Split(':');
            if (parts.Count() < 2) {
                throw new ArgumentException("Wrong format. Address expected in format 'ip:port'");
            }
            return new TcpPushSession<T>(parts[parts.Count() - 2].TrimStart('/'), int.Parse(parts[parts.Count() - 1]));
        }

        public IPullSession CreatePullSession(string address, IObservable<T> source)
        {
            if (string.IsNullOrWhiteSpace(address)) {
                throw new ArgumentNullException("address");
            }
            var parts = address.Split(':');
            if (parts.Count() < 2) {
                throw new ArgumentException("Wrong format. Address expected in format 'ip:port'");
            }
            return new TcpPullSession<T>(source, parts[parts.Count() - 2].TrimStart('/'), int.Parse(parts[parts.Count()-1]));
        }
        public ISubSession<T> CreateSubSession(string address)
        {
            if (string.IsNullOrWhiteSpace(address)) {
                throw new ArgumentNullException("address");
            }
            var parts = address.Split(':');
            if (parts.Count() < 2) {
                throw new ArgumentException("Wrong format. Address expected in format 'ip:port'");
            }
            return new TcpSubSession<T>(parts[parts.Count() - 2].TrimStart('/'), int.Parse(parts[parts.Count() - 1]));
        }

        public IPubSession CreatePubSession(string address, IObservable<T> source)
        {
            if (string.IsNullOrWhiteSpace(address)) {
                throw new ArgumentNullException("address");
            }
            var parts = address.Split(':');
            if (parts.Count() < 2) {
                throw new ArgumentException("Wrong format. Address expected in format 'ip:port'");
            }
            return new TcpPubSession<T>(source, parts[parts.Count() - 2].TrimStart('/'), int.Parse(parts[parts.Count() - 1]));
        }
    }
}
