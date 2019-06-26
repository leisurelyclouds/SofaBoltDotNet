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
using System.Collections.Generic;

namespace com.alipay.remoting.rpc.userprocessor.multiinterestprocessor
{
    /// <summary>
    /// @antuor muyun.cyt (muyun.cyt@antfin.com)  2018/7/5   11:20 AM
    /// </summary>
    public class SimpleClientMultiInterestUserProcessor : SyncMutiInterestUserProcessor
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

		private AtomicInteger c1invokeTimes = new AtomicInteger();
		private AtomicInteger c1onewayTimes = new AtomicInteger();
		private AtomicInteger c1syncTimes = new AtomicInteger();
		private AtomicInteger c1futureTimes = new AtomicInteger();
		private AtomicInteger c1callbackTimes = new AtomicInteger();

		private AtomicInteger c2invokeTimes = new AtomicInteger();
		private AtomicInteger c2onewayTimes = new AtomicInteger();
		private AtomicInteger c2syncTimes = new AtomicInteger();
		private AtomicInteger c2futureTimes = new AtomicInteger();
		private AtomicInteger c2callbackTimes = new AtomicInteger();

		public SimpleClientMultiInterestUserProcessor()
		{
			this.delaySwitch = false;
			this.delayMs = 0;
			this.executor = new ThreadPoolExecutor(1, 3, 60, TimeUnit.SECONDS, new ArrayBlockingQueue(4), new NamedThreadFactory("Request-process-pool"));
		}

		public SimpleClientMultiInterestUserProcessor(int delay) : this()
		{
			if (delay < 0)
			{
				throw new ArgumentException("delay time illegal!");
			}
			this.delaySwitch = true;
			this.delayMs = delay;
		}

		public SimpleClientMultiInterestUserProcessor(int delay, int core, int max, int keepaliveSeconds, int workQueue) : this(delay)
		{
			this.executor = new ThreadPoolExecutor(core, max, keepaliveSeconds, TimeUnit.SECONDS, new ArrayBlockingQueue(workQueue), new NamedThreadFactory("Request-process-pool"));
		}

		// ~~~ override methods

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public Object handleRequest(com.alipay.remoting.BizContext bizCtx, MultiInterestBaseRequestBody request) throws Exception
		public override object handleRequest(BizContext bizCtx, object request)
		{
			logger.LogWarning("Request received:" + request);
			if (bizCtx.RequestTimeout)
			{
				string errMsg = "Stop process in client biz thread, already timeout!";
				logger.LogWarning(errMsg);
				throw new Exception(errMsg);
			}

			if (request is RequestBodyC1)
			{
				Assert.Equal(typeof(RequestBodyC1), request.GetType());
				return handleRequest(bizCtx, (RequestBodyC1) request);
			}
			else if (request is RequestBodyC2)
			{
				Assert.Equal(typeof(RequestBodyC2), request.GetType());
				return handleRequest(bizCtx, (RequestBodyC2) request);
			}
			else
			{
				throw new Exception("RequestBody does not belong to defined interests !");
			}
		}

		private object handleRequest(BizContext bizCtx, RequestBodyC1 request)
		{

			long? waittime = (long?) bizCtx.InvokeContext.get(InvokeContext.BOLT_PROCESS_WAIT_TIME);
			Assert.NotNull(waittime);
			if (logger.IsEnabled(LogLevel.Information))
			{
				logger.LogInformation("Client User processor process wait time {}", waittime);
			}

			processTimes(request);
			if (!delaySwitch)
			{
				return RequestBodyC1.DEFAULT_CLIENT_RETURN_STR;
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
			return RequestBodyC1.DEFAULT_CLIENT_RETURN_STR;
		}

		private object handleRequest(BizContext bizCtx, RequestBodyC2 request)
		{

			long? waittime = (long?) bizCtx.InvokeContext.get(InvokeContext.BOLT_PROCESS_WAIT_TIME);
			Assert.NotNull(waittime);
			if (logger.IsEnabled(LogLevel.Information))
			{
				logger.LogInformation("Client User processor process wait time {}", waittime);
			}

			processTimes(request);
			if (!delaySwitch)
			{
				return RequestBodyC2.DEFAULT_CLIENT_RETURN_STR;
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
			return RequestBodyC2.DEFAULT_CLIENT_RETURN_STR;
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
		public virtual int InvokeTimesC1
		{
			get
			{
				return this.c1invokeTimes.get();
			}
		}

		public virtual int InvokeTimesC2
		{
			get
			{
				return this.c2invokeTimes.get();
			}
		}

		public virtual int getInvokeTimesEachCallTypeC1(RequestBody.InvokeType type)
		{
			return new int[] {this.c1onewayTimes.get(), this.c1syncTimes.get(), this.c1futureTimes.get(), this.c1callbackTimes.get()}[(int)type];
		}

		public virtual int getInvokeTimesEachCallTypeC2(RequestBody.InvokeType type)
		{
			return new int[] {this.c2onewayTimes.get(), this.c2syncTimes.get(), this.c2futureTimes.get(), this.c2callbackTimes.get()}[(int)type];
		}

		// ~~~ private methods
		private void processTimes(RequestBodyC1 req)
		{
			this.c1invokeTimes.incrementAndGet();
			if (req.Msg.Equals(RequestBodyC1.DEFAULT_ONEWAY_STR))
			{
				this.c1onewayTimes.incrementAndGet();
			}
			else if (req.Msg.Equals(RequestBodyC1.DEFAULT_SYNC_STR))
			{
				this.c1syncTimes.incrementAndGet();
			}
			else if (req.Msg.Equals(RequestBodyC1.DEFAULT_FUTURE_STR))
			{
				this.c1futureTimes.incrementAndGet();
			}
			else if (req.Msg.Equals(RequestBodyC1.DEFAULT_CALLBACK_STR))
			{
				this.c1callbackTimes.incrementAndGet();
			}
		}

		private void processTimes(RequestBodyC2 req)
		{
			this.c2invokeTimes.incrementAndGet();
			if (req.Msg.Equals(RequestBodyC2.DEFAULT_ONEWAY_STR))
			{
				this.c2onewayTimes.incrementAndGet();
			}
			else if (req.Msg.Equals(RequestBodyC2.DEFAULT_SYNC_STR))
			{
				this.c2syncTimes.incrementAndGet();
			}
			else if (req.Msg.Equals(RequestBodyC2.DEFAULT_FUTURE_STR))
			{
				this.c2futureTimes.incrementAndGet();
			}
			else if (req.Msg.Equals(RequestBodyC2.DEFAULT_CALLBACK_STR))
			{
				this.c2callbackTimes.incrementAndGet();
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


		public override List<Type> multiInterest()
		{
			var list = new List<Type>();
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			list.Add(typeof(RequestBodyC1));
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			list.Add(typeof(RequestBodyC2));
			return list;
		}
	}
}