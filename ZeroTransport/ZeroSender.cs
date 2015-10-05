using System;

namespace ZeroTransport
{
	public class ZeroSender<T>
	{
		public ZeroSender (IObservable<T> source, string address)
		{
		}
		public void Start (){}
		public void Stop(){}
	}
}

