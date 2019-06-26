using Remoting.exception;
using System;


namespace Remoting.rpc.exception
{
    /// <summary>
    /// Server exception caught when invoking
    /// </summary>
    public class InvokeServerException : RemotingException
	{
		/// <summary>
		/// For serialization
		/// </summary>
		private const long serialVersionUID = 4480283862377034355L;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public InvokeServerException()
		{
		}

		public InvokeServerException(string msg) : base(msg)
		{
		}

		public InvokeServerException(string msg, Exception cause) : base(msg, cause)
		{
		}
	}

}