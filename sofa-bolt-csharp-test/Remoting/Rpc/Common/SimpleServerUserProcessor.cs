using System.Threading;
using java.util.concurrent;
using java.util.concurrent.atomic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting;
using Remoting.rpc.protocol;
using System;
using Xunit;

namespace com.alipay.remoting.rpc.common
{
    /// <summary>
    /// a demo user processor for rpc server
    /// </summary>
    public class SimpleServerUserProcessor : SyncUserProcessor
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
        private ThreadPoolExecutor executor;

        /// <summary>
        /// default is true
		/// </summary>
        //JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
        private bool timeoutDiscard_Renamed = true;

        private AtomicInteger invokeTimes = new AtomicInteger();

        private AtomicInteger onewayTimes = new AtomicInteger();
        private AtomicInteger syncTimes = new AtomicInteger();
        private AtomicInteger futureTimes = new AtomicInteger();
        private AtomicInteger callbackTimes = new AtomicInteger();

        private string remoteAddr;
        private CountdownEvent latch = new CountdownEvent(1);

        public SimpleServerUserProcessor()
        {
            this.delaySwitch = false;
            this.delayMs = 0;
            this.executor = new ThreadPoolExecutor(1, 3, 60, TimeUnit.SECONDS, new ArrayBlockingQueue(4), new NamedThreadFactory("Request-process-pool"));
        }

        public SimpleServerUserProcessor(int delay) : this()
        {
            if (delay < 0)
            {
                throw new ArgumentException("delay time illegal!");
            }
            this.delaySwitch = true;
            this.delayMs = delay;
        }

        public SimpleServerUserProcessor(int delay, int core, int max, int keepaliveSeconds, int workQueue) : this(delay)
        {
            this.executor = new ThreadPoolExecutor(core, max, keepaliveSeconds, TimeUnit.SECONDS, new ArrayBlockingQueue(workQueue), new NamedThreadFactory("Request-process-pool"));
        }

        // ~~~ override methods

        public override object handleRequest(BizContext bizCtx, object request)
        {
            logger.LogWarning("Request received:" + request + ", timeout:" + bizCtx.ClientTimeout + ", arriveTimestamp:" + bizCtx.ArriveTimestamp);

            if (bizCtx.RequestTimeout)
            {
                string errMsg = "Stop process in server biz thread, already timeout!";
                processTimes((RequestBody)request);
                logger.LogWarning(errMsg);
                throw new Exception(errMsg);
            }

            this.remoteAddr = bizCtx.RemoteAddress;

            //test biz context get connection
            Assert.NotNull(bizCtx.Connection);
            Assert.True(bizCtx.Connection.Fine);

            long? waittime = (long?)bizCtx.InvokeContext.get(InvokeContext.BOLT_PROCESS_WAIT_TIME);
            Assert.NotNull(waittime);
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Server User processor process wait time {}", waittime);
            }

            if (!latch.IsSet)
            {
                latch.Signal();
            }
            logger.LogWarning("Server User processor say, remote address is [" + this.remoteAddr + "].");
            Assert.Equal(typeof(RequestBody), request.GetType());
            processTimes((RequestBody)request);
            if (!delaySwitch)
            {
                return RequestBody.DEFAULT_SERVER_RETURN_STR;
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
            return RequestBody.DEFAULT_SERVER_RETURN_STR;
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

        public override bool timeoutDiscard()
        {
            return this.timeoutDiscard_Renamed;
        }

        // ~~~ public methods
        public virtual int InvokeTimes
        {
            get
            {
                return this.invokeTimes.get();
            }
        }

        public virtual int getInvokeTimesEachCallType(RequestBody.InvokeType type)
        {
            return new int[] { this.onewayTimes.get(), this.syncTimes.get(), this.futureTimes.get(), this.callbackTimes.get() }[(int)type];
        }

        public virtual string RemoteAddr
        {
            get
            {
                latch.Wait(100);
                return this.remoteAddr;
            }
        }

        // ~~~ private methods
        private void processTimes(RequestBody req)
        {
            this.invokeTimes.incrementAndGet();
            if (req.Msg.Equals(RequestBody.DEFAULT_ONEWAY_STR))
            {
                this.onewayTimes.incrementAndGet();
            }
            else if (req.Msg.Equals(RequestBody.DEFAULT_SYNC_STR))
            {
                this.syncTimes.incrementAndGet();
            }
            else if (req.Msg.Equals(RequestBody.DEFAULT_FUTURE_STR))
            {
                this.futureTimes.incrementAndGet();
            }
            else if (req.Msg.Equals(RequestBody.DEFAULT_CALLBACK_STR))
            {
                this.callbackTimes.incrementAndGet();
            }
        }

        // ~~~ getters and setters
        /// <summary>
        /// Getter method for property <tt>timeoutDiscard</tt>.
        /// </summary>
        /// <returns> property value of timeoutDiscard </returns>
        public virtual bool TimeoutDiscard
        {
            get
            {
                return timeoutDiscard_Renamed;
            }
            set
            {
                this.timeoutDiscard_Renamed = value;
            }
        }

    }
}