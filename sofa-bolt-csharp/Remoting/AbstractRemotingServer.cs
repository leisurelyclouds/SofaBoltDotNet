using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting.Config;
using Remoting.Config.configs;
using Remoting.Config.switches;
using Remoting.rpc.protocol;
using System;
using System.Net;

namespace Remoting
{
    /// <summary>
    /// Server template for remoting.
    /// </summary>
    public abstract class AbstractRemotingServer : AbstractLifeCycle, RemotingServer, ConfigurableInstance
    {
        public abstract void registerUserProcessor(UserProcessor processor);
        public abstract void registerDefaultExecutor(byte protocolCode, java.util.concurrent.ExecutorService executor);
        public abstract void registerProcessor(byte protocolCode, CommandCode commandCode, RemotingProcessor processor);

        private static readonly ILogger logger = NullLogger.Instance;

        private IPAddress ip_Renamed;
        private int port_Renamed;

        private readonly BoltOptions options;
        private readonly ConfigType configType;
        private readonly GlobalSwitch globalSwitch;
        private readonly ConfigContainer configContainer;

        public AbstractRemotingServer(int port) : this(IPAddress.Any, port)
        {
        }

        public AbstractRemotingServer(IPAddress ip, int port)
        {
            ip_Renamed = ip;
            port_Renamed = port;

            options = new BoltOptions();
            configType = ConfigType.SERVER_SIDE;
            globalSwitch = new GlobalSwitch();
            configContainer = new DefaultConfigContainer();
        }

        public override void startup()
        {
            base.startup();

            try
            {
                doInit();

                logger.LogWarning("Prepare to start server on port {} ", port_Renamed);
                if (doStart())
                {
                    logger.LogWarning("Server started on port {}", port_Renamed);
                }
                else
                {
                    logger.LogWarning("Failed starting server on port {}", port_Renamed);
                    throw new LifeCycleException("Failed starting server on port: " + port_Renamed);
                }
            }
            catch (Exception t)
            {
                shutdown(); // do stop to ensure close resources created during doInit()
                throw new InvalidOperationException("ERROR: Failed to start the Server!", t);
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void shutdown() throws LifeCycleException
        public override void shutdown()
        {
            base.shutdown();
            if (!doStop())
            {
                throw new LifeCycleException("doStop fail");
            }
        }

        public virtual IPAddress ip()
        {
            return ip_Renamed;
        }

        public virtual int port()
        {
            return port_Renamed;
        }

        protected internal abstract void doInit();

        protected internal abstract bool doStart();

        protected internal abstract bool doStop();

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