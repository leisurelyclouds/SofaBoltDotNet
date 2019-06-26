using Remoting.rpc;
using System;
using com.alipay.remoting.rpc.common;
using Xunit;
using Remoting.exception;
using Remoting.Config;

namespace com.alipay.remoting.rpc.watermark
{
    /// <summary>
    /// water mark init test
    /// </summary>
    [Collection("Sequential")]
    public class WaterMarkInitTest
    {
		private bool InstanceFieldsInitialized = false;

		public WaterMarkInitTest()
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			addr = "127.0.0.1:" + port;
		}

		internal BoltServer server;
		internal RpcClient client;

		internal int port = PortScan.select();
		internal string addr;

        [Fact]
		public virtual void testLowBiggerThanHigh()
		{
			java.lang.System.setProperty(Configs.NETTY_BUFFER_HIGH_WATERMARK, Convert.ToString(1));
			java.lang.System.setProperty(Configs.NETTY_BUFFER_LOW_WATERMARK, Convert.ToString(2));
			try
			{
				server = new BoltServer(port, true);
				server.start();
				Assert.Null("should not reach here");
			}
			catch (InvalidOperationException)
			{
				// expect IllegalStateException
			}

			try
			{
				client = new RpcClient();
				client.startup();
				Assert.Null("should not reach here");
			}
			catch (ArgumentException)
			{
				// expect IllegalStateException
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testLowBiggerThanDefaultHigh() throws ThreadInterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testLowBiggerThanDefaultHigh()
		{
			java.lang.System.setProperty(Configs.NETTY_BUFFER_HIGH_WATERMARK, Convert.ToString(300 * 1024));
			java.lang.System.setProperty(Configs.NETTY_BUFFER_LOW_WATERMARK, Convert.ToString(200 * 1024));
			server = new BoltServer(port, true);
			Assert.True(server.start());

			try
			{
				client = new RpcClient();
				client.startup();
				client.getConnection(addr, 3000);
			}
			catch (ArgumentException)
			{
				Assert.Null("should not reach here");
			}
			catch (RemotingException)
			{
				// not connected, but ok, here only test args init
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testHighSmallerThanDefaultLow() throws ThreadInterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testHighSmallerThanDefaultLow()
		{
			java.lang.System.setProperty(Configs.NETTY_BUFFER_HIGH_WATERMARK, Convert.ToString(3 * 1024));
			java.lang.System.setProperty(Configs.NETTY_BUFFER_LOW_WATERMARK, Convert.ToString(2 * 1024));
			server = new BoltServer(port, true);
			Assert.True(server.start());

			try
			{
				client = new RpcClient();
				client.startup();
				client.getConnection(addr, 3000);
			}
			catch (ArgumentException)
			{
				Assert.Null("should not reach here");
			}
			catch (RemotingException)
			{
				// not connected, but ok, here only test args init
			}
		}
	}
}