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
    /// a demo user processor for rpc client
    /// </summary>
    public class SimpleClientUserProcessor : SyncUserProcessor
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

		public SimpleClientUserProcessor()
		{
			this.delaySwitch = false;
			this.delayMs = 0;
			this.executor = new ThreadPoolExecutor(1, 3, 60, TimeUnit.SECONDS, new ArrayBlockingQueue(4), new NamedThreadFactory("Request-process-pool"));
		}

		public SimpleClientUserProcessor(int delay) : this()
		{
			if (delay < 0)
			{
				throw new ArgumentException("delay time illegal!");
			}
			this.delaySwitch = true;
			this.delayMs = delay;
		}

		public SimpleClientUserProcessor(int delay, int core, int max, int keepaliveSeconds, int workQueue) : this(delay)
		{
			this.executor = new ThreadPoolExecutor(core, max, keepaliveSeconds, TimeUnit.SECONDS, new ArrayBlockingQueue(workQueue), new NamedThreadFactory("Request-process-pool"));
		}

		// ~~~ override methods

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public Object handleRequest(com.alipay.remoting.BizContext bizCtx, RequestBody request) throws Exception
		public override  object  handleRequest(BizContext bizCtx, object request)
		{
			logger.LogWarning("Request received:" + request);
			if (bizCtx.RequestTimeout)
			{
				string errMsg = "Stop process in client biz thread, already timeout!";
				logger.LogWarning(errMsg);
				processTimes((RequestBody)request);
				throw new Exception(errMsg);
			}
			Assert.Equal(typeof(RequestBody), request.GetType());

			long? waittime = (long?) bizCtx.InvokeContext.get(InvokeContext.BOLT_PROCESS_WAIT_TIME);
			Assert.NotNull(waittime);
			if (logger.IsEnabled(LogLevel.Information))
			{
				logger.LogInformation("Client User processor process wait time {}", waittime);
			}

			processTimes((RequestBody)request);
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
			return new int[] {this.onewayTimes.get(), this.syncTimes.get(), this.futureTimes.get(), this.callbackTimes.get()}[(int)type];
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