using System;

namespace Remoting.rpc
{
    /// <summary>
    /// Command of request.
    /// </summary>
    [Serializable]
	public abstract class RequestCommand : RpcCommand
	{

		/// <summary>
		/// For serialization
		/// </summary>
		private const long serialVersionUID = -3457717009326601317L;
		/// <summary>
		/// timeout, -1 stands for no timeout
		/// </summary>
		private int timeout = -1;

		public RequestCommand() : base(RpcCommandType.REQUEST)
		{
		}

		public RequestCommand(CommandCode code) : base(RpcCommandType.REQUEST, code)
		{
		}

		public RequestCommand(byte type, CommandCode code) : base(type, code)
		{
		}

		public RequestCommand(byte version, byte type, CommandCode code) : base(version, type, code)
		{
		}

		/// <summary>
		/// Getter method for property <tt>timeout</tt>.
		/// </summary>
		/// <returns> property value of timeout </returns>
		public virtual int Timeout
		{
			get
			{
				return timeout;
			}
			set
			{
				timeout = value;
			}
		}
	}
}