﻿using System.Threading;
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

namespace com.alipay.remoting.rpc.serializer
{
    /// <summary>
    /// test custom serializer codec
    /// </summary>
    [Collection("Sequential")]
    public class CustomSerializerCodecTest : IDisposable
    {
        private bool InstanceFieldsInitialized = false;

        public CustomSerializerCodecTest()
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
            CustomSerializerManager.clear();
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
            NormalRequestBodyCustomSerializer s1 = new NormalRequestBodyCustomSerializer();
            NormalStringCustomSerializer s2 = new NormalStringCustomSerializer();
            //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            CustomSerializerManager.registerCustomSerializer(typeof(RequestBody), s1);
            //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            CustomSerializerManager.registerCustomSerializer(typeof(string), s2);

            RequestBody req = new RequestBody(2, "hello world oneway");
            for (int i = 0; i < invokeTimes; i++)
            {
                try
                {
                    byte testCodec = (byte)i;
                    InvokeContext invokeContext = new InvokeContext();
                    invokeContext.put(InvokeContext.BOLT_CUSTOM_SERIALIZER, testCodec);
                    client.oneway(addr, req, invokeContext);

                    Assert.Equal(testCodec, s1.ContentSerializer);
                    Assert.Equal(255, s2.ContentSerializer);
                    Thread.Sleep(100);
                }
                catch (RemotingException e)
                {
                    string errMsg = "RemotingException caught in oneway!";
                    logger.LogError(errMsg, e);
                    Assert.Null(errMsg);
                }
            }

            Assert.True(serverConnectProcessor.Connected);
            Assert.Equal(1, serverConnectProcessor.ConnectTimes);
            Assert.Equal(invokeTimes, serverUserProcessor.InvokeTimes);
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testSync() throws ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        public virtual void testSync()
        {
            NormalRequestBodyCustomSerializer s1 = new NormalRequestBodyCustomSerializer();
            NormalStringCustomSerializer s2 = new NormalStringCustomSerializer();
            //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            CustomSerializerManager.registerCustomSerializer(typeof(RequestBody), s1);
            //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            CustomSerializerManager.registerCustomSerializer(typeof(string), s2);

            RequestBody req = new RequestBody(1, "hello world sync");
            for (int i = 0; i < invokeTimes; i++)
            {
                try
                {
                    byte testCodec = (byte)i;
                    InvokeContext invokeContext = new InvokeContext();
                    invokeContext.put(InvokeContext.BOLT_CUSTOM_SERIALIZER, testCodec);
                    string res = (string)client.invokeSync(addr, req, invokeContext, 3000);
                    logger.LogWarning("Result received in sync: " + res);
                    Assert.Equal(RequestBody.DEFAULT_SERVER_RETURN_STR + "RANDOM", res);

                    Assert.Equal(testCodec, s1.ContentSerializer);
                    Assert.Equal(testCodec, s2.ContentSerializer);
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
            Assert.Equal(invokeTimes, serverUserProcessor.InvokeTimes);
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testFuture() throws ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        public virtual void testFuture()
        {
            NormalRequestBodyCustomSerializer s1 = new NormalRequestBodyCustomSerializer();
            NormalStringCustomSerializer s2 = new NormalStringCustomSerializer();
            //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            CustomSerializerManager.registerCustomSerializer(typeof(RequestBody), s1);
            //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            CustomSerializerManager.registerCustomSerializer(typeof(string), s2);

            RequestBody req = new RequestBody(2, "hello world future");
            for (int i = 0; i < invokeTimes; i++)
            {
                try
                {
                    byte testCodec = (byte)i;
                    InvokeContext invokeContext = new InvokeContext();
                    invokeContext.put(InvokeContext.BOLT_CUSTOM_SERIALIZER, testCodec);

                    RpcResponseFuture future = client.invokeWithFuture(addr, req, invokeContext, 3000);
                    string res = (string)future.get();
                    Assert.Equal(RequestBody.DEFAULT_SERVER_RETURN_STR + "RANDOM", res);

                    Assert.Equal(testCodec, s1.ContentSerializer);
                    Assert.Equal(testCodec, s2.ContentSerializer);
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
            Assert.Equal(invokeTimes, serverUserProcessor.InvokeTimes);
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testCallback() throws ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        public virtual void testCallback()
        {
            NormalRequestBodyCustomSerializer s1 = new NormalRequestBodyCustomSerializer();
            NormalStringCustomSerializer s2 = new NormalStringCustomSerializer();
            //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            CustomSerializerManager.registerCustomSerializer(typeof(RequestBody), s1);
            //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            CustomSerializerManager.registerCustomSerializer(typeof(string), s2);

            RequestBody req = new RequestBody(1, "hello world callback");
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final java.util.List<String> rets = new java.util.ArrayList<String>(1);
            IList<string> rets = new List<string>(1);
            for (int i = 0; i < invokeTimes; i++)
            {
                //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                //ORIGINAL LINE: final java.util.concurrent.CountdownEvent latch = new java.util.concurrent.CountdownEvent(1);
                CountdownEvent latch = new CountdownEvent(1);

                byte testCodec = (byte)i;
                InvokeContext invokeContext = new InvokeContext();
                invokeContext.put(InvokeContext.BOLT_CUSTOM_SERIALIZER, testCodec);

                try
                {
                    client.invokeWithCallback(addr, req, invokeContext, new InvokeCallbackAnonymousInnerClass(this, rets, latch), 1000);

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
                Assert.Equal(RequestBody.DEFAULT_SERVER_RETURN_STR + "RANDOM", rets[0]);
                rets.Clear();

                Assert.Equal(testCodec, s1.ContentSerializer);
                Assert.Equal(testCodec, s2.ContentSerializer);
            }

            Assert.True(serverConnectProcessor.Connected);
            Assert.Equal(1, serverConnectProcessor.ConnectTimes);
            Assert.Equal(invokeTimes, serverUserProcessor.InvokeTimes);
        }

        private class InvokeCallbackAnonymousInnerClass : InvokeCallback
        {
            private readonly CustomSerializerCodecTest outerInstance;

            private IList<string> rets;
            private CountdownEvent latch;

            public InvokeCallbackAnonymousInnerClass(CustomSerializerCodecTest outerInstance, IList<string> rets, CountdownEvent latch)
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
                rets.Add((string)result);
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

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testServerSyncUsingConnection() throws Exception
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        public virtual void testServerSyncUsingConnection()
        {
            NormalRequestBodyCustomSerializer s1 = new NormalRequestBodyCustomSerializer();
            NormalStringCustomSerializer s2 = new NormalStringCustomSerializer();
            //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            CustomSerializerManager.registerCustomSerializer(typeof(RequestBody), s1);
            //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            CustomSerializerManager.registerCustomSerializer(typeof(string), s2);

            Connection clientConn = client.createStandaloneConnection(ip, port, 1000);

            for (int i = 0; i < invokeTimes; i++)
            {
                byte testCodec = (byte)i;
                InvokeContext invokeContext = new InvokeContext();
                invokeContext.put(InvokeContext.BOLT_CUSTOM_SERIALIZER, testCodec);

                RequestBody req1 = new RequestBody(1, RequestBody.DEFAULT_CLIENT_STR);
                string serverres = (string)client.invokeSync(clientConn, req1, invokeContext, 1000);
                Assert.Equal(serverres, RequestBody.DEFAULT_SERVER_RETURN_STR + "RANDOM");

                Assert.Equal(testCodec, s1.ContentSerializer);
                Assert.Equal(testCodec, s2.ContentSerializer);

                Assert.NotNull(serverConnectProcessor.Connection);
                Connection serverConn = serverConnectProcessor.Connection;

                invokeContext.clear();
                invokeContext.put(InvokeContext.BOLT_CUSTOM_SERIALIZER, (byte)(testCodec + 1));
                s1.reset();
                s2.reset();

                RequestBody req = new RequestBody(1, RequestBody.DEFAULT_SERVER_STR);
                string clientres = (string)server.RpcServer.invokeSync(serverConn, req, invokeContext, 1000);
                Assert.Equal(clientres, RequestBody.DEFAULT_CLIENT_RETURN_STR + "RANDOM");

                Assert.Equal(testCodec + 1, s1.ContentSerializer);
                Assert.Equal(testCodec + 1, s2.ContentSerializer);
            }

            Assert.True(serverConnectProcessor.Connected);
            Assert.Equal(1, serverConnectProcessor.ConnectTimes);
            Assert.Equal(invokeTimes, serverUserProcessor.InvokeTimes);
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testServerSyncUsingAddress() throws Exception
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        public virtual void testServerSyncUsingAddress()
        {
            NormalRequestBodyCustomSerializer s1 = new NormalRequestBodyCustomSerializer();
            NormalStringCustomSerializer s2 = new NormalStringCustomSerializer();
            //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            CustomSerializerManager.registerCustomSerializer(typeof(RequestBody), s1);
            //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            CustomSerializerManager.registerCustomSerializer(typeof(string), s2);

            Connection clientConn = client.createStandaloneConnection(ip, port, 1000);
            string remote = clientConn.Channel.RemoteAddress.ToString();
            string local = clientConn.Channel.LocalAddress.ToString();
            logger.LogWarning("Client say local:" + local);
            logger.LogWarning("Client say remote:" + remote);

            for (int i = 0; i < invokeTimes; i++)
            {
                byte testCodec = (byte)i;
                InvokeContext invokeContext = new InvokeContext();
                invokeContext.put(InvokeContext.BOLT_CUSTOM_SERIALIZER, testCodec);

                RequestBody req1 = new RequestBody(1, RequestBody.DEFAULT_CLIENT_STR);
                string serverres = (string)client.invokeSync(clientConn, req1, invokeContext, 1000);
                Assert.Equal(serverres, RequestBody.DEFAULT_SERVER_RETURN_STR + "RANDOM");

                Assert.Equal(testCodec, s1.ContentSerializer);
                Assert.Equal(testCodec, s2.ContentSerializer);

                invokeContext.clear();
                invokeContext.put(InvokeContext.BOLT_CUSTOM_SERIALIZER, (byte)(testCodec + 1));
                s1.reset();
                s2.reset();

                Assert.NotNull(serverConnectProcessor.Connection);
                // only when client invoked, the remote address can be get by UserProcessor
                // otherwise, please use ConnectionEventProcessor
                string remoteAddr = serverUserProcessor.RemoteAddr;
                RequestBody req = new RequestBody(1, RequestBody.DEFAULT_SERVER_STR);
                string clientres = (string)server.RpcServer.invokeSync(remoteAddr, req, invokeContext, 1000);
                Assert.Equal(clientres, RequestBody.DEFAULT_CLIENT_RETURN_STR + "RANDOM");

                Assert.Equal(testCodec + 1, s1.ContentSerializer);
                Assert.Equal(testCodec + 1, s2.ContentSerializer);
            }

            Assert.True(serverConnectProcessor.Connected);
            Assert.Equal(1, serverConnectProcessor.ConnectTimes);
            Assert.Equal(invokeTimes, serverUserProcessor.InvokeTimes);
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testIllegalType() throws Exception
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        public virtual void testIllegalType()
        {
            NormalRequestBodyCustomSerializer s1 = new NormalRequestBodyCustomSerializer();
            NormalStringCustomSerializer s2 = new NormalStringCustomSerializer();
            //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            CustomSerializerManager.registerCustomSerializer(typeof(RequestBody), s1);
            //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            CustomSerializerManager.registerCustomSerializer(typeof(string), s2);

            RequestBody req = new RequestBody(1, "hello world sync");
            for (int i = 0; i < invokeTimes; i++)
            {
                try
                {
                    byte testCodec = (byte)i;
                    InvokeContext invokeContext = new InvokeContext();
                    invokeContext.put(InvokeContext.BOLT_CUSTOM_SERIALIZER, (int)testCodec);
                    string res = (string)client.invokeSync(addr, req, invokeContext, 3000);
                    logger.LogWarning("Result received in sync: " + res);
                    Assert.Equal(RequestBody.DEFAULT_SERVER_RETURN_STR + "RANDOM", res);

                    Assert.Equal(testCodec, s1.ContentSerializer);
                    Assert.Equal(testCodec, s2.ContentSerializer);
                }
                catch (ArgumentException e)
                {
                    logger.LogError("ArgumentException", e);
                    Assert.True(true);
                    return;
                }
                catch (ThreadInterruptedException e)
                {
                    string errMsg = "ThreadInterruptedException caught in sync!";
                    logger.LogError(errMsg, e);
                    Assert.Null(errMsg);
                }
                Assert.Null("Should not reach here!");
            }

            Assert.True(serverConnectProcessor.Connected);
            Assert.Equal(1, serverConnectProcessor.ConnectTimes);
            Assert.Equal(invokeTimes, serverUserProcessor.InvokeTimes);
        }
    }
}