using System;
using Remoting.Config;
using Remoting.util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Remoting
{
    /// <summary>
    /// An implemented strategy to monitor connections:
    ///   <lu>
    ///       <li>each time scheduled, filter connections with <seealso cref="Configs#CONN_SERVICE_STATUS_OFF"/> at first.</li>
    ///       <li>then close connections.</li>
    ///   </lu>
    /// </summary>
    public class ScheduledDisconnectStrategy : ConnectionMonitorStrategy
    {
        private static readonly ILogger logger = NullLogger.Instance;

        private readonly int connectionThreshold;
        private readonly java.util.Random random;

        public ScheduledDisconnectStrategy()
        {
            connectionThreshold = ConfigManager.conn_threshold();
            random = new java.util.Random();
        }

        /// <summary>
        /// This method only invoked in ScheduledDisconnectStrategy, so no need to be exposed.
        /// This method will be remove in next version, do not use this method.
        /// 
        /// The user cannot call ScheduledDisconnectStrategy#filter, so modifying the implementation of this method is safe.
        /// </summary>
        /// <param name="connections"> connections from a connection pool </param>
        [Obsolete]
        public virtual ConcurrentDictionary<string, List<Connection>> filter(List<Connection> connections)
        {
            var serviceOnConnections = new List<Connection>();
            var serviceOffConnections = new List<Connection>();
            var filteredConnections = new ConcurrentDictionary<string, List<Connection>>();

            foreach (var connection in connections)
            {
                if (isConnectionOn(connection))
                {
                    serviceOnConnections.Add(connection);
                }
                else
                {
                    serviceOffConnections.Add(connection);
                }
            }
            filteredConnections.AddOrUpdate(Configs.CONN_SERVICE_STATUS_ON, serviceOnConnections, (_, __) => serviceOnConnections);
            filteredConnections.AddOrUpdate(Configs.CONN_SERVICE_STATUS_OFF, serviceOffConnections, (_, __) => serviceOnConnections);
            return filteredConnections;
        }

        public virtual void monitor(ConcurrentDictionary<string, RunStateRecordedFutureTask> connPools)
        {
            try
            {
                if (connPools == null || connPools.Count == 0)
                {
                    return;
                }

                foreach (var entry in connPools)
                {
                    string poolKey = entry.Key;
                    ConnectionPool pool = (ConnectionPool)FutureTaskUtil.getFutureTaskResult(entry.Value, logger);

                    var serviceOnConnections = new List<Connection>();
                    var serviceOffConnections = new List<Connection>();


                    foreach (var connection in pool.All)
                    {
                        if (isConnectionOn(connection))
                        {
                            serviceOnConnections.Add(connection);
                        }
                        else
                        {
                            serviceOffConnections.Add(connection);
                        }
                    }

                    if (serviceOnConnections.Count > connectionThreshold)
                    {
                        Connection freshSelectConnect = (Connection)serviceOnConnections[random.nextInt(serviceOnConnections.Count)];
                        freshSelectConnect.setAttribute(Configs.CONN_SERVICE_STATUS, Configs.CONN_SERVICE_STATUS_OFF);
                        serviceOffConnections.Add(freshSelectConnect);
                    }
                    else
                    {
                        if (logger.IsEnabled(LogLevel.Information))
                        {
                            logger.LogInformation("serviceOnConnections({}) size[{}], CONNECTION_THRESHOLD[{}].", poolKey, serviceOnConnections.Count, connectionThreshold);
                        }
                    }

                    foreach (var offConn in serviceOffConnections)
                    {
                        if (offConn.InvokeFutureMapFinish)
                        {
                            if (offConn.Fine)
                            {
                                offConn.close();
                            }
                        }
                        else
                        {
                            if (logger.IsEnabled(LogLevel.Information))
                            {
                                logger.LogInformation("Address={} won't close at this schedule turn", offConn.Channel.RemoteAddress.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError("ScheduledDisconnectStrategy monitor error", e);
            }
        }

        private bool isConnectionOn(Connection connection)
        {
            string serviceStatus = (string)connection.getAttribute(Configs.CONN_SERVICE_STATUS);
            return serviceStatus == null || serviceStatus == Configs.CONN_SERVICE_STATUS_ON;
        }
    }
}