using Remoting.exception;
using System;


namespace Remoting.rpc.exception
{
    /// <summary>
    /// Exception when thread pool busy of process server
    /// </summary>
    public class InvokeServerBusyException : RemotingException
	{
		/// <summary>
		/// For serialization
		/// </summary>
		private const long serialVersionUID = 4480283862377034355L;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public InvokeServerBusyException()
		{
		}

		public InvokeServerBusyException(string msg) : base(msg)
		{
		}

		public InvokeServerBusyException(string msg, Exception cause) : base(msg, cause)
		{
		}
	}

}