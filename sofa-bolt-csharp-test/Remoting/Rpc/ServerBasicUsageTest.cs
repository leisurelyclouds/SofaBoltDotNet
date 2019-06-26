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

namespace com.alipay.remoting.rpc
{
    /// <summary>
    /// Basic usage test of Rpc Server invoke apis
    /// </summary>
    [Collection("Sequential")]
    public class ServerBasicUsageTest: IDisposable
    {
        private bool InstanceFieldsInitialized = false;

        public ServerBasicUsageTest()
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

        [Fact]
        public virtual void testOneway()
        {
            client.getConnection(addr, 1000);

            RequestBody req = new RequestBody(1, RequestBody.DEFAULT_SERVER_STR);
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
                    server.RpcServer.oneway(remoteAddr, req);
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
            Assert.Equal(0, serverUserProcessor.InvokeTimes);
            Assert.Equal(invokeTimes, clientUserProcessor.InvokeTimes);
        }

        [Fact]
        public virtual void testSync()
        {
            client.getConnection(addr, 1000);

            RequestBody req = new RequestBody(1, RequestBody.DEFAULT_SERVER_STR);
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
                    string clientres = (string)server.RpcServer.invokeSync(remoteAddr, req, 1000);
                    Assert.Equal(clientres, RequestBody.DEFAULT_CLIENT_RETURN_STR);
                }
                catch (RemotingException e)
                {
                    string errMsg = "RemotingException caught in sync!";
                    logger.LogError(errMsg, e);
                    Assert.Null(errMsg);
                }
            }

            Assert.True(serverConnectProcessor.Connected);
            Assert.Equal(1, serverConnectProcessor.ConnectTimes);
            Assert.Equal(0, serverUserProcessor.InvokeTimes);
            Assert.Equal(invokeTimes, clientUserProcessor.InvokeTimes);
        }

        [Fact]
        public virtual void testFuture()
        {
            client.getConnection(addr, 1000);

            RequestBody req = new RequestBody(1, RequestBody.DEFAULT_SERVER_STR);
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
                    RpcResponseFuture future = server.RpcServer.invokeWithFuture(remoteAddr, req, 1000);
                    string clientres = (string)future.get(1000);
                    Assert.Equal(clientres, RequestBody.DEFAULT_CLIENT_RETURN_STR);
                }
                catch (RemotingException e)
                {
                    string errMsg = "RemotingException caught in future!";
                    logger.LogError(errMsg, e);
                    Assert.Null(errMsg);
                }
            }

            Assert.True(serverConnectProcessor.Connected);
            Assert.Equal(1, serverConnectProcessor.ConnectTimes);
            Assert.Equal(0, serverUserProcessor.InvokeTimes);
            Assert.Equal(invokeTimes, clientUserProcessor.InvokeTimes);
        }

        [Fact]
        public virtual void testCallback()
        {
            client.getConnection(addr, 1000);

            RequestBody req = new RequestBody(1, RequestBody.DEFAULT_SERVER_STR);
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

                    //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                    //ORIGINAL LINE: final java.util.List<String> rets = new java.util.ArrayList<String>(1);
                    IList<string> rets = new List<string>(1);
                    //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                    //ORIGINAL LINE: final java.util.concurrent.CountdownEvent latch = new java.util.concurrent.CountdownEvent(1);
                    CountdownEvent latch = new CountdownEvent(1);
                    server.RpcServer.invokeWithCallback(remoteAddr, req, new InvokeCallbackAnonymousInnerClass(this, rets, latch), 1000);
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
                    Assert.Equal(RequestBody.DEFAULT_CLIENT_RETURN_STR, rets[0]);
                    rets.Clear();
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
            Assert.Equal(0, serverUserProcessor.InvokeTimes);
            Assert.Equal(invokeTimes, clientUserProcessor.InvokeTimes);
        }

        private class InvokeCallbackAnonymousInnerClass : InvokeCallback
        {
            private readonly ServerBasicUsageTest outerInstance;

            private IList<string> rets;
            private CountdownEvent latch;

            public InvokeCallbackAnonymousInnerClass(ServerBasicUsageTest outerInstance, IList<string> rets, CountdownEvent latch)
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
    }
}