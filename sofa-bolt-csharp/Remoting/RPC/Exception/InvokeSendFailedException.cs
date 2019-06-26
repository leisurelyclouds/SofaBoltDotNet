using Remoting.exception;
using System;


namespace Remoting.rpc.exception
{
    /// <summary>
    /// Exception when invoke send failed
    /// </summary>
    public class InvokeSendFailedException : RemotingException
	{

		/// <summary>
		/// For serialization
		/// </summary>
		private const long serialVersionUID = 4832257777758730796L;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public InvokeSendFailedException()
		{
		}

		public InvokeSendFailedException(string msg) : base(msg)
		{
		}

		public InvokeSendFailedException(string msg, Exception cause) : base(msg, cause)
		{
		}
	}
}