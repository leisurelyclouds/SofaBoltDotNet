using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting;
using Remoting.rpc;
using System.Collections.Generic;
using com.alipay.remoting.rpc.common;
using Xunit;
using System;
using System.Net;
using Remoting.exception;

namespace com.alipay.remoting.rpc.watermark
{
    /// <summary>
    /// water mark exception test, set a small buffer mark by system property, and trigger write over flow.
    /// </summary>
    [Collection("Sequential")]
    public class WaterMark_UserProperty_ExceptionTest: IDisposable
    {
		private bool InstanceFieldsInitialized = false;

		public WaterMark_UserProperty_ExceptionTest()
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

		internal int invokeTimes = 10;

		internal SimpleServerUserProcessor serverUserProcessor = new SimpleServerUserProcessor(0, 20, 20, 60, 100);
		internal SimpleClientUserProcessor clientUserProcessor = new SimpleClientUserProcessor(0, 20, 20, 60, 100);
		internal CONNECTEventProcessor clientConnectProcessor = new CONNECTEventProcessor();
		internal CONNECTEventProcessor serverConnectProcessor = new CONNECTEventProcessor();
		internal DISCONNECTEventProcessor clientDisConnectProcessor = new DISCONNECTEventProcessor();
		internal DISCONNECTEventProcessor serverDisConnectProcessor = new DISCONNECTEventProcessor();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before private void init()
		private void init()
		{
			server = new BoltServer(port, true);
			server.RpcServer.initWriteBufferWaterMark(1, 2);
			server.start();
			server.addConnectionEventProcessor(ConnectionEventType.CONNECT, serverConnectProcessor);
			server.addConnectionEventProcessor(ConnectionEventType.CLOSE, serverDisConnectProcessor);
			server.registerUserProcessor(serverUserProcessor);

			client = new RpcClient();
			client.addConnectionEventProcessor(ConnectionEventType.CONNECT, clientConnectProcessor);
			client.addConnectionEventProcessor(ConnectionEventType.CLOSE, clientDisConnectProcessor);
			client.registerUserProcessor(clientUserProcessor);
			client.initWriteBufferWaterMark(1, 2);
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
        //ORIGINAL LINE: @Test public void testSync() throws ThreadInterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testSync()
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final com.alipay.remoting.rpc.common.RequestBody req = new com.alipay.remoting.rpc.common.RequestBody(1, 1024);
			RequestBody req = new RequestBody(1, 1024);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<Nullable<bool>> overFlow = new java.util.ArrayList<Nullable<bool>>();
			IList<bool> overFlow = new List<bool>();
			for (int i = 0; i < invokeTimes; i++)
			{
				new ThreadAnonymousInnerClass(this, req, overFlow, i)
				.start();
			}

            Thread.Sleep(3000);

			if (overFlow.Count > 0 && overFlow[0])
			{
				Assert.True(serverConnectProcessor.Connected);
				Assert.Equal(1, serverConnectProcessor.ConnectTimes);
				Assert.True(invokeTimes * invokeTimes > serverUserProcessor.InvokeTimes);
			}
			else
			{
				Assert.Null("Should not reach here");
			}
		}

		private class ThreadAnonymousInnerClass : java.lang.Thread
        {
			private readonly WaterMark_UserProperty_ExceptionTest outerInstance;

			private RequestBody req;
			private IList<bool> overFlow;
			private int i;

			public ThreadAnonymousInnerClass(WaterMark_UserProperty_ExceptionTest outerInstance, RequestBody req, IList<bool> overFlow, int i)
			{
				this.outerInstance = outerInstance;
				this.req = req;
				this.overFlow = overFlow;
				this.i = i;
			}

			public override void run()
			{
				string res = null;
				try
				{
					for (int i = 0; i < outerInstance.invokeTimes; i++)
					{
						res = (string) outerInstance.client.invokeSync(outerInstance.addr, req, 3000);
					}
				}
				catch (RemotingException e)
				{
					if (e.Message.Contains("overflow"))
					{
						logger.LogError("overflow exception!");
						overFlow.Add(true);
					}
				}
				catch (ThreadInterruptedException e)
				{
					string errMsg = "ThreadInterruptedException caught in sync!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
				logger.LogWarning("Result received in sync: " + res);
				Assert.Equal(RequestBody.DEFAULT_SERVER_RETURN_STR, res);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testServerSyncUsingConnection() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testServerSyncUsingConnection()
		{
			Connection clientConn = client.createStandaloneConnection(ip, port, 1000);

			RequestBody req1 = new RequestBody(1, RequestBody.DEFAULT_CLIENT_STR);
			string serverres = (string) client.invokeSync(clientConn, req1, 1000);
			Assert.Equal(serverres, RequestBody.DEFAULT_SERVER_RETURN_STR);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String remoteAddr = serverUserProcessor.getRemoteAddr();
			string remoteAddr = serverUserProcessor.RemoteAddr;
			Assert.NotNull(remoteAddr);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<Nullable<bool>> overFlow = new java.util.ArrayList<Nullable<bool>>();
			IList<bool> overFlow = new List<bool>();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final com.alipay.remoting.rpc.common.RequestBody req = new com.alipay.remoting.rpc.common.RequestBody(1, 1024);
			RequestBody req = new RequestBody(1, 1024);
			for (int i = 0; i < invokeTimes; i++)
			{
				new ThreadAnonymousInnerClass2(this, remoteAddr, overFlow, req, i)
				.start();
			}

            Thread.Sleep(3000);

			if (overFlow.Count > 0 && overFlow[0])
			{
				Assert.True(serverConnectProcessor.Connected);
				Assert.Equal(1, serverConnectProcessor.ConnectTimes);
				Assert.True(invokeTimes * invokeTimes > clientUserProcessor.InvokeTimes);
			}
			else
			{
				Assert.Null("Should not reach here");
			}
		}

		private class ThreadAnonymousInnerClass2 : java.lang.Thread
        {
			private readonly WaterMark_UserProperty_ExceptionTest outerInstance;

			private string remoteAddr;
			private IList<bool> overFlow;
			private RequestBody req;
			private int i;

			public ThreadAnonymousInnerClass2(WaterMark_UserProperty_ExceptionTest outerInstance, string remoteAddr, IList<bool> overFlow, RequestBody req, int i)
			{
				this.outerInstance = outerInstance;
				this.remoteAddr = remoteAddr;
				this.overFlow = overFlow;
				this.req = req;
				this.i = i;
			}

			public override void run()
			{
				try
				{
					for (int i = 0; i < outerInstance.invokeTimes; i++)
					{
						string clientres = (string) outerInstance.server.RpcServer.invokeSync(remoteAddr, req, 1000);
						Assert.Equal(clientres, RequestBody.DEFAULT_CLIENT_RETURN_STR);
					}
				}
				catch (RemotingException e)
				{
					if (e.Message.Contains("overflow"))
					{
						logger.LogError("overflow exception!");
						overFlow.Add(true);
					}
				}
				catch (ThreadInterruptedException e)
				{
					string errMsg = "ThreadInterruptedException caught in sync!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}

			}
		}
	}
}