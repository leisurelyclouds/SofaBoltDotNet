using System;

namespace Remoting.exception
{
    /// <summary>
    /// Exception when codec problems occur
    /// </summary>
    public class CodecException : RemotingException
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public CodecException()
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message"> the detail message. </param>
		public CodecException(string message) : base(message)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message"> the detail message </param>
		/// <param name="cause"> the cause </param>
		public CodecException(string message, Exception cause) : base(message, cause)
		{
		}
	}
}