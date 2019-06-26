using System;
using Remoting.util;

namespace Remoting.rpc
{
    /// <summary>
    /// Heart beat.
    /// </summary>
    [Serializable]
	public class HeartbeatCommand : RequestCommand
	{
		/// <summary>
		/// For serialization
		/// </summary>
		private const long serialVersionUID = 4949981019109517725L;

		/// <summary>
		/// Construction.
		/// </summary>
		public HeartbeatCommand() : base(CommonCommandCode.HEARTBEAT)
		{
			Id = IDGenerator.nextId();
		}
	}
}