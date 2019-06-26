using System;

namespace Remoting.exception
{
    /// <summary>
    /// Exception for default remoting problems
    /// </summary>
    public class RuntimeException : Exception
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public RuntimeException()
		{

		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public RuntimeException(string message) : base(message)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public RuntimeException(string message, Exception cause) : base(message, cause)
		{
		}
	}
}