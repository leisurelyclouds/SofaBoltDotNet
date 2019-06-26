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
using Remoting.rpc.exception;

namespace com.alipay.remoting.rpc.exception
{
    [Collection("Sequential")]
    public class ServerBusyTest : IDisposable
    {
        private bool InstanceFieldsInitialized = false;

        public ServerBusyTest()
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
            concurrent = maxThread + workQueue;
            serverUserProcessor = new SimpleServerUserProcessor(timeout, coreThread, maxThread, 60, workQueue);
        }

        internal static ILogger logger = NullLogger.Instance;

        internal BoltServer server;
        internal RpcClient client;

        internal int port = PortScan.select();
        internal IPAddress ip = IPAddress.Parse("127.0.0.1");
        internal string addr;

        internal int invokeTimes = 5;
        internal int timeout = 15000;

        internal int coreThread = 1;
        internal int maxThread = 3;
        internal int workQueue = 4;
        internal int concurrent;

        internal SimpleServerUserProcessor serverUserProcessor;
        internal SimpleClientUserProcessor clientUserProcessor = new SimpleClientUserProcessor();
        internal CONNECTEventProcessor clientConnectProcessor = new CONNECTEventProcessor();
        internal CONNECTEventProcessor serverConnectProcessor = new CONNECTEventProcessor();
        internal DISCONNECTEventProcessor clientDisConnectProcessor = new DISCONNECTEventProcessor();
        internal DISCONNECTEventProcessor serverDisConnectProcessor = new DISCONNECTEventProcessor();

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Before private void init() throws ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        private void init()
        {
            concurrent = maxThread + workQueue;
            //        java.lang.System.setProperty(RpcConfigs.TP_MIN_SIZE, "1");
            //        java.lang.System.setProperty(RpcConfigs.TP_QUEUE_SIZE, "4");
            //        java.lang.System.setProperty(RpcConfigs.TP_MAX_SIZE, "3");

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

            for (int i = 0; i < concurrent; i++)
            {
                //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                //ORIGINAL LINE: final com.alipay.remoting.rpc.common.RequestBody bd = new com.alipay.remoting.rpc.common.RequestBody(i + 1, "Hello world!");
                RequestBody bd = new RequestBody(i + 1, "Hello world!");
                Thread t = new Thread(() =>
                {
                    object obj = null;
                    try
                    {
                        logger.LogInformation("client fire! =========" + bd.Id);
                        obj = client.invokeSync(addr, bd, 2000);
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

                });
                t.Start();
                Thread.Sleep(100);
            }
            Thread.Sleep(100);
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @After public void stop()
        public void Dispose()
        {
            server.stop();
            try
            {
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
            object obj = null;
            try
            {
                //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                //ORIGINAL LINE: final com.alipay.remoting.rpc.common.RequestBody bd = new com.alipay.remoting.rpc.common.RequestBody(8, "Hello world!");
                RequestBody bd = new RequestBody(8, "Hello world!");
                logger.LogInformation("client last sync invoke! =========" + bd.Id);
                obj = client.invokeSync(addr, bd, 3000);
                Assert.Null("Should not reach here!");
            }
            catch (InvokeServerBusyException)
            {
                Assert.Null(obj);
            }
            catch (RemotingException e)
            {
                logger.LogError("Other RemotingException but InvokeServerBusyException occurred in sync", e);
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
        //ORIGINAL LINE: @Test public void testFuture()
        public virtual void testFuture()
        {
            RequestBody b4 = new RequestBody(4, "Hello world!");
            object obj = null;
            try
            {
                RpcResponseFuture future = client.invokeWithFuture(addr, b4, 1000);
                obj = future.get(1500);
                Assert.Null("Should not reach here!");
            }
            catch (InvokeServerBusyException)
            {
                Assert.Null(obj);
            }
            catch (RemotingException e)
            {
                logger.LogError("Other RemotingException but InvokeServerBusyException occurred in future", e);
                Assert.Null("Should not reach here!");
            }
            catch (ThreadInterruptedException e)
            {
                logger.LogError("ThreadInterruptedException in future", e);
                Assert.Null("Should not reach here!");
            }

        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void callback() throws ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        public virtual void callback()
        {
            RequestBody b3 = new RequestBody(3, "Hello world!");
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final java.util.concurrent.CountdownEvent latch = new java.util.concurrent.CountdownEvent(1);
            CountdownEvent latch = new CountdownEvent(1);
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final java.util.List<Throwable> ret = new java.util.ArrayList<Throwable>(1);
            IList<Exception> ret = new List<Exception>(1);
            try
            {
                client.invokeWithCallback(addr, b3, new InvokeCallbackAnonymousInnerClass(this, latch, ret), 1000);

            }
            catch (RemotingException e)
            {
                logger.LogError("Other RemotingException but InvokeServerBusyException occurred in callback", e);
                Assert.Null("Should not reach here!");
            }
            latch.Wait();
            Assert.Equal(typeof(InvokeServerBusyException), ret[0].GetType());
        }

        private class InvokeCallbackAnonymousInnerClass : InvokeCallback
        {
            private readonly ServerBusyTest outerInstance;

            private CountdownEvent latch;
            private IList<Exception> ret;

            public InvokeCallbackAnonymousInnerClass(ServerBusyTest outerInstance, CountdownEvent latch, IList<Exception> ret)
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
                ret.Add(e);
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