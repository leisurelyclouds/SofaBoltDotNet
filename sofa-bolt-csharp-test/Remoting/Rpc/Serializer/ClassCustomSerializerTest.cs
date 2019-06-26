using java.lang;
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
using Remoting.Config;

namespace com.alipay.remoting.rpc.serializer
{
    /// <summary>
    /// Custom Serializer Test: Normal, System.Exception included
    /// </summary>
    [Collection("Sequential")]
    public class ClassCustomSerializerTest: IDisposable
    {
		private bool InstanceFieldsInitialized = false;

		public ClassCustomSerializerTest()
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

		internal int invokeTimes = 5;

		internal SimpleServerUserProcessor serverUserProcessor = new SimpleServerUserProcessor();
		internal SimpleClientUserProcessor clientUserProcessor = new SimpleClientUserProcessor();
		internal CONNECTEventProcessor clientConnectProcessor = new CONNECTEventProcessor();
		internal CONNECTEventProcessor serverConnectProcessor = new CONNECTEventProcessor();
		internal DISCONNECTEventProcessor clientDisConnectProcessor = new DISCONNECTEventProcessor();
		internal DISCONNECTEventProcessor serverDisConnectProcessor = new DISCONNECTEventProcessor();

		internal bool flag1_RequestBody = false;
		internal bool flag2_RequestBody = false;
		internal bool flag1_String = false;
		internal bool flag2_String = false;

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
			CustomSerializerManager.clear();
			server.stop();
			try
			{
			    System.Threading.Thread.Sleep(100);
			}
			catch (ThreadInterruptedException e)
			{
				logger.LogError("Stop server failed!", e);
			}
		}

		/// <summary>
		/// normal test
		/// </summary>
		/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testNormalCustomSerializer() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testNormalCustomSerializer()
		{
			NormalRequestBodyCustomSerializer s1 = new NormalRequestBodyCustomSerializer();
			NormalStringCustomSerializer s2 = new NormalStringCustomSerializer();
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			CustomSerializerManager.registerCustomSerializer(typeof(RequestBody), s1);
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			CustomSerializerManager.registerCustomSerializer(typeof(string), s2);

			RequestBody body = new RequestBody(1, "hello world!");
			string ret = (string) client.invokeSync(addr, body, 1000);
			Assert.Equal(RequestBody.DEFAULT_SERVER_RETURN_STR + "RANDOM", ret);
			Assert.True(s1.Serialized);
			Assert.True(s1.Deserialized);
			Assert.True(s2.Serialized);
			Assert.True(s2.Deserialized);
		}

		/// <summary>
		/// test SerializationException when serial request
		/// </summary>
		/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testRequestSerialException() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testRequestSerialException()
		{
			ExceptionRequestBodyCustomSerializer s1 = new ExceptionRequestBodyCustomSerializer(true, false, false, false);
			NormalStringCustomSerializer s2 = new NormalStringCustomSerializer();
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			CustomSerializerManager.registerCustomSerializer(typeof(RequestBody), s1);
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			CustomSerializerManager.registerCustomSerializer(typeof(string), s2);

			RequestBody body = new RequestBody(1, "hello world!");
			string ret = null;
			try
			{
				ret = (string) client.invokeSync(addr, body, 1000);
				Assert.Null("Should not reach here!");
			}
			catch (SerializationException e)
			{
				logger.LogError("", e);
				Assert.False(e.ServerSide);
				Assert.Null(ret);
				Assert.True(s1.Serialized);
				Assert.False(s1.Deserialized);
				Assert.False(s2.Serialized);
				Assert.False(s2.Deserialized);
			}
			catch (System.Exception)
			{
				Assert.Null("Should not reach here!");
			}
		}

		/// <summary>
		/// test RuntimeException when serial request
		/// </summary>
		/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testRequestSerialRuntimeException() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testRequestSerialRuntimeException()
		{
			ExceptionRequestBodyCustomSerializer s1 = new ExceptionRequestBodyCustomSerializer(false, true, false, false);
			NormalStringCustomSerializer s2 = new NormalStringCustomSerializer();
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			CustomSerializerManager.registerCustomSerializer(typeof(RequestBody), s1);
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			CustomSerializerManager.registerCustomSerializer(typeof(string), s2);

			RequestBody body = new RequestBody(1, "hello world!");
			string ret = null;
			try
			{
				ret = (string) client.invokeSync(addr, body, 1000);
				Assert.Null("Should not reach here!");
			}
			catch (SerializationException e)
			{
				logger.LogError("", e);
				Assert.False(e.ServerSide);
				Assert.Null(ret);
				Assert.True(s1.Serialized);
				Assert.False(s1.Deserialized);
				Assert.False(s2.Serialized);
				Assert.False(s2.Deserialized);
			}
			catch (System.Exception)
			{
				Assert.Null("Should not reach here!");
			}
		}

		/// <summary>
		/// test DeserializationException when deserial request
		/// </summary>
		/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testRequestDeserialException() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testRequestDeserialException()
		{
			java.lang.System.setProperty(Configs.SERIALIZER, Convert.ToString(1));
			ExceptionRequestBodyCustomSerializer s1 = new ExceptionRequestBodyCustomSerializer(false, false, true, false);
			NormalStringCustomSerializer s2 = new NormalStringCustomSerializer();
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			CustomSerializerManager.registerCustomSerializer(typeof(RequestBody), s1);
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			CustomSerializerManager.registerCustomSerializer(typeof(string), s2);

			RequestBody body = new RequestBody(1, "hello world!");
			string ret = null;
			try
			{
				ret = (string) client.invokeSync(addr, body, 1000);
				Assert.Null("Should not reach here!");
			}
			catch (DeserializationException e)
			{
				logger.LogError("", e);
				Assert.True(e.ServerSide);
				Assert.Null(ret);
				Assert.True(s1.Serialized);
				Assert.True(s1.Deserialized);
				Assert.False(s2.Serialized);
				Assert.False(s2.Deserialized);
			}
			catch (System.Exception)
			{
				Assert.Null("Should not reach here!");
			}
		}

		/// <summary>
		/// test RuntimeException when deserial request
		/// </summary>
		/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testRequestDeserialRuntimeException() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testRequestDeserialRuntimeException()
		{
			java.lang.System.setProperty(Configs.SERIALIZER, Convert.ToString(1));
			ExceptionRequestBodyCustomSerializer s1 = new ExceptionRequestBodyCustomSerializer(false, false, false, true);
			NormalStringCustomSerializer s2 = new NormalStringCustomSerializer();
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			CustomSerializerManager.registerCustomSerializer(typeof(RequestBody), s1);
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			CustomSerializerManager.registerCustomSerializer(typeof(string), s2);

			RequestBody body = new RequestBody(1, "hello world!");
			string ret = null;
			try
			{
				ret = (string) client.invokeSync(addr, body, 1000);
				Assert.Null("Should not reach here!");
			}
			catch (DeserializationException e)
			{
				logger.LogError("", e);
				Assert.True(e.ServerSide);
				Assert.Null(ret);
				Assert.True(s1.Serialized);
				Assert.True(s1.Deserialized);
				Assert.False(s2.Serialized);
				Assert.False(s2.Deserialized);
			}
			catch (System.Exception)
			{
				Assert.Null("Should not reach here!");
			}
		}

		/// <summary>
		/// test SerializationException when serial response
		/// </summary>
		/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testResponseSerialException() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testResponseSerialException()
		{
			NormalRequestBodyCustomSerializer s1 = new NormalRequestBodyCustomSerializer();
			ExceptionStringCustomSerializer s2 = new ExceptionStringCustomSerializer(true, false, false, false);
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			CustomSerializerManager.registerCustomSerializer(typeof(RequestBody), s1);
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			CustomSerializerManager.registerCustomSerializer(typeof(string), s2);

			RequestBody body = new RequestBody(1, "hello world!");
			string ret = null;
			try
			{
				ret = (string) client.invokeSync(addr, body, 1000);
				Assert.Null("Should not reach here!");
			}
			catch (SerializationException e)
			{
				logger.LogError("", e);
				Assert.True(e.ServerSide);
				Assert.Null(ret);
				Assert.True(s1.Serialized);
				Assert.True(s1.Deserialized);
				Assert.True(s2.Serialized);
				Assert.False(s2.Deserialized);
			}
			catch (System.Exception)
			{
				Assert.Null("Should not reach here!");
			}
		}

		/// <summary>
		/// test RuntimeException when serial response
		/// </summary>
		/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testResponseSerialRuntimeException() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testResponseSerialRuntimeException()
		{
			NormalRequestBodyCustomSerializer s1 = new NormalRequestBodyCustomSerializer();
			ExceptionStringCustomSerializer s2 = new ExceptionStringCustomSerializer(false, true, false, false);
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			CustomSerializerManager.registerCustomSerializer(typeof(RequestBody), s1);
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			CustomSerializerManager.registerCustomSerializer(typeof(string), s2);

			RequestBody body = new RequestBody(1, "hello world!");
			string ret = null;
			try
			{
				ret = (string) client.invokeSync(addr, body, 1000);
				Assert.Null("Should not reach here!");
			}
			catch (SerializationException e)
			{
				logger.LogError("", e);
				Assert.True(e.ServerSide);
				Assert.Null(ret);
				Assert.True(s1.Serialized);
				Assert.True(s1.Deserialized);
				Assert.True(s2.Serialized);
				Assert.False(s2.Deserialized);
			}
			catch (System.Exception)
			{
				Assert.Null("Should not reach here!");
			}
		}

		/// <summary>
		/// test DeserializationException when deserial response
		/// </summary>
		/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testResponseDeserialzeException() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testResponseDeserialzeException()
		{
			NormalRequestBodyCustomSerializer s1 = new NormalRequestBodyCustomSerializer();
			ExceptionStringCustomSerializer s2 = new ExceptionStringCustomSerializer(false, false, true, false);
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			CustomSerializerManager.registerCustomSerializer(typeof(RequestBody), s1);
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			CustomSerializerManager.registerCustomSerializer(typeof(string), s2);

			RequestBody body = new RequestBody(1, "hello world!");
			string ret = null;
			try
			{
				ret = (string) client.invokeSync(addr, body, 1000);
				Assert.Null("Should not reach here!");
			}
			catch (DeserializationException e)
			{
				logger.LogError("", e);
				Assert.False(e.ServerSide);
				Assert.Null(ret);
				Assert.True(s1.Serialized);
				Assert.True(s1.Deserialized);
				Assert.True(s2.Serialized);
				Assert.True(s2.Deserialized);
			}
			catch (System.Exception)
			{
				Assert.Null("Should not reach here!");
			}
		}

		/// <summary>
		/// test RuntimeException when deserial response
		/// </summary>
		/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testResponseDeserialzeRuntimeException() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testResponseDeserialzeRuntimeException()
		{
			NormalRequestBodyCustomSerializer s1 = new NormalRequestBodyCustomSerializer();
			ExceptionStringCustomSerializer s2 = new ExceptionStringCustomSerializer(false, false, false, true);
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			CustomSerializerManager.registerCustomSerializer(typeof(RequestBody), s1);
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			CustomSerializerManager.registerCustomSerializer(typeof(string), s2);

			RequestBody body = new RequestBody(1, "hello world!");
			string ret = null;
			try
			{
				ret = (string) client.invokeSync(addr, body, 1000);
				Assert.Null("Should not reach here!");
			}
			catch (DeserializationException e)
			{
				logger.LogError("", e);
				Assert.False(e.ServerSide);
				Assert.Null(ret);
				Assert.True(s1.Serialized);
				Assert.True(s1.Deserialized);
				Assert.True(s2.Serialized);
				Assert.True(s2.Deserialized);
			}
			catch (System.Exception)
			{
				Assert.Null("Should not reach here!");
			}
		}

		/// <summary>
		/// test custom serializer using invoke contxt in sync
		/// </summary>
		/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testInvokeContextCustomSerializer_SYNC() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testInvokeContextCustomSerializer_SYNC()
		{
			NormalRequestBodyCustomSerializer_InvokeContext s1 = new NormalRequestBodyCustomSerializer_InvokeContext();
			NormalStringCustomSerializer_InvokeContext s2 = new NormalStringCustomSerializer_InvokeContext();
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			CustomSerializerManager.registerCustomSerializer(typeof(RequestBody), s1);
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			CustomSerializerManager.registerCustomSerializer(typeof(string), s2);

			RequestBody body = new RequestBody(1, "hello world!");
			InvokeContext invokeContext = new InvokeContext();
			invokeContext.putIfAbsent(NormalRequestBodyCustomSerializer_InvokeContext.SERIALTYPE_KEY, NormalRequestBodyCustomSerializer_InvokeContext.SERIALTYPE1_value);
			string ret = (string) client.invokeSync(addr, body, invokeContext, 1000);
			Assert.Equal(RequestBody.DEFAULT_SERVER_RETURN_STR + "RANDOM", ret);
			Assert.True(s1.Serialized);
			Assert.True(s1.Deserialized);

			invokeContext.clear();
			invokeContext.putIfAbsent(NormalRequestBodyCustomSerializer_InvokeContext.SERIALTYPE_KEY, NormalRequestBodyCustomSerializer_InvokeContext.SERIALTYPE2_value);
			ret = (string) client.invokeSync(addr, body, invokeContext, 1000);
			Assert.Equal(NormalStringCustomSerializer_InvokeContext.UNIVERSAL_RESP, ret);
			Assert.True(s1.Serialized);
			Assert.True(s1.Deserialized);
		}

		/// <summary>
		/// test custom serializer using invoke contxt in future
		/// </summary>
		/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testInvokeContextCustomSerializer_FUTURE() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testInvokeContextCustomSerializer_FUTURE()
		{
			NormalRequestBodyCustomSerializer_InvokeContext s1 = new NormalRequestBodyCustomSerializer_InvokeContext();
			NormalStringCustomSerializer_InvokeContext s2 = new NormalStringCustomSerializer_InvokeContext();
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			CustomSerializerManager.registerCustomSerializer(typeof(RequestBody), s1);
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			CustomSerializerManager.registerCustomSerializer(typeof(string), s2);

			RequestBody body = new RequestBody(1, "hello world!");
			InvokeContext invokeContext = new InvokeContext();
			invokeContext.putIfAbsent(NormalRequestBodyCustomSerializer_InvokeContext.SERIALTYPE_KEY, NormalRequestBodyCustomSerializer_InvokeContext.SERIALTYPE1_value);
			RpcResponseFuture future = client.invokeWithFuture(addr, body, invokeContext, 1000);
			string ret = (string) future.get(1000);
			Assert.Equal(RequestBody.DEFAULT_SERVER_RETURN_STR + "RANDOM", ret);
			Assert.True(s1.Serialized);
			Assert.True(s1.Deserialized);

			invokeContext.clear();
			invokeContext.putIfAbsent(NormalRequestBodyCustomSerializer_InvokeContext.SERIALTYPE_KEY, NormalRequestBodyCustomSerializer_InvokeContext.SERIALTYPE2_value);
			future = client.invokeWithFuture(addr, body, invokeContext, 1000);
			ret = (string) future.get(1000);
			Assert.Equal(NormalStringCustomSerializer_InvokeContext.UNIVERSAL_RESP, ret);
			Assert.True(s1.Serialized);
			Assert.True(s1.Deserialized);
		}

		/// <summary>
		/// test custom serializer using invoke contxt in callback
		/// </summary>
		/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testInvokeContextCustomSerializer_CALLBACK() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testInvokeContextCustomSerializer_CALLBACK()
		{
			NormalRequestBodyCustomSerializer_InvokeContext s1 = new NormalRequestBodyCustomSerializer_InvokeContext();
			NormalStringCustomSerializer_InvokeContext s2 = new NormalStringCustomSerializer_InvokeContext();
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			CustomSerializerManager.registerCustomSerializer(typeof(RequestBody), s1);
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			CustomSerializerManager.registerCustomSerializer(typeof(string), s2);

			RequestBody body = new RequestBody(1, "hello world!");
			InvokeContext invokeContext = new InvokeContext();
			invokeContext.putIfAbsent(NormalRequestBodyCustomSerializer_InvokeContext.SERIALTYPE_KEY, NormalRequestBodyCustomSerializer_InvokeContext.SERIALTYPE1_value);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<Object> rets = new java.util.ArrayList<Object>();
			IList<object> rets = new List<object>();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountdownEvent latch = new java.util.concurrent.CountdownEvent(1);
			CountdownEvent latch = new CountdownEvent(1);
			client.invokeWithCallback(addr, body, invokeContext, new InvokeCallbackAnonymousInnerClass(this, rets, latch), 1000);
			latch.Wait();
			string ret = (string) rets[0];
			Assert.Equal(RequestBody.DEFAULT_SERVER_RETURN_STR + "RANDOM", ret);
			Assert.True(s1.Serialized);
			Assert.True(s1.Deserialized);

			invokeContext.clear();
			invokeContext.putIfAbsent(NormalRequestBodyCustomSerializer_InvokeContext.SERIALTYPE_KEY, NormalRequestBodyCustomSerializer_InvokeContext.SERIALTYPE2_value);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountdownEvent latch1 = new java.util.concurrent.CountdownEvent(1);
			CountdownEvent latch1 = new CountdownEvent(1);
			client.invokeWithCallback(addr, body, invokeContext, new InvokeCallbackAnonymousInnerClass2(this, rets, latch1), 1000);
			latch1.Wait();
			ret = (string) rets[0];
			Assert.Equal(NormalStringCustomSerializer_InvokeContext.UNIVERSAL_RESP, ret);
			Assert.True(s1.Serialized);
			Assert.True(s1.Deserialized);
		}

		private class InvokeCallbackAnonymousInnerClass : InvokeCallback
		{
			private readonly ClassCustomSerializerTest outerInstance;

			private IList<object> rets;
			private CountdownEvent latch;

			public InvokeCallbackAnonymousInnerClass(ClassCustomSerializerTest outerInstance, IList<object> rets, CountdownEvent latch)
			{
				this.outerInstance = outerInstance;
				this.rets = rets;
				this.latch = latch;
			}

			public void onResponse(object result)
			{
				rets.Clear();
				rets.Add(result);
				if (!latch.IsSet)
            {
                latch.Signal();
            }
			}

			public void onException(System.Exception e)
			{

			}

			public Executor Executor
			{
				get
				{
					return null;
				}
			}
		}

		private class InvokeCallbackAnonymousInnerClass2 : InvokeCallback
		{
			private readonly ClassCustomSerializerTest outerInstance;

			private IList<object> rets;
			private CountdownEvent latch1;

			public InvokeCallbackAnonymousInnerClass2(ClassCustomSerializerTest outerInstance, IList<object> rets, CountdownEvent latch1)
			{
				this.outerInstance = outerInstance;
				this.rets = rets;
				this.latch1 = latch1;
			}

			public void onResponse(object result)
			{
				rets.Clear();
				rets.Add(result);
				if (!latch1.IsSet)
            {
				latch1.Signal();
			}
			}

			public void onException(System.Exception e)
			{

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