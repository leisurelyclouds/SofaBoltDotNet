using System;

namespace Remoting.util
{
    public class FutureTaskNotCompleted : Exception
	{
		public FutureTaskNotCompleted()
		{
		}

		public FutureTaskNotCompleted(string message) : base(message)
		{
		}

		public FutureTaskNotCompleted(string message, Exception cause) : base(message, cause)
		{
		}
	}
}