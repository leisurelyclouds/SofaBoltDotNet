using DotNetty.Buffers;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting.Codecs;
using Remoting.Config;
using Remoting.rpc.protocol;
using Remoting.util;
using System;
using System.Net;

namespace Remoting.Connections
{
    /// <summary>
    /// ConnectionFactory to create connection.
    /// </summary>
    public abstract class AbstractConnectionFactory : ConnectionFactory
    {
        private static readonly ILogger logger = NullLogger.Instance;
        private static readonly IEventLoopGroup workerGroup = NettyEventLoopUtil.newEventLoopGroup(Environment.ProcessorCount + 1, group => new SingleThreadEventLoop(group, "bolt-netty-client-worker"));
        private readonly ConfigurableInstance confInstance;
        private readonly Codec codec;
        private readonly IChannelHandler heartbeatHandler;
        private readonly IChannelHandler handler;
        protected internal Bootstrap bootstrap;

        public AbstractConnectionFactory(Codec codec, IChannelHandler heartbeatHandler, IChannelHandler handler, ConfigurableInstance confInstance)
        {
            this.confInstance = confInstance;
            this.codec = codec ?? throw new ArgumentException("null codec");
            this.heartbeatHandler = heartbeatHandler;
            this.handler = handler ?? throw new ArgumentException("null handler");
        }

        public virtual void init(ConnectionEventHandler connectionEventHandler)
        {
            bootstrap = new Bootstrap();

            bootstrap
                .Group(workerGroup)
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, ConfigManager.tcp_nodelay())
                .Option(ChannelOption.SoReuseaddr, ConfigManager.tcp_so_reuseaddr())
                .Option(ChannelOption.SoKeepalive, ConfigManager.tcp_so_keepalive());

            // init netty write buffer water mark
            initWriteBufferWaterMark();

            // init byte buf allocator
            if (ConfigManager.netty_buffer_pooled())
            {
                bootstrap.Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default);
            }
            else
            {
                bootstrap.Option(ChannelOption.Allocator, UnpooledByteBufferAllocator.Default);
            }

            bootstrap.Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
            {
                IChannelPipeline pipeline = channel.Pipeline;
                pipeline.AddLast("decoder", codec.newDecoder());
                pipeline.AddLast("encoder", codec.newEncoder());

                bool idleSwitch = ConfigManager.tcp_idle_switch();
                if (idleSwitch)
                {
                    pipeline.AddLast("idleStateHandler", new IdleStateHandler(TimeSpan.FromMilliseconds(ConfigManager.tcp_idle()), TimeSpan.FromMilliseconds(ConfigManager.tcp_idle()) , TimeSpan.FromMilliseconds(0)));
                    pipeline.AddLast("heartbeatHandler", heartbeatHandler);
                }

                pipeline.AddLast("connectionEventHandler", connectionEventHandler);
                pipeline.AddLast("handler", handler);
            }));
        }


        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public Connection createConnection(Url url) throws Exception
        public virtual Connection createConnection(Url url)
        {
            IChannel channel = doCreateConnection(url.Ip, url.Port, url.ConnectTimeout);
            Connection conn = new Connection(channel, ProtocolCode.fromBytes(url.Protocol), url.Version, url);
            channel.Pipeline.FireUserEventTriggered(ConnectionEventType.CONNECT);
            return conn;
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public Connection createConnection(String targetIP, int targetPort, int connectTimeout) throws Exception
        public virtual Connection createConnection(IPAddress targetIP, int targetPort, int connectTimeout)
        {
            IChannel channel = doCreateConnection(targetIP, targetPort, connectTimeout);
            Connection conn = new Connection(channel, ProtocolCode.fromBytes(RpcProtocol.PROTOCOL_CODE), RpcProtocolV2.PROTOCOL_VERSION_1, new Url(targetIP, targetPort));
            channel.Pipeline.FireUserEventTriggered(ConnectionEventType.CONNECT);
            return conn;
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public Connection createConnection(String targetIP, int targetPort, byte version, int connectTimeout) throws Exception
        public virtual Connection createConnection(IPAddress targetIP, int targetPort, byte version, int connectTimeout)
        {
            IChannel channel = doCreateConnection(targetIP, targetPort, connectTimeout);
            Connection conn = new Connection(channel, ProtocolCode.fromBytes(RpcProtocolV2.PROTOCOL_CODE), version, new Url(targetIP, targetPort));
            channel.Pipeline.FireUserEventTriggered(ConnectionEventType.CONNECT);
            return conn;
        }

        /// <summary>
        /// init netty write buffer water mark
        /// </summary>
        private void initWriteBufferWaterMark()
        {
            int lowWaterMark = confInstance.netty_buffer_low_watermark();
            int highWaterMark = confInstance.netty_buffer_high_watermark();
            if (lowWaterMark > highWaterMark)
            {
                throw new ArgumentException(string.Format("[client side] bolt netty high water mark {{{0}}} should not be smaller than low water mark {{{1}}} bytes)", highWaterMark, lowWaterMark));
            }
            else
            {
                logger.LogWarning("[client side] bolt netty low water mark is {} bytes, high water mark is {} bytes", lowWaterMark, highWaterMark);
            }
            bootstrap.Option(ChannelOption.WriteBufferLowWaterMark, lowWaterMark);
            bootstrap.Option(ChannelOption.WriteBufferHighWaterMark, highWaterMark);
        }

        protected internal virtual IChannel doCreateConnection(IPAddress targetIP, int targetPort, int connectTimeout)
        {
            // prevent unreasonable value, at least 1000
            connectTimeout = Math.Max(connectTimeout, 1000);
            string address = targetIP + ":" + targetPort;
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("connectTimeout of address [{}] is [{}].", address, connectTimeout);
            }
            bootstrap.Option(ChannelOption.ConnectTimeout, TimeSpan.FromMilliseconds(connectTimeout));
            var future = bootstrap.ConnectAsync(new IPEndPoint(targetIP, targetPort));
            future.Wait(TimeSpan.FromSeconds(5));
            var channel = future.Result;
            if (!future.IsCompleted)
            {
                string errMsg = "Create connection to " + address + " timeout!";
                logger.LogWarning(errMsg);
                throw new java.lang.Exception(errMsg);
            }
            if (future.IsCanceled)
            {
                string errMsg = "Create connection to " + address + " cancelled by user!";
                logger.LogWarning(errMsg);
                throw new java.lang.Exception(errMsg);
            }
            if (!future.IsCompletedSuccessfully)
            {
                string errMsg = "Create connection to " + address + " error!";
                logger.LogWarning(errMsg);
                throw new java.lang.Exception(errMsg, future.Exception);
            }
            return channel;
        }
    }
}