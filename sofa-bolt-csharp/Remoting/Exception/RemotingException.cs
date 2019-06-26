using System;

namespace Remoting.exception
{
    /// <summary>
    /// Exception for default remoting problems
    /// </summary>
    public class RemotingException : Exception
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public RemotingException()
		{

		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public RemotingException(string message) : base(message)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public RemotingException(string message, Exception cause) : base(message, cause)
		{
		}
	}
}