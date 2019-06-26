using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting;
using System;
using java.util.concurrent.atomic;

namespace com.alipay.remoting.rpc.heartbeat
{
    /// <summary>
    /// CustomHeartBeatProcessor
    /// </summary>
    public class CustomHeartBeatProcessor : AbstractRemotingProcessor
	{
		internal static ILogger logger = NullLogger.Instance;

		private AtomicInteger heartBeatTimes = new AtomicInteger();

		public virtual int HeartBeatTimes
		{
			get
			{
				return heartBeatTimes.get();
			}
		}

		public virtual void reset()
		{
			this.heartBeatTimes.set(0);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void doProcess(com.alipay.remoting.RemotingContext ctx, com.alipay.remoting.RemotingCommand msg) throws Exception
		public override void doProcess(RemotingContext ctx, RemotingCommand msg)
		{
			heartBeatTimes.incrementAndGet();
			logger.LogWarning("heart beat received:" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
		}
	}

}