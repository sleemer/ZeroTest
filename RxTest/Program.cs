using System;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Threading;

namespace RxTest
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("started");
//			TestObservables ();
			TestObservableFromAsync();
			Console.ReadKey ();
		}

		private static async void TestObservableFromAsync()
		{
			var sub = CreateFromAsync ()
				.Subscribe (i => Console.WriteLine (i));
			await Task.Delay (2000);
			sub.Dispose ();
		}
		private static async void TestObservables()
		{
			var stream = CreateStream ().Publish ();
			var dis = stream.Connect ();
			await Task.Delay (2000);
			var sub1 = stream.Subscribe (i => Console.WriteLine ("sub1: {0}", i));
			await Task.Delay (2000);
			var sub2 = stream.Subscribe (i => Console.WriteLine ("sub2: {0}", i));
			await Task.Delay (2000);
			sub1.Dispose ();
			await Task.Delay (2000);
			sub2.Dispose ();
			await Task.Delay (2000);
			dis.Dispose ();
		}

		private static int _next = 1;
		private static async Task<int> GetIntAsync()
		{
			await Task.Delay (100);
			return _next++;
		}
		private static IObservable<int> CreateFromAsync()
		{
			return Observable.FromAsync (GetIntAsync)
				.Repeat ()
				.Publish ()
				.RefCount ();
		}

		private static IObservable<long> CreateStream()
		{
			return Observable.Create<long> (o => {
				int subNumber = Interlocked.Increment(ref _subNumber);
				Console.WriteLine("subsribing to internal subscription {0}", subNumber);
				var sub = Observable.Interval (TimeSpan.FromSeconds (1))
					.Do (i => Console.WriteLine ("do{0}: {1}",subNumber, i))
					.Subscribe (o);
				var disposable = Disposable.Create (() => {
					Console.WriteLine ("internal subscription {0} disposed", subNumber);
				});
				var compositeDisposable = new CompositeDisposable ();
				compositeDisposable.Add (sub);
				compositeDisposable.Add (disposable);
				return compositeDisposable;
			});
		}
		private static int _subNumber = 0;
	}
}
