using System;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Threading;
using System.Reactive.Concurrency;
using System.Collections.Concurrent;

namespace RxTest
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("started");
            TestReply();
            Console.ReadKey();
        }

        private static async void TestReply()
        {
            var stream = CreateWithBuffer(Observable.Interval(TimeSpan.FromMilliseconds(1000 / 7))
                .Publish()
                .RefCount(), 5);
            var sub1 = stream.ObserveOn(NewThreadScheduler.Default).Subscribe(item => Console.WriteLine("sub1:{0}", item));
            await Task.Delay(TimeSpan.FromSeconds(3));
            var sub2 = stream.ObserveOn(NewThreadScheduler.Default).Subscribe(item => Console.WriteLine("sub2:{0}", item));
            await Task.Delay(TimeSpan.FromSeconds(2));
            sub1.Dispose();
            sub2.Dispose();
        }

        private static IObservable<T> CreateWithBuffer<T>(IObservable<T> source, int bufferLength)
        {
            var buffer = new FixedConcurrentQueue<T>(bufferLength);
            source.ObserveOn(NewThreadScheduler.Default).Subscribe(item => buffer.Enqueue(item));
            return Observable.Create<T>(o => {
                var items = buffer.ToArray();
                foreach (var item in items) {
                    o.OnNext(item);
                }
                return new CompositeDisposable { source.Subscribe(o) };
            });
        }
    }
}
