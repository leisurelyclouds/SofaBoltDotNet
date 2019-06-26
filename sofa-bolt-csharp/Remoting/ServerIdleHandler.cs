using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Net;

namespace Remoting
{
    /// <summary>
    /// Server Idle handler.
    /// 
    /// In the server side, the connection will be closed if it is idle for a certain period of time.
    /// </summary>
    public class ServerIdleHandler : ChannelDuplexHandler
    {
        public override bool IsSharable => true;

        private static readonly ILogger logger = NullLogger.Instance;

        /// <seealso cref= io.netty.channel.ChannelHandlerAdapter#userEventTriggered(io.netty.channel.IChannelHandlerContext, java.lang.Object) </seealso>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void userEventTriggered(final io.netty.channel.IChannelHandlerContext ctx, Object evt) throws Exception
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public override void UserEventTriggered(IChannelHandlerContext ctx, object evt)
        {
            if (evt is IdleStateEvent)
            {
                try
                {
                    logger.LogWarning($"Connection idle, close it from server side: {{{((IPEndPoint)ctx.Channel?.RemoteAddress)?.ToString()}}}");
                    ctx.CloseAsync();
                }
                catch (Exception e)
                {
                    logger.LogWarning($"Exception caught when closing connection in ServerIdleHandler.{e}");
                }
            }
            else
            {
                base.UserEventTriggered(ctx, evt);
            }
        }
    }
}