using Remoting.exception;
using System;

namespace Remoting.rpc.exception
{
    /// <summary>
    /// Rpc server exception when processing request
    /// </summary>
    public class RpcServerException : RemotingException
	{
		/// <summary>
		/// For serialization
		/// </summary>
		private const long serialVersionUID = 4480283862377034355L;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public RpcServerException()
		{
		}

		public RpcServerException(string msg) : base(msg)
		{
		}

		public RpcServerException(string msg, Exception cause) : base(msg, cause)
		{
		}
	}
}