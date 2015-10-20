using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace RxTest
{
    public class FixedConcurrentQueue<T>
    {
        private readonly ConcurrentQueue<T> _backedQueue;
        private readonly int _maxCapacity;
        private readonly object _syncObj = new object();

        public FixedConcurrentQueue(int maxCapacity)
        {
            _maxCapacity = maxCapacity;
            _backedQueue = new ConcurrentQueue<T>();
        }
        public FixedConcurrentQueue(IEnumerable<T> collection, int maxCapacity)
        {
            if (collection == null) {
                throw new ArgumentNullException("collection");
            }
            if (maxCapacity < 1) {
                throw new ArgumentOutOfRangeException("maxCapacity", "maxCapacity should be equal or greater than 1.");
            }
            _maxCapacity = maxCapacity;
            _backedQueue = (collection.Count() > maxCapacity)
                ? new ConcurrentQueue<T>(collection.Skip(collection.Count() - maxCapacity).Take(maxCapacity))
                : new ConcurrentQueue<T>(collection);
        }

        public void Enqueue(T item)
        {
            lock (_syncObj) {
                _backedQueue.Enqueue(item);
                T _;
                while (_backedQueue.Count > _maxCapacity) {
                    _backedQueue.TryDequeue(out _);
                }
            }
        }
        public bool TryDequeue(out T item)
        {
            lock (_syncObj) {
                return _backedQueue.TryDequeue(out item);
            }
        }
        public T[] ToArray()
        {
            lock (_syncObj) {
                return _backedQueue.ToArray();
            }
        }
    }
}
