using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting;
using Remoting.rpc;
using com.alipay.remoting.rpc.common;
using Xunit;
using Remoting.exception;
using System.Net;

namespace com.alipay.remoting.rpc.resrelease
{
    /// <summary>
    /// test server stop and client shutdown
    /// </summary>
    [Collection("Sequential")]
    public class ServerClientStopTest
    {
		private bool InstanceFieldsInitialized = false;

		public ServerClientStopTest()
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
            init();
		}

		private void InitializeInstanceFields()
		{
			addr = "127.0.0.1:" + port;
		}

		internal static ILogger logger = NullLogger.Instance;

		internal BoltServer server;
		internal RpcClient client;

		internal int port = PortScan.select();
		internal IPAddress ip = IPAddress.Parse("127.0.0.1");
		internal string addr;

		internal int invokeTimes = 5;

		internal SimpleServerUserProcessor serverUserProcessor = new SimpleServerUserProcessor();
		internal SimpleClientUserProcessor clientUserProcessor = new SimpleClientUserProcessor();
		internal CONNECTEventProcessor clientConnectProcessor = new CONNECTEventProcessor();
		internal CONNECTEventProcessor serverConnectProcessor = new CONNECTEventProcessor();
		internal DISCONNECTEventProcessor clientDisConnectProcessor = new DISCONNECTEventProcessor();
		internal DISCONNECTEventProcessor serverDisConnectProcessor = new DISCONNECTEventProcessor();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before private void init()
		private void init()
		{
			server = new BoltServer(port, true, true);
			server.start();
			server.addConnectionEventProcessor(ConnectionEventType.CONNECT, serverConnectProcessor);
			server.addConnectionEventProcessor(ConnectionEventType.CLOSE, serverDisConnectProcessor);
			server.registerUserProcessor(serverUserProcessor);

			client = new RpcClient();
			client.addConnectionEventProcessor(ConnectionEventType.CONNECT, clientConnectProcessor);
			client.addConnectionEventProcessor(ConnectionEventType.CLOSE, clientDisConnectProcessor);
			client.registerUserProcessor(clientUserProcessor);
			client.startup();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testRpcServerStop() throws ThreadInterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testRpcServerStop()
		{
			string connNumAddr = addr + "?_CONNECTIONNUM=8&_CONNECTIONWARMUP=true";
			try
			{
				client.getConnection(connNumAddr, 1000);
			}
			catch (RemotingException e)
			{
				logger.LogError("get connection exception!", e);
			}
			server.stop();
		    System.Threading.Thread.Sleep(3000);
			Assert.True(serverConnectProcessor.Connected);
			Assert.Equal(8, serverConnectProcessor.ConnectTimes);
			Assert.True(serverDisConnectProcessor.DisConnected);
			Assert.Equal(8, serverDisConnectProcessor.DisConnectTimes);

			RequestBody req1 = new RequestBody(1, RequestBody.DEFAULT_CLIENT_STR);
			try
			{
				client.invokeSync(connNumAddr, req1, 1000);
				Assert.Null("Should not reach here, server should not be connected now!");
			}
			catch (RemotingException e)
			{
				logger.LogError("invoke sync failed!", e);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testRpcClientShutdown() throws ThreadInterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testRpcClientShutdown()
		{
			string connNumAddr = addr + "?_CONNECTIONNUM=8&_CONNECTIONWARMUP=true";
			try
			{
				client.getConnection(connNumAddr, 1000);
			}
			catch (RemotingException e)
			{
				logger.LogError("get connection exception!", e);
			}
			client.shutdown();
		    System.Threading.Thread.Sleep(1500);
			Assert.True(serverConnectProcessor.Connected);
			Assert.Equal(8, serverConnectProcessor.ConnectTimes);
			Assert.True(serverDisConnectProcessor.DisConnected);
			Assert.Equal(8, serverDisConnectProcessor.DisConnectTimes);
		}
	}

}