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
using Remoting.rpc.exception;
using System.Net;
using System;

namespace com.alipay.remoting.rpc.timeout
{
    /// <summary>
    /// server process timeout test (timeout check in io thread)
    /// if already timeout waiting in work queue, then discard this request and return timeout exception.
    /// Oneway will not do this.
    /// </summary>
    [Collection("Sequential")]
    public class ServerTimeoutTest : IDisposable
    {
        private bool InstanceFieldsInitialized = false;

        public ServerTimeoutTest()
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
            serverUserProcessor = new SimpleServerUserProcessor(max_timeout, coreThread, maxThread, 60, workQueue);
            clientUserProcessor = new SimpleClientUserProcessor(max_timeout, coreThread, maxThread, 60, workQueue);
        }

        internal static ILogger logger = NullLogger.Instance;

        internal BoltServer server;
        internal RpcClient client;

        internal int port = PortScan.select();
        internal IPAddress ip = IPAddress.Parse("127.0.0.1");
        internal string addr;

        internal int invokeTimes = 5;
        internal int max_timeout = 500;

        internal int coreThread = 1;
        internal int maxThread = 1;
        internal int workQueue = 1;
        internal int concurrent;

        internal SimpleServerUserProcessor serverUserProcessor;
        internal SimpleClientUserProcessor clientUserProcessor;
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

        // ~~~ client and server invoke test methods

        /// <summary>
        /// the second request will not timeout in oneway process work queue
        /// </summary>
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testOneway()
        public virtual void testOneway()
        {
            for (int i = 0; i <= 1; ++i)
            {
                new ThreadAnonymousInnerClass(this).start();
            }
            try
            {
                Thread.Sleep(max_timeout * 2);
            }
            catch (ThreadInterruptedException e)
            {
                logger.LogError("", e);
            }

            Assert.Equal(2, serverUserProcessor.getInvokeTimesEachCallType(RequestBody.InvokeType.ONEWAY));
        }

        private class ThreadAnonymousInnerClass : java.lang.Thread
        {
            private readonly ServerTimeoutTest outerInstance;

            public ThreadAnonymousInnerClass(ServerTimeoutTest outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public override void run()
            {
                outerInstance.oneway(outerInstance.client, null);
            }
        }

        /// <summary>
        /// the second request will not timeout in oneway process work queue
        /// </summary>
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testServerOneway()
        public virtual void testServerOneway()
        {
            for (int i = 0; i <= 1; ++i)
            {
                new ThreadAnonymousInnerClass2(this)
                .start();
            }
            try
            {
                Thread.Sleep(max_timeout * 2);
            }
            catch (ThreadInterruptedException e)
            {
                logger.LogError("", e);
            }

            Assert.Equal(2, clientUserProcessor.getInvokeTimesEachCallType(RequestBody.InvokeType.ONEWAY));
        }

        private class ThreadAnonymousInnerClass2 : java.lang.Thread
        {
            private readonly ServerTimeoutTest outerInstance;

            public ThreadAnonymousInnerClass2(ServerTimeoutTest outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public override void run()
            {
                outerInstance.oneway(outerInstance.client, outerInstance.server.RpcServer);
            }
        }

        /// <summary>
        /// the second request will timeout in work queue
        /// </summary>
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testSync()
        public virtual void testSync()
        {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final int timeout[] = { max_timeout / 2, max_timeout / 3 };
            int[] timeout = new int[] { max_timeout / 2, max_timeout / 3 };
            for (int i = 0; i <= 1; ++i)
            {
                //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                //ORIGINAL LINE: final int j = i;
                int j = i;
                new ThreadAnonymousInnerClass3(this, timeout, j)
                .start();
            }
            try
            {
                Thread.Sleep(max_timeout * 2);
            }
            catch (ThreadInterruptedException e)
            {
                logger.LogError("", e);
            }
            Assert.Equal(1, serverUserProcessor.getInvokeTimesEachCallType(RequestBody.InvokeType.SYNC));
        }

        private class ThreadAnonymousInnerClass3 : java.lang.Thread
        {
            private readonly ServerTimeoutTest outerInstance;

            private int[] timeout;
            private int j;

            public ThreadAnonymousInnerClass3(ServerTimeoutTest outerInstance, int[] timeout, int j)
            {
                this.outerInstance = outerInstance;
                this.timeout = timeout;
                this.j = j;
            }

            public override void run()
            {
                outerInstance.sync(outerInstance.client, null, timeout[j]);
            }
        }

        /// <summary>
        /// the second request will timeout in work queue
        /// </summary>
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testServerSync()
        public virtual void testServerSync()
        {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final int timeout[] = { max_timeout / 2, max_timeout / 3 };
            int[] timeout = new int[] { max_timeout / 2, max_timeout / 3 };
            for (int i = 0; i <= 1; ++i)
            {
                //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                //ORIGINAL LINE: final int j = i;
                int j = i;
                new ThreadAnonymousInnerClass4(this, timeout, j)
                .start();
            }
            try
            {
                Thread.Sleep(max_timeout * 2);
            }
            catch (ThreadInterruptedException e)
            {
                logger.LogError("", e);
            }
            Assert.Equal(1, clientUserProcessor.getInvokeTimesEachCallType(RequestBody.InvokeType.SYNC));
        }

        private class ThreadAnonymousInnerClass4 : java.lang.Thread
        {
            private readonly ServerTimeoutTest outerInstance;

            private int[] timeout;
            private int j;

            public ThreadAnonymousInnerClass4(ServerTimeoutTest outerInstance, int[] timeout, int j)
            {
                this.outerInstance = outerInstance;
                this.timeout = timeout;
                this.j = j;
            }

            public override void run()
            {
                outerInstance.sync(outerInstance.client, outerInstance.server.RpcServer, timeout[j]);
            }
        }

        /// <summary>
        /// the second request will timeout in work queue
        /// </summary>
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testFuture()
        public virtual void testFuture()
        {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final int timeout[] = { max_timeout / 2, max_timeout / 3 };
            int[] timeout = new int[] { max_timeout / 2, max_timeout / 3 };
            for (int i = 0; i <= 1; ++i)
            {
                //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                //ORIGINAL LINE: final int j = i;
                int j = i;
                new ThreadAnonymousInnerClass5(this, timeout, j)
                .start();
            }
            try
            {
                Thread.Sleep(max_timeout * 2);
            }
            catch (ThreadInterruptedException e)
            {
                logger.LogError("", e);
            }
            Assert.Equal(1, serverUserProcessor.getInvokeTimesEachCallType(RequestBody.InvokeType.FUTURE));
        }

        private class ThreadAnonymousInnerClass5 : java.lang.Thread
        {
            private readonly ServerTimeoutTest outerInstance;

            private int[] timeout;
            private int j;

            public ThreadAnonymousInnerClass5(ServerTimeoutTest outerInstance, int[] timeout, int j)
            {
                this.outerInstance = outerInstance;
                this.timeout = timeout;
                this.j = j;
            }

            public override void run()
            {
                outerInstance.future(outerInstance.client, null, timeout[j]);
            }
        }

        /// <summary>
        /// the second request will timeout in work queue
        /// </summary>
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testServerFuture()
        public virtual void testServerFuture()
        {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final int timeout[] = { max_timeout / 2, max_timeout / 3 };
            int[] timeout = new int[] { max_timeout / 2, max_timeout / 3 };
            for (int i = 0; i <= 1; ++i)
            {
                //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                //ORIGINAL LINE: final int j = i;
                int j = i;
                new ThreadAnonymousInnerClass6(this, timeout, j)
                .start();
            }
            try
            {
                Thread.Sleep(max_timeout * 2);
            }
            catch (ThreadInterruptedException e)
            {
                logger.LogError("", e);
            }
            Assert.Equal(1, clientUserProcessor.getInvokeTimesEachCallType(RequestBody.InvokeType.FUTURE));
        }

        private class ThreadAnonymousInnerClass6 : java.lang.Thread
        {
            private readonly ServerTimeoutTest outerInstance;

            private int[] timeout;
            private int j;

            public ThreadAnonymousInnerClass6(ServerTimeoutTest outerInstance, int[] timeout, int j)
            {
                this.outerInstance = outerInstance;
                this.timeout = timeout;
                this.j = j;
            }

            public override void run()
            {
                outerInstance.future(outerInstance.client, outerInstance.server.RpcServer, timeout[j]);
            }
        }

        /// <summary>
        /// the second request will timeout in work queue
        /// </summary>
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testCallBack()
        public virtual void testCallBack()
        {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final int timeout[] = { max_timeout / 2, max_timeout / 3 };
            int[] timeout = new int[] { max_timeout / 2, max_timeout / 3 };
            for (int i = 0; i <= 1; ++i)
            {
                //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                //ORIGINAL LINE: final int j = i;
                int j = i;
                new ThreadAnonymousInnerClass7(this, timeout, j)
                .start();
            }
            try
            {
                Thread.Sleep(max_timeout * 2);
            }
            catch (ThreadInterruptedException e)
            {
                logger.LogError("", e);
            }
            Assert.Equal(1, serverUserProcessor.getInvokeTimesEachCallType(RequestBody.InvokeType.CALLBACK));
        }

        private class ThreadAnonymousInnerClass7 : java.lang.Thread
        {
            private readonly ServerTimeoutTest outerInstance;

            private int[] timeout;
            private int j;

            public ThreadAnonymousInnerClass7(ServerTimeoutTest outerInstance, int[] timeout, int j)
            {
                this.outerInstance = outerInstance;
                this.timeout = timeout;
                this.j = j;
            }

            public override void run()
            {
                outerInstance.callback(outerInstance.client, null, timeout[j]);
            }
        }

        /// <summary>
        /// the second request will timeout in work queue
        /// </summary>
        [Fact]
        public virtual void testServerCallBack()
        {
            int[] timeout = new int[] { max_timeout / 2, max_timeout / 3 };
            for (int i = 0; i <= 1; ++i)
            {
                int j = i;
                new ThreadAnonymousInnerClass8(this, timeout, j)
                .start();
            }
            try
            {
                Thread.Sleep(max_timeout * 2);
            }
            catch (ThreadInterruptedException e)
            {
                logger.LogError("", e);
            }
            Assert.Equal(1, clientUserProcessor.getInvokeTimesEachCallType(RequestBody.InvokeType.CALLBACK));
        }

        private class ThreadAnonymousInnerClass8 : java.lang.Thread
        {
            private readonly ServerTimeoutTest outerInstance;

            private int[] timeout;
            private int j;

            public ThreadAnonymousInnerClass8(ServerTimeoutTest outerInstance, int[] timeout, int j)
            {
                this.outerInstance = outerInstance;
                this.timeout = timeout;
                this.j = j;
            }

            public override void run()
            {
                outerInstance.callback(outerInstance.client, outerInstance.server.RpcServer, timeout[j]);
            }
        }

        // ~~~ server invoke test methods

        // ~~~ private methods

        private void oneway(RpcClient client, RpcServer server)
        {
            RequestBody b2 = new RequestBody(2, RequestBody.DEFAULT_ONEWAY_STR);
            try
            {
                if (null == server)
                {
                    client.oneway(addr, b2);
                }
                else
                {
                    Connection conn = client.getConnection(addr, 1000);
                    Assert.NotNull(serverConnectProcessor.Connection);
                    Connection serverConn = serverConnectProcessor.Connection;
                    server.oneway(serverConn, b2);
                }
                Thread.Sleep(50);
            }
            catch (RemotingException e)
            {
                logger.LogError("Exception caught in oneway!", e);
                Assert.Null("Exception caught!");
            }
            catch (ThreadInterruptedException e)
            {
                logger.LogError("ThreadInterruptedException in oneway", e);
                Assert.Null("Should not reach here!");
            }
        }

        private void sync(RpcClient client, RpcServer server, int timeout)
        {
            RequestBody b1 = new RequestBody(1, RequestBody.DEFAULT_SYNC_STR);
            object obj = null;
            try
            {
                if (null == server)
                {
                    obj = client.invokeSync(addr, b1, timeout);
                }
                else
                {
                    Connection conn = client.getConnection(addr, timeout);
                    Assert.NotNull(serverConnectProcessor.Connection);
                    Connection serverConn = serverConnectProcessor.Connection;
                    obj = server.invokeSync(serverConn, b1, timeout);
                }
                Assert.Null("Should not reach here!");
            }
            catch (InvokeTimeoutException)
            {
                Assert.Null(obj);
            }
            catch (RemotingException e)
            {
                logger.LogError("Other RemotingException but RpcServerTimeoutException occurred in sync", e);
                Assert.Null("Should not reach here!");
            }
            catch (ThreadInterruptedException e)
            {
                logger.LogError("ThreadInterruptedException in sync", e);
                Assert.Null("Should not reach here!");
            }
        }

        private void future(RpcClient client, RpcServer server, int timeout)
        {
            RequestBody b1 = new RequestBody(1, RequestBody.DEFAULT_FUTURE_STR);
            object obj = null;
            try
            {
                RpcResponseFuture future = null;
                if (null == server)
                {
                    future = client.invokeWithFuture(addr, b1, timeout);
                }
                else
                {
                    Connection conn = client.getConnection(addr, timeout);
                    Assert.NotNull(serverConnectProcessor.Connection);
                    Connection serverConn = serverConnectProcessor.Connection;
                    future = server.invokeWithFuture(serverConn, b1, timeout);
                }
                obj = future.get(timeout);
                Assert.Null("Should not reach here!");
            }
            catch (InvokeTimeoutException)
            {
                Assert.Null(obj);
            }
            catch (RemotingException e)
            {
                logger.LogError("Other RemotingException but RpcServerTimeoutException occurred in sync", e);
                Assert.Null("Should not reach here!");
            }
            catch (ThreadInterruptedException e)
            {
                logger.LogError("ThreadInterruptedException in sync", e);
                Assert.Null("Should not reach here!");
            }
        }

        private void callback(RpcClient client, RpcServer server, int timeout)
        {
            RequestBody b1 = new RequestBody(1, RequestBody.DEFAULT_CALLBACK_STR);
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final java.util.List<String> rets = new java.util.ArrayList<String>(1);
            IList<string> rets = new List<string>(1);
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final java.util.concurrent.CountdownEvent latch = new java.util.concurrent.CountdownEvent(1);
            CountdownEvent latch = new CountdownEvent(1);
            try
            {
                if (null == server)
                {
                    client.invokeWithCallback(addr, b1, new InvokeCallbackAnonymousInnerClass(this, rets, latch), timeout);
                }
                else
                {
                    Connection conn = client.getConnection(addr, timeout);
                    Assert.NotNull(serverConnectProcessor.Connection);
                    Connection serverConn = serverConnectProcessor.Connection;
                    server.invokeWithCallback(serverConn, b1, new InvokeCallbackAnonymousInnerClass2(this, rets, latch), timeout);
                }

            }
            catch (RemotingException e)
            {
                logger.LogError("Other RemotingException but RpcServerTimeoutException occurred in sync", e);
                Assert.Null("Should not reach here!");
            }
            catch (ThreadInterruptedException e)
            {
                logger.LogError("ThreadInterruptedException but RpcServerTimeoutException occurred in sync", e);
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
            //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            Assert.Equal(typeof(InvokeTimeoutException).FullName, rets[0]);
            rets.Clear();
        }

        private class InvokeCallbackAnonymousInnerClass : InvokeCallback
        {
            private readonly ServerTimeoutTest outerInstance;

            private IList<string> rets;
            private CountdownEvent latch;

            public InvokeCallbackAnonymousInnerClass(ServerTimeoutTest outerInstance, IList<string> rets, CountdownEvent latch)
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

            public void onException(System.Exception e)
            {
                logger.LogError("Process exception in callback.", e);
                //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
                rets.Add(e.GetType().FullName);
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
            private readonly ServerTimeoutTest outerInstance;

            private IList<string> rets;
            private CountdownEvent latch;

            public InvokeCallbackAnonymousInnerClass2(ServerTimeoutTest outerInstance, IList<string> rets, CountdownEvent latch)
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

            public void onException(System.Exception e)
            {
                logger.LogError("Process exception in callback.", e);
                //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
                rets.Add(e.GetType().FullName);
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