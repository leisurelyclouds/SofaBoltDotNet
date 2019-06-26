using java.util.concurrent;
using Remoting;
using Remoting.rpc.protocol;
using System;
using Xunit;

namespace com.alipay.remoting.rpc.userprocessor.executorselector
{
    /// <summary>
    /// Default Executor Selector
    /// </summary>
    public class DefaultExecutorSelector : UserProcessor_ExecutorSelector
    {
		public const string EXECUTOR0 = "executor0";
		public const string EXECUTOR1 = "executor1";
		private string chooseExecutorStr;
		/// <summary>
		/// executor
		/// </summary>
		private ThreadPoolExecutor executor0;
		private ThreadPoolExecutor executor1;

		public DefaultExecutorSelector(string chooseExecutorStr)
		{
			this.chooseExecutorStr = chooseExecutorStr;
			this.executor0 = new ThreadPoolExecutor(1, 3, 60, TimeUnit.SECONDS, new ArrayBlockingQueue(4), new NamedThreadFactory("Rpc-specific0-executor"));
			this.executor1 = new ThreadPoolExecutor(1, 3, 60, TimeUnit.SECONDS, new ArrayBlockingQueue(4), new NamedThreadFactory("Rpc-specific1-executor"));
		}

		public Executor select(Type requestClass, object requestHeader)
		{
			Assert.NotNull(requestClass);
			Assert.NotNull(requestHeader);
			if (string.Equals(chooseExecutorStr, (string) requestHeader))
			{
				return executor1;
			}
			else
			{
				return executor0;
			}
		}
	}
}