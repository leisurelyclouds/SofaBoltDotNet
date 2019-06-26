using DotNetty.Buffers;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using System.Threading;
using java.util.concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting.Codecs;
using Remoting.Config;
using Remoting.Config.switches;
using Remoting.rpc.protocol;
using Remoting.util;
using System.Net;
using System.Threading.Tasks;
using System;
using System.Collections.Concurrent;

namespace Remoting.rpc
{
    /// <summary>
    /// Server for Rpc.
    /// 
    /// Usage:
    /// You can initialize RpcServer with one of the three constructors:
    ///   <seealso cref="#RpcServer(int)"/>, <seealso cref="#RpcServer(int, boolean)"/>, <seealso cref="#RpcServer(int, boolean, boolean)"/>
    /// Then call start() to start a rpc server, and call stop() to stop a rpc server.
    /// 
    /// Notice:
    ///   Once rpc server has been stopped, it can never be start again. You should init another instance of RpcServer to use.
    /// 
    /// @author jiangping
    /// @version $Id: RpcServer.java, v 0.1 2015-8-31 PM5:22:22 tao Exp $
    /// </summary>
    public class RpcServer : AbstractRemotingServer
    {

        /// <summary>
        /// logger
		/// </summary>
        private static readonly ILogger logger = NullLogger.Instance;
        /// <summary>
        /// server bootstrap
		/// </summary>
        private ServerBootstrap bootstrap;

        /// <summary>
        /// channelTask
        /// </summary>
        private Task<IChannel> channelTask;

        /// <summary>
        /// connection event handler
		/// </summary>
        private ConnectionEventHandler connectionEventHandler;

        /// <summary>
        /// connection event listener
		/// </summary>
        private ConnectionEventListener connectionEventListener = new ConnectionEventListener();

        /// <summary>
        /// user processors of rpc server
		/// </summary>
        //JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
        //ORIGINAL LINE: private java.util.concurrent.ConcurrentHashMap<String, rpc.protocol.UserProcessor<?>> userProcessors = new java.util.concurrent.ConcurrentHashMap<String, rpc.protocol.UserProcessor<?>>(4);
        private ConcurrentDictionary<Type, UserProcessor> userProcessors = new ConcurrentDictionary<Type, UserProcessor>();

        /// <summary>
        /// boss event loop group, boss group should not be daemon, need shutdown manually
		/// </summary>
        private readonly IEventLoopGroup bossGroup = NettyEventLoopUtil.newEventLoopGroup(1, group => new SingleThreadEventLoop(group, "Rpc-netty-server-boss"));
        /// <summary>
        /// worker event loop group. Reuse I/O worker threads between rpc servers. </summary>
        private static readonly IEventLoopGroup workerGroup = NettyEventLoopUtil.newEventLoopGroup(Environment.ProcessorCount * 2, group => new SingleThreadEventLoop(group, "Rpc-netty-server-worker"));

        /// <summary>
        /// address parser to get custom args
		/// </summary>
        private RemotingAddressParser addressParser;

        /// <summary>
        /// connection manager
		/// </summary>
        private DefaultServerConnectionManager connectionManager;

        /// <summary>
        /// rpc remoting
		/// </summary>
        protected internal RpcRemoting rpcRemoting;

        public bool IsUseLibuv { get; set; }

        /// <summary>
        /// rpc codec
		/// </summary>
        private Codec codec = new RpcCodec();

        static RpcServer()
        {
        }

        /// <summary>
        /// Construct a rpc server. <br>
        /// 
        /// Note:<br>
        /// You can only use invoke methods with params <seealso cref="Connection"/>, for example <seealso cref="#invokeSync(Connection, Object, int)"/> <br>
        /// Otherwise <seealso cref="UnsupportedOperationException"/> will be thrown.
        /// </summary>
        public RpcServer(int port) : this(port, false)
        {
        }

        /// <summary>
        /// Construct a rpc server. <br>
        /// 
        /// Note:<br>
        /// You can only use invoke methods with params <seealso cref="Connection"/>, for example <seealso cref="#invokeSync(Connection, Object, int)"/> <br>
        /// Otherwise <seealso cref="UnsupportedOperationException"/> will be thrown.
        /// </summary>
        public RpcServer(IPAddress ip, int port) : this(ip, port, false)
        {
        }

        /// <summary>
        /// Construct a rpc server. <br>
        /// 
        /// <ul>
		/// </summary>
        /// <li>You can enable connection management feature by specify <param name="manageConnection"> true.</li>
        /// <ul>
        /// <li>When connection management feature enabled, you can use all invoke methods with params <seealso cref="String"/>, <seealso cref="Url"/>, <seealso cref="Connection"/> methods.</li>
        /// <li>When connection management feature disabled, you can only use invoke methods with params <seealso cref="Connection"/>, otherwise <seealso cref="UnsupportedOperationException"/> will be thrown.</li>
        /// </ul>
        /// </ul>
        /// </param>
        /// <param name="port"> listened port </param>
        /// <param name="manageConnection"> true to enable connection management feature </param>
        public RpcServer(int port, bool manageConnection) : base(port)
        {
            /* server connection management feature enabled or not, default value false, means disabled. */
            if (manageConnection)
            {
                switches().turnOn(GlobalSwitch.SERVER_MANAGE_CONNECTION_SWITCH);
            }
        }

        /// <summary>
        /// Construct a rpc server. <br>
        /// 
        /// <ul>
		/// </summary>
        /// <li>You can enable connection management feature by specify <param name="manageConnection"> true.</li>
        /// <ul>
        /// <li>When connection management feature enabled, you can use all invoke methods with params <seealso cref="String"/>, <seealso cref="Url"/>, <seealso cref="Connection"/> methods.</li>
        /// <li>When connection management feature disabled, you can only use invoke methods with params <seealso cref="Connection"/>, otherwise <seealso cref="UnsupportedOperationException"/> will be thrown.</li>
        /// </ul>
        /// </ul>
        /// </param>
        /// <param name="port"> listened port </param>
        /// <param name="manageConnection"> true to enable connection management feature </param>
        public RpcServer(IPAddress ip, int port, bool manageConnection) : base(ip, port)
        {
            /* server connection management feature enabled or not, default value false, means disabled. */
            if (manageConnection)
            {
                switches().turnOn(GlobalSwitch.SERVER_MANAGE_CONNECTION_SWITCH);
            }
        }

        /// <summary>
        /// Construct a rpc server. <br>
        /// </summary>
        /// You can construct a rpc server with synchronous or asynchronous stop strategy by {<param name="syncStop">}.
        /// </param>
        /// <param name="port"> listened port </param>
        /// <param name="manageConnection"> manage connection </param>
        /// <param name="syncStop"> true to enable stop in synchronous way </param>
        public RpcServer(int port, bool manageConnection, bool syncStop) : this(port, manageConnection)
        {
            if (syncStop)
            {
                switches().turnOn(GlobalSwitch.SERVER_SYNC_STOP);
            }
        }

        protected internal override void doInit()
        {
            if (addressParser == null)
            {
                addressParser = new RpcAddressParser();
            }
            if (switches().isOn(GlobalSwitch.SERVER_MANAGE_CONNECTION_SWITCH))
            {
                // in server side, do not care the connection service state, so use null instead of global switch
                ConnectionSelectStrategy connectionSelectStrategy = new RandomSelectStrategy(null);
                connectionManager = new DefaultServerConnectionManager(connectionSelectStrategy);
                connectionManager.startup();

                connectionEventHandler = new RpcConnectionEventHandler(switches());
                connectionEventHandler.ConnectionManager = connectionManager;
                connectionEventHandler.ConnectionEventListener = connectionEventListener;
            }
            else
            {
                connectionEventHandler = new ConnectionEventHandler(switches());
                connectionEventHandler.ConnectionEventListener = connectionEventListener;
            }
            initRpcRemoting();
            bootstrap = new ServerBootstrap();

            if (IsUseLibuv)
            {
                bootstrap.Channel<TcpServerChannel>();
            }
            else
            {
                bootstrap.Channel<TcpServerSocketChannel>();
            }
            bootstrap.Group(bossGroup, workerGroup)
                .Option(ChannelOption.SoBacklog, ConfigManager.tcp_so_backlog())
                .Option(ChannelOption.SoReuseaddr, ConfigManager.tcp_so_reuseaddr())
                .ChildOption(ChannelOption.TcpNodelay, ConfigManager.tcp_nodelay())
                .ChildOption(ChannelOption.SoKeepalive, ConfigManager.tcp_so_keepalive());

            // set write buffer water mark
            initWriteBufferWaterMark();

            // init byte buf allocator
            if (ConfigManager.netty_buffer_pooled())
            {
                bootstrap
                    .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                    .ChildOption(ChannelOption.Allocator, PooledByteBufferAllocator.Default);
            }
            else
            {
                bootstrap
                    .Option(ChannelOption.Allocator, UnpooledByteBufferAllocator.Default)
                    .ChildOption(ChannelOption.Allocator, PooledByteBufferAllocator.Default);
            }

            // enable trigger mode for epoll if need
            NettyEventLoopUtil.enableTriggeredMode(bootstrap);

            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final boolean idleSwitch = config.ConfigManager.tcp_idle_switch();
            bool idleSwitch = ConfigManager.tcp_idle_switch();
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final int idleTime = config.ConfigManager.tcp_server_idle();
            int idleTime = ConfigManager.tcp_server_idle();
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final io.netty.channel.ChannelHandler serverIdleHandler = new ServerIdleHandler();
            IChannelHandler serverIdleHandler = new ServerIdleHandler();
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final RpcHandler rpcHandler = new RpcHandler(true, this.userProcessors);
            RpcHandler rpcHandler = new RpcHandler(true, userProcessors);
            bootstrap.ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
            {
                IChannelPipeline pipeline = channel.Pipeline;
                pipeline.AddLast("decoder", codec.newDecoder());
                pipeline.AddLast("encoder", codec.newEncoder());
                if (idleSwitch)
                {
                    pipeline.AddLast("idleStateHandler", new IdleStateHandler(TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(idleTime)));
                    pipeline.AddLast("serverIdleHandler", serverIdleHandler);
                }
                pipeline.AddLast("connectionEventHandler", connectionEventHandler);
                pipeline.AddLast("handler", rpcHandler);

                Url url = addressParser.parse(((IPEndPoint)channel?.RemoteAddress)?.ToString());
                if (switches().isOn(GlobalSwitch.SERVER_MANAGE_CONNECTION_SWITCH))
                {
                    connectionManager.add(new Connection(channel, url), url.UniqueKey);
                }
                else
                {
                    new Connection(channel, url);
                }
                channel.Pipeline.FireUserEventTriggered(ConnectionEventType.CONNECT);
            }));
        }

        protected internal override bool doStart()
        {
            channelTask = bootstrap.BindAsync(new IPEndPoint(ip(), port()));
            channelTask.Wait();
            return channelTask.IsCompletedSuccessfully;
        }

        /// <summary>
        /// Notice: only <seealso cref="GlobalSwitch#SERVER_MANAGE_CONNECTION_SWITCH"/> switch on, will close all connections.
        /// </summary>
        /// <seealso cref= AbstractRemotingServer#doStop() </seealso>
        protected internal override bool doStop()
        {
            if (null != channelTask)
            {
                channelTask.Result.CloseAsync();
            }
            if (switches().isOn(GlobalSwitch.SERVER_SYNC_STOP))
            {
                bossGroup.ShutdownGracefullyAsync().Wait();
            }
            else
            {
                bossGroup.ShutdownGracefullyAsync();
            }
            if (switches().isOn(GlobalSwitch.SERVER_MANAGE_CONNECTION_SWITCH) && null != connectionManager)
            {
                connectionManager.shutdown();
                logger.LogWarning("Close all connections from server side!");
            }
            logger.LogWarning("Rpc Server stopped!");
            return true;
        }

        /// <summary>
        /// init rpc remoting
        /// </summary>
        protected internal virtual void initRpcRemoting()
        {
            rpcRemoting = new RpcServerRemoting(new RpcCommandFactory(), addressParser, connectionManager);
        }

        /// <seealso cref= RemotingServer#registerProcessor(byte, CommandCode, RemotingProcessor) </seealso>
        public override void registerProcessor(byte protocolCode, CommandCode cmd, RemotingProcessor processor)
        {
            ProtocolManager.getProtocol(ProtocolCode.fromBytes(protocolCode)).CommandHandler.registerProcessor(cmd, processor);
        }

        /// <seealso cref= RemotingServer#registerDefaultExecutor(byte, ExecutorService) </seealso>
        public override void registerDefaultExecutor(byte protocolCode, ExecutorService executor)
        {
            ProtocolManager.getProtocol(ProtocolCode.fromBytes(protocolCode)).CommandHandler.registerDefaultExecutor(executor);
        }

        /// <summary>
        /// Add processor to process connection event.
        /// </summary>
        /// <param name="type"> connection event type </param>
        /// <param name="processor"> connection event processor </param>
        public virtual void addConnectionEventProcessor(ConnectionEventType type, ConnectionEventProcessor processor)
        {
            connectionEventListener.addConnectionEventProcessor(type, processor);
        }

        /// <summary>
        /// Use UserProcessorRegisterHelper<seealso cref="UserProcessorRegisterHelper"/> to help register user processor for server side.
        /// </summary>
        /// <seealso cref= AbstractRemotingServer#registerUserProcessor(rpc.protocol.UserProcessor) </seealso>
        public override void registerUserProcessor(UserProcessor processor)
        {
            UserProcessorRegisterHelper.registerUserProcessor(processor, userProcessors);
        }

        /// <summary>
        /// One way invocation using a string address, address format example - 127.0.0.1:12200?key1=value1&key2=value2 <br>
        /// <para>
        /// Notice:<br>
        ///   <ol>
        ///   <li><b>DO NOT modify the request object concurrently when this method is called.</b></li>
        ///   <li>When do invocation, use the string address to find a available client connection, if none then throw exception</li>
        ///   <li>Unlike rpc client, address arguments takes no effect here, for rpc server will not create connection.</li>
        ///   </ol>
        /// 
        /// </para>
        /// </summary>
        /// <param name="addr"> </param>
        /// <param name="request"> </param>
        /// <exception cref="RemotingException"> </exception>
        /// <exception cref="ThreadInterruptedException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void oneway(final String addr, final Object request) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual void oneway(string addr, object request)
        {
            check();
            rpcRemoting.oneway(addr, request, null);
        }

        /// <summary>
        /// One way invocation with a <seealso cref="InvokeContext"/>, common api notice please see <seealso cref="#oneway(String, Object)"/>
        /// </summary>
        /// <param name="addr"> </param>
        /// <param name="request"> </param>
        /// <param name="invokeContext"> </param>
        /// <exception cref="RemotingException"> </exception>
        /// <exception cref="ThreadInterruptedException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void oneway(final String addr, final Object request, final InvokeContext invokeContext) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual void oneway(string addr, object request, InvokeContext invokeContext)
        {
            check();
            rpcRemoting.oneway(addr, request, invokeContext);
        }

        /// <summary>
        /// One way invocation using a parsed <seealso cref="Url"/> <br>
        /// <para>
        /// Notice:<br>
        ///   <ol>
        ///   <li><b>DO NOT modify the request object concurrently when this method is called.</b></li>
        ///   <li>When do invocation, use the parsed <seealso cref="Url"/> to find a available client connection, if none then throw exception</li>
        ///   </ol>
        /// 
        /// </para>
        /// </summary>
        /// <param name="url"> </param>
        /// <param name="request"> </param>
        /// <exception cref="RemotingException"> </exception>
        /// <exception cref="ThreadInterruptedException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void oneway(final Url url, final Object request) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual void oneway(Url url, object request)
        {
            check();
            rpcRemoting.oneway(url, request, null);
        }

        /// <summary>
        /// One way invocation with a <seealso cref="InvokeContext"/>, common api notice please see <seealso cref="#oneway(Url, Object)"/>
        /// </summary>
        /// <param name="url"> </param>
        /// <param name="request"> </param>
        /// <param name="invokeContext"> </param>
        /// <exception cref="RemotingException"> </exception>
        /// <exception cref="ThreadInterruptedException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void oneway(final Url url, final Object request, final InvokeContext invokeContext) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual void oneway(Url url, object request, InvokeContext invokeContext)
        {
            check();
            rpcRemoting.oneway(url, request, invokeContext);
        }

        /// <summary>
        /// One way invocation using a <seealso cref="Connection"/> <br>
        /// <para>
        /// Notice:<br>
        ///   <b>DO NOT modify the request object concurrently when this method is called.</b>
        /// 
        /// </para>
        /// </summary>
        /// <param name="conn"> </param>
        /// <param name="request"> </param>
        /// <exception cref="RemotingException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void oneway(final Connection conn, final Object request) throws exception.RemotingException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual void oneway(Connection conn, object request)
        {
            rpcRemoting.oneway(conn, request, null);
        }

        /// <summary>
        /// One way invocation with a <seealso cref="InvokeContext"/>, common api notice please see <seealso cref="#oneway(Connection, Object)"/>
        /// </summary>
        /// <param name="conn"> </param>
        /// <param name="request"> </param>
        /// <param name="invokeContext"> </param>
        /// <exception cref="RemotingException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void oneway(final Connection conn, final Object request, final InvokeContext invokeContext) throws exception.RemotingException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual void oneway(Connection conn, object request, InvokeContext invokeContext)
        {
            rpcRemoting.oneway(conn, request, invokeContext);
        }

        /// <summary>
        /// Synchronous invocation using a string address, address format example - 127.0.0.1:12200?key1=value1&key2=value2 <br>
        /// <para>
        /// Notice:<br>
        ///   <ol>
        ///   <li><b>DO NOT modify the request object concurrently when this method is called.</b></li>
        ///   <li>When do invocation, use the string address to find a available client connection, if none then throw exception</li>
        ///   <li>Unlike rpc client, address arguments takes no effect here, for rpc server will not create connection.</li>
        ///   </ol>
        /// 
        /// </para>
        /// </summary>
        /// <param name="addr"> </param>
        /// <param name="request"> </param>
        /// <param name="timeoutMillis"> </param>
        /// <returns> Object </returns>
        /// <exception cref="RemotingException"> </exception>
        /// <exception cref="ThreadInterruptedException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public Object invokeSync(final String addr, final Object request, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual object invokeSync(string addr, object request, int timeoutMillis)
        {
            check();
            return rpcRemoting.invokeSync(addr, request, null, timeoutMillis);
        }

        /// <summary>
        /// Synchronous invocation with a <seealso cref="InvokeContext"/>, common api notice please see <seealso cref="#invokeSync(String, Object, int)"/>
        /// </summary>
        /// <param name="addr"> </param>
        /// <param name="request"> </param>
        /// <param name="invokeContext"> </param>
        /// <param name="timeoutMillis">
        /// @return </param>
        /// <exception cref="RemotingException"> </exception>
        /// <exception cref="ThreadInterruptedException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public Object invokeSync(final String addr, final Object request, final InvokeContext invokeContext, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual object invokeSync(string addr, object request, InvokeContext invokeContext, int timeoutMillis)
        {
            check();
            return rpcRemoting.invokeSync(addr, request, invokeContext, timeoutMillis);
        }

        /// <summary>
        /// Synchronous invocation using a parsed <seealso cref="Url"/> <br>
        /// <para>
        /// Notice:<br>
        ///   <ol>
        ///   <li><b>DO NOT modify the request object concurrently when this method is called.</b></li>
        ///   <li>When do invocation, use the parsed <seealso cref="Url"/> to find a available client connection, if none then throw exception</li>
        ///   </ol>
        /// 
        /// </para>
        /// </summary>
        /// <param name="url"> </param>
        /// <param name="request"> </param>
        /// <param name="timeoutMillis"> </param>
        /// <returns> Object </returns>
        /// <exception cref="RemotingException"> </exception>
        /// <exception cref="ThreadInterruptedException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public Object invokeSync(Url url, Object request, int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        public virtual object invokeSync(Url url, object request, int timeoutMillis)
        {
            check();
            return rpcRemoting.invokeSync(url, request, null, timeoutMillis);
        }

        /// <summary>
        /// Synchronous invocation with a <seealso cref="InvokeContext"/>, common api notice please see <seealso cref="#invokeSync(Url, Object, int)"/>
        /// </summary>
        /// <param name="url"> </param>
        /// <param name="request"> </param>
        /// <param name="invokeContext"> </param>
        /// <param name="timeoutMillis">
        /// @return </param>
        /// <exception cref="RemotingException"> </exception>
        /// <exception cref="ThreadInterruptedException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public Object invokeSync(final Url url, final Object request, final InvokeContext invokeContext, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual object invokeSync(Url url, object request, InvokeContext invokeContext, int timeoutMillis)
        {
            check();
            return rpcRemoting.invokeSync(url, request, invokeContext, timeoutMillis);
        }

        /// <summary>
        /// Synchronous invocation using a <seealso cref="Connection"/> <br>
        /// <para>
        /// Notice:<br>
        ///   <b>DO NOT modify the request object concurrently when this method is called.</b>
        /// 
        /// </para>
        /// </summary>
        /// <param name="conn"> </param>
        /// <param name="request"> </param>
        /// <param name="timeoutMillis"> </param>
        /// <returns> Object </returns>
        /// <exception cref="RemotingException"> </exception>
        /// <exception cref="ThreadInterruptedException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public Object invokeSync(final Connection conn, final Object request, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual object invokeSync(Connection conn, object request, int timeoutMillis)
        {
            return rpcRemoting.invokeSync(conn, request, null, timeoutMillis);
        }

        /// <summary>
        /// Synchronous invocation with a <seealso cref="InvokeContext"/>, common api notice please see <seealso cref="#invokeSync(Connection, Object, int)"/>
        /// </summary>
        /// <param name="conn"> </param>
        /// <param name="request"> </param>
        /// <param name="invokeContext"> </param>
        /// <param name="timeoutMillis">
        /// @return </param>
        /// <exception cref="RemotingException"> </exception>
        /// <exception cref="ThreadInterruptedException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public Object invokeSync(final Connection conn, final Object request, final InvokeContext invokeContext, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual object invokeSync(Connection conn, object request, InvokeContext invokeContext, int timeoutMillis)
        {
            return rpcRemoting.invokeSync(conn, request, invokeContext, timeoutMillis);
        }

        /// <summary>
        /// Future invocation using a string address, address format example - 127.0.0.1:12200?key1=value1&key2=value2 <br>
        /// You can get result use the returned <seealso cref="RpcResponseFuture"/>.
        /// <para>
        /// Notice:<br>
        ///   <ol>
        ///   <li><b>DO NOT modify the request object concurrently when this method is called.</b></li>
        ///   <li>When do invocation, use the string address to find a available client connection, if none then throw exception</li>
        ///   <li>Unlike rpc client, address arguments takes no effect here, for rpc server will not create connection.</li>
        ///   </ol>
        /// 
        /// </para>
        /// </summary>
        /// <param name="addr"> </param>
        /// <param name="request"> </param>
        /// <param name="timeoutMillis"> </param>
        /// <returns> RpcResponseFuture </returns>
        /// <exception cref="RemotingException"> </exception>
        /// <exception cref="ThreadInterruptedException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public RpcResponseFuture invokeWithFuture(final String addr, final Object request, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual RpcResponseFuture invokeWithFuture(string addr, object request, int timeoutMillis)
        {
            check();
            return rpcRemoting.invokeWithFuture(addr, request, null, timeoutMillis);
        }

        /// <summary>
        /// Future invocation with a <seealso cref="InvokeContext"/>, common api notice please see <seealso cref="#invokeWithFuture(String, Object, int)"/>
        /// </summary>
        /// <param name="addr"> </param>
        /// <param name="request"> </param>
        /// <param name="invokeContext"> </param>
        /// <param name="timeoutMillis">
        /// @return </param>
        /// <exception cref="RemotingException"> </exception>
        /// <exception cref="ThreadInterruptedException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public RpcResponseFuture invokeWithFuture(final String addr, final Object request, final InvokeContext invokeContext, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual RpcResponseFuture invokeWithFuture(string addr, object request, InvokeContext invokeContext, int timeoutMillis)
        {
            check();
            return rpcRemoting.invokeWithFuture(addr, request, invokeContext, timeoutMillis);
        }

        /// <summary>
        /// Future invocation using a parsed <seealso cref="Url"/> <br>
        /// You can get result use the returned <seealso cref="RpcResponseFuture"/>.
        /// <para>
        /// Notice:<br>
        ///   <ol>
        ///   <li><b>DO NOT modify the request object concurrently when this method is called.</b></li>
        ///   <li>When do invocation, use the parsed <seealso cref="Url"/> to find a available client connection, if none then throw exception</li>
        ///   </ol>
        /// 
        /// </para>
        /// </summary>
        /// <param name="url"> </param>
        /// <param name="request"> </param>
        /// <param name="timeoutMillis"> </param>
        /// <returns> RpcResponseFuture </returns>
        /// <exception cref="RemotingException"> </exception>
        /// <exception cref="ThreadInterruptedException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public RpcResponseFuture invokeWithFuture(final Url url, final Object request, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual RpcResponseFuture invokeWithFuture(Url url, object request, int timeoutMillis)
        {
            check();
            return rpcRemoting.invokeWithFuture(url, request, null, timeoutMillis);
        }

        /// <summary>
        /// Future invocation with a <seealso cref="InvokeContext"/>, common api notice please see <seealso cref="#invokeWithFuture(Url, Object, int)"/>
        /// </summary>
        /// <param name="url"> </param>
        /// <param name="request"> </param>
        /// <param name="invokeContext"> </param>
        /// <param name="timeoutMillis">
        /// @return </param>
        /// <exception cref="RemotingException"> </exception>
        /// <exception cref="ThreadInterruptedException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public RpcResponseFuture invokeWithFuture(final Url url, final Object request, final InvokeContext invokeContext, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual RpcResponseFuture invokeWithFuture(Url url, object request, InvokeContext invokeContext, int timeoutMillis)
        {
            check();
            return rpcRemoting.invokeWithFuture(url, request, invokeContext, timeoutMillis);
        }

        /// <summary>
        /// Future invocation using a <seealso cref="Connection"/> <br>
        /// You can get result use the returned <seealso cref="RpcResponseFuture"/>.
        /// <para>
        /// Notice:<br>
        ///   <b>DO NOT modify the request object concurrently when this method is called.</b>
        /// 
        /// </para>
        /// </summary>
        /// <param name="conn"> </param>
        /// <param name="request"> </param>
        /// <param name="timeoutMillis">
        /// @return </param>
        /// <exception cref="RemotingException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public RpcResponseFuture invokeWithFuture(final Connection conn, final Object request, final int timeoutMillis) throws exception.RemotingException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual RpcResponseFuture invokeWithFuture(Connection conn, object request, int timeoutMillis)
        {

            return rpcRemoting.invokeWithFuture(conn, request, null, timeoutMillis);
        }

        /// <summary>
        /// Future invocation with a <seealso cref="InvokeContext"/>, common api notice please see <seealso cref="#invokeWithFuture(Connection, Object, int)"/>
        /// </summary>
        /// <param name="conn"> </param>
        /// <param name="request"> </param>
        /// <param name="invokeContext"> </param>
        /// <param name="timeoutMillis">
        /// @return </param>
        /// <exception cref="RemotingException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public RpcResponseFuture invokeWithFuture(final Connection conn, final Object request, final InvokeContext invokeContext, final int timeoutMillis) throws exception.RemotingException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual RpcResponseFuture invokeWithFuture(Connection conn, object request, InvokeContext invokeContext, int timeoutMillis)
        {

            return rpcRemoting.invokeWithFuture(conn, request, invokeContext, timeoutMillis);
        }

        /// <summary>
        /// Callback invocation using a string address, address format example - 127.0.0.1:12200?key1=value1&key2=value2 <br>
        /// You can specify an implementation of <seealso cref="InvokeCallback"/> to get the result.
        /// <para>
        /// Notice:<br>
        ///   <ol>
        ///   <li><b>DO NOT modify the request object concurrently when this method is called.</b></li>
        ///   <li>When do invocation, use the string address to find a available client connection, if none then throw exception</li>
        ///   <li>Unlike rpc client, address arguments takes no effect here, for rpc server will not create connection.</li>
        ///   </ol>
        /// 
        /// </para>
        /// </summary>
        /// <param name="addr"> </param>
        /// <param name="request"> </param>
        /// <param name="invokeCallback"> </param>
        /// <param name="timeoutMillis"> </param>
        /// <exception cref="RemotingException"> </exception>
        /// <exception cref="ThreadInterruptedException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void invokeWithCallback(final String addr, final Object request, final InvokeCallback invokeCallback, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual void invokeWithCallback(string addr, object request, InvokeCallback invokeCallback, int timeoutMillis)
        {
            check();
            rpcRemoting.invokeWithCallback(addr, request, null, invokeCallback, timeoutMillis);
        }

        /// <summary>
        /// Callback invocation with a <seealso cref="InvokeContext"/>, common api notice please see <seealso cref="#invokeWithCallback(String, Object, InvokeCallback, int)"/>
        /// </summary>
        /// <param name="addr"> </param>
        /// <param name="request"> </param>
        /// <param name="invokeContext"> </param>
        /// <param name="invokeCallback"> </param>
        /// <param name="timeoutMillis"> </param>
        /// <exception cref="RemotingException"> </exception>
        /// <exception cref="ThreadInterruptedException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void invokeWithCallback(final String addr, final Object request, final InvokeContext invokeContext, final InvokeCallback invokeCallback, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual void invokeWithCallback(string addr, object request, InvokeContext invokeContext, InvokeCallback invokeCallback, int timeoutMillis)
        {
            check();
            rpcRemoting.invokeWithCallback(addr, request, invokeContext, invokeCallback, timeoutMillis);
        }

        /// <summary>
        /// Callback invocation using a parsed <seealso cref="Url"/> <br>
        /// You can specify an implementation of <seealso cref="InvokeCallback"/> to get the result.
        /// <para>
        /// Notice:<br>
        ///   <ol>
        ///   <li><b>DO NOT modify the request object concurrently when this method is called.</b></li>
        ///   <li>When do invocation, use the parsed <seealso cref="Url"/> to find a available client connection, if none then throw exception</li>
        ///   </ol>
        /// 
        /// </para>
        /// </summary>
        /// <param name="url"> </param>
        /// <param name="request"> </param>
        /// <param name="invokeCallback"> </param>
        /// <param name="timeoutMillis"> </param>
        /// <exception cref="RemotingException"> </exception>
        /// <exception cref="ThreadInterruptedException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void invokeWithCallback(final Url url, final Object request, final InvokeCallback invokeCallback, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual void invokeWithCallback(Url url, object request, InvokeCallback invokeCallback, int timeoutMillis)
        {
            check();
            rpcRemoting.invokeWithCallback(url, request, null, invokeCallback, timeoutMillis);
        }

        /// <summary>
        /// Callback invocation with a <seealso cref="InvokeContext"/>, common api notice please see <seealso cref="#invokeWithCallback(Url, Object, InvokeCallback, int)"/>
        /// </summary>
        /// <param name="url"> </param>
        /// <param name="request"> </param>
        /// <param name="invokeContext"> </param>
        /// <param name="invokeCallback"> </param>
        /// <param name="timeoutMillis"> </param>
        /// <exception cref="RemotingException"> </exception>
        /// <exception cref="ThreadInterruptedException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void invokeWithCallback(final Url url, final Object request, final InvokeContext invokeContext, final InvokeCallback invokeCallback, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual void invokeWithCallback(Url url, object request, InvokeContext invokeContext, InvokeCallback invokeCallback, int timeoutMillis)
        {
            check();
            rpcRemoting.invokeWithCallback(url, request, invokeContext, invokeCallback, timeoutMillis);
        }

        /// <summary>
        /// Callback invocation using a <seealso cref="Connection"/> <br>
        /// You can specify an implementation of <seealso cref="InvokeCallback"/> to get the result.
        /// <para>
        /// Notice:<br>
        ///   <b>DO NOT modify the request object concurrently when this method is called.</b>
        /// 
        /// </para>
        /// </summary>
        /// <param name="conn"> </param>
        /// <param name="request"> </param>
        /// <param name="invokeCallback"> </param>
        /// <param name="timeoutMillis"> </param>
        /// <exception cref="RemotingException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void invokeWithCallback(final Connection conn, final Object request, final InvokeCallback invokeCallback, final int timeoutMillis) throws exception.RemotingException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual void invokeWithCallback(Connection conn, object request, InvokeCallback invokeCallback, int timeoutMillis)
        {
            rpcRemoting.invokeWithCallback(conn, request, null, invokeCallback, timeoutMillis);
        }

        /// <summary>
        /// Callback invocation with a <seealso cref="InvokeContext"/>, common api notice please see <seealso cref="#invokeWithCallback(Connection, Object, InvokeCallback, int)"/>
        /// </summary>
        /// <param name="conn"> </param>
        /// <param name="request"> </param>
        /// <param name="invokeCallback"> </param>
        /// <param name="timeoutMillis"> </param>
        /// <exception cref="RemotingException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void invokeWithCallback(final Connection conn, final Object request, final InvokeContext invokeContext, final InvokeCallback invokeCallback, final int timeoutMillis) throws exception.RemotingException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual void invokeWithCallback(Connection conn, object request, InvokeContext invokeContext, InvokeCallback invokeCallback, int timeoutMillis)
        {
            rpcRemoting.invokeWithCallback(conn, request, invokeContext, invokeCallback, timeoutMillis);
        }

        /// <summary>
        /// check whether a client address connected
        /// </summary>
        /// <param name="remoteAddr">
        /// @return </param>
        public virtual bool isConnected(string remoteAddr)
        {
            Url url = rpcRemoting.addressParser.parse(remoteAddr);
            return isConnected(url);
        }

        /// <summary>
        /// check whether a <seealso cref="Url"/> connected
        /// </summary>
        /// <param name="url">
        /// @return </param>
        public virtual bool isConnected(Url url)
        {
            Connection conn = rpcRemoting.connectionManager.get(url.UniqueKey);
            if (null != conn)
            {
                return conn.Fine;
            }
            return false;
        }

        /// <summary>
        /// check whether connection manage feature enabled
        /// </summary>
        private void check()
        {
            if (!switches().isOn(GlobalSwitch.SERVER_MANAGE_CONNECTION_SWITCH))
            {
                throw new System.NotSupportedException("Please enable connection manage feature of Rpc Server before call this method! See comments in constructor RpcServer(int port, boolean manageConnection) to find how to enable!");
            }
        }

        /// <summary>
        /// init netty write buffer water mark
        /// </summary>
        private void initWriteBufferWaterMark()
        {
            int lowWaterMark = netty_buffer_low_watermark();
            int highWaterMark = netty_buffer_high_watermark();
            if (lowWaterMark > highWaterMark)
            {
                throw new System.ArgumentException(string.Format("[server side] bolt netty high water mark {{{0}}} should not be smaller than low water mark {{{1}}} bytes)", highWaterMark, lowWaterMark));
            }
            else
            {
                logger.LogWarning("[server side] bolt netty low water mark is {} bytes, high water mark is {} bytes", lowWaterMark, highWaterMark);
            }
            bootstrap.ChildOption(ChannelOption.WriteBufferLowWaterMark, lowWaterMark);
            bootstrap.ChildOption(ChannelOption.WriteBufferHighWaterMark, highWaterMark);
        }

        // ~~~ getter and setter

        /// <summary>
        /// Getter method for property <tt>addressParser</tt>.
        /// </summary>
        /// <returns> property value of addressParser </returns>
        public virtual RemotingAddressParser AddressParser
        {
            get
            {
                return addressParser;
            }
            set
            {
                addressParser = value;
            }
        }


        /// <summary>
        /// Getter method for property <tt>connectionManager</tt>.
        /// </summary>
        /// <returns> property value of connectionManager </returns>
        public virtual DefaultConnectionManager ConnectionManager
        {
            get
            {
                return connectionManager;
            }
        }
    }

}