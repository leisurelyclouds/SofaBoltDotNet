using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting.Config;
using System;
using System.Net;

namespace Remoting.rpc.protocol
{
    /// <summary>
    /// Handler for heart beat.
    /// </summary>
    public class RpcHeartbeatTrigger : HeartbeatTrigger
    {
        private static readonly ILogger logger = NullLogger.Instance;

        /// <summary>
        /// max trigger times
		/// </summary>
        public static readonly int? maxCount = ConfigManager.tcp_idle_maxtimes();

        private const long heartbeatTimeoutMillis = 1000;

        private CommandFactory commandFactory;

        public RpcHeartbeatTrigger(CommandFactory commandFactory)
        {
            this.commandFactory = commandFactory;
        }

        public virtual void heartbeatTriggered(IChannelHandlerContext ctx)
        {
            int? heartbeatTimes = (int?)ctx.Channel.GetAttribute(Connection.HEARTBEAT_COUNT).Get();
            Connection conn = ctx.Channel.GetAttribute(Connection.CONNECTION).Get();
            if (heartbeatTimes >= maxCount)
            {
                try
                {
                    conn.close();
                    logger.LogError("Heartbeat failed for {} times, close the connection from client side: {} ", heartbeatTimes, ((IPEndPoint)ctx.Channel?.RemoteAddress)?.ToString());
                }
                catch (System.Exception e)
                {
                    logger.LogWarning("Exception caught when closing connection in SharableHandler.", e);
                }
            }
            else
            {
                bool? heartbeatSwitch = (bool?)ctx.Channel.GetAttribute(Connection.HEARTBEAT_SWITCH).Get();
                if (!heartbeatSwitch.HasValue || !heartbeatSwitch.Value)
                {
                    return;
                }
                //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                //ORIGINAL LINE: final rpc.HeartbeatCommand heartbeat = new rpc.HeartbeatCommand();
                HeartbeatCommand heartbeat = new HeartbeatCommand();

                //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                //ORIGINAL LINE: final InvokeFuture future = new rpc.DefaultInvokeFuture(heartbeat.getId(), new InvokeCallbackListener()
                InvokeFuture future = new DefaultInvokeFuture(heartbeat.Id, new InvokeCallbackListenerAnonymousInnerClass(this, ctx, heartbeat), null, heartbeat.ProtocolCode.FirstByte, commandFactory);
                //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                //ORIGINAL LINE: final int heartbeatId = heartbeat.getId();
                int heartbeatId = heartbeat.Id;
                conn.addInvokeFuture(future);
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("Send heartbeat, successive count={}, Id={}, to remoteAddr={}", heartbeatTimes, heartbeatId, ctx.Channel.RemoteAddress.ToString());
                }
                var writeFlushTask = ctx.WriteAndFlushAsync(heartbeat);
                writeFlushTask.ContinueWith((task) =>
                {
                    if (task.IsCompletedSuccessfully)
                    {
                        if (logger.IsEnabled(LogLevel.Debug))
                        {
                            logger.LogDebug("Send heartbeat done! Id={}, to remoteAddr={}", heartbeatId, ctx.Channel.RemoteAddress.ToString());
                        }
                    }
                    else
                    {
                        logger.LogError("Send heartbeat failed! Id={}, to remoteAddr={}", heartbeatId, ctx.Channel.RemoteAddress.ToString());
                    }
                });
                //.addListener(new ChannelFutureListenerAnonymousInnerClass(this, ctx, future, heartbeatId));
                TimerHolder.Timer.NewTimeout(new TimerTaskAnonymousInnerClass(this, conn, future, heartbeatId), TimeSpan.FromMilliseconds(heartbeatTimeoutMillis));
            }

        }

        private class InvokeCallbackListenerAnonymousInnerClass : InvokeCallbackListener
        {
            private readonly RpcHeartbeatTrigger outerInstance;

            private IChannelHandlerContext ctx;
            private HeartbeatCommand heartbeat;

            public InvokeCallbackListenerAnonymousInnerClass(RpcHeartbeatTrigger outerInstance, IChannelHandlerContext ctx, HeartbeatCommand heartbeat)
            {
                this.outerInstance = outerInstance;
                this.ctx = ctx;
                this.heartbeat = heartbeat;
            }

            public virtual void onResponse(InvokeFuture future)
            {
                ResponseCommand response;
                try
                {
                    response = (ResponseCommand)future.waitResponse(0);
                }
                catch (ThreadInterruptedException e)
                {
                    logger.LogError("Heartbeat ack process error! Id={}, from remoteAddr={}", heartbeat.Id, ((IPEndPoint)ctx.Channel?.RemoteAddress)?.ToString(), e);
                    return;
                }
                if (response != null && response.ResponseStatus == ResponseStatus.SUCCESS)
                {
                    if (logger.IsEnabled(LogLevel.Debug))
                    {
                        logger.LogDebug("Heartbeat ack received! Id={}, from remoteAddr={}", response.Id, ((IPEndPoint)ctx.Channel?.RemoteAddress)?.ToString());
                    }
                    ctx.Channel.GetAttribute(Connection.HEARTBEAT_COUNT).Set(0);
                }
                else
                {
                    if (response != null && response.ResponseStatus == ResponseStatus.TIMEOUT)
                    {
                        logger.LogError("Heartbeat timeout! The address is {}", ((IPEndPoint)ctx.Channel?.RemoteAddress)?.ToString());
                    }
                    else
                    {
                        logger.LogError("Heartbeat exception caught! Error code={}, The address is {}", response?.ResponseStatus, ((IPEndPoint)ctx.Channel?.RemoteAddress)?.ToString());
                    }
                    int? times = (int?)ctx.Channel.GetAttribute(Connection.HEARTBEAT_COUNT).Get();
                    ctx.Channel.GetAttribute(Connection.HEARTBEAT_COUNT).Set(times + 1);
                }
            }

            public virtual string RemoteAddress
            {
                get
                {
                    return ctx.Channel.RemoteAddress.ToString();
                }
            }
        }


        private class TimerTaskAnonymousInnerClass : ITimerTask
        {
            private readonly RpcHeartbeatTrigger outerInstance;

            private Connection conn;
            private InvokeFuture future;
            private int heartbeatId;

            public TimerTaskAnonymousInnerClass(RpcHeartbeatTrigger outerInstance, Connection conn, InvokeFuture future, int heartbeatId)
            {
                this.outerInstance = outerInstance;
                this.conn = conn;
                this.future = future;
                this.heartbeatId = heartbeatId;
            }

            public void Run(ITimeout timeout)
            {
                InvokeFuture future = conn.removeInvokeFuture(heartbeatId);
                if (future != null)
                {
                    future.putResponse(outerInstance.commandFactory.createTimeoutResponse(conn.RemoteAddress));
                    future.tryAsyncExecuteInvokeCallbackAbnormally();
                }
            }
        }
    }

}