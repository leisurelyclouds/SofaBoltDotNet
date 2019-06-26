using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting;
using Remoting.rpc;
using System.Collections.Generic;
using com.alipay.remoting.rpc.common;
using Xunit;
using Remoting.exception;
using java.util.concurrent;
using System;
using System.Net;
using Remoting.Config;

namespace com.alipay.remoting.rpc.serializer
{
    /// <summary>
    /// Codec Test
    /// Test three type serializer [HESSIAN, JAVA, JSON]
    /// note: json need the class have a default constructor
    /// </summary>
    [Collection("Sequential")]
    public class CodecTest: IDisposable
    {
		private bool InstanceFieldsInitialized = false;

		public CodecTest()
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

		internal ILogger logger = NullLogger.Instance;

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
//ORIGINAL LINE: @Before private void init()
		private void init()
		{
			server = new BoltServer(port);
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
        //ORIGINAL LINE: @Test public void testFuture()
		public virtual void testFuture()
		{
			RequestBody b4 = new RequestBody(4, "hello world future");

			for (int i = 0; i < 3; i++)
			{
				try
				{
					java.lang.System.setProperty(Configs.SERIALIZER, i.ToString());
					RpcResponseFuture future = client.invokeWithFuture(addr, b4, 1000);
					object obj = future.get(1500);
					Assert.Equal(RequestBody.DEFAULT_SERVER_RETURN_STR, obj);
					logger.LogWarning("Result received in future:" + obj);
				}
				catch (CodecException e)
				{
					string errMsg = "Codec System.Exception caught in callback!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
				catch (RemotingException e)
				{
					string errMsg = "Remoting System.Exception caught in callback!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
				catch (ThreadInterruptedException e)
				{
					string errMsg = "Interrupted System.Exception caught in callback!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testCallback() throws ThreadInterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testCallback()
		{
			RequestBody b3 = new RequestBody(3, "hello world callback");
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<String> rets = new java.util.ArrayList<String>(1);
			IList<string> rets = new List<string>(1);
			for (int i = 0; i < 3; i++)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountdownEvent latch = new java.util.concurrent.CountdownEvent(1);
				CountdownEvent latch = new CountdownEvent(1);
				try
				{
					java.lang.System.setProperty(Configs.SERIALIZER, i.ToString());
					client.invokeWithCallback(addr, b3, new InvokeCallbackAnonymousInnerClass(this, rets, latch), 1000);

				}
				catch (CodecException e)
				{
					string errMsg = "Codec System.Exception caught in callback!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
				catch (RemotingException e)
				{
					string errMsg = "Remoting System.Exception caught in callback!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
				catch (ThreadInterruptedException e)
				{
					string errMsg = "Interrupted System.Exception caught in callback!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
				latch.Wait();
				Assert.Equal(RequestBody.DEFAULT_SERVER_RETURN_STR, rets[0]);
				rets.Clear();
			}
		}

		private class InvokeCallbackAnonymousInnerClass : InvokeCallback
		{
			private readonly CodecTest outerInstance;

			private IList<string> rets;
			private CountdownEvent latch;

			public InvokeCallbackAnonymousInnerClass(CodecTest outerInstance, IList<string> rets, CountdownEvent latch)
			{
				this.outerInstance = outerInstance;
				this.rets = rets;
				this.latch = latch;
			}


			public void onResponse(object result)
			{
				outerInstance.logger.LogWarning("Result received in callback: " + result);
				rets.Add((string) result);
				if (!latch.IsSet)
            {
                latch.Signal();
            }
			}

			public void onException(Exception e)
			{
				outerInstance.logger.LogError("Process exception in callback.", e);
			}

			public Executor Executor
			{
				get
				{
					return null;
				}
			}

		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testOneway()
		public virtual void testOneway()
		{
			RequestBody b2 = new RequestBody(2, "hello world oneway");

			for (int i = 0; i < 3; i++)
			{
				try
				{
					java.lang.System.setProperty(Configs.SERIALIZER, i.ToString());
					client.oneway(addr, b2);
				}
				catch (CodecException e)
				{
					string errMsg = "Codec System.Exception caught in callback!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
				catch (RemotingException e)
				{
					string errMsg = "Remoting System.Exception caught in callback!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
				catch (ThreadInterruptedException e)
				{
					string errMsg = "Interrupted System.Exception caught in callback!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testSync()
		public virtual void testSync()
		{
			RequestBody b1 = new RequestBody(1, "hello world sync");
			for (int i = 0; i < 3; i++)
			{
				try
				{
					java.lang.System.setProperty(Configs.SERIALIZER, i.ToString());
					string ret = (string) client.invokeSync(addr, b1, 3000);
					logger.LogWarning("Result received in sync: " + ret);
					Assert.Equal(RequestBody.DEFAULT_SERVER_RETURN_STR, ret);
				}
				catch (CodecException e)
				{
					string errMsg = "Codec System.Exception caught in callback!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
				catch (RemotingException e)
				{
					string errMsg = "Remoting System.Exception caught in callback!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
				catch (ThreadInterruptedException e)
				{
					string errMsg = "Interrupted System.Exception caught in callback!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
			}
		}
	}

}