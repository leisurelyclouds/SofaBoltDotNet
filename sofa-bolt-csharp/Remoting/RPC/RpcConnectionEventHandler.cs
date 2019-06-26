using DotNetty.Transport.Channels;
using Remoting.Config.switches;

namespace Remoting.rpc
{
	/// <summary>
	/// ConnectionEventHandler for Rpc.
	/// </summary>
	public class RpcConnectionEventHandler : ConnectionEventHandler
	{
		public RpcConnectionEventHandler() : base()
		{
		}

		public RpcConnectionEventHandler(GlobalSwitch globalSwitch) : base(globalSwitch)
		{
		}

		public override void ChannelInactive(IChannelHandlerContext ctx)
		{
			Connection conn = ctx.Channel.GetAttribute(Connection.CONNECTION).Get();
			if (conn != null)
			{
				ConnectionManager.remove(conn);
			}
			base.ChannelInactive(ctx);
		}
	}

}