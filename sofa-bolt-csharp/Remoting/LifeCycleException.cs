using System;

namespace Remoting
{
    public class LifeCycleException : Exception
	{
		public LifeCycleException(string message) : base(message)
		{
		}
	}
}