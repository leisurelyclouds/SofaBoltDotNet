using System;

namespace Remoting.rpc
{
    /// <summary>
    /// Heartbeat ack.
    /// </summary>
    [Serializable]
	public class HeartbeatAckCommand : ResponseCommand
	{
		/// <summary>
		/// For serialization
		/// </summary>
		private const long serialVersionUID = 2584912495844320855L;

		/// <summary>
		/// Constructor.
		/// </summary>
		public HeartbeatAckCommand() : base(CommonCommandCode.HEARTBEAT)
		{
			ResponseStatus = ResponseStatus.SUCCESS;
		}
	}

}