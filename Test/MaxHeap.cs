using System;
using System.Collections.Generic;

namespace Test
{
	public sealed class Heap<T>
	{
		private readonly IComparer<T> _comparer;
		private T[] _store;

		public int Count{ get; private set; }

		public Heap (int length, IComparer<T> comparer = null)
		{
			_store = new T[length];
			_comparer = comparer ?? Comparer<T>.Default;
		}

		public Heap (T[] source, IComparer<T> comparer = null) : this (source.Length, comparer)
		{
			foreach (var item in source) {
				Add (item);
			}
			Count = source.Length;
		}

		public void Add (T item)
		{
			if (item == null) {
				throw new ArgumentNullException ("item");
			}
			if (Count == _store.Length) {
				IncreaseStore ();
			}

			_store [Count] = item;
			int index = Count;
			int parent = GetParentIndex (Count);
			while (index > 0 && _comparer.Compare (_store [parent], _store [index]) < 0) {
				Swap (_store, index, parent);
				index = parent;
				parent = GetParentIndex (index);
			}

			Count++;
		}

		public T Remove ()
		{
			if (Count == 0) {
				throw new InvalidOperationException ();
			}

			var item = _store [0];
			Count--;
			if (Count > 0) {
				_store [0] = _store [Count];
				int index = 0;
				int leftChild = index * 2 + 1;
				if (leftChild < Count) {
					int rightChild = index * 2 + 2;
					int maxChild = GetMaxChildIndex (leftChild, rightChild);

					while (leftChild < Count && _comparer.Compare (_store [index], _store [maxChild]) < 0) {
						Swap (_store, index, maxChild);
						index = maxChild;
						leftChild = index * 2 + 1;
						rightChild = index * 2 + 2;
						maxChild = GetMaxChildIndex (leftChild, rightChild);
					}
				}
			}
			return item;
		}

		private int GetParentIndex (int index)
		{
			return (index - 1) / 2;
		}

		private int GetMaxChildIndex (int left, int right)
		{
			return right >= Count || _comparer.Compare (_store [left], _store [right]) > 0
				? left
				: right;
		}

		private void IncreaseStore ()
		{
			var newStore = new T[_store.Length * 2];
			_store.CopyTo (newStore, 0);
			_store = newStore;
		}

		private static void Swap (T[] arrray, int left, int right)
		{
			if (left != right) {
				var tmp = arrray [left];
				arrray [left] = arrray [right];
				arrray [right] = tmp;
			}
		}
	}
}

