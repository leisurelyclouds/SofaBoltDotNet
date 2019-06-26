using java.lang;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting;
using System;
using Xunit;
using java.util.concurrent;
using java.util.concurrent.atomic;
using Remoting.rpc.protocol;

namespace com.alipay.remoting.rpc.common
{
    /// <summary>
    /// a demo aysnc user processor for rpc server
    /// </summary>
    public class AsyncServerUserProcessor : AsyncUserProcessor
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
        /// whether exception
        /// </summary>
        private bool isException;

        /// <summary>
        /// whether null
        /// </summary>
        private bool isNull;

        /// <summary>
        /// executor
        /// </summary>
        private ThreadPoolExecutor executor;

        private ThreadPoolExecutor asyncExecutor;

        private AtomicInteger invokeTimes = new AtomicInteger();

        private AtomicInteger onewayTimes = new AtomicInteger();
        private AtomicInteger syncTimes = new AtomicInteger();
        private AtomicInteger futureTimes = new AtomicInteger();
        private AtomicInteger callbackTimes = new AtomicInteger();

        private string remoteAddr;
        private CountdownEvent latch = new CountdownEvent(1);

        public AsyncServerUserProcessor()
        {
            this.delaySwitch = false;
            this.isException = false;
            this.delayMs = 0;
            this.executor = new ThreadPoolExecutor(1, 3, 60, TimeUnit.SECONDS, new ArrayBlockingQueue(4), new NamedThreadFactory("Request-process-pool"));
            this.asyncExecutor = new ThreadPoolExecutor(1, 3, 60, TimeUnit.SECONDS, new ArrayBlockingQueue(4), new NamedThreadFactory("Another-aysnc-process-pool"));
        }

        public AsyncServerUserProcessor(bool isException, bool isNull) : this()
        {
            this.isException = isException;
            this.isNull = isNull;
        }

        public AsyncServerUserProcessor(int delay) : this()
        {
            if (delay < 0)
            {
                throw new ArgumentException("delay time illegal!");
            }
            this.delaySwitch = true;
            this.delayMs = delay;
        }

        public AsyncServerUserProcessor(int delay, int core, int max, int keepaliveSeconds, int workQueue) : this(delay)
        {
            this.executor = new ThreadPoolExecutor(core, max, keepaliveSeconds, TimeUnit.SECONDS, new ArrayBlockingQueue(workQueue), new NamedThreadFactory("Request-process-pool"));
        }

        public override void handleRequest(BizContext bizCtx, AsyncContext asyncCtx, object request)
        {
            this.asyncExecutor.execute(new InnerTask(this, bizCtx, asyncCtx, request));
        }

        internal class InnerTask : Runnable
        {
            private readonly AsyncServerUserProcessor outerInstance;

            internal BizContext bizCtx;
            internal AsyncContext asyncCtx;
            internal object request;

            public InnerTask(AsyncServerUserProcessor outerInstance, BizContext bizCtx, AsyncContext asyncCtx, object request)
            {
                this.outerInstance = outerInstance;
                this.bizCtx = bizCtx;
                this.asyncCtx = asyncCtx;
                this.request = request;
            }

            public virtual void run()
            {
                logger.LogWarning("Request received:" + request);
                outerInstance.remoteAddr = bizCtx.RemoteAddress;
                if (!outerInstance.latch.IsSet)
                {
                    outerInstance.latch.Signal();
                }
                logger.LogWarning("Server User processor say, remote address is [" + outerInstance.remoteAddr + "].");
                Assert.Equal(typeof(RequestBody), request.GetType());
                outerInstance.processTimes((RequestBody)request);
                if (outerInstance.isException)
                {
                    this.asyncCtx.sendResponse(new ArgumentException("Exception test"));
                }
                else if (outerInstance.isNull)
                {
                    this.asyncCtx.sendResponse(null);
                }
                else
                {
                    if (!outerInstance.delaySwitch)
                    {
                        this.asyncCtx.sendResponse(RequestBody.DEFAULT_SERVER_RETURN_STR);
                        return;
                    }
                    try
                    {
                        System.Threading.Thread.Sleep(outerInstance.delayMs);
                    }
                    catch (ThreadInterruptedException e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace);
                    }
                    this.asyncCtx.sendResponse(RequestBody.DEFAULT_SERVER_RETURN_STR);
                }
            }
        }

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

        public virtual int getInvokeTimesEachCallType(RequestBody.InvokeType type)
        {
            return new int[] { this.onewayTimes.get(), this.syncTimes.get(), this.futureTimes.get(), this.callbackTimes.get() }[(int)type];
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public String getRemoteAddr() throws ThreadInterruptedException
        public virtual string RemoteAddr
        {
            get
            {
                latch.Wait(100);
                return this.remoteAddr;
            }
        }
    }
}