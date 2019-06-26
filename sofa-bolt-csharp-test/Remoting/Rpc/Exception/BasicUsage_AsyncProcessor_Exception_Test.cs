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

namespace com.alipay.remoting.rpc.exception
{
    /// <summary>
    /// test async send back exception
    /// </summary>
    [Collection("Sequential")]
    public class BasicUsage_AsyncProcessor_Exception_Test : IDisposable
    {
        private bool InstanceFieldsInitialized = false;

        public BasicUsage_AsyncProcessor_Exception_Test()
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

        internal AsyncServerUserProcessor serverUserProcessor = new AsyncServerUserProcessor(true, false);
        internal AsyncClientUserProcessor clientUserProcessor = new AsyncClientUserProcessor(true, false);
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
            RequestBody req = new RequestBody(2, "hello world oneway");
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
            RequestBody req = new RequestBody(1, "hello world sync");
            for (int i = 0; i < invokeTimes; i++)
            {
                Exception res = null;
                try
                {
                    res = (Exception)client.invokeSync(addr, req, 3000);
                    logger.LogWarning("Result received in sync: " + res);
                    //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					Assert.Equal(typeof(ArgumentException), res.GetType());

                }
                catch (RemotingException e)
                {
                    string errMsg = "RemotingException caught in sync!";
                    logger.LogError(errMsg, e);
                    Assert.Null("Should not reach here!");
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
            RequestBody req = new RequestBody(2, "hello world future");
            for (int i = 0; i < invokeTimes; i++)
            {
                Exception res = null;
                try
                {
                    RpcResponseFuture future = client.invokeWithFuture(addr, req, 3000);
                    res = (Exception)future.get();
                    //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					Assert.Equal(typeof(ArgumentException), res.GetType());
                }
                catch (RemotingException e)
                {
                    string errMsg = "RemotingException caught in future!";
                    logger.LogError(errMsg, e);
                    Assert.Null("Should not reach here!");
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
            RequestBody req = new RequestBody(1, "hello world callback");
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final java.util.List<Object> rets = new java.util.ArrayList<Object>(1);
            IList<object> rets = new List<object>(1);
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
                    Assert.Null("Should not reach here!");
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
                //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
                Assert.Equal(typeof(ArgumentException), rets[0].GetType());
                rets.Clear();
            }

            Assert.True(serverConnectProcessor.Connected);
            Assert.Equal(1, serverConnectProcessor.ConnectTimes);
            Assert.Equal(invokeTimes, serverUserProcessor.InvokeTimes);
        }

        private class InvokeCallbackAnonymousInnerClass : InvokeCallback
        {
            private readonly BasicUsage_AsyncProcessor_Exception_Test outerInstance;

            private IList<object> rets;
            private CountdownEvent latch;

            public InvokeCallbackAnonymousInnerClass(BasicUsage_AsyncProcessor_Exception_Test outerInstance, IList<object> rets, CountdownEvent latch)
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
                rets.Add(result);
                if (!latch.IsSet)
            {
                latch.Signal();
            }
            }

            public void onException(Exception e)
            {
                logger.LogError("Process exception in callback.", e);
                rets.Add(e);
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
            Connection clientConn = client.createStandaloneConnection(ip, port, 1000);

            for (int i = 0; i < invokeTimes; i++)
            {
                RequestBody req1 = new RequestBody(1, RequestBody.DEFAULT_CLIENT_STR);
                Exception serverres = null;
                try
                {
                    serverres = (Exception)client.invokeSync(clientConn, req1, 1000);
                    //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					Assert.Equal(typeof(ArgumentException), serverres.GetType());
                }
                catch (RemotingException)
                {
                    Assert.Null("Should not reach here!");
                }

                Assert.NotNull(serverConnectProcessor.Connection);
                Connection serverConn = serverConnectProcessor.Connection;
                RequestBody req = new RequestBody(1, RequestBody.DEFAULT_SERVER_STR);
                Exception clientres = null;
                try
                {
                    clientres = (Exception)server.RpcServer.invokeSync(serverConn, req, 1000);
                    //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					Assert.Equal(typeof(ArgumentException), clientres.GetType());
                }
                catch (RemotingException)
                {
                    Assert.Null("Should not reach here!");
                }
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
            Connection clientConn = client.createStandaloneConnection(ip, port, 1000);
            string remote = clientConn.Channel.RemoteAddress.ToString();
            string local = clientConn.Channel.LocalAddress.ToString();
            logger.LogWarning("Client say local:" + local);
            logger.LogWarning("Client say remote:" + remote);

            for (int i = 0; i < invokeTimes; i++)
            {
                RequestBody req1 = new RequestBody(1, RequestBody.DEFAULT_CLIENT_STR);
                Exception serverres = null;
                try
                {
                    serverres = (Exception)client.invokeSync(clientConn, req1, 1000);
                    //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					Assert.Equal(typeof(ArgumentException), serverres.GetType());
                }
                catch (RemotingException)
                {
                    Assert.Null("Should not reach here!");
                }

                Assert.NotNull(serverConnectProcessor.Connection);
                // only when client invoked, the remote address can be get by UserProcessor
                // otherwise, please use ConnectionEventProcessor
                string remoteAddr = serverUserProcessor.RemoteAddr;
                RequestBody req = new RequestBody(1, RequestBody.DEFAULT_SERVER_STR);

                Exception clientres = null;
                try
                {
                    clientres = (Exception)server.RpcServer.invokeSync(remoteAddr, req, 1000);
                    //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					Assert.Equal(typeof(ArgumentException), clientres.GetType());
                }
                catch (RemotingException)
                {
                    Assert.Null("Should not reach here!");
                }
            }

            Assert.True(serverConnectProcessor.Connected);
            Assert.Equal(1, serverConnectProcessor.ConnectTimes);
            Assert.Equal(invokeTimes, serverUserProcessor.InvokeTimes);
        }
    }
}