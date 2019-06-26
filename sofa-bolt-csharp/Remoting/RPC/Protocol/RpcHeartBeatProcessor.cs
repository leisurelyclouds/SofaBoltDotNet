using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;

namespace Remoting.rpc.protocol
{
    /// <summary>
    /// Processor for heart beat.
    /// 
    /// @author tsui
    /// @version $Id: RpcHeartBeatProcessor.java, v 0.1 2018-03-29 11:02 tsui Exp $
    /// </summary>
    public class RpcHeartBeatProcessor : AbstractRemotingProcessor
    {
        private static readonly ILogger logger = NullLogger.Instance;

        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        //ORIGINAL LINE: @Override public void doProcess(final RemotingContext ctx, RemotingCommand msg)
        public override void doProcess(RemotingContext ctx, RemotingCommand msg)
        {
            if (msg is HeartbeatCommand)
            { // process the heartbeat
              //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
              //ORIGINAL LINE: final int id = msg.getId();
                int id = msg.Id;
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("Heartbeat received! Id=" + id + ", from " + ctx.ChannelContext.Channel.RemoteAddress.ToString());
                }
                HeartbeatAckCommand ack = new HeartbeatAckCommand();
                ack.Id = id;
                var writeFlushTask = ctx.writeAndFlush(ack);
                writeFlushTask.ContinueWith((task) =>
                {
                    if (task.IsCompletedSuccessfully)
                    {
                        if (logger.IsEnabled(LogLevel.Debug))
                        {
                            logger.LogDebug("Send heartbeat ack done! Id={}, to remoteAddr={}", id, ctx.ChannelContext.Channel.RemoteAddress.ToString());
                        }
                    }
                    else
                    {
                        logger.LogError("Send heartbeat ack failed! Id={}, to remoteAddr={}", id, ctx.ChannelContext.Channel.RemoteAddress.ToString());
                    }
                });
                //.addListener(new ChannelFutureListenerAnonymousInnerClass(this, ctx, id));
            }
            else if (msg is HeartbeatAckCommand)
            {
                Connection conn = (Connection)ctx.ChannelContext.Channel.GetAttribute(Connection.CONNECTION).Get();
                InvokeFuture future = conn.removeInvokeFuture(msg.Id);
                if (future != null)
                {
                    future.putResponse(msg);
                    future.cancelTimeout();
                    try
                    {
                        future.executeInvokeCallback();
                    }
                    catch (Exception e)
                    {
                        logger.LogError("Exception caught when executing heartbeat invoke callback. From {}", ctx.ChannelContext.Channel.RemoteAddress.ToString(), e);
                    }
                }
                else
                {
                    logger.LogWarning("Cannot find heartbeat InvokeFuture, maybe already timeout. Id={}, From {}", msg.Id, ctx.ChannelContext.Channel.RemoteAddress.ToString());
                }
            }
            else
            {
                //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
                throw new Exception("Cannot process command: " + msg.GetType().FullName);
            }
        }


    }
}