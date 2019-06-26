using Remoting.Config;
using Remoting.Config.configs;
using Remoting.Config.switches;
using Remoting.rpc.protocol;
using System.Net;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Remoting
{
    public abstract class AbstractBoltClient : AbstractLifeCycle, BoltClient, ConfigurableInstance
    {
        public abstract ConnectionMonitorStrategy MonitorStrategy { set; }
        public abstract RemotingAddressParser AddressParser { set; get; }
        public abstract DefaultConnectionManager ConnectionManager { get; }
        public abstract bool ConnectionMonitorSwitchOn { get; }
        public abstract void disableConnectionMonitorSwitch();
        public abstract void enableConnectionMonitorSwitch();
        public abstract bool ReconnectSwitchOn { get; }
        public abstract void disableReconnectSwith();
        public abstract void enableReconnectSwitch();
        public abstract void disableConnHeartbeat(Url url);
        public abstract void disableConnHeartbeat(string address);
        public abstract void enableConnHeartbeat(Url url);
        public abstract void enableConnHeartbeat(string address);
        public abstract void closeConnection(Url url);
        public abstract void closeConnection(string address);
        public abstract bool checkConnection(string address);
        public abstract ConcurrentDictionary<string, List<Connection>> AllManagedConnections { get; }
        public abstract Connection getConnection(Url url, int connectTimeout);
        public abstract Connection getConnection(string addr, int connectTimeout);
        public abstract void closeStandaloneConnection(Connection conn);
        public abstract Connection createStandaloneConnection(string address, int connectTimeout);
        public abstract Connection createStandaloneConnection(IPAddress ip, int port, int connectTimeout);
        public abstract void registerUserProcessor(UserProcessor processor);
        public abstract void addConnectionEventProcessor(ConnectionEventType type, ConnectionEventProcessor processor);
        public abstract void invokeWithCallback(Connection conn, object request, InvokeContext invokeContext, InvokeCallback invokeCallback, int timeoutMillis);
        public abstract void invokeWithCallback(Connection conn, object request, InvokeCallback invokeCallback, int timeoutMillis);
        public abstract void invokeWithCallback(Url url, object request, InvokeContext invokeContext, InvokeCallback invokeCallback, int timeoutMillis);
        public abstract void invokeWithCallback(Url url, object request, InvokeCallback invokeCallback, int timeoutMillis);
        public abstract void invokeWithCallback(string addr, object request, InvokeContext invokeContext, InvokeCallback invokeCallback, int timeoutMillis);
        public abstract void invokeWithCallback(string addr, object request, InvokeCallback invokeCallback, int timeoutMillis);
        public abstract rpc.RpcResponseFuture invokeWithFuture(Connection conn, object request, InvokeContext invokeContext, int timeoutMillis);
        public abstract rpc.RpcResponseFuture invokeWithFuture(Connection conn, object request, int timeoutMillis);
        public abstract rpc.RpcResponseFuture invokeWithFuture(Url url, object request, InvokeContext invokeContext, int timeoutMillis);
        public abstract rpc.RpcResponseFuture invokeWithFuture(Url url, object request, int timeoutMillis);
        public abstract rpc.RpcResponseFuture invokeWithFuture(string address, object request, InvokeContext invokeContext, int timeoutMillis);
        public abstract rpc.RpcResponseFuture invokeWithFuture(string address, object request, int timeoutMillis);
        public abstract object invokeSync(Connection conn, object request, InvokeContext invokeContext, int timeoutMillis);
        public abstract object invokeSync(Connection conn, object request, int timeoutMillis);
        public abstract object invokeSync(Url url, object request, InvokeContext invokeContext, int timeoutMillis);
        public abstract object invokeSync(Url url, object request, int timeoutMillis);
        public abstract object invokeSync(string address, object request, InvokeContext invokeContext, int timeoutMillis);
        public abstract object invokeSync(string address, object request, int timeoutMillis);
        public abstract void oneway(Connection conn, object request, InvokeContext invokeContext);
        public abstract void oneway(Connection conn, object request);
        public abstract void oneway(Url url, object request, InvokeContext invokeContext);
        public abstract void oneway(Url url, object request);
        public abstract void oneway(string address, object request, InvokeContext invokeContext);
        public abstract void oneway(string address, object request);

        private readonly BoltOptions options;
        private readonly ConfigType configType;
        private readonly GlobalSwitch globalSwitch;
        private readonly ConfigContainer configContainer;

        public AbstractBoltClient()
        {
            options = new BoltOptions();
            configType = ConfigType.CLIENT_SIDE;
            globalSwitch = new GlobalSwitch();
            configContainer = new DefaultConfigContainer();
        }

        public virtual object option(BoltOption option)
        {
            return options.option(option);
        }

        public virtual Configurable option(BoltOption option, object value)
        {
            options.option(option, value);
            return this;
        }

        public virtual ConfigContainer conf()
        {
            return configContainer;
        }

        public virtual GlobalSwitch switches()
        {
            return globalSwitch;
        }

        public virtual void initWriteBufferWaterMark(int low, int high)
        {
            configContainer.set(configType, ConfigItem.NETTY_BUFFER_LOW_WATER_MARK, low);
            configContainer.set(configType, ConfigItem.NETTY_BUFFER_HIGH_WATER_MARK, high);
        }

        public virtual int netty_buffer_low_watermark()
        {
            var config = configContainer.get(configType, ConfigItem.NETTY_BUFFER_LOW_WATER_MARK);
            if (config != null)
            {
                return (int)config;
            }
            else
            {
                return ConfigManager.netty_buffer_low_watermark();
            }
        }

        public virtual int netty_buffer_high_watermark()
        {
            var config = configContainer.get(configType, ConfigItem.NETTY_BUFFER_HIGH_WATER_MARK);
            if (config != null)
            {
                return (int)config;
            }
            else
            {
                return ConfigManager.netty_buffer_high_watermark();
            }
        }
    }

}