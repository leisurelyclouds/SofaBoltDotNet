using System;

namespace Remoting.util
{
    /// <summary>
    /// Exception to represent the run method of a future task has not been called.
    /// </summary>
    public class FutureTaskNotRunYetException : Exception
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public FutureTaskNotRunYetException()
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public FutureTaskNotRunYetException(string message) : base(message)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public FutureTaskNotRunYetException(string message, Exception cause) : base(message, cause)
		{
		}
	}
}