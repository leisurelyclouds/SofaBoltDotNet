using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting;
using Remoting.rpc;
using System;
using System.Net;
using System.Collections.Generic;
using com.alipay.remoting.rpc.common;
using Xunit;
using Remoting.exception;
using java.util.concurrent;
using Remoting.rpc.exception;

namespace com.alipay.remoting.rpc.timeout
{
    /// <summary>
    /// Timeout Test
    /// </summary>
    [Collection("Sequential")]
    public class TimeoutTest: IDisposable
    {
		private bool InstanceFieldsInitialized = false;

		public TimeoutTest()
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
			serverUserProcessor = new SimpleServerUserProcessor(timeout * 2);
		}


		internal static ILogger logger = NullLogger.Instance;

		internal BoltServer server;
		internal RpcClient client;

		internal int port = PortScan.select();
		internal IPAddress ip = IPAddress.Parse("127.0.0.1");
		internal string addr;

		internal int invokeTimes = 5;
		internal int timeout = 250;

		internal SimpleServerUserProcessor serverUserProcessor;
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
        //ORIGINAL LINE: @Test public void testSyncTimeout()
		public virtual void testSyncTimeout()
		{
			RequestBody b1 = new RequestBody(1, "Hello world!");
			object obj = null;
			try
			{
				obj = client.invokeSync(addr, b1, timeout);
				Assert.Null("Should not reach here!");
			}
			catch (InvokeTimeoutException)
			{
				Assert.Null(obj);
			}
			catch (RemotingException e)
			{
				logger.LogError("Other RemotingException but InvokeTimeoutException occurred in sync", e);
				Assert.Null("Should not reach here!");
			}
			catch (ThreadInterruptedException e)
			{
				logger.LogError("ThreadInterruptedException in sync", e);
				Assert.Null("Should not reach here!");
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testSyncOK()
		public virtual void testSyncOK()
		{
			RequestBody b1 = new RequestBody(1, "Hello world!");
			try
			{
				client.invokeSync(addr, b1, timeout + 500);
			}
			catch (InvokeTimeoutException)
			{
				Assert.Null("Should not reach here!");
			}
			catch (RemotingException e)
			{
				logger.LogError("Other RemotingException but InvokeTimeoutException occurred in sync", e);
				Assert.Null("Should not reach here!");
			}
			catch (ThreadInterruptedException e)
			{
				logger.LogError("ThreadInterruptedException in sync", e);
				Assert.Null("Should not reach here!");
			}
		}

		/// <summary>
		/// future try to get result wait a longer time than specified timeout in invokeWithFuture
		/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testFutureWithLongerTime()
		public virtual void testFutureWithLongerTime()
		{
			RequestBody b4 = new RequestBody(4, "Hello world!");
			object obj = null;
			try
			{
				RpcResponseFuture future = client.invokeWithFuture(addr, b4, timeout);
				obj = future.get(timeout + 100);
				Assert.Null("Should not reach here!");
			}
			catch (InvokeTimeoutException)
			{
				Assert.Null(obj);
			}
			catch (RemotingException e)
			{
				logger.LogError("Other RemotingException but InvokeTimeoutException occurred in future", e);
				Assert.Null("Should not reach here!");
			}
			catch (ThreadInterruptedException e)
			{
				logger.LogError("ThreadInterruptedException in sync", e);
				Assert.Null("Should not reach here!");
			}

		}

		/// <summary>
		/// future try to get result wait a shorter time or just the same with specified timeout in invokeWithFuture
		/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testFutureWithShorterOrJustTheSameTime()
		public virtual void testFutureWithShorterOrJustTheSameTime()
		{
			RequestBody b4 = new RequestBody(4, "Hello world!");
			object obj = null;
			try
			{
				RpcResponseFuture future = client.invokeWithFuture(addr, b4, timeout);
				obj = future.get(timeout - 50);
				Assert.Null("Should not reach here!");
			}
			catch (InvokeTimeoutException)
			{
				Assert.Null(obj);
			}
			catch (RemotingException e)
			{
				logger.LogError("Should not catch any exception here", e);
				Assert.Null("Should not reach here!");
			}
			catch (ThreadInterruptedException e)
			{
				logger.LogError("ThreadInterruptedException in sync", e);
				Assert.Null("Should not reach here!");
			}

		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testCallback() throws ThreadInterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testCallback()
		{
			RequestBody b3 = new RequestBody(3, "Hello world!");
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountdownEvent latch = new java.util.concurrent.CountdownEvent(1);
			CountdownEvent latch = new CountdownEvent(1);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<Class> ret = new java.util.ArrayList<Class>();
			IList<Type> ret = new List<Type>();
			try
			{
				client.invokeWithCallback(addr, b3, new InvokeCallbackAnonymousInnerClass(this, latch, ret), timeout);

			}
			catch (RemotingException e)
			{
				logger.LogError("Other RemotingException but InvokeTimeoutException occurred in future", e);
				Assert.Null("Should not reach here!");
			}
			latch.Wait();
			Assert.Equal(typeof(InvokeTimeoutException), ret[0]);
		}

		private class InvokeCallbackAnonymousInnerClass : InvokeCallback
		{
			private readonly TimeoutTest outerInstance;

			private CountdownEvent latch;
			private IList<Type> ret;

			public InvokeCallbackAnonymousInnerClass(TimeoutTest outerInstance, CountdownEvent latch, IList<Type> ret)
			{
				this.outerInstance = outerInstance;
				this.latch = latch;
				this.ret = ret;
			}


			public void onResponse(object result)
			{
				Assert.Null("Should not reach here!");
			}

			public void onException(Exception e)
			{
				//logger.LogError("Process exception in callback.", e);
				ret.Add(e.GetType());
				if (!latch.IsSet)
            {
                latch.Signal();
            }
			}

			public Executor Executor
			{
				get
				{
					return null;
				}
			}

		}
	}

}