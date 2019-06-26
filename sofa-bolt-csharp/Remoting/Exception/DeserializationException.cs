using System;

namespace Remoting.exception
{
    /// <summary>
    /// Exception when deserialize failed
    /// </summary>
    public class DeserializationException : CodecException
	{
		private bool serverSide = false;

		/// <summary>
		/// Constructor.
		/// </summary>
		public DeserializationException()
		{

		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public DeserializationException(string message) : base(message)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public DeserializationException(string message, bool serverSide) : this(message)
		{
			this.serverSide = serverSide;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public DeserializationException(string message, Exception cause) : base(message, cause)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public DeserializationException(string message, Exception cause, bool serverSide) : this(message, cause)
		{
			this.serverSide = serverSide;
		}

		/// <summary>
		/// Getter method for property <tt>serverSide</tt>.
		/// </summary>
		/// <returns> property value of serverSide </returns>
		public virtual bool ServerSide
		{
			get
			{
				return serverSide;
			}
		}
	}
}