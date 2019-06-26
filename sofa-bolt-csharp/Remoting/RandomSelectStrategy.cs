using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting.Config;
using Remoting.Config.switches;
using System;
using System.Collections.Generic;

namespace Remoting
{
    /// <summary>
    /// Select a connection randomly
    /// </summary>
    public class RandomSelectStrategy : ConnectionSelectStrategy
    {
        private static readonly ILogger logger = NullLogger.Instance;
        private const int MAX_TIMES = 5;
        private readonly java.util.Random random = new java.util.Random();
        private readonly GlobalSwitch globalSwitch;

        public RandomSelectStrategy(GlobalSwitch globalSwitch)
        {
            this.globalSwitch = globalSwitch;
        }

        public virtual Connection select(List<Connection> connections)
        {
            try
            {
                if (connections == null)
                {
                    return null;
                }
                int size = connections.Count;
                if (size == 0)
                {
                    return null;
                }

                Connection result;
                if (null != globalSwitch && globalSwitch.isOn(GlobalSwitch.CONN_MONITOR_SWITCH))
                {
                    var serviceStatusOnConnections = new List<Connection>();

                    foreach (var conn in connections)
                    {
                        string serviceStatus = (string)conn.getAttribute(Configs.CONN_SERVICE_STATUS);
                        if (!string.Equals(serviceStatus, Configs.CONN_SERVICE_STATUS_OFF))
                        {
                            (serviceStatusOnConnections).Add(conn);
                        }
                    }
                    if (serviceStatusOnConnections.Count == 0)
                    {
                        throw new Exception("No available connection when select in RandomSelectStrategy.");
                    }
                    result = randomGet(serviceStatusOnConnections);
                }
                else
                {
                    result = randomGet(connections);
                }
                return result;
            }
            catch (Exception e)
            {
                logger.LogError("Choose connection failed using RandomSelectStrategy!", e);
                return null;
            }
        }

        /// <summary>
        /// get one connection randomly
        /// </summary>
        /// <param name="connections"> source connections </param>
        /// <returns> result connection </returns>
        private Connection randomGet(List<Connection> connections)
        {
            if (null == connections || connections.Count == 0)
            {
                return null;
            }

            int size = connections.Count;
            int tries = 0;
            Connection result = null;
            while ((result == null || !result.Fine) && tries++ < MAX_TIMES)
            {
                result = (Connection)connections[(random.nextInt(size))];
            }

            if (result != null && !result.Fine)
            {
                result = null;
            }
            return result;
        }
    }
}