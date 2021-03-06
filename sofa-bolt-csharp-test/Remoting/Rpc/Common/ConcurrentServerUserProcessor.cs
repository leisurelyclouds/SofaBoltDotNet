﻿using System.Threading;
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
    /// a demo user processor for rpc server
    /// </summary>
    public class ConcurrentServerUserProcessor : SyncUserProcessor
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

		private AtomicInteger invokeTimes = new AtomicInteger();

		private AtomicInteger onewayTimes = new AtomicInteger();
		private AtomicInteger syncTimes = new AtomicInteger();
		private AtomicInteger futureTimes = new AtomicInteger();
		private AtomicInteger callbackTimes = new AtomicInteger();

		private string remoteAddr;
		private CountdownEvent latch = new CountdownEvent(1);

		public ConcurrentServerUserProcessor()
		{
			this.delaySwitch = false;
			this.delayMs = 0;
			this.executor = new ThreadPoolExecutor(50, 100, 60, TimeUnit.SECONDS, new ArrayBlockingQueue(1000), new NamedThreadFactory("Request-process-pool"));
		}

		public ConcurrentServerUserProcessor(int delay) : this()
		{
			if (delay < 0)
			{
				throw new ArgumentException("delay time illegal!");
			}
			this.delaySwitch = true;
			this.delayMs = delay;
		}

		public ConcurrentServerUserProcessor(int delay, int core, int max, int keepaliveSeconds, int workQueue) : this(delay)
		{
			this.executor = new ThreadPoolExecutor(core, max, keepaliveSeconds, TimeUnit.SECONDS, new ArrayBlockingQueue(workQueue), new NamedThreadFactory("Request-process-pool"));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public Object handleRequest(com.alipay.remoting.BizContext bizCtx, RequestBody request) throws Exception
		public override  object  handleRequest(BizContext bizCtx, object request)
		{
			logger.LogWarning("Request received:" + request);
			this.remoteAddr = bizCtx.RemoteAddress;

			long waittime = ((long?) bizCtx.InvokeContext.get(InvokeContext.BOLT_PROCESS_WAIT_TIME)).Value;
			logger.LogWarning("Server User processor process wait time [" + waittime + "].");

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
			return new int[] {this.onewayTimes.get(), this.syncTimes.get(), this.futureTimes.get(), this.callbackTimes.get()}[(int)type];
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