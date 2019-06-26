using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;

namespace Remoting
{
    /// <summary>
    /// Connection pool
    /// </summary>
    public class ConnectionPool : Scannable
    {
        private static readonly ILogger logger = NullLogger.Instance;
        private List<Connection> connections;
        private ConnectionSelectStrategy strategy;
        private volatile int lastAccessTimestamp;
        private volatile bool asyncCreationDone;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="strategy"> ConnectionSelectStrategy </param>
        public ConnectionPool(ConnectionSelectStrategy strategy)
        {
            this.strategy = strategy;
            connections = new List<Connection>();
            asyncCreationDone = true;
        }

        /// <summary>
        /// add a connection
        /// </summary>
        /// <param name="connection"> Connection </param>
        public virtual void add(Connection connection)
        {
            markAccess();
            if (null == connection)
            {
                return;
            }
            bool res = !connections.Contains(connection);
            if (res)
            {
                connections.Add(connection);
                connection.increaseRef();
            }
        }

        /// <summary>
        /// check weather a connection already added
        /// </summary>
        /// <param name="connection"> Connection </param>
        /// <returns> whether this pool contains the target connection </returns>
        public virtual bool contains(Connection connection)
        {
            return connections.Contains(connection);
        }

        /// <summary>
        /// removeAndTryClose a connection
        /// </summary>
        /// <param name="connection"> Connection </param>
        public virtual void removeAndTryClose(Connection connection)
        {
            if (null == connection)
            {
                return;
            }
            bool res = connections.Remove(connection);
            if (res)
            {
                connection.decreaseRef();
            }
            if (connection.noRef())
            {
                connection.close();
            }
        }

        /// <summary>
        /// remove all connections
        /// </summary>
        public virtual void removeAllAndTryClose()
        {
            var removeList = new List<Connection>();
            foreach (var connection in connections)
            {
                if (null != connection)
                {
                    removeList.Add(connection);
                }
            }
            foreach (var connection in removeList)
            {
                if (null == connection)
                {
                    continue;
                }
                bool res = connections.Remove(connection);
                if (res)
                {
                    connection.decreaseRef();
                }
                if (connection.noRef())
                {
                    connection.close();
                }
            }

            connections.Clear();
        }

        /// <summary>
        /// get a connection
        /// </summary>
        /// <returns> Connection </returns>
        public virtual Connection get()
        {
            markAccess();
            if (null != connections)
            {
                var snapshot = new List<Connection>(connections);
                if (snapshot.Count > 0)
                {
                    return strategy.select(snapshot);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// get all connections
        /// </summary>
        /// <returns> Connection List<object> </returns>
        public virtual List<Connection> All
        {
            get
            {
                markAccess();
                return new List<Connection>(connections);
            }
        }

        /// <summary>
        /// connection pool size
        /// </summary>
        /// <returns> pool size </returns>
        public virtual int size()
        {
            return connections.Count;
        }

        /// <summary>
        /// is connection pool empty
        /// </summary>
        /// <returns> true if this connection pool has no connection </returns>
        public virtual bool Empty
        {
            get
            {
                return connections.Count == 0;
            }
        }

        /// <summary>
        /// Getter method for property <tt>lastAccessTimestamp</tt>.
        /// </summary>
        /// <returns> property value of lastAccessTimestamp </returns>
        public virtual long LastAccessTimestamp
        {
            get
            {
                return lastAccessTimestamp;
            }
        }

        /// <summary>
        /// do mark the time stamp when access this pool
        /// </summary>
        private void markAccess()
        {
            lastAccessTimestamp = (int)new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// is async create connection done
		/// </summary>
        /// <returns> true if async create connection done </returns>
        public virtual bool AsyncCreationDone
        {
            get
            {
                return asyncCreationDone;
            }
        }

        /// <summary>
        /// do mark async create connection done
        /// </summary>
        public virtual void markAsyncCreationDone()
        {
            asyncCreationDone = true;
        }

        /// <summary>
        /// do mark async create connection start
        /// </summary>
        public virtual void markAsyncCreationStart()
        {
            asyncCreationDone = false;
        }

        public virtual void scan()
        {
            if (null != connections && !(connections.Count == 0))
            {
                var removeList = new List<Connection>();
                foreach (var connection in connections)
                {
                    if (!connection.Fine)
                    {
                        logger.LogWarning("Remove bad connection when scanning conns of ConnectionPool - {}:{}", connection.RemoteIP, connection.RemotePort);
                        connection.close();
                        if (null != connection)
                        {
                            removeList.Add(connection);
                        }
                    }
                }

                foreach (var connection in removeList)
                {
                    if (null == connection)
                    {
                        continue;
                    }
                    bool res = connections.Remove(connection);
                    if (res)
                    {
                        connection.decreaseRef();
                    }
                    if (connection.noRef())
                    {
                        connection.close();
                    }
                }
            }
        }
    }

}