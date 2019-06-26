using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting;
using System;
using java.util.concurrent;
using java.util.concurrent.atomic;
using Remoting.rpc.protocol;

namespace com.alipay.remoting.rpc.common
{
    /// <summary>
    /// a demo user processor
    /// </summary>
    public class AsyncExceptionUserProcessor : AsyncUserProcessor
    {
        /// <summary>
        /// logger
		/// </summary>
        private static readonly ILogger logger = NullLogger.Instance;

        /// <summary>
        /// delay milliseconds
		/// </summary>
        private int delayMs;

        /// <summary>
        /// whether delay or not
		/// </summary>
        private bool delaySwitch;

        /// <summary>
        /// executor
		/// </summary>
        private ThreadPoolExecutor executor = new ThreadPoolExecutor(1, 3, 60, TimeUnit.SECONDS, new ArrayBlockingQueue(4), new NamedThreadFactory("Request-process-pool"));

        private AtomicInteger invokeTimes = new AtomicInteger();

        public AsyncExceptionUserProcessor()
        {
            this.delaySwitch = false;
            this.delayMs = 0;
        }

        public AsyncExceptionUserProcessor(int delay)
        {
            if (delay < 0)
            {
                throw new ArgumentException("delay time illegal!");
            }
            this.delaySwitch = true;
            this.delayMs = delay;
        }

        public override Type interest()
        {
            //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            return typeof(RequestBody);
        }

        public override Executor Executor
        {
            get
            {
                return executor;
            }
        }

        public virtual int InvokeTimes
        {
            get
            {
                return this.invokeTimes.get();
            }
        }

        public override void handleRequest(BizContext bizCtx, AsyncContext asyncCtx, object request)
        {
            logger.LogWarning("Request received:" + request);
            invokeTimes.incrementAndGet();
            if (!delaySwitch)
            {
                throw new Exception("Hello exception!");
            }
            try
            {
                Thread.Sleep(delayMs);
            }
            catch (ThreadInterruptedException e)
            {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
            throw new Exception("Hello exception!");
        }
    }

}