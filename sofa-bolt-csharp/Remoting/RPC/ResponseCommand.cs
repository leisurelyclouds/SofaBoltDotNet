using System;
using System.Net;

namespace Remoting.rpc
{
    /// <summary>
    /// Command of response.
    /// </summary>
    [Serializable]
	public class ResponseCommand : RpcCommand
	{

		/// <summary>
		/// For serialization
		/// </summary>
		private const long serialVersionUID = -5194754228565292441L;
		private ResponseStatus responseStatus;
		private long responseTimeMillis;
		private IPEndPoint responseHost;
		private Exception cause;

		public ResponseCommand() : base(RpcCommandType.RESPONSE)
		{
		}

		public ResponseCommand(CommandCode code) : base(RpcCommandType.RESPONSE, code)
		{
		}

		public ResponseCommand(int id) : base(RpcCommandType.RESPONSE)
		{
			Id = id;
		}

		public ResponseCommand(CommandCode code, int id) : base(RpcCommandType.RESPONSE, code)
		{
			Id = id;
		}

		public ResponseCommand(byte version, byte type, CommandCode code, int id) : base(version, type, code)
		{
			Id = id;
		}

		/// <summary>
		/// Getter method for property <tt>responseTimeMillis</tt>.
		/// </summary>
		/// <returns> property value of responseTimeMillis </returns>
		public virtual long ResponseTimeMillis
		{
			get
			{
				return responseTimeMillis;
			}
			set
			{
				responseTimeMillis = value;
			}
		}


		/// <summary>
		/// Getter method for property <tt>responseHost</tt>.
		/// </summary>
		/// <returns> property value of responseHost </returns>
		public virtual IPEndPoint ResponseHost
		{
			get
			{
				return responseHost;
			}
			set
			{
				responseHost = value;
			}
		}


		/// <summary>
		/// Getter method for property <tt>responseStatus</tt>.
		/// </summary>
		/// <returns> property value of responseStatus </returns>
		public virtual ResponseStatus ResponseStatus
		{
			get
			{
				return responseStatus;
			}
			set
			{
				responseStatus = value;
			}
		}


		/// <summary>
		/// Getter method for property <tt>cause</tt>.
		/// </summary>
		/// <returns> property value of cause </returns>
		public virtual Exception Cause
		{
			get
			{
				return cause;
			}
			set
			{
				cause = value;
			}
		}
	}
}