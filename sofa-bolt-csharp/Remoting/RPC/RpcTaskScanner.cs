using java.lang;
using java.util.concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;

namespace Remoting.rpc
{
    /// <summary>
    /// Scanner is used to do scan task.
    /// </summary>
    public class RpcTaskScanner : AbstractLifeCycle
    {

        private static readonly ILogger logger = NullLogger.Instance;

        private readonly List<Scannable> scanList;

        private ScheduledExecutorService scheduledService;

        public RpcTaskScanner()
        {
            scanList = new List<Scannable>();
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void startup() throws LifeCycleException
        public override void startup()
        {
            base.startup();

            scheduledService = new ScheduledThreadPoolExecutor(1, new NamedThreadFactory("RpcTaskScannerThread", true));
            scheduledService.scheduleWithFixedDelay(new TempRunnable(scanList), 10000, 10000, TimeUnit.MILLISECONDS);
        }

        public class TempRunnable : Runnable
        {
            private readonly List<Scannable> scanList;

            public TempRunnable(List<Scannable> scanList)
            {
                this.scanList = scanList;
            }

            public void run()
            {
                foreach (var scannable in scanList)
                {
                    try
                    {
                        scannable.scan();
                    }
                    catch (System.Exception t)
                    {
                        logger.LogError("Exception caught when scannings.", t);
                    }
                }
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void shutdown() throws LifeCycleException
        public override void shutdown()
        {
            base.shutdown();

            scheduledService.shutdown();
        }

        /// <summary>
        /// Use <seealso cref="RpcTaskScanner#startup()"/> instead
        /// </summary>
        [Obsolete]
        public virtual void start()
        {
            startup();
        }

        /// <summary>
        /// Add scan target.
        /// </summary>
        public virtual void add(Scannable target)
        {
            scanList.Add(target);
        }
    }
}