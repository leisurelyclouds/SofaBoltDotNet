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

namespace com.alipay.remoting.rpc.userprocessor.multiinterestprocessor
{
    [Collection("Sequential")]
    public class BasicUsage_MultiInterestUserProcessorTest: IDisposable
    {
		private bool InstanceFieldsInitialized = false;

		public BasicUsage_MultiInterestUserProcessorTest()
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

		internal SimpleServerMultiInterestUserProcessor serverUserProcessor = new SimpleServerMultiInterestUserProcessor();
		internal SimpleClientMultiInterestUserProcessor clientUserProcessor = new SimpleClientMultiInterestUserProcessor();
		internal CONNECTEventProcessor clientConnectProcessor = new CONNECTEventProcessor();
		internal CONNECTEventProcessor serverConnectProcessor = new CONNECTEventProcessor();
		internal DISCONNECTEventProcessor clientDisConnectProcessor = new DISCONNECTEventProcessor();
		internal DISCONNECTEventProcessor serverDisConnectProcessor = new DISCONNECTEventProcessor();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before private void init()
		private void init()
		{
			server = new BoltServer(port, true);
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
        //ORIGINAL LINE: @Test public void testOneway() throws ThreadInterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testOneway()
		{
			MultiInterestBaseRequestBody req = new RequestBodyC1(2, "hello world oneway--c1");
			MultiInterestBaseRequestBody req2 = new RequestBodyC2(3, "hello world oneway--c2");
			for (int i = 0; i < invokeTimes; i++)
			{
				try
				{
					client.oneway(addr, req);
                    Thread.Sleep(100);
				}
				catch (RemotingException e)
				{
					string errMsg = "RemotingException caught in oneway!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
			}
			for (int j = 0; j < invokeTimes; j++)
			{
				try
				{
					client.oneway(addr, req2);
                    Thread.Sleep(100);
				}
				catch (RemotingException e)
				{
					string errMsg = "RemotingException caught in oneway!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
			}
			//System.out.println(serverUserProcessor.getInvokeTimesC1()+" "+serverUserProcessor.getInvokeTimesC2());

			Assert.True(serverConnectProcessor.Connected);
			Assert.Equal(1, serverConnectProcessor.ConnectTimes);
			Assert.Equal(invokeTimes, serverUserProcessor.InvokeTimesC1);
			Assert.Equal(invokeTimes, serverUserProcessor.InvokeTimesC2);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testSync() throws ThreadInterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testSync()
		{
			MultiInterestBaseRequestBody req = new RequestBodyC1(1, "hello world sync--c1");
			MultiInterestBaseRequestBody req2 = new RequestBodyC2(4, "hello world sync--c2");
			for (int i = 0; i < invokeTimes; i++)
			{
				try
				{
					string res = (string) client.invokeSync(addr, req, 3000);
					logger.LogWarning("Result received in sync: " + res);
					Assert.Equal(RequestBodyC1.DEFAULT_SERVER_RETURN_STR, res);
				}
				catch (RemotingException e)
				{
					string errMsg = "RemotingException caught in sync!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
				catch (ThreadInterruptedException e)
				{
					string errMsg = "ThreadInterruptedException caught in sync!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
			}
			for (int i = 0; i < invokeTimes; i++)
			{
				try
				{
					string res = (string) client.invokeSync(addr, req2, 3000);
					logger.LogWarning("Result received in sync: " + res);
					Assert.Equal(RequestBodyC2.DEFAULT_SERVER_RETURN_STR, res);
				}
				catch (RemotingException e)
				{
					string errMsg = "RemotingException caught in sync!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
				catch (ThreadInterruptedException e)
				{
					string errMsg = "ThreadInterruptedException caught in sync!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
			}

			Assert.True(serverConnectProcessor.Connected);
			Assert.Equal(1, serverConnectProcessor.ConnectTimes);
			Assert.Equal(invokeTimes, serverUserProcessor.InvokeTimesC1);
			Assert.Equal(invokeTimes, serverUserProcessor.InvokeTimesC2);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testFuture() throws ThreadInterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testFuture()
		{
			MultiInterestBaseRequestBody req = new RequestBodyC1(2, "hello world future--c1");
			MultiInterestBaseRequestBody req2 = new RequestBodyC2(3, "hello world future--c2");
			for (int i = 0; i < invokeTimes; i++)
			{
				try
				{
					RpcResponseFuture future = client.invokeWithFuture(addr, req, 3000);
					string res = (string) future.get();
					Assert.Equal(RequestBodyC1.DEFAULT_SERVER_RETURN_STR, res);
				}
				catch (RemotingException e)
				{
					string errMsg = "RemotingException caught in future!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
				catch (ThreadInterruptedException e)
				{
					string errMsg = "ThreadInterruptedException caught in future!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
			}
			for (int i = 0; i < invokeTimes; i++)
			{
				try
				{
					RpcResponseFuture future = client.invokeWithFuture(addr, req2, 3000);
					string res = (string) future.get();
					Assert.Equal(RequestBodyC2.DEFAULT_SERVER_RETURN_STR, res);
				}
				catch (RemotingException e)
				{
					string errMsg = "RemotingException caught in future!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
				catch (ThreadInterruptedException e)
				{
					string errMsg = "ThreadInterruptedException caught in future!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
			}

			Assert.True(serverConnectProcessor.Connected);
			Assert.Equal(1, serverConnectProcessor.ConnectTimes);
			Assert.Equal(invokeTimes, serverUserProcessor.InvokeTimesC1);
			Assert.Equal(invokeTimes, serverUserProcessor.InvokeTimesC2);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testCallback() throws ThreadInterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testCallback()
		{
			MultiInterestBaseRequestBody req = new RequestBodyC1(1, "hello world callback--c1");
			MultiInterestBaseRequestBody req2 = new RequestBodyC2(1, "hello world callback--c2");
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
					client.invokeWithCallback(addr, req, new InvokeCallbackAnonymousInnerClass(this, rets, latch), 1000);

				}
				catch (RemotingException e)
				{
					if (!latch.IsSet)
            {
                latch.Signal();
            }
					string errMsg = "RemotingException caught in callback!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
				try
				{
					latch.Wait();
				}
				catch (ThreadInterruptedException e)
				{
					string errMsg = "ThreadInterruptedException caught in callback!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
				if (rets.Count == 0)
				{
					Assert.Null("No result! Maybe exception caught!");
				}
				Assert.Equal(RequestBodyC1.DEFAULT_SERVER_RETURN_STR, rets[0]);
				rets.Clear();
			}
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<String> rets2 = new java.util.ArrayList<String>(1);
			IList<string> rets2 = new List<string>(1);
			for (int i = 0; i < invokeTimes; i++)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountdownEvent latch = new java.util.concurrent.CountdownEvent(1);
				CountdownEvent latch = new CountdownEvent(1);
				try
				{
					client.invokeWithCallback(addr, req2, new InvokeCallbackAnonymousInnerClass2(this, rets2, latch), 1000);

				}
				catch (RemotingException e)
				{
					if (!latch.IsSet)
            {
                latch.Signal();
            }
					string errMsg = "RemotingException caught in callback!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
				try
				{
					latch.Wait();
				}
				catch (ThreadInterruptedException e)
				{
					string errMsg = "ThreadInterruptedException caught in callback!";
					logger.LogError(errMsg, e);
					Assert.Null(errMsg);
				}
				if (rets2.Count == 0)
				{
					Assert.Null("No result! Maybe exception caught!");
				}
				Assert.Equal(RequestBodyC2.DEFAULT_SERVER_RETURN_STR, rets2[0]);
				rets.Clear();
			}

			Assert.True(serverConnectProcessor.Connected);
			Assert.Equal(1, serverConnectProcessor.ConnectTimes);
			Assert.Equal(invokeTimes, serverUserProcessor.InvokeTimesC1);
			Assert.Equal(invokeTimes, serverUserProcessor.InvokeTimesC2);
		}

		private class InvokeCallbackAnonymousInnerClass : InvokeCallback
		{
			private readonly BasicUsage_MultiInterestUserProcessorTest outerInstance;

			private IList<string> rets;
			private CountdownEvent latch;

			public InvokeCallbackAnonymousInnerClass(BasicUsage_MultiInterestUserProcessorTest outerInstance, IList<string> rets, CountdownEvent latch)
			{
				this.outerInstance = outerInstance;
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

		private class InvokeCallbackAnonymousInnerClass2 : InvokeCallback
		{
			private readonly BasicUsage_MultiInterestUserProcessorTest outerInstance;

			private IList<string> rets2;
			private CountdownEvent latch;

			public InvokeCallbackAnonymousInnerClass2(BasicUsage_MultiInterestUserProcessorTest outerInstance, IList<string> rets2, CountdownEvent latch)
			{
				this.outerInstance = outerInstance;
				this.rets2 = rets2;
				this.latch = latch;
				executor = Executors.newCachedThreadPool();
			}

			internal Executor executor;

			public void onResponse(object result)
			{
				logger.LogWarning("Result received in callback: " + result);
				rets2.Add((string) result);
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