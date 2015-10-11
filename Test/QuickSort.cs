using System;
using System.Collections.Generic;

namespace Test
{
	public sealed class QuickSort<T>
	{
		private static Random _rnd = new Random ();
		private readonly IComparer<T> _comparer;

		public QuickSort (IComparer<T> comparer = null)
		{
			_comparer = comparer ?? Comparer<T>.Default;
		}

		public void Sort (T[] array)
		{
			if (array == null) {
				throw new ArgumentNullException ("array");
			}

			Sort (array, 0, array.Length - 1);
		}

		private void Sort (T[] array, int left, int right)
		{
			if (left < right) {
				var pivotIndex = _rnd.Next (left, right);
				pivotIndex = Partition (array, left, right, pivotIndex);
				Sort (array, left, pivotIndex - 1);
				Sort (array, pivotIndex + 1, right);
			}
		}

		private int Partition (T[] array, int left, int right, int pivotIndex)
		{
			var pivot = array [pivotIndex];
			Swap (array, pivotIndex, right);
			pivotIndex = left;
			for (var index = left; index < right; index++) {
				if (_comparer.Compare (array [index], pivot) > 0) {
					Swap (array, index, pivotIndex);
					pivotIndex++;
				}
			}
			Swap (array, pivotIndex, right);
			return pivotIndex;
		}

		private static void Swap (T[] array, int left, int right)
		{
			if (left != right) {
				T tmp = array [left];
				array [left] = array [right];
				array [right] = tmp;
			}
		}
	}
}

