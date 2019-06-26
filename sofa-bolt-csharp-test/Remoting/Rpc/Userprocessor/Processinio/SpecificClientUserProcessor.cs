using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting;
using System;
using com.alipay.remoting.rpc.common;
using Xunit;
using java.util.concurrent;
using java.util.concurrent.atomic;
using Remoting.rpc.protocol;

namespace com.alipay.remoting.rpc.userprocessor.processinio
{
    /// <summary>
    /// a demo specific user processor for rpc client
    /// </summary>
    public class SpecificClientUserProcessor : SyncUserProcessor
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

        private ThreadPoolExecutor executor;

        private AtomicInteger invokeTimes = new AtomicInteger();

        public SpecificClientUserProcessor()
        {
            this.delaySwitch = false;
            this.delayMs = 0;
            this.executor = new ThreadPoolExecutor(1, 3, 60, TimeUnit.SECONDS, new ArrayBlockingQueue(4), new NamedThreadFactory("Rpc-common-executor"));
        }

        public SpecificClientUserProcessor(int delay) : this()
        {
            if (delay < 0)
            {
                throw new ArgumentException("delay time illegal!");
            }
            this.delaySwitch = true;
            this.delayMs = delay;
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public Object handleRequest(com.alipay.remoting.BizContext bizCtx, com.alipay.remoting.rpc.common.RequestBody request) throws Exception
        public override object handleRequest(BizContext bizCtx, object request)
        {
            string threadName = Thread.CurrentThread.Name;
            //Assert.Contains("bolt-netty-client-worker", threadName);

            logger.LogWarning("Request received:" + request);
            Assert.Equal(typeof(RequestBody), request.GetType());

            long waittime = ((long?)bizCtx.InvokeContext.get(InvokeContext.BOLT_PROCESS_WAIT_TIME)).Value;
            logger.LogWarning("Client User processor process wait time [" + waittime + "].");

            invokeTimes.incrementAndGet();
            if (!delaySwitch)
            {
                return RequestBody.DEFAULT_CLIENT_RETURN_STR;
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
            return RequestBody.DEFAULT_CLIENT_RETURN_STR;
        }

        public override Type interest()
        {
            //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            return typeof(RequestBody);
        }

        public override bool processInIOThread()
        {
            return true;
        }

        public virtual int InvokeTimes
        {
            get
            {
                return this.invokeTimes.get();
            }
        }
    }

}