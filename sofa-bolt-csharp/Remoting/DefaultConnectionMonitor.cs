 ﻿using java.lang;
 using java.util.concurrent;
 using Microsoft.Extensions.Logging;
 using Microsoft.Extensions.Logging.Abstractions;
 using Remoting.Config;
 using System;
 
 namespace Remoting
{
    /// <summary>
    ///  A default connection monitor that handle connections with strategies
    /// </summary>
    public class DefaultConnectionMonitor : AbstractLifeCycle
    {
        private static readonly ILogger logger = NullLogger.Instance;

        private readonly DefaultConnectionManager connectionManager;
        private readonly ConnectionMonitorStrategy strategy;

        private ScheduledThreadPoolExecutor executor;

        public DefaultConnectionMonitor(ConnectionMonitorStrategy strategy, DefaultConnectionManager connectionManager)
        {
            this.strategy = strategy ?? throw new ArgumentException("null strategy");
            this.connectionManager = connectionManager ?? throw new ArgumentException("null connectionManager");
        }

        public override void startup()
        {
            base.startup();

            /* initial delay to execute schedule task, unit: ms */
            long initialDelay = ConfigManager.conn_monitor_initial_delay();

            /* period of schedule task, unit: ms*/
            long period = ConfigManager.conn_monitor_period();

            executor = new ScheduledThreadPoolExecutor(1, new NamedThreadFactory("ConnectionMonitorThread", true), new ThreadPoolExecutor.AbortPolicy());
            executor.scheduleAtFixedRate(new TempRunnable(this), initialDelay, period, TimeUnit.MILLISECONDS);
        }

        public class TempRunnable : Runnable
        {
            private readonly DefaultConnectionMonitor defaultConnectionMonitor;

            public TempRunnable(DefaultConnectionMonitor defaultConnectionMonitor)
            {
                this.defaultConnectionMonitor = defaultConnectionMonitor;
            }

            public void run()
            {
                try
                {
                    var connPools = defaultConnectionMonitor.connectionManager.ConnPools;
                    defaultConnectionMonitor.strategy.monitor(connPools);
                }
                catch (System.Exception e)
                {
                    logger.LogWarning("MonitorTask error", e);
                }
            }
        }

        public override void shutdown()
        {
            base.shutdown();

            executor.purge();
            executor.shutdown();
        }
    }
}
