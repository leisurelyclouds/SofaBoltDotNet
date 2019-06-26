using java.util.concurrent;
using java.util.concurrent.atomic;

namespace Remoting.util
{
	/// <summary>
	/// A customized FutureTask which can record whether the run method has been called.
	/// </summary>
	public class RunStateRecordedFutureTask : FutureTask
    {
		private AtomicBoolean hasRun = new AtomicBoolean();

		public RunStateRecordedFutureTask(Callable callable) : base(callable)
		{
		}

		public override void run()
		{
			hasRun.set(true);
			base.run();
		}

		public virtual object AfterRun
		{
			get
			{
				if (!hasRun.get())
				{
					throw new FutureTaskNotRunYetException();
				}
    
				if (!isDone())
				{
					throw new FutureTaskNotCompleted();
				}
    
				return base.get();
			}
		}
	}
}