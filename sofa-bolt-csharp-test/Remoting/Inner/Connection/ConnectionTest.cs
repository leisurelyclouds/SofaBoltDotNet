using com.alipay.remoting.rpc.common;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting;
using Remoting.rpc;
using Xunit;
using System;
using System.Net;

namespace com.alipay.remoting.inner.connection
{
    /// <summary>
    /// Connection test
    /// </summary>
    [Collection("Sequential")]
    public class ConnectionTest: IDisposable
    {
		private bool InstanceFieldsInitialized = false;

		public ConnectionTest()
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

		internal SimpleServerUserProcessor serverUserProcessor = new SimpleServerUserProcessor();
		internal SimpleClientUserProcessor clientUserProcessor = new SimpleClientUserProcessor();
		internal CONNECTEventProcessor clientConnectProcessor = new CONNECTEventProcessor();
		internal CONNECTEventProcessor serverConnectProcessor = new CONNECTEventProcessor();
		internal DISCONNECTEventProcessor clientDisConnectProcessor = new DISCONNECTEventProcessor();
		internal DISCONNECTEventProcessor serverDisConnectProcessor = new DISCONNECTEventProcessor();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before private void init() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		private void init()
		{
			server = new BoltServer(port);
			server.start();
			server.addConnectionEventProcessor(ConnectionEventType.CONNECT, serverConnectProcessor);
			server.addConnectionEventProcessor(ConnectionEventType.CLOSE, serverDisConnectProcessor);
			server.registerUserProcessor(serverUserProcessor); // no use here

			client = new RpcClient();
			client.addConnectionEventProcessor(ConnectionEventType.CONNECT, clientConnectProcessor);
			client.addConnectionEventProcessor(ConnectionEventType.CLOSE, clientDisConnectProcessor);
			client.registerUserProcessor(clientUserProcessor); // no use here
			client.startup();
            Thread.Sleep(100);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void stop()
		public void Dispose()
		{
			try
			{
				server.stop();
                Thread.Sleep(100);
			}
			catch (ThreadInterruptedException e)
			{
				logger.LogError("Stop server failed!", e);
			}
		}

        [Fact]
		public virtual void connectionTest()
        {
			Connection conn = client.createStandaloneConnection(ip, port, 1000);
            Thread.Sleep(100);
			Assert.True(conn.Fine);
			Assert.True(serverConnectProcessor.Connected);
			Assert.Equal(1, clientConnectProcessor.ConnectTimes);
			Assert.Equal(1, serverConnectProcessor.ConnectTimes);
			client.closeStandaloneConnection(conn);
            Thread.Sleep(100);
			Assert.True(!conn.Fine);
			Assert.True(serverDisConnectProcessor.DisConnected);
			Assert.Equal(1, clientDisConnectProcessor.DisConnectTimes);
			Assert.Equal(1, serverDisConnectProcessor.DisConnectTimes);

			conn = client.createStandaloneConnection(ip, port, 1000);
            Thread.Sleep(100);
			Assert.True(conn.Fine);
			Assert.Equal(2, clientConnectProcessor.ConnectTimes);
			Assert.Equal(2, serverConnectProcessor.ConnectTimes);
			client.closeStandaloneConnection(conn);
            Thread.Sleep(100);
			Assert.True(!conn.Fine);
			Assert.Equal(2, clientDisConnectProcessor.DisConnectTimes);
			Assert.Equal(2, serverDisConnectProcessor.DisConnectTimes);

			conn = client.createStandaloneConnection(ip, port, 1000);
            Thread.Sleep(100);
			Assert.True(conn.Fine);
			Assert.Equal(3, clientConnectProcessor.ConnectTimes);
			Assert.Equal(3, serverConnectProcessor.ConnectTimes);
			client.closeStandaloneConnection(conn);
            Thread.Sleep(100);
			Assert.True(!conn.Fine);
			Assert.Equal(3, clientDisConnectProcessor.DisConnectTimes);
			Assert.Equal(3, serverDisConnectProcessor.DisConnectTimes);
            Thread.Sleep(100);
			Assert.Equal(3, serverConnectProcessor.ConnectTimes);
		}
	}

}