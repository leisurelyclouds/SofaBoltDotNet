using DotNetty.Transport.Channels;

namespace Remoting
{
	/// <summary>
	/// Heartbeat triggers here.
	/// </summary>
	public interface HeartbeatTrigger
	{
		void heartbeatTriggered(IChannelHandlerContext ctx);
	}
}