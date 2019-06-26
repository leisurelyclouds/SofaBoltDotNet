using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;

namespace Remoting.rpc
{
    /// <summary>
    /// Heart beat triggerd.
    /// </summary>
    public class HeartbeatHandler : ChannelDuplexHandler
    {
        public override bool IsSharable => true;
        public override void UserEventTriggered(IChannelHandlerContext ctx, object evt)
        {
            if (evt is IdleStateEvent)
            {
                ProtocolCode protocolCode = ctx.Channel.GetAttribute(Connection.PROTOCOL).Get();
                Protocol protocol = ProtocolManager.getProtocol(protocolCode);
                protocol.HeartbeatTrigger.heartbeatTriggered(ctx);
            }
            else
            {
                base.UserEventTriggered(ctx, evt);
            }
        }
    }
}