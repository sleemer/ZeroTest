using System;
using System.Collections.Generic;

namespace Test
{
	public sealed class Heap<T>
	{
		private readonly IComparer<T> _comparer;
		private T[] _backstore;

		public Heap (int length, IComparer<T> comparer = null)
		{
			_backstore = new T[length];
			_comparer = comparer ?? Comparer<T>.Default;
		}
		public Heap (T[] source, IComparer<T> comparer = null):this(source.Length, comparer)
		{
			foreach (var item in source) {
				Add (item);
			}
		}

		public int Count{ get; private set; }
		public void Add (T item)
		{
			if (_backstore.Length == Count) {
				IncreaseBackstore ();
			}

			_backstore [Count] = item;
			var index = Count;
			var parent = GetParentIndex (index);
			while (index > 0 && _comparer.Compare (_backstore [parent], _backstore [index]) < 0) {
				Swap (index, parent);
				index = parent;
				parent = GetParentIndex (index);
			}
			Count++;
		}
		public T Remove(){
			if (Count == 0) {
				throw new InvalidOperationException ();
			}
			var item = _backstore [0];

			var index = 0;
			_backstore [0] = _backstore [Count-1];
			Count--;
			while (true) {
				var leftChild = index * 2 + 1;
				if (leftChild >= Count)
					break;
				var rightChild = leftChild + 1;
				var maxChild = GetMaxChildIndex (leftChild, rightChild);
				if (_comparer.Compare (_backstore [index], _backstore [maxChild]) >= 0) {
					break;
				}
				Swap (index, maxChild);
				index = maxChild;
			}

			return item;
		}

		private void IncreaseBackstore ()
		{
			var newStore = new T[_backstore.Length * 2];
			_backstore.CopyTo (newStore, 0);
			_backstore = newStore;
		}

		private void Swap (int left, int right)
		{
			if (left != right) {
				var tmp = _backstore [left];
				_backstore [left] = _backstore [right];
				_backstore [right] = tmp;
			}
		}

		private int GetParentIndex (int index)
		{
			return (index - 1) / 2;
		}

		private int GetMaxChildIndex (int leftChild, int rightChild)
		{
			return (rightChild >= Count || _comparer.Compare (_backstore [leftChild], _backstore [rightChild]) > 0)
				? leftChild
				: rightChild;
		}
	}
}

