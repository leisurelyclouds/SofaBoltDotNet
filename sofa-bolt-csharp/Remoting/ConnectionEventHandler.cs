using DotNetty.Transport.Channels;
using java.lang;
using java.util.concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting.Config.switches;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Remoting
{
    /// <summary>
    /// Log the channel status event.
    /// </summary>
    public class ConnectionEventHandler : ChannelDuplexHandler
    {
        public override bool IsSharable => true;

        private static readonly ILogger logger = NullLogger.Instance;

        private ConnectionManager connectionManager;

        private ConnectionEventListener eventListener;

        private ConnectionEventExecutor eventExecutor;

        private Reconnector reconnectManager;

        private GlobalSwitch globalSwitch;

        public ConnectionEventHandler()
        {

        }

        public ConnectionEventHandler(GlobalSwitch globalSwitch)
        {
            this.globalSwitch = globalSwitch;
        }

        public override Task ConnectAsync(IChannelHandlerContext context, EndPoint remoteAddress, EndPoint localAddress)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                //ORIGINAL LINE: final String local = localAddress == null ? null : util.RemotingUtil.parseSocketAddressToString(localAddress);
                string local = localAddress == null ? null : localAddress.ToString();
                //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                //ORIGINAL LINE: final String remote = remoteAddress == null ? "UNKNOWN" : util.RemotingUtil.parseSocketAddressToString(remoteAddress);
                string remote = remoteAddress == null ? "UNKNOWN" : remoteAddress.ToString();
                if (ReferenceEquals(local, null))
                {
                    if (logger.IsEnabled(LogLevel.Information))
                    {
                        logger.LogInformation("Try connect to {}", remote);
                    }
                }
                else
                {
                    if (logger.IsEnabled(LogLevel.Information))
                    {
                        logger.LogInformation("Try connect from {} to {}", local, remote);
                    }
                }
            }
            return base.ConnectAsync(context, remoteAddress, localAddress);
        }

        public override Task DisconnectAsync(IChannelHandlerContext context)
        {
            infoLog("Connection disconnect to {}", ((IPEndPoint)context.Channel?.RemoteAddress)?.ToString());
            return base.DisconnectAsync(context);
        }


        /// <seealso cref= io.netty.channel.ChannelDuplexHandler#close(io.netty.channel.IChannelHandlerContext, io.netty.channel.ChannelPromise) </seealso>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void close(io.netty.channel.IChannelHandlerContext ctx, io.netty.channel.ChannelPromise promise) throws Exception
        public override Task CloseAsync(IChannelHandlerContext ctx)
        {
            infoLog("Connection closed: {}", ((IPEndPoint)ctx.Channel?.RemoteAddress)?.ToString());
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final Connection conn = ctx.Channel.attr(Connection.CONNECTION).get();
            Connection conn = ctx.Channel.GetAttribute(Connection.CONNECTION).Get();
            if (conn != null)
            {
                //conn.onClose();
            }
            return base.CloseAsync(ctx);
        }

        /// <seealso cref= io.netty.channel.ChannelHandlerAdapter#channelRegistered(io.netty.channel.IChannelHandlerContext) </seealso>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void channelRegistered(io.netty.channel.IChannelHandlerContext ctx) throws Exception
        public override void ChannelRegistered(IChannelHandlerContext ctx)
        {
            infoLog("Connection channel registered: {}", ((IPEndPoint)ctx.Channel?.RemoteAddress)?.ToString());
            base.ChannelRegistered(ctx);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void channelUnregistered(io.netty.channel.IChannelHandlerContext ctx) throws Exception
        public override void ChannelUnregistered(IChannelHandlerContext ctx)
        {
            infoLog("Connection channel unregistered: {}", ((IPEndPoint)ctx.Channel?.RemoteAddress)?.ToString());
            base.ChannelUnregistered(ctx);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void channelActive(io.netty.channel.IChannelHandlerContext ctx) throws Exception
        public override void ChannelActive(IChannelHandlerContext ctx)
        {
            infoLog("Connection channel active: {}", ((IPEndPoint)ctx.Channel?.RemoteAddress)?.ToString());
            base.ChannelActive(ctx);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void channelInactive(io.netty.channel.IChannelHandlerContext ctx) throws Exception
        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
            string remoteAddress = ((IPEndPoint)ctx.Channel?.RemoteAddress)?.ToString();
            infoLog("Connection channel inactive: {}", remoteAddress);
            base.ChannelInactive(ctx);
            var attr = ctx.Channel.GetAttribute(Connection.CONNECTION);
            if (null != attr)
            {
                // add reconnect task
                if (globalSwitch != null && globalSwitch.isOn(GlobalSwitch.CONN_RECONNECT_SWITCH))
                {
                    Connection conn = attr.Get();
                    if (reconnectManager != null)
                    {
                        reconnectManager.reconnect(conn.Url);
                    }
                }
                // trigger close connection event
                onEvent((Connection)attr.Get(), remoteAddress, ConnectionEventType.CLOSE);
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void userEventTriggered(io.netty.channel.IChannelHandlerContext ctx, Object event) throws Exception
        public override void UserEventTriggered(IChannelHandlerContext ctx, object @event)
        {
            if (@event is ConnectionEventType)
            {
                switch ((ConnectionEventType)@event)
                {
                    case ConnectionEventType.CONNECT:
                        IChannel channel = ctx.Channel;
                        if (null != channel)
                        {
                            Connection connection = channel.GetAttribute(Connection.CONNECTION).Get();
                            onEvent(connection, connection.Url.OriginUrl, ConnectionEventType.CONNECT);
                        }
                        else
                        {
                            //logger.LogWarning("channel null when handle user triggered event in ConnectionEventHandler!");
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                base.UserEventTriggered(ctx, @event);
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void exceptionCaught(io.netty.channel.IChannelHandlerContext ctx, Throwable cause) throws Exception
        public override void ExceptionCaught(IChannelHandlerContext ctx, System.Exception cause)
        {
            string remoteAddress = ((IPEndPoint)ctx.Channel?.RemoteAddress).ToString();
            string localAddress = ((IPEndPoint)ctx.Channel.LocalAddress).ToString();
            logger.LogWarning("ExceptionCaught in connection: local[{}], remote[{}], close the connection! Cause[{}:{}]", localAddress, remoteAddress, cause.GetType().Name, cause.Message);
            ctx.Channel.CloseAsync();
        }

        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        //ORIGINAL LINE: private void onEvent(final Connection conn, final String remoteAddress, final ConnectionEventType type)
        private void onEvent(Connection conn, string remoteAddress, ConnectionEventType type)
        {
            if (eventListener != null)
            {
                eventExecutor.onEvent(new TempRunnable(eventListener, type, remoteAddress, conn));
            }
        }

        public class TempRunnable : Runnable
        {
            ConnectionEventListener eventListener;
            private readonly ConnectionEventType type;
            private readonly string remoteAddress;
            private readonly Connection conn;

            public TempRunnable(ConnectionEventListener eventListener, ConnectionEventType type, string remoteAddress, Connection conn)
            {
                this.eventListener = eventListener;
                this.type = type;
                this.remoteAddress = remoteAddress;
                this.conn = conn;
            }
            public void run()
            {
                eventListener.onEvent(type, remoteAddress, conn);
            }
        }


        /// <summary>
        /// Getter method for property <tt>listener</tt>.
        /// </summary>
        /// <returns> property value of listener </returns>
        public virtual ConnectionEventListener ConnectionEventListener
        {
            get
            {
                return eventListener;
            }
            set
            {
                if (value != null)
                {
                    eventListener = value;
                    if (eventExecutor == null)
                    {
                        eventExecutor = new ConnectionEventExecutor(this);
                    }
                }
            }
        }


        /// <summary>
        /// Getter method for property <tt>connectionManager</tt>.
        /// </summary>
        /// <returns> property value of connectionManager </returns>
        public virtual ConnectionManager ConnectionManager
        {
            get
            {
                return connectionManager;
            }
            set
            {
                connectionManager = value;
            }
        }


        /// <summary>
        /// please use <seealso cref="ConnectionEventHandler#setReconnector(Reconnector)"/> instead </summary>
        /// <param name="reconnectManager"> value to be assigned to property reconnectManager </param>
        [Obsolete]
        public virtual ReconnectManager ReconnectManager
        {
            set
            {
                reconnectManager = value;
            }
        }

        public virtual Reconnector Reconnector
        {
            set
            {
                reconnectManager = value;
            }
        }

        /// <summary>
        /// Dispatch connection event.
        /// </summary>
        public class ConnectionEventExecutor
        {
            private readonly ConnectionEventHandler outerInstance;

            public ConnectionEventExecutor(ConnectionEventHandler outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            internal ExecutorService executor = new ThreadPoolExecutor(1, 1, 60L, TimeUnit.SECONDS, new LinkedBlockingQueue(10000), new NamedThreadFactory("Bolt-conn-event-executor", true));

            /// <summary>
            /// Process event.
            /// </summary>
            /// <param name="runnable"> Runnable </param>
            public virtual void onEvent(Runnable runnable)
            {
                try
                {
                    executor.execute(runnable);
                }
                catch (java.lang.Exception t)
                {
                    logger.LogError("Exception caught when execute connection event!", t);
                }
            }
        }

        private void infoLog(string format, string addr)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                if (string.IsNullOrEmpty(addr))
                {
                    logger.LogInformation(format, addr);
                }
                else
                {
                    logger.LogInformation(format, "UNKNOWN-ADDR");
                }
            }
        }
    }

}