using System;

namespace Remoting.exception
{
    /// <summary>
    /// Exception when connection is closed.
    /// </summary>
    public class ConnectionClosedException : RemotingException
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		public ConnectionClosedException()
		{
		}

		public ConnectionClosedException(string msg) : base(msg)
		{
		}

		public ConnectionClosedException(string msg, Exception cause) : base(msg, cause)
		{
		}
	}
}