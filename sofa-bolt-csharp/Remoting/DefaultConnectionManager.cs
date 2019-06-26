using System;
using System.Net;
using System.Collections.Generic;
using Remoting.Config.switches;
using java.util.concurrent;
using Remoting.Config;
using Remoting.Connections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting.util;
using Remoting.exception;
using Remoting.Constant;
using java.lang;
using System.Threading;
using System.Collections.Concurrent;

namespace Remoting
{
    /// <summary>
    /// Abstract implementation of connection manager
    /// </summary>
    public class DefaultConnectionManager : AbstractLifeCycle, ConnectionManager, ConnectionHeartbeatManager, Scannable, LifeCycle
    {
        private static readonly ILogger logger = NullLogger.Instance;

        /// <summary>
        /// executor to create connections in async way
        /// </summary>
        private ThreadPoolExecutor asyncCreateConnectionExecutor;

        /// <summary>
        /// switch status
        /// </summary>
        private GlobalSwitch globalSwitch;

        /// <summary>
        /// connection pool initialize tasks
        /// </summary>
        protected internal ConcurrentDictionary<string, RunStateRecordedFutureTask> connTasks;

        /// <summary>
        /// heal connection tasks
        /// </summary>
        protected internal ConcurrentDictionary<string, FutureTask> healTasks;

        /// <summary>
        /// connection pool select strategy
        /// </summary>
        protected internal ConnectionSelectStrategy connectionSelectStrategy;

        /// <summary>
        /// address parser
        /// </summary>
        protected internal RemotingAddressParser addressParser;

        /// <summary>
        /// connection factory
        /// </summary>
        protected internal ConnectionFactory connectionFactory;

        /// <summary>
        /// connection event handler
        /// </summary>
        protected internal ConnectionEventHandler connectionEventHandler;

        /// <summary>
        /// connection event listener
        /// </summary>
        protected internal ConnectionEventListener connectionEventListener;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DefaultConnectionManager()
        {
            connTasks = new ConcurrentDictionary<string, RunStateRecordedFutureTask>();
            healTasks = new ConcurrentDictionary<string, FutureTask>();
            connectionSelectStrategy = new RandomSelectStrategy(globalSwitch);
        }

        /// <summary>
        /// Construct with parameters.
        /// </summary>
        /// <param name="connectionSelectStrategy"> connection selection strategy </param>
        public DefaultConnectionManager(ConnectionSelectStrategy connectionSelectStrategy) : this()
        {
            this.connectionSelectStrategy = connectionSelectStrategy;
        }

        /// <summary>
        /// Construct with parameters.
        /// </summary>
        /// <param name="connectionSelectStrategy"> connection selection strategy </param>
        /// <param name="connectionFactory"> connection factory </param>
        public DefaultConnectionManager(ConnectionSelectStrategy connectionSelectStrategy, ConnectionFactory connectionFactory) : this(connectionSelectStrategy)
        {
            this.connectionFactory = connectionFactory;
        }

        /// <summary>
        /// Construct with parameters.
		/// </summary>
        /// <param name="connectionFactory"> connection selection strategy </param>
        /// <param name="addressParser"> address parser </param>
        /// <param name="connectionEventHandler"> connection event handler </param>
        public DefaultConnectionManager(ConnectionFactory connectionFactory, RemotingAddressParser addressParser, ConnectionEventHandler connectionEventHandler) : this(new RandomSelectStrategy(null), connectionFactory)
        {
            this.addressParser = addressParser;
            this.connectionEventHandler = connectionEventHandler;
        }

        /// <summary>
        /// Construct with parameters.
        /// </summary>
        /// <param name="connectionSelectStrategy"> connection selection strategy </param>
        /// <param name="connectionFactory"> connection factory </param>
        /// <param name="connectionEventHandler"> connection event handler </param>
        /// <param name="connectionEventListener"> connection event listener </param>
        public DefaultConnectionManager(ConnectionSelectStrategy connectionSelectStrategy, ConnectionFactory connectionFactory, ConnectionEventHandler connectionEventHandler, ConnectionEventListener connectionEventListener) : this(connectionSelectStrategy, connectionFactory)
        {
            this.connectionEventHandler = connectionEventHandler;
            this.connectionEventListener = connectionEventListener;
        }

        /// <summary>
        /// Construct with parameters.
        /// </summary>
        /// <param name="connectionSelectStrategy"> connection selection strategy. </param>
        /// <param name="connectionFactory"> connection factory </param>
        /// <param name="connectionEventHandler"> connection event handler </param>
        /// <param name="connectionEventListener"> connection event listener </param>
        /// <param name="globalSwitch"> global switch </param>
        public DefaultConnectionManager(ConnectionSelectStrategy connectionSelectStrategy, ConnectionFactory connectionFactory, ConnectionEventHandler connectionEventHandler, ConnectionEventListener connectionEventListener, GlobalSwitch globalSwitch) : this(connectionSelectStrategy, connectionFactory, connectionEventHandler, connectionEventListener)
        {
            this.globalSwitch = globalSwitch;
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void startup() throws LifeCycleException
        public override void startup()
        {
            base.startup();

            long keepAliveTime = ConfigManager.conn_create_tp_keepalive();
            int queueSize = ConfigManager.conn_create_tp_queue_size();
            int minPoolSize = ConfigManager.conn_create_tp_min_size();
            int maxPoolSize = ConfigManager.conn_create_tp_max_size();
            asyncCreateConnectionExecutor = new ThreadPoolExecutor(minPoolSize, maxPoolSize, keepAliveTime, TimeUnit.SECONDS, new ArrayBlockingQueue(queueSize), new NamedThreadFactory("Bolt-conn-warmup-executor", true));
        }

        public override void shutdown()
        {
            base.shutdown();

            if (asyncCreateConnectionExecutor != null)
            {
                asyncCreateConnectionExecutor.shutdown();
            }

            if (null == connTasks || connTasks.IsEmpty)
            {
                return;
            }

            var removeList = new List<string>();

            foreach (var poolKey in connTasks.Keys)
            {
                removeTask(poolKey);
                removeList.Add(poolKey);
            }

            removeList.ForEach((poolKey) => connTasks.TryRemove(poolKey, out _));

            logger.LogWarning("All connection pool and connections have been removed!");
        }

        [Obsolete]
        public void init()
        {
            connectionEventHandler.ConnectionManager = this;
            connectionEventHandler.ConnectionEventListener = connectionEventListener;
            connectionFactory.init(connectionEventHandler);
        }

        /// <seealso cref= ConnectionManager#add(Connection) </seealso>
        public virtual void add(Connection connection)
        {
            ISet<string> poolKeys = connection.PoolKeys;
            foreach (string poolKey in poolKeys)
            {
                add(connection, poolKey);
            }
        }

        /// <seealso cref= ConnectionManager#add(Connection, java.lang.String) </seealso>
        public virtual void add(Connection connection, string poolKey)
        {
            ConnectionPool pool = null;
            try
            {
                // get or create an empty connection pool
                pool = getConnectionPoolAndCreateIfAbsent(poolKey, new ConnectionPoolCall(this));
            }
            catch (System.Exception e)
            {
                // should not reach here.
                logger.LogError("[NOTIFYME] Exception occurred when getOrCreateIfAbsent an empty ConnectionPool!", e);
            }
            if (pool != null)
            {
                pool.add(connection);
            }
            else
            {
                // should not reach here.
                logger.LogError("[NOTIFYME] Connection pool NULL!");
            }
        }

        /// <seealso cref= ConnectionManager#get(String) </seealso>
        public virtual Connection get(string poolKey)
        {
            connTasks.TryGetValue(poolKey, out var value);
            ConnectionPool pool = getConnectionPool(value);
            return null == pool ? null : pool.get();
        }

        /// <seealso cref= ConnectionManager#getAll(java.lang.String) </seealso>
        public virtual List<Connection> getAll(string poolKey)
        {
            connTasks.TryGetValue(poolKey, out var value);
            ConnectionPool pool = getConnectionPool(value);
            return null == pool ? new List<Connection>() : pool.All;
        }

        /// <summary>
        /// Get all connections of all poolKey.
        /// </summary>
        /// <returns> a map with poolKey as key and a list of connections in ConnectionPool as value </returns>
        public virtual ConcurrentDictionary<string, List<Connection>> All
        {
            get
            {
                var allConnections = new ConcurrentDictionary<string, List<Connection>>();

                foreach (var entry in ConnPools)
                {
                    ConnectionPool pool = (ConnectionPool)FutureTaskUtil.getFutureTaskResult(entry.Value, logger);
                    if (null != pool)
                    {
                        (allConnections).AddOrUpdate(entry.Key, pool.All, (_, __) => pool.All);
                    }
                }
                return allConnections;
            }
        }

        /// <seealso cref= ConnectionManager#remove(Connection) </seealso>
        public virtual void remove(Connection connection)
        {
            if (null == connection)
            {
                return;
            }
            ISet<string> poolKeys = connection.PoolKeys;
            if (null == poolKeys || poolKeys.Count == 0)
            {
                connection.close();
                logger.LogWarning("Remove and close a standalone connection.");
            }
            else
            {
                foreach (string poolKey in poolKeys)
                {
                    remove(connection, poolKey);
                }
            }
        }

        /// <seealso cref= ConnectionManager#remove(Connection, java.lang.String) </seealso>
        public virtual void remove(Connection connection, string poolKey)
        {
            if (null == connection || string.IsNullOrWhiteSpace(poolKey))
            {
                return;
            }
            connTasks.TryGetValue(poolKey, out var value);
            ConnectionPool pool = getConnectionPool(value);
            if (null == pool)
            {
                connection.close();
                logger.LogWarning("Remove and close a standalone connection.");
            }
            else
            {
                pool.removeAndTryClose(connection);
                if (pool.Empty)
                {
                    removeTask(poolKey);
                    logger.LogWarning("Remove and close the last connection in ConnectionPool with poolKey {}", poolKey);
                }
                else
                {
                    logger.LogWarning("Remove and close a connection in ConnectionPool with poolKey {}, {} connections left.", poolKey, pool.size());
                }
            }
        }

        /// <seealso cref= ConnectionManager#remove(java.lang.String) </seealso>
        public virtual void remove(string poolKey)
        {
            if (string.IsNullOrWhiteSpace(poolKey))
            {
                return;
            }

            connTasks.TryRemove(poolKey, out var task);
            if (null != task)
            {
                ConnectionPool pool = getConnectionPool(task);
                if (null != pool)
                {
                    pool.removeAllAndTryClose();
                    logger.LogWarning("Remove and close all connections in ConnectionPool of poolKey={}", poolKey);
                }
            }
        }

        /// <summary>
        /// Warning! This is weakly consistent implementation, to prevent lock the whole <seealso cref="ConcurrentHashMap"/>.
        /// </summary>
        [Obsolete]
        public virtual void removeAll()
        {
            if (null == connTasks || connTasks.IsEmpty)
            {
                return;
            }

            var removeList = new List<string>();
            foreach (var poolKey in connTasks.Keys)
            {
                removeTask(poolKey);
                removeList.Add(poolKey);
            }
            removeList.ForEach((poolKey) => connTasks.TryRemove(poolKey, out _));


            logger.LogWarning("All connection pool and connections have been removed!");
        }

        /// <seealso cref= ConnectionManager#check(Connection) </seealso>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void check(Connection connection) throws exception.RemotingException
        public virtual void check(Connection connection)
        {
            if (connection == null)
            {
                throw new RemotingException("Connection is null when do check!");
            }
            if (connection.Channel == null || !connection.Channel.Active)
            {
                remove(connection);
                throw new RemotingException("Check connection failed for address: " + connection.Url);
            }
            if (!connection.Channel.IsWritable)
            {
                // No remove. Most of the time it is unwritable temporarily.
                throw new RemotingException("Check connection failed for address: " + connection.Url + ", maybe write overflow!");
            }
        }

        /// <seealso cref= ConnectionManager#count(java.lang.String) </seealso>
        public virtual int count(string poolKey)
        {
            if (string.IsNullOrWhiteSpace(poolKey))
            {
                return 0;
            }
            connTasks.TryGetValue(poolKey, out var value);
            ConnectionPool connectionPool = getConnectionPool(value);
            if (null != connectionPool)
            {
                return connectionPool.size();
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// in case of cache pollution and connection leak, to do schedule scan
        /// </summary>
        public virtual void scan()
        {
            if (null != connTasks && !connTasks.IsEmpty)
            {
                var removeList = new List<string>();
                foreach (var poolKey in connTasks.Keys)
                {
                    connTasks.TryGetValue(poolKey, out var value);
                    ConnectionPool connectionPool = getConnectionPool(value);
                    if (null != connectionPool)
                    {
                        connectionPool.scan();
                        if (connectionPool.Empty)
                        {
                            if (((int)new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() - connectionPool.LastAccessTimestamp) > Constants.DEFAULT_EXPIRE_TIME)
                            {
                                //JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
                                removeList.Add(poolKey);
                                logger.LogWarning("Remove expired pool task of poolKey {} which is empty.", poolKey);
                            }
                        }
                    }
                }
                removeList.ForEach((poolKey) => connTasks.TryRemove(poolKey, out _));
            }
        }

        /// <summary>
        /// If no task cached, create one and initialize the connections.
        /// </summary>
        /// <seealso cref= ConnectionManager#getAndCreateIfAbsent(Url) </seealso>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public Connection getAndCreateIfAbsent(Url url) throws ThreadInterruptedException, exception.RemotingException
        public virtual Connection getAndCreateIfAbsent(Url url)
        {
            // get and create a connection pool with initialized connections.
            ConnectionPool pool = getConnectionPoolAndCreateIfAbsent(url.UniqueKey, new ConnectionPoolCall(this, url));
            if (null != pool)
            {
                return pool.get();
            }
            else
            {
                logger.LogError("[NOTIFYME] bug detected! pool here must not be null!");
                return null;
            }
        }

        /// <summary>
        /// If no task cached, create one and initialize the connections.
        /// If task cached, check whether the number of connections adequate, if not then heal it.
        /// </summary>
        /// <param name="url"> target url </param>
        /// <exception cref="ThreadInterruptedException"> </exception>
        /// <exception cref="RemotingException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void createConnectionAndHealIfNeed(Url url) throws ThreadInterruptedException, exception.RemotingException
        public virtual void createConnectionAndHealIfNeed(Url url)
        {
            // get and create a connection pool with initialized connections.
            ConnectionPool pool = getConnectionPoolAndCreateIfAbsent(url.UniqueKey, new ConnectionPoolCall(this, url));
            if (null != pool)
            {
                healIfNeed(pool, url);
            }
            else
            {
                //logger.LogError("[NOTIFYME] bug detected! pool here must not be null!");
            }
        }

        /// <seealso cref= ConnectionManager#create(Url) </seealso>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public Connection create(Url url) throws exception.RemotingException
        public virtual Connection create(Url url)
        {
            Connection conn;
            try
            {
                conn = connectionFactory.createConnection(url);
            }
            catch (System.Exception e)
            {
                throw new RemotingException("Create connection failed. The address is " + url.OriginUrl, e);
            }
            return conn;
        }

        /// <seealso cref= ConnectionManager#create(java.lang.String, int, int) </seealso>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public Connection create(String ip, int port, int connectTimeout) throws exception.RemotingException
        public virtual Connection create(IPAddress ip, int port, int connectTimeout)
        {
            try
            {
                return connectionFactory.createConnection(ip, port, connectTimeout);
            }
            catch (System.Exception e)
            {
                throw new RemotingException("Create connection failed. The address is " + ip + ":" + port, e);
            }
        }

        /// <seealso cref= ConnectionManager#create(java.lang.String, int) </seealso>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public Connection create(String address, int connectTimeout) throws exception.RemotingException
        public virtual Connection create(string address, int connectTimeout)
        {
            Url url = addressParser.parse(address);
            url.ConnectTimeout = connectTimeout;
            return create(url);
        }

        /// <seealso cref= ConnectionHeartbeatManager#disableHeartbeat(Connection) </seealso>
        public virtual void disableHeartbeat(Connection connection)
        {
            if (null != connection)
            {
                connection.Channel.GetAttribute(Connection.HEARTBEAT_SWITCH).Set(false);
            }
        }

        /// <seealso cref= ConnectionHeartbeatManager#enableHeartbeat(Connection) </seealso>
        public virtual void enableHeartbeat(Connection connection)
        {
            if (null != connection)
            {
                connection.Channel.GetAttribute(Connection.HEARTBEAT_SWITCH).Set(true);
            }
        }

        /// <summary>
        /// get connection pool from future task
        /// </summary>
        /// <param name="task"> future task </param>
        /// <returns> connection pool </returns>
        private ConnectionPool getConnectionPool(RunStateRecordedFutureTask task)
        {
            return (ConnectionPool)FutureTaskUtil.getFutureTaskResult(task, logger);

        }

        /// <summary>
        /// Get the mapping instance of <seealso cref="ConnectionPool"/> with the specified poolKey,
        /// or create one if there is none mapping in connTasks.
        /// </summary>
        /// <param name="poolKey">  mapping key of <seealso cref="ConnectionPool"/> </param>
        /// <param name="callable"> the callable task </param>
        /// <returns> a non-nullable instance of <seealso cref="ConnectionPool"/> </returns>
        /// <exception cref="RemotingException"> if there is no way to get an available <seealso cref="ConnectionPool"/> </exception>
        /// <exception cref="ThreadInterruptedException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: private ConnectionPool getConnectionPoolAndCreateIfAbsent(String poolKey, java.util.concurrent.Callable<ConnectionPool> callable) throws exception.RemotingException, ThreadInterruptedException
        private ConnectionPool getConnectionPoolAndCreateIfAbsent(string poolKey, Callable callable)
        {
            RunStateRecordedFutureTask initialTask;
            ConnectionPool pool = null;

            int retry = Constants.DEFAULT_RETRY_TIMES;

            int timesOfResultNull = 0;
            int timesOfInterrupt = 0;

            for (int i = 0; (i < retry) && (pool == null); ++i)
            {
                connTasks.TryGetValue(poolKey, out initialTask);
                if (null == initialTask)
                {
                    RunStateRecordedFutureTask newTask = new RunStateRecordedFutureTask(callable);

                    if (!connTasks.ContainsKey(poolKey))
                    {
                        connTasks.AddOrUpdate(poolKey, newTask, (_, __) => newTask);
                        initialTask = newTask;
                    }
                    else
                    {
                        connTasks.TryGetValue(poolKey, out initialTask);
                    }

                    initialTask.run();
                }

                try
                {
                    pool = (ConnectionPool)initialTask.get();
                    if (null == pool)
                    {
                        if (i + 1 < retry)
                        {
                            timesOfResultNull++;
                            continue;
                        }
                        connTasks.TryRemove(poolKey, out _);
                        string errMsg = "Get future task result null for poolKey [" + poolKey + "] after [" + (timesOfResultNull + 1) + "] times try.";
                        throw new RemotingException(errMsg);
                    }
                }
                catch (ThreadInterruptedException e)
                {
                    if (i + 1 < retry)
                    {
                        timesOfInterrupt++;
                        continue; // retry if interrupted
                    }
                    connTasks.TryRemove(poolKey, out _);
                    logger.LogWarning("Future task of poolKey {} interrupted {} times. ThreadInterruptedException thrown and stop retry.", poolKey, timesOfInterrupt + 1, e);
                    throw;
                }
                catch (ExecutionException e)
                {
                    // DO NOT retry if ExecutionException occurred
                    connTasks.TryRemove(poolKey, out _);

                    System.Exception cause = e.InnerException;
                    if (cause is RemotingException)
                    {
                        throw (RemotingException)cause;
                    }
                    else
                    {
                        FutureTaskUtil.launderThrowable(cause);
                    }
                }
            }
            return pool;
        }

        /// <summary>
        /// remove task and remove all connections
        /// </summary>
        /// <param name="poolKey"> target pool key </param>
        protected internal virtual void removeTask(string poolKey)
        {
            connTasks.TryRemove(poolKey, out var task);
            if (null != task)
            {
                ConnectionPool pool = (ConnectionPool)FutureTaskUtil.getFutureTaskResult(task, logger);
                if (null != pool)
                {
                    pool.removeAllAndTryClose();
                }
            }
        }

        /// <summary>
        /// execute heal connection tasks if the actual number of connections in pool is less than expected
        /// </summary>
        /// <param name="pool"> connection pool </param>
        /// <param name="url"> target url </param>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: private void healIfNeed(ConnectionPool pool, Url url) throws exception.RemotingException, ThreadInterruptedException
        private void healIfNeed(ConnectionPool pool, Url url)
        {
            string poolKey = url.UniqueKey;
            // only when async creating connections done
            // and the actual size of connections less than expected, the healing task can be run.
            if (pool.AsyncCreationDone && pool.size() < url.ConnNum)
            {
                this.healTasks.TryGetValue(poolKey, out var task);
                if (null == task)
                {
                    FutureTask newTask = new FutureTask(new HealConnectionCall(this, url, pool));

                    if (!healTasks.ContainsKey(poolKey))
                    {
                        healTasks.AddOrUpdate(poolKey, newTask, (_, __) => newTask);
                        task = newTask;
                    }
                    else
                    {
                        healTasks.TryGetValue(poolKey, out task);
                    }
                    task.run();
                }
                try
                {
                    int numAfterHeal = (int)task.@get();
                    if (logger.IsEnabled(LogLevel.Debug))
                    {
                        logger.LogDebug("[NOTIFYME] - conn num after heal {}, expected {}, warmup {}", numAfterHeal, url.ConnNum, url.ConnWarmup);
                    }
                }
                catch (ThreadInterruptedException)
                {
                    healTasks.TryRemove(poolKey, out _);
                    throw;
                }
                catch (ExecutionException e)
                {
                    healTasks.TryRemove(poolKey, out _);
                    System.Exception cause = e.InnerException;
                    if (cause is RemotingException)
                    {
                        throw (RemotingException)cause;
                    }
                    else
                    {
                        FutureTaskUtil.launderThrowable(cause);
                    }
                }
                // heal task is one-off, remove from cache directly after run
                healTasks.TryRemove(poolKey, out _);
            }
        }

        /// <summary>
        /// a callable definition for initialize <seealso cref="ConnectionPool"/>
        /// 
        /// @author tsui
        /// @version $Id: ConnectionPoolCall.java, v 0.1 Mar 8, 2016 10:43:51 AM xiaomin.cxm Exp $
        /// </summary>
        private class ConnectionPoolCall : Callable
        {
            private readonly DefaultConnectionManager outerInstance;

            internal bool whetherInitConnection;
            internal Url url;

            /// <summary>
            /// create a <seealso cref="ConnectionPool"/> but not init connections
            /// </summary>
            public ConnectionPoolCall(DefaultConnectionManager outerInstance)
            {
                this.outerInstance = outerInstance;
                whetherInitConnection = false;
            }

            /// <summary>
            /// create a <seealso cref="ConnectionPool"/> and init connections with the specified <seealso cref="Url"/>
            /// </summary>
            /// <param name="url"> target url </param>
            public ConnectionPoolCall(DefaultConnectionManager outerInstance, Url url)
            {
                this.outerInstance = outerInstance;
                whetherInitConnection = true;
                this.url = url;
            }

            //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
            //ORIGINAL LINE: @Override public ConnectionPool call() throws Exception
            public virtual object call()
            {
                //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                //ORIGINAL LINE: final ConnectionPool pool = new ConnectionPool(connectionSelectStrategy);
                ConnectionPool pool = new ConnectionPool(outerInstance.connectionSelectStrategy);
                if (whetherInitConnection)
                {
                    try
                    {
                        outerInstance.doCreate(url, pool, GetType().Name, 1);
                    }
                    catch
                    {
                        pool.removeAllAndTryClose();
                        throw;
                    }
                }
                return pool;
            }

        }

        /// <summary>
        /// a callable definition for healing connections in <seealso cref="ConnectionPool"/>
        /// </summary>
        private class HealConnectionCall : Callable
        {
            private readonly DefaultConnectionManager outerInstance;

            internal Url url;
            internal ConnectionPool pool;

            /// <summary>
            /// create a <seealso cref="ConnectionPool"/> and init connections with the specified <seealso cref="Url"/>
            /// </summary>
            /// <param name="url"> target url </param>
            public HealConnectionCall(DefaultConnectionManager outerInstance, Url url, ConnectionPool pool)
            {
                this.outerInstance = outerInstance;
                this.url = url;
                this.pool = pool;
            }

            //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
            //ORIGINAL LINE: @Override public Nullable<int> call() throws Exception
            public virtual object call()
            {
                outerInstance.doCreate(url, pool, GetType().Name, 0);
                return pool.size();
            }
        }

        /// <summary>
        /// do create connections
        /// </summary>
        /// <param name="url"> target url </param>
        /// <param name="pool"> connection pool </param>
        /// <param name="taskName"> task name </param>
        /// <param name="syncCreateNumWhenNotWarmup"> you can specify this param to ensure at least desired number of connections available in sync way </param>
        /// <exception cref="RemotingException"> </exception>
        private void doCreate(Url url, ConnectionPool pool, string taskName, int syncCreateNumWhenNotWarmup)
        {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final int actualNum = pool.size();
            int actualNum = pool.size();
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final int expectNum = url.getConnNum();
            int expectNum = url.ConnNum;
            if (actualNum >= expectNum)
            {
                return;
            }
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("actual num {}, expect num {}, task name {}", actualNum, expectNum, taskName);
            }
            if (url.ConnWarmup)
            {
                for (int i = actualNum; i < expectNum; ++i)
                {
                    Connection connection = create(url);
                    pool.add(connection);
                }
            }
            else
            {
                if (syncCreateNumWhenNotWarmup < 0 || syncCreateNumWhenNotWarmup > url.ConnNum)
                {
                    throw new ArgumentException("sync create number when not warmup should be [0," + url.ConnNum + "]");
                }
                // create connection in sync way
                if (syncCreateNumWhenNotWarmup > 0)
                {
                    for (int i = 0; i < syncCreateNumWhenNotWarmup; ++i)
                    {
                        Connection connection = create(url);
                        pool.add(connection);
                    }
                    if (syncCreateNumWhenNotWarmup >= url.ConnNum)
                    {
                        return;
                    }
                }

                pool.markAsyncCreationStart(); // mark the start of async
                try
                {
                    asyncCreateConnectionExecutor.execute(new TempRunnable(this, pool, url, taskName));
                }
                catch (RejectedExecutionException)
                {
                    pool.markAsyncCreationDone(); // mark the end of async when reject
                    throw;
                }
            } // end of NOT warm up
        }

        public class TempRunnable : Runnable
        {
            private readonly DefaultConnectionManager defaultConnectionManager;
            private readonly ConnectionPool connectionPool;
            private readonly Url url;
            private readonly string taskName;

            public TempRunnable(DefaultConnectionManager defaultConnectionManager, ConnectionPool connectionPool, Url url, string taskName)
            {
                this.defaultConnectionManager = defaultConnectionManager;
                this.connectionPool = connectionPool;
                this.url = url;
                this.taskName = taskName;
            }

            public void run()
            {
                try
                {
                    for (int i = connectionPool.size(); i < url.ConnNum; ++i)
                    {
                        Connection conn = null;
                        try
                        {
                            conn = defaultConnectionManager.create(url);

                        }
                        catch (RemotingException e)
                        {
                            logger.LogError("Exception occurred in async create connection thread for {}, taskName {}", url.UniqueKey, taskName, e);
                        }
                        connectionPool.add(conn);
                    }
                }
                finally
                {
                    connectionPool.markAsyncCreationDone(); // mark the end of async
                }
            }
        }

        /// <summary>
        /// Getter method for property <tt>connectionSelectStrategy</tt>.
        /// </summary>
        /// <returns> property value of connectionSelectStrategy </returns>
        public virtual ConnectionSelectStrategy ConnectionSelectStrategy
        {
            get
            {
                return connectionSelectStrategy;
            }
            set
            {
                connectionSelectStrategy = value;
            }
        }


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
        /// Getter method for property <tt>connctionFactory</tt>.
        /// </summary>
        /// <returns> property value of connctionFactory </returns>
        public virtual ConnectionFactory ConnectionFactory
        {
            get
            {
                return connectionFactory;
            }
            set
            {
                connectionFactory = value;
            }
        }


        /// <summary>
        /// Getter method for property <tt>connectionEventHandler</tt>.
        /// </summary>
        /// <returns> property value of connectionEventHandler </returns>
        public virtual ConnectionEventHandler ConnectionEventHandler
        {
            get
            {
                return connectionEventHandler;
            }
            set
            {
                connectionEventHandler = value;
            }
        }


        /// <summary>
        /// Getter method for property <tt>connectionEventListener</tt>.
        /// </summary>
        /// <returns> property value of connectionEventListener </returns>
        public virtual ConnectionEventListener ConnectionEventListener
        {
            get
            {
                return connectionEventListener;
            }
            set
            {
                connectionEventListener = value;
            }
        }


        /// <summary>
        /// Getter method for property <tt>connPools</tt>.
        /// </summary>
        /// <returns> property value of connPools </returns>
        public virtual ConcurrentDictionary<string, RunStateRecordedFutureTask> ConnPools
        {
            get
            {
                return connTasks;
            }
        }
    }

}