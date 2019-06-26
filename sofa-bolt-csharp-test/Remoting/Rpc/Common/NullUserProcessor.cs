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
    /// a demo user processor return null
    /// </summary>
    public class NullUserProcessor : SyncUserProcessor
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

		public NullUserProcessor()
		{
			this.delaySwitch = false;
			this.delayMs = 0;
		}

		public NullUserProcessor(int delay)
		{
			if (delay < 0)
			{
				throw new ArgumentException("delay time illegal!");
			}
			this.delaySwitch = true;
			this.delayMs = delay;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public Object handleRequest(com.alipay.remoting.BizContext bizCtx, RequestBody request) throws Exception
		public override  object  handleRequest(BizContext bizCtx, object request)
		{
			logger.LogWarning("Request received:" + request);
			invokeTimes.incrementAndGet();
			if (!delaySwitch)
			{
				return null;
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
			return null;
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
	}

}