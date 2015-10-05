using System;
using System.Threading.Tasks;
using System.Threading;

namespace Test
{
	public sealed class PauseTokenSource
	{
		private static Task _completed = Task.FromResult(true);
		internal static Task Completed{get{ return _completed;}}
		static PauseTokenSource(){
		}

		private TaskCompletionSource<bool> _paused;
		public bool IsPaused {
			get{ return _paused != null;}
			set{
				if (value) {
					Interlocked.CompareExchange (ref _paused, new TaskCompletionSource<bool> (), null); 
				} else {
					while (true) {
						var paused = _paused;
						if (paused == null)
							return;
						if (Interlocked.CompareExchange (ref _paused, null, paused) == paused) {
							paused.SetResult (true);
							break;
						}
					}
				}
			}
		}
		public PauseToken Token {
			get{ return new PauseToken (this);}
		}
		internal Task WaitWhilePausedAsync(){
			var paused = _paused;
			return paused == null
				? Completed
				: paused.Task;
		}
	}
	public sealed class PauseToken
	{
		private readonly PauseTokenSource _pts;
		internal PauseToken(PauseTokenSource pts){
			_pts = pts;
		}

		public bool IsPaused{ get{ return _pts.IsPaused;} }
		public Task WaitWhilePausedAsync(){
			return _pts.IsPaused
				? _pts.WaitWhilePausedAsync ()
				: PauseTokenSource.Completed;
		}
	}
}

