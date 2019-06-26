using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting.rpc;
using Xunit;

namespace com.alipay.remoting.rpc
{
    /// <summary>
    /// test rpc server and stop logic
    /// </summary>
    [Collection("Sequential")]
    public class RpcServerTest
    {
		internal static ILogger logger = NullLogger.Instance;


        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public void doTestStartAndStop(bool syncStop)
        {
			// 1. start a rpc server successfully
			RpcServer rpcServer1 = new RpcServer(1111, false, syncStop);
			try
			{
				rpcServer1.startup();
			}
			catch (System.Exception)
			{
				logger.LogWarning("start fail");
				Assert.Null("Should not reach here");
			}

			logger.LogWarning("start success");
			// 2. start a rpc server with the same port number failed
			RpcServer rpcServer2 = new RpcServer(1111, false, syncStop);
			try
			{
				rpcServer2.startup();
				Assert.Null("Should not reach here");
				logger.LogWarning("start success");
			}
			catch (System.Exception)
			{
				logger.LogWarning("start fail");
			}

			// 3. stop the first rpc server successfully
			try
			{
				rpcServer1.shutdown();
			}
			catch (System.InvalidOperationException)
			{
				Assert.Null("Should not reach here");
			}

			// 4. stop the second rpc server failed, for if start failed, stop method will be called automatically
			try
			{
				rpcServer2.shutdown();
				Assert.Null("Should not reach here");
			}
			catch (System.Exception)
			{
				// expect
			}
		}
	}
}