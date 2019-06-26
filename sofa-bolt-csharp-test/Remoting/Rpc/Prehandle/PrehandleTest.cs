using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting;
using Remoting.rpc;
using System.Collections.Generic;
using com.alipay.remoting.rpc.common;
using Xunit;
using Remoting.exception;
using System;
using System.Net;
using java.util.concurrent;

namespace com.alipay.remoting.rpc.prehandle
{
    /// <summary>
    /// test pre handle usage of UserProcessor
    /// </summary>
    [Collection("Sequential")]
    public class PrehandleTest: IDisposable
    {
		private bool InstanceFieldsInitialized = false;

		public PrehandleTest()
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

		internal PreHandleUserProcessor serverUserProcessor = new PreHandleUserProcessor();
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
        //ORIGINAL LINE: @Test public void testSyncPreHandle()
		public virtual void testSyncPreHandle()
		{
			RequestBody b1 = new RequestBody(1, "hello world sync");
			for (int i = 0; i < invokeTimes; i++)
			{
				try
				{
					object ret = client.invokeSync(addr, b1, 3000);
					logger.LogWarning("Result received in sync: " + ret);
					Assert.Equal("test", ret);
				}
				catch (RemotingException e)
				{
					string errMsg = "RemotingException caught in testSyncPreHandle!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
				catch (ThreadInterruptedException e)
				{
					string errMsg = "ThreadInterruptedException caught in testSyncPreHandle!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testOnewayPreHandle()
		public virtual void testOnewayPreHandle()
		{
			RequestBody b1 = new RequestBody(1, "hello world sync");
			for (int i = 0; i < invokeTimes; i++)
			{
				try
				{
					client.oneway(addr, b1);
                    Thread.Sleep(100);
				}
				catch (RemotingException e)
				{
					string errMsg = "RemotingException caught in testOnewayPreHandle!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
				catch (ThreadInterruptedException e)
				{
					string errMsg = "ThreadInterruptedException caught in testOnewayPreHandle!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testFuturePreHandle()
		public virtual void testFuturePreHandle()
		{
			RequestBody b1 = new RequestBody(1, "hello world sync");
			for (int i = 0; i < invokeTimes; i++)
			{
				try
				{
					RpcResponseFuture future = client.invokeWithFuture(addr, b1, 3000);
					object ret = future.get();
					logger.LogWarning("Result received in sync: " + ret);
					Assert.Equal("test", ret);
				}
				catch (RemotingException e)
				{
					string errMsg = "RemotingException caught in testFuturePreHandle!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
				catch (ThreadInterruptedException e)
				{
					string errMsg = "ThreadInterruptedException caught in testFuturePreHandle!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testCallbackPreHandle()
		public virtual void testCallbackPreHandle()
		{
			RequestBody req = new RequestBody(1, "hello world callback");
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<String> rets = new java.util.ArrayList<String>(1);
			IList<string> rets = new List<string>(1);
			for (int i = 0; i < invokeTimes; i++)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountdownEvent latch = new java.util.concurrent.CountdownEvent(1);
				CountdownEvent latch = new CountdownEvent(1);
				try
				{
					client.invokeWithCallback(addr, req, new InvokeCallbackAnonymousInnerClass(rets, latch), 1000);
				}
				catch (RemotingException e)
				{
					if (!latch.IsSet)
            {
                latch.Signal();
            }
					string errMsg = "RemotingException caught in testCallbackPreHandle!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
				catch (ThreadInterruptedException e)
				{
					if (!latch.IsSet)
            {
                latch.Signal();
            }
					string errMsg = "ThreadInterruptedException caught in testCallbackPreHandle!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
				try
				{
					latch.Wait();
				}
				catch (ThreadInterruptedException e)
				{
					string errMsg = "ThreadInterruptedException caught in testCallbackPreHandle!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
				if (rets.Count == 0)
				{
					Assert.Null("No result! Maybe exception caught!");
				}
				logger.LogWarning("Result received in sync: " + rets[0]);
				Assert.Equal("test", rets[0]);
				rets.Clear();
			}
		}

		private class InvokeCallbackAnonymousInnerClass : InvokeCallback
		{
			private IList<string> rets;
			private CountdownEvent latch;

			public InvokeCallbackAnonymousInnerClass(IList<string> rets, CountdownEvent latch)
			{
				this.rets = rets;
				this.latch = latch;
				executor = Executors.newCachedThreadPool();
			}

			internal Executor executor;

			public void onResponse(object result)
			{
				logger.LogWarning("Result received in callback: " + result);
				rets.Add((string) result);
				if (!latch.IsSet)
            {
                latch.Signal();
            }
			}

			public void onException(Exception e)
			{
				logger.LogError("Process exception in callback.", e);
				if (!latch.IsSet)
            {
                latch.Signal();
            }
			}

			public Executor Executor
			{
				get
				{
					return executor;
				}
			}

		}
	}

}