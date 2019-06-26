using Remoting.exception;
using System;

namespace Remoting.rpc.exception
{
    /// <summary>
    /// Exception when invoke timeout
    /// </summary>
    public class InvokeTimeoutException : RemotingException
	{

		/// <summary>
		/// For serialization
		/// </summary>
		private const long serialVersionUID = -7772633244795043476L;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public InvokeTimeoutException()
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="msg"> the detail message </param>
		public InvokeTimeoutException(string msg) : base(msg)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="msg"> the detail message </param>
		/// <param name="cause"> the cause </param>
		public InvokeTimeoutException(string msg, Exception cause) : base(msg, cause)
		{
		}
	}
}