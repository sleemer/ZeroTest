using System;
using System.Linq;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;

namespace Test
{
	class MainClass
	{
		public static void Main (string[] args)
		{
//			var pts = new PauseTokenSource ();
//			Do (pts.Token);
//			while (true) {
//				if (Console.ReadKey ().Key == ConsoleKey.Enter) {
//					break;
//				}
//				pts.IsPaused = !pts.IsPaused;
//			}

//			int[] array = new int[]{ 2, 8, 7, 3, 10, 20, 4, 6, 21 };
//			Log ("before", array);
			
//			var heap = new Heap<int> (array);
//			while (heap.Count > 0) {
//				var max = heap.Remove ();
//				Console.WriteLine (max);
//			}

//			var quickSort = new QuickSort<int>();
//			quickSort.Sort (array);
//			Log ("after", array);

			var dataSubscription = Observable.Interval (TimeSpan.FromSeconds (1))
				.Take (5)
				.Merge(Observable.Delay(Observable.Interval(TimeSpan.FromSeconds(3)).Take(3), TimeSpan.FromSeconds(5)))
				.Merge(Observable.Delay(Observable.Interval(TimeSpan.FromSeconds(3)).Take(1), TimeSpan.FromSeconds(15)))
				.DetectStale (TimeSpan.FromSeconds (2))
				.Subscribe (item => {
					Console.WriteLine (item.IsStale);
				});
			Console.ReadKey ();
			dataSubscription.Dispose ();
		}

		static async void Do (PauseToken token)
		{
			for (int i = 0; i < 50; i++) {
				await token.WaitWhilePausedAsync ();
				Console.WriteLine (i);
				await Task.Delay (100);
			}
		}

		static void Log (string prefix, int[] array)
		{
			var str = array.Aggregate (new StringBuilder (), (res, i) => res.AppendFormat ("{0},", i)).ToString ();
			Console.WriteLine ("{0}: {1}", prefix, str);
		}
	}
}
