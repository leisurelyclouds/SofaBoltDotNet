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
    /// a demo aysnc user processor for rpc client
    /// </summary>
    public class AsyncClientUserProcessor : AsyncUserProcessor
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

		public AsyncClientUserProcessor()
		{
			this.delaySwitch = false;
			this.isException = false;
			this.isNull = false;
			this.delayMs = 0;
			this.executor = new ThreadPoolExecutor(1, 3, 60, TimeUnit.SECONDS, new ArrayBlockingQueue(4), new NamedThreadFactory("Request-process-pool"));
			this.asyncExecutor = new ThreadPoolExecutor(1, 3, 60, TimeUnit.SECONDS, new ArrayBlockingQueue(4), new NamedThreadFactory("Another-aysnc-process-pool"));
		}

		public AsyncClientUserProcessor(bool isException, bool isNull) : this()
		{
			this.isException = isException;
			this.isNull = isNull;
		}

		public AsyncClientUserProcessor(int delay) : this()
		{
			if (delay < 0)
			{
				throw new ArgumentException("delay time illegal!");
			}
			this.delaySwitch = true;
			this.delayMs = delay;
		}

		public AsyncClientUserProcessor(int delay, int core, int max, int keepaliveSeconds, int workQueue) : this(delay)
		{
			this.executor = new ThreadPoolExecutor(core, max, keepaliveSeconds, TimeUnit.SECONDS, new ArrayBlockingQueue(workQueue), new NamedThreadFactory("Request-process-pool"));
		}

		public override void handleRequest(BizContext bizCtx, AsyncContext asyncCtx, object request)
		{
			this.asyncExecutor.execute(new InnerTask(this, asyncCtx, request));
		}

		internal class InnerTask : Runnable
		{
			private readonly AsyncClientUserProcessor outerInstance;

			internal AsyncContext asyncCtx;
			internal object request;

			public InnerTask(AsyncClientUserProcessor outerInstance, AsyncContext asyncCtx, object request)
			{
				this.outerInstance = outerInstance;
				this.asyncCtx = asyncCtx;
				this.request = request;
			}

			public virtual void run()
			{
				logger.LogWarning("Request received:" + request);
				Assert.Equal(typeof(RequestBody), request.GetType());
				outerInstance.invokeTimes.incrementAndGet();
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
						this.asyncCtx.sendResponse(RequestBody.DEFAULT_CLIENT_RETURN_STR);
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
					this.asyncCtx.sendResponse(RequestBody.DEFAULT_CLIENT_RETURN_STR);
				}
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
	}

}