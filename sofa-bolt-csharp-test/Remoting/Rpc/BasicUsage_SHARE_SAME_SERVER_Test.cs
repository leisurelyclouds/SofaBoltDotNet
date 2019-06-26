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
using Xunit.Abstractions;
using Xunit.Priority;

namespace com.alipay.remoting.rpc
{
    public class ShareSameServerTestFixture : IDisposable
    {
        public static ILogger logger = NullLogger.Instance;
        public static BoltServer server;

        public static int port = PortScan.select();
        public static SimpleServerUserProcessor serverUserProcessor = new SimpleServerUserProcessor();
        public static CONNECTEventProcessor serverConnectProcessor = new CONNECTEventProcessor();

        public ShareSameServerTestFixture()
        {
            server = new BoltServer(port);
            server.start();
            server.registerUserProcessor(serverUserProcessor);
            server.addConnectionEventProcessor(ConnectionEventType.CONNECT, serverConnectProcessor);
        }

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
    }

    [CollectionDefinition("Database collection")]
    public class DatabaseCollection : ICollectionFixture<ShareSameServerTestFixture>
    {
    }

    /// <summary>
    /// basic usage test
    /// each test shared the same server
    /// </summary>
    [Collection("Database collection")]
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class BasicUsage_SHARE_SAME_SERVER_Test : IDisposable
    {
        private readonly ITestOutputHelper testOutputHelper;
        ShareSameServerTestFixture fixture;

        public BasicUsage_SHARE_SAME_SERVER_Test(ITestOutputHelper testOutputHelper, ShareSameServerTestFixture fixture)
        {
            this.testOutputHelper = testOutputHelper;
            this.fixture = fixture;
            
            inti();
        }

        internal RpcClient client;

        internal static IPAddress ip = IPAddress.Parse("127.0.0.1");
        internal static string addr = "127.0.0.1:" + ShareSameServerTestFixture.port;

        internal int invokeTimes = 5;

        internal SimpleClientUserProcessor clientUserProcessor = new SimpleClientUserProcessor();
        internal CONNECTEventProcessor clientConnectProcessor = new CONNECTEventProcessor();
        internal DISCONNECTEventProcessor clientDisConnectProcessor = new DISCONNECTEventProcessor();
        internal DISCONNECTEventProcessor serverDisConnectProcessor = new DISCONNECTEventProcessor();

        private void inti()
        {
            client = new RpcClient();
            client.registerUserProcessor(clientUserProcessor);
            client.startup();
        }

        public void Dispose()
        {
            try
            {
                Thread.Sleep(100);
            }
            catch (ThreadInterruptedException e)
            {
                ShareSameServerTestFixture.logger.LogError("InterruptedException!", e);
            }
        }

        [Fact, Priority(1)]
        public virtual void test_a_Oneway()
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
                    ShareSameServerTestFixture.logger.LogError(errMsg, e);
                    Assert.Null(errMsg);
                }
            }

            Assert.True(ShareSameServerTestFixture.serverConnectProcessor.Connected);
            Assert.Equal(1, ShareSameServerTestFixture.serverConnectProcessor.ConnectTimes);
            Assert.Equal(invokeTimes, ShareSameServerTestFixture.serverUserProcessor.InvokeTimes);
        }

        [Fact, Priority(2)]
        public virtual void test_b_Sync()
        {
            RequestBody req = new RequestBody(1, "hello world sync");
            for (int i = 0; i < invokeTimes; i++)
            {
                try
                {
                    string res = (string)client.invokeSync(addr, req, 3000);
                    ShareSameServerTestFixture.logger.LogWarning("Result received in sync: " + res);
                    Assert.Equal(RequestBody.DEFAULT_SERVER_RETURN_STR, res);
                }
                catch (RemotingException e)
                {
                    string errMsg = "RemotingException caught in sync!";
                    ShareSameServerTestFixture.logger.LogError(errMsg, e);
                    Assert.Null(errMsg);
                }
                catch (ThreadInterruptedException e)
                {
                    string errMsg = "ThreadInterruptedException caught in sync!";
                    ShareSameServerTestFixture.logger.LogError(errMsg, e);
                    Assert.Null(errMsg);
                }
            }

            Assert.True(ShareSameServerTestFixture.serverConnectProcessor.Connected);
            Assert.Equal(2, ShareSameServerTestFixture.serverConnectProcessor.ConnectTimes);
            Assert.Equal(invokeTimes * 2, ShareSameServerTestFixture.serverUserProcessor.InvokeTimes);
        }

        [Fact, Priority(3)]
        public virtual void test_c_Future()
        {
            RequestBody req = new RequestBody(2, "hello world future");
            for (int i = 0; i < invokeTimes; i++)
            {
                try
                {
                    RpcResponseFuture future = client.invokeWithFuture(addr, req, 3000);
                    string res = (string)future.get();
                    Assert.Equal(RequestBody.DEFAULT_SERVER_RETURN_STR, res);
                }
                catch (RemotingException e)
                {
                    string errMsg = "RemotingException caught in future!";
                    ShareSameServerTestFixture.logger.LogError(errMsg, e);
                    Assert.Null(errMsg);
                }
                catch (ThreadInterruptedException e)
                {
                    string errMsg = "ThreadInterruptedException caught in future!";
                    ShareSameServerTestFixture.logger.LogError(errMsg, e);
                    Assert.Null(errMsg);
                }
            }

            Assert.True(ShareSameServerTestFixture.serverConnectProcessor.Connected);
            Assert.Equal(3, ShareSameServerTestFixture.serverConnectProcessor.ConnectTimes);
            Assert.Equal(invokeTimes * 3, ShareSameServerTestFixture.serverUserProcessor.InvokeTimes);
        }

        [Fact, Priority(4)]
        public virtual void test_d_Callback()
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
                    client.invokeWithCallback(addr, req, new InvokeCallbackAnonymousInnerClass(this, rets, latch), 1000);

                }
                catch (RemotingException e)
                {
                    if (!latch.IsSet)
            {
                latch.Signal();
            }
                    string errMsg = "RemotingException caught in callback!";
                    ShareSameServerTestFixture.logger.LogError(errMsg, e);
                    Assert.Null(errMsg);
                }
                try
                {
                    latch.Wait();
                }
                catch (ThreadInterruptedException e)
                {
                    string errMsg = "ThreadInterruptedException caught in callback!";
                    ShareSameServerTestFixture.logger.LogError(errMsg, e);
                    Assert.Null(errMsg);
                }
                if (rets.Count == 0)
                {
                    Assert.Null("No result! Maybe exception caught!");
                }
                Assert.Equal(RequestBody.DEFAULT_SERVER_RETURN_STR, rets[0]);
                rets.Clear();
            }

            Assert.True(ShareSameServerTestFixture.serverConnectProcessor.Connected);
            Assert.Equal(4, ShareSameServerTestFixture.serverConnectProcessor.ConnectTimes);
            Assert.Equal(invokeTimes * 4, ShareSameServerTestFixture.serverUserProcessor.InvokeTimes);
        }

        private class InvokeCallbackAnonymousInnerClass : InvokeCallback
        {
            private readonly BasicUsage_SHARE_SAME_SERVER_Test outerInstance;

            private IList<string> rets;
            private CountdownEvent latch;

            public InvokeCallbackAnonymousInnerClass(BasicUsage_SHARE_SAME_SERVER_Test outerInstance, IList<string> rets, CountdownEvent latch)
            {
                this.outerInstance = outerInstance;
                this.rets = rets;
                this.latch = latch;
                executor = Executors.newCachedThreadPool();
            }

            internal Executor executor;

            public void onResponse(object result)
            {
                ShareSameServerTestFixture.logger.LogWarning("Result received in callback: " + result);
                rets.Add((string)result);
                if (!latch.IsSet)
            {
                latch.Signal();
            }
            }

            public void onException(Exception e)
            {
                ShareSameServerTestFixture.logger.LogError("Process exception in callback.", e);
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

        [Fact, Priority(5)]
        public virtual void test_e_ServerSync()
        {
            Connection clientConn = client.createStandaloneConnection(ip, ShareSameServerTestFixture.port, 1000);

            for (int i = 0; i < invokeTimes; i++)
            {
                RequestBody req1 = new RequestBody(1, RequestBody.DEFAULT_CLIENT_STR);
                string serverres = (string)client.invokeSync(clientConn, req1, 1000);
                Assert.Equal(serverres, RequestBody.DEFAULT_SERVER_RETURN_STR);

                Assert.NotNull(ShareSameServerTestFixture.serverConnectProcessor.Connection);
                Connection serverConn = ShareSameServerTestFixture.serverConnectProcessor.Connection;
                RequestBody req = new RequestBody(1, RequestBody.DEFAULT_SERVER_STR);
                string clientres = (string)ShareSameServerTestFixture.server.RpcServer.invokeSync(serverConn, req, 1000);
                Assert.Equal(clientres, RequestBody.DEFAULT_CLIENT_RETURN_STR);
            }

            Assert.True(ShareSameServerTestFixture.serverConnectProcessor.Connected);
            Assert.Equal(5, ShareSameServerTestFixture.serverConnectProcessor.ConnectTimes);
            Assert.Equal(invokeTimes * 5, ShareSameServerTestFixture.serverUserProcessor.InvokeTimes);
        }
    }
}