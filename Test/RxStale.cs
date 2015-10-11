using System;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Threading;

namespace Test
{
	public static class RxStale
	{
		public static IObservable<Stale<T>> DetectStale<T> (this IObservable<T> source, TimeSpan timeout)
		{
			return Observable.Create<Stale<T>> (o => {
				var timeoutStream = Observable.Interval (timeout).Take (1);
				var timeoutSubscription = timeoutStream.Subscribe (_ => o.OnNext (new Stale<T> ()));
				var internalSubscription = source.Subscribe (
					item => {
						timeoutSubscription.Dispose ();
						o.OnNext (new Stale<T> (item));
						timeoutSubscription = timeoutStream.Subscribe (_ => {
							o.OnNext (new Stale<T> ());
						});
					},
					ex => o.OnError (ex),
					() => o.OnCompleted ());
				return new CompositeDisposable(internalSubscription, timeoutSubscription);
			});
		}
	}

	public class Stale<T>
	{
		public Stale ()
		{
			IsStale = true;
			Value = default(T);
		}

		public Stale (T value)
		{
			IsStale = false;
			Value = value;
		}

		public bool IsStale{ get; private set; }

		public T Value{ get; private set; }
	}
}

