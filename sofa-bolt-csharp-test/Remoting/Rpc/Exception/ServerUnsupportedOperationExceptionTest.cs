using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting;
using Remoting.rpc;
using com.alipay.remoting.rpc.common;
using Xunit;
using System;
using System.Net;
using Remoting.exception;

namespace com.alipay.remoting.rpc.exception
{
    /// <summary>
    /// Test rpc server invoke unsuport exception
    /// </summary>
    [Collection("Sequential")]
    public class ServerUnsupportedOperationExceptionTest : IDisposable
    {
        private bool InstanceFieldsInitialized = false;

		public ServerUnsupportedOperationExceptionTest()
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

		internal int invokeTimes = 1;

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
			server = new BoltServer(port, false);
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

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testUnsupportedException() throws ThreadInterruptedException, com.alipay.remoting.exception.RemotingException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testUnsupportedException()
		{
			client.getConnection(addr, 1000);

			RequestBody req = new RequestBody(1, RequestBody.DEFAULT_SERVER_STR);
			string clientres = null;
			for (int i = 0; i < invokeTimes; i++)
			{
				try
				{
					// only when client invoked, the remote address can be get by UserProcessor
					// otherwise, please use ConnectionEventProcessor
					string remoteAddr = serverUserProcessor.RemoteAddr;
					Assert.Null(remoteAddr);
					remoteAddr = serverConnectProcessor.RemoteAddr;
					Assert.NotNull(remoteAddr);
					clientres = (string) server.RpcServer.invokeSync(remoteAddr, req, 1000);
					Assert.Null("Connection removed! Should throw UnsupportedOperationException here.");
				}
				catch (RemotingException e)
				{
					logger.LogError(e.Message);
					Assert.Null("Connection removed! Should throw UnsupportedOperationException here.");
				}
				catch (NotSupportedException e)
				{
					logger.LogError(e.Message);
					Assert.Null(clientres);
				}
			}

			Assert.True(serverConnectProcessor.Connected);
			Assert.Equal(1, serverConnectProcessor.ConnectTimes);
			Assert.Equal(0, serverUserProcessor.InvokeTimes);
			Assert.Equal(0, clientUserProcessor.InvokeTimes);
		}
	}
}