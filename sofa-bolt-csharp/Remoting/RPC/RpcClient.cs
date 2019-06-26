using System;
using System.Net;
using Remoting.Config.switches;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting.rpc.protocol;
using Remoting.Config;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Remoting.rpc
{
    /// <summary>
    /// Client for Rpc.
    /// </summary>
    public class RpcClient : AbstractBoltClient
    {
        private static readonly ILogger logger = NullLogger.Instance;

        private readonly RpcTaskScanner taskScanner;
        private readonly ConcurrentDictionary<Type, UserProcessor> userProcessors;
        private readonly ConnectionEventHandler connectionEventHandler;
        private readonly ConnectionEventListener connectionEventListener;

        private DefaultClientConnectionManager connectionManager;
        private Reconnector reconnectManager;
        private RemotingAddressParser addressParser;
        private DefaultConnectionMonitor connectionMonitor;
        private ConnectionMonitorStrategy monitorStrategy;

        // used in RpcClientAdapter (bolt-tr-adapter)
        protected internal RpcRemoting rpcRemoting;

        public RpcClient()
        {
            taskScanner = new RpcTaskScanner();
            userProcessors = new ConcurrentDictionary<Type, UserProcessor>();
            connectionEventHandler = new RpcConnectionEventHandler(switches());
            connectionEventListener = new ConnectionEventListener();
        }

        /// <summary>
        /// Shutdown.
        /// <para>
        /// Notice:<br>
        ///   <li>Rpc client can not be used any more after shutdown.
        ///   <li>If you need, you should destroy it, and instantiate another one.
        /// </para>
        /// </summary>
        public override void shutdown()
        {
            base.shutdown();

            connectionManager.shutdown();
            logger.LogWarning("Close all connections from client side!");
            taskScanner.shutdown();
            logger.LogWarning("Rpc client shutdown!");
            if (reconnectManager != null)
            {
                reconnectManager.shutdown();
            }
            if (connectionMonitor != null)
            {
                connectionMonitor.shutdown();
            }
        }

        public override void startup()
        {
            base.startup();

            if (addressParser == null)
            {
                addressParser = new RpcAddressParser();
            }

            ConnectionSelectStrategy connectionSelectStrategy = (ConnectionSelectStrategy)option(BoltGenericOption.CONNECTION_SELECT_STRATEGY);
            if (connectionSelectStrategy == null)
            {
                connectionSelectStrategy = new RandomSelectStrategy(switches());
            }
            connectionManager = new DefaultClientConnectionManager(connectionSelectStrategy, new RpcConnectionFactory(userProcessors, this), connectionEventHandler, connectionEventListener, switches());
            connectionManager.AddressParser = addressParser;
            connectionManager.startup();
            rpcRemoting = new RpcClientRemoting(new RpcCommandFactory(), addressParser, connectionManager);
            taskScanner.add(connectionManager);
            taskScanner.startup();

            if (switches().isOn(GlobalSwitch.CONN_MONITOR_SWITCH))
            {
                if (monitorStrategy == null)
                {
                    connectionMonitor = new DefaultConnectionMonitor(new ScheduledDisconnectStrategy(), connectionManager);
                }
                else
                {
                    connectionMonitor = new DefaultConnectionMonitor(monitorStrategy, connectionManager);
                }
                connectionMonitor.startup();
                logger.LogWarning("Switch on connection monitor");
            }
            if (switches().isOn(GlobalSwitch.CONN_RECONNECT_SWITCH))
            {
                reconnectManager = new ReconnectManager(connectionManager);
                reconnectManager.startup();

                connectionEventHandler.Reconnector = reconnectManager;
                logger.LogWarning("Switch on reconnect manager");
            }
        }

        public override void oneway(string address, object request)
        {
            rpcRemoting.oneway(address, request, null);
        }

        public override void oneway(string address, object request, InvokeContext invokeContext)
        {
            rpcRemoting.oneway(address, request, invokeContext);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void oneway(final Url url, final Object request) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public override void oneway(Url url, object request)
        {
            rpcRemoting.oneway(url, request, null);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void oneway(final Url url, final Object request, final InvokeContext invokeContext) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public override void oneway(Url url, object request, InvokeContext invokeContext)
        {
            rpcRemoting.oneway(url, request, invokeContext);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void oneway(final Connection conn, final Object request) throws exception.RemotingException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public override void oneway(Connection conn, object request)
        {
            rpcRemoting.oneway(conn, request, null);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void oneway(final Connection conn, final Object request, final InvokeContext invokeContext) throws exception.RemotingException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public override void oneway(Connection conn, object request, InvokeContext invokeContext)
        {
            rpcRemoting.oneway(conn, request, invokeContext);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public Object invokeSync(final String address, final Object request, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public override object invokeSync(string address, object request, int timeoutMillis)
        {
            return rpcRemoting.invokeSync(address, request, null, timeoutMillis);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public Object invokeSync(final String address, final Object request, final InvokeContext invokeContext, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public override object invokeSync(string address, object request, InvokeContext invokeContext, int timeoutMillis)
        {
            return rpcRemoting.invokeSync(address, request, invokeContext, timeoutMillis);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public Object invokeSync(final Url url, final Object request, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public override object invokeSync(Url url, object request, int timeoutMillis)
        {
            return invokeSync(url, request, null, timeoutMillis);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public Object invokeSync(final Url url, final Object request, final InvokeContext invokeContext, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public override object invokeSync(Url url, object request, InvokeContext invokeContext, int timeoutMillis)
        {
            return rpcRemoting.invokeSync(url, request, invokeContext, timeoutMillis);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public Object invokeSync(final Connection conn, final Object request, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public override object invokeSync(Connection conn, object request, int timeoutMillis)
        {
            return rpcRemoting.invokeSync(conn, request, null, timeoutMillis);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public Object invokeSync(final Connection conn, final Object request, final InvokeContext invokeContext, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public override object invokeSync(Connection conn, object request, InvokeContext invokeContext, int timeoutMillis)
        {
            return rpcRemoting.invokeSync(conn, request, invokeContext, timeoutMillis);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public RpcResponseFuture invokeWithFuture(final String address, final Object request, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public override RpcResponseFuture invokeWithFuture(string address, object request, int timeoutMillis)
        {
            return rpcRemoting.invokeWithFuture(address, request, null, timeoutMillis);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public RpcResponseFuture invokeWithFuture(final String address, final Object request, final InvokeContext invokeContext, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public override RpcResponseFuture invokeWithFuture(string address, object request, InvokeContext invokeContext, int timeoutMillis)
        {
            return rpcRemoting.invokeWithFuture(address, request, invokeContext, timeoutMillis);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public RpcResponseFuture invokeWithFuture(final Url url, final Object request, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public override RpcResponseFuture invokeWithFuture(Url url, object request, int timeoutMillis)
        {
            return rpcRemoting.invokeWithFuture(url, request, null, timeoutMillis);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public RpcResponseFuture invokeWithFuture(final Url url, final Object request, final InvokeContext invokeContext, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public override RpcResponseFuture invokeWithFuture(Url url, object request, InvokeContext invokeContext, int timeoutMillis)
        {
            return rpcRemoting.invokeWithFuture(url, request, invokeContext, timeoutMillis);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public RpcResponseFuture invokeWithFuture(final Connection conn, final Object request, int timeoutMillis) throws exception.RemotingException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public override RpcResponseFuture invokeWithFuture(Connection conn, object request, int timeoutMillis)
        {
            return rpcRemoting.invokeWithFuture(conn, request, null, timeoutMillis);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public RpcResponseFuture invokeWithFuture(final Connection conn, final Object request, final InvokeContext invokeContext, int timeoutMillis) throws exception.RemotingException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public override RpcResponseFuture invokeWithFuture(Connection conn, object request, InvokeContext invokeContext, int timeoutMillis)
        {
            return rpcRemoting.invokeWithFuture(conn, request, invokeContext, timeoutMillis);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void invokeWithCallback(final String address, final Object request, final InvokeCallback invokeCallback, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public override void invokeWithCallback(string address, object request, InvokeCallback invokeCallback, int timeoutMillis)
        {
            rpcRemoting.invokeWithCallback(address, request, null, invokeCallback, timeoutMillis);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void invokeWithCallback(final String address, final Object request, final InvokeContext invokeContext, final InvokeCallback invokeCallback, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public override void invokeWithCallback(string address, object request, InvokeContext invokeContext, InvokeCallback invokeCallback, int timeoutMillis)
        {
            rpcRemoting.invokeWithCallback(address, request, invokeContext, invokeCallback, timeoutMillis);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void invokeWithCallback(final Url url, final Object request, final InvokeCallback invokeCallback, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public override void invokeWithCallback(Url url, object request, InvokeCallback invokeCallback, int timeoutMillis)
        {
            rpcRemoting.invokeWithCallback(url, request, null, invokeCallback, timeoutMillis);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void invokeWithCallback(final Url url, final Object request, final InvokeContext invokeContext, final InvokeCallback invokeCallback, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public override void invokeWithCallback(Url url, object request, InvokeContext invokeContext, InvokeCallback invokeCallback, int timeoutMillis)
        {
            rpcRemoting.invokeWithCallback(url, request, invokeContext, invokeCallback, timeoutMillis);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void invokeWithCallback(final Connection conn, final Object request, final InvokeCallback invokeCallback, final int timeoutMillis) throws exception.RemotingException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public override void invokeWithCallback(Connection conn, object request, InvokeCallback invokeCallback, int timeoutMillis)
        {
            rpcRemoting.invokeWithCallback(conn, request, null, invokeCallback, timeoutMillis);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void invokeWithCallback(final Connection conn, final Object request, final InvokeContext invokeContext, final InvokeCallback invokeCallback, final int timeoutMillis) throws exception.RemotingException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public override void invokeWithCallback(Connection conn, object request, InvokeContext invokeContext, InvokeCallback invokeCallback, int timeoutMillis)
        {
            rpcRemoting.invokeWithCallback(conn, request, invokeContext, invokeCallback, timeoutMillis);
        }

        public override void addConnectionEventProcessor(ConnectionEventType type, ConnectionEventProcessor processor)
        {
            connectionEventListener.addConnectionEventProcessor(type, processor);
        }

        public override void registerUserProcessor(UserProcessor processor)
        {
            UserProcessorRegisterHelper.registerUserProcessor(processor, userProcessors);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public Connection createStandaloneConnection(String ip, int port, int connectTimeout) throws exception.RemotingException
        public override Connection createStandaloneConnection(IPAddress ip, int port, int connectTimeout)
        {
            return connectionManager.create(ip, port, connectTimeout);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public Connection createStandaloneConnection(String address, int connectTimeout) throws exception.RemotingException
        public override Connection createStandaloneConnection(string address, int connectTimeout)
        {
            return connectionManager.create(address, connectTimeout);
        }

        public override void closeStandaloneConnection(Connection conn)
        {
            if (null != conn)
            {
                conn.close();
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public Connection getConnection(String address, int connectTimeout) throws exception.RemotingException, ThreadInterruptedException
        public override Connection getConnection(string address, int connectTimeout)
        {
            Url url = addressParser.parse(address);
            return getConnection(url, connectTimeout);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public Connection getConnection(Url url, int connectTimeout) throws exception.RemotingException, ThreadInterruptedException
        public override Connection getConnection(Url url, int connectTimeout)
        {
            url.ConnectTimeout = connectTimeout;
            return connectionManager.getAndCreateIfAbsent(url);
        }

        public override ConcurrentDictionary<string, List<Connection>> AllManagedConnections
        {
            get
            {
                return connectionManager.All;
            }
        }

        public override bool checkConnection(string address)
        {
            Url url = addressParser.parse(address);
            Connection conn = connectionManager.get(url.UniqueKey);
            try
            {
                connectionManager.check(conn);
            }
            catch (Exception e)
            {
                logger.LogWarning("check failed. connection: {}", conn, e);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Close all connections of a address
        /// </summary>
        /// <param name="addr"> </param>
        public override void closeConnection(string addr)
        {
            Url url = addressParser.parse(addr);
            if (switches().isOn(GlobalSwitch.CONN_RECONNECT_SWITCH) && reconnectManager != null)
            {
                reconnectManager.disableReconnect(url);
            }
            connectionManager.remove(url.UniqueKey);
        }

        public override void closeConnection(Url url)
        {
            if (switches().isOn(GlobalSwitch.CONN_RECONNECT_SWITCH) && reconnectManager != null)
            {
                reconnectManager.disableReconnect(url);
            }
            connectionManager.remove(url.UniqueKey);
        }

        public override void enableConnHeartbeat(string address)
        {
            Url url = addressParser.parse(address);
            enableConnHeartbeat(url);
        }

        public override void enableConnHeartbeat(Url url)
        {
            if (null != url)
            {
                connectionManager.enableHeartbeat(connectionManager.get(url.UniqueKey));
            }
        }

        public override void disableConnHeartbeat(string address)
        {
            Url url = addressParser.parse(address);
            disableConnHeartbeat(url);
        }

        public override void disableConnHeartbeat(Url url)
        {
            if (null != url)
            {
                connectionManager.disableHeartbeat(connectionManager.get(url.UniqueKey));
            }
        }

        public override void enableReconnectSwitch()
        {
            switches().turnOn(GlobalSwitch.CONN_RECONNECT_SWITCH);
        }

        public override void disableReconnectSwith()
        {
            switches().turnOff(GlobalSwitch.CONN_RECONNECT_SWITCH);
        }

        public override bool ReconnectSwitchOn
        {
            get
            {
                return switches().isOn(GlobalSwitch.CONN_RECONNECT_SWITCH);
            }
        }

        public override void enableConnectionMonitorSwitch()
        {
            switches().turnOn(GlobalSwitch.CONN_MONITOR_SWITCH);
        }

        public override void disableConnectionMonitorSwitch()
        {
            switches().turnOff(GlobalSwitch.CONN_MONITOR_SWITCH);
        }

        public override bool ConnectionMonitorSwitchOn
        {
            get
            {
                return switches().isOn(GlobalSwitch.CONN_MONITOR_SWITCH);
            }
        }

        public override DefaultConnectionManager ConnectionManager
        {
            get
            {
                return connectionManager;
            }
        }

        public override RemotingAddressParser AddressParser
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


        public override ConnectionMonitorStrategy MonitorStrategy
        {
            set
            {
                monitorStrategy = value;
            }
        }
    }
}