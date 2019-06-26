using java.lang;
using java.util.concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Remoting
{
    /// <summary>
    /// Reconnect manager.
    /// </summary>
    public class ReconnectManager : AbstractLifeCycle, Reconnector
    {
        private static readonly ILogger logger = NullLogger.Instance;
        private const int HEAL_CONNECTION_INTERVAL = 1000;
        private readonly ConnectionManager connectionManager;
        private readonly LinkedBlockingQueue tasks;
        private readonly IList<Url> canceled;

        private System.Threading.Thread healConnectionThreads;

        public ReconnectManager(ConnectionManager connectionManager)
        {
            this.connectionManager = connectionManager;
            tasks = new LinkedBlockingQueue();
            canceled = new List<Url>();
        }

        public virtual void reconnect(Url url)
        {
            tasks.add(new ReconnectTask(this, url));
        }

        public virtual void disableReconnect(Url url)
        {
            canceled.Add(url);
        }

        public virtual void enableReconnect(Url url)
        {
            canceled.Remove(url);
        }

        public override void startup()
        {
            base.startup();

            healConnectionThreads = new System.Threading.Thread(Run);
            healConnectionThreads.Start(this);
        }

        internal long lastConnectTime = -1;

        public void Run(object obj)
        {
            var outerInstance = (ReconnectManager)obj;
            while (outerInstance.Started)
            {
                long start = -1;
                ReconnectTask task = null;
                try
                {
                    if (lastConnectTime < HEAL_CONNECTION_INTERVAL)
                    {
                        System.Threading.Thread.Sleep(HEAL_CONNECTION_INTERVAL);
                    }

                    try
                    {
                        task = (ReconnectTask)outerInstance.tasks.take();
                    }
                    catch (ThreadInterruptedException)
                    {
                        // ignore
                    }

                    if (task == null)
                    {
                        continue;
                    }

                    start = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                    if (!outerInstance.canceled.Contains(task.url))
                    {
                        task.run();
                    }
                    else
                    {
                        logger.LogWarning("Invalid reconnect request task {}, cancel list size {}", task.url, outerInstance.canceled.Count);
                    }
                    lastConnectTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() - start;
                }
                catch (System.Exception e)
                {
                    if (start != -1)
                    {
                        lastConnectTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() - start;
                    }

                    if (task != null)
                    {
                        logger.LogWarning("reconnect target: {} failed.", task.url, e);
                        outerInstance.tasks.add(task);
                    }
                }
            }
        }

        public override void shutdown()
        {
            base.shutdown();

            healConnectionThreads.Interrupt();
            tasks.clear();
            canceled.Clear();
        }

        /// <summary>
        /// please use <seealso cref="Reconnector#disableReconnect(Url)"/> instead
        /// </summary>
        [Obsolete]
        public virtual void addCancelUrl(Url url)
        {
            disableReconnect(url);
        }

        /// <summary>
        /// please use <seealso cref="Reconnector#enableReconnect(Url)"/> instead
        /// </summary>
        [Obsolete]
        public virtual void removeCancelUrl(Url url)
        {
            enableReconnect(url);
        }

        /// <summary>
        /// please use <seealso cref="Reconnector#reconnect(Url)"/> instead
        /// </summary>
        [Obsolete]
        public virtual void addReconnectTask(Url url)
        {
            reconnect(url);
        }

        /// <summary>
        /// please use <seealso cref="Reconnector#shutdown()"/> instead
        /// </summary>
        [Obsolete]
        public virtual void stop()
        {
            shutdown();
        }

        private class ReconnectTask : Runnable
        {
            private readonly ReconnectManager outerInstance;

            internal Url url;

            public ReconnectTask(ReconnectManager outerInstance, Url url)
            {
                this.outerInstance = outerInstance;
                this.url = url;
            }

            public void run()
            {
                outerInstance.connectionManager.createConnectionAndHealIfNeed(url);
            }
        }
    }
}