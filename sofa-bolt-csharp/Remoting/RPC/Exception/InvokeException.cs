using Remoting.exception;
using System;

namespace Remoting.rpc.exception
{
    /// <summary>
    /// Exception when invoke failed
    /// </summary>
    public class InvokeException : RemotingException
	{
		/// <summary>
		/// For serialization
		/// </summary>
		private const long serialVersionUID = -3974514863386363570L;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public InvokeException()
		{
		}

		public InvokeException(string msg) : base(msg)
		{
		}

		public InvokeException(string msg, Exception cause) : base(msg, cause)
		{
		}
	}

}