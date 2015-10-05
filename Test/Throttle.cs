using System;
using System.Threading;

namespace Test
{
	public interface IPin<T>
	{
		event EventHandler<T> Data;
	}

	public sealed class Throttle<T,T1,T2,T3>
	{
		private readonly IPin<T> _mainPin;
		private readonly IPin<T1> _pin1;
		private readonly IPin<T2> _pin2;
		private readonly IPin<T3> _pin3;

		private T1 _payload1;
		private T2 _payload2;
		private T3 _payload3;

		public Throttle (IPin<T> mainPin, IPin<T1> pin1, IPin<T2> pin2, IPin<T3> pin3)
		{
//			_mainPin = mainPin;
//			_pin1 = pin1;
//			_pin2 = pin2;
//			_pin3 = pin3;
//
//			_mainPin.Data += (sender, item) => {
//				RaiseData(new Tuple<T, T1, T2, T3>(item, (_payload1, _payload2, _payload3));
//			};
//			_pin1.Data+=(sender, item) => {
//				Interlocked.Exchange(ref _payload1, item);
//			};
//			_pin2.Data+=(sender, item) => {
//				Interlocked.Exchange(ref _payload2, item);
//			};
//			_pin3.Data+=(sender, item) => {
//				Interlocked.Exchange(ref _payload3, item);
//			};
		}

		public event EventHandler<Tuple<T,T1,T2,T3>> Data;
		private void RaiseData(Tuple<T,T1,T2,T3> data)
		{
			var handler = Data;
			if (handler != null) {
				handler (this, data);
			}
		}
	}
}

