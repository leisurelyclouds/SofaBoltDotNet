using java.lang;
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
using Remoting.rpc.exception;

namespace com.alipay.remoting.rpc.exception
{
    /// <summary>
    /// exception test
    /// </summary>
    [Collection("Sequential")]
    public class ExceptionTest : IDisposable
    {
        private bool InstanceFieldsInitialized = false;

        public ExceptionTest()
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
        internal string addr;
        internal int invokeTimes = 5;

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Before private void init()
        private void init()
        {
            server = new BoltServer(port);
            server.start();
            server.addConnectionEventProcessor(ConnectionEventType.CONNECT, new CONNECTEventProcessor());
            client = new RpcClient();
            client.startup();
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @After public void stop()
        public void Dispose()
        {
            try
            {
                server.stop();
                System.Threading.Thread.Sleep(100);
            }
            catch (ThreadInterruptedException e)
            {
                logger.LogError("Stop server failed!", e);
            }
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testSyncNoProcessor()
        public virtual void testSyncNoProcessor()
        {
            server.registerUserProcessor(new SimpleServerUserProcessor());
            try
            {
                client.invokeSync(addr, "No processor for String now!", 3000);
            }
            catch (System.Exception e)
            {
                Assert.Equal(typeof(InvokeServerException), e.GetType());
            }
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testProcessorReturnNull()
        public virtual void testProcessorReturnNull()
        {
            server.registerUserProcessor(new NullUserProcessor());
            RequestBody req = new RequestBody(4, "hello world");
            try
            {
                object obj = client.invokeSync(addr, req, 3000);
                Assert.Null(obj);
            }
            catch (RemotingException e)
            {
                string errMsg = "RemotingException caught in testProcessorReturnNull!";
                logger.LogError(errMsg, e);
                Assert.Null(errMsg);
            }
            catch (ThreadInterruptedException e)
            {
                string errMsg = "ThreadInterruptedException caught in testProcessorReturnNull!";
                logger.LogError(errMsg, e);
                Assert.Null(errMsg);
            }
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testFutureWait0()
        public virtual void testFutureWait0()
        {
            server.registerUserProcessor(new SimpleServerUserProcessor());
            RequestBody req = new RequestBody(4, "hello world future");

            object res = null;
            try
            {
                RpcResponseFuture future = client.invokeWithFuture(addr, req, 1000);
                res = future.get(0);
                Assert.Null("Should not reach here!");
            }
            catch (InvokeTimeoutException)
            {
                Assert.Null(res);
            }
            catch (RemotingException e)
            {
                string errMsg = "RemotingException caught in testFutureWait0!";
                logger.LogError(errMsg, e);
                Assert.Null(errMsg);
            }
            catch (ThreadInterruptedException e)
            {
                string errMsg = "ThreadInterruptedException caught in testFutureWait0!";
                logger.LogError(errMsg, e);
                Assert.Null(errMsg);
            }
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testFutureWaitShort()
        public virtual void testFutureWaitShort()
        {
            server.registerUserProcessor(new SimpleServerUserProcessor(100));
            RequestBody req = new RequestBody(4, "hello world future");

            object res = null;
            try
            {
                RpcResponseFuture future = client.invokeWithFuture(addr, req, 1000);
                res = future.get(10);
                Assert.Null("Should not reach here!");
            }
            catch (InvokeTimeoutException)
            {
                Assert.Null(res);
            }
            catch (RemotingException e)
            {
                string errMsg = "RemotingException caught in testFutureWaitShort!";
                logger.LogError(errMsg, e);
                Assert.Null(errMsg);
            }
            catch (ThreadInterruptedException e)
            {
                string errMsg = "ThreadInterruptedException caught in testFutureWaitShort!";
                logger.LogError(errMsg, e);
                Assert.Null(errMsg);
            }
        }

        [Fact]
        public virtual void testRegisterDuplicateProcessor()
        {
            server.registerUserProcessor(new SimpleServerUserProcessor());
            try
            {
                server.registerUserProcessor(new SimpleServerUserProcessor());
                string errMsg = "Can not register duplicate processor, should throw exception here";
                logger.LogError(errMsg);
                Assert.Null(errMsg);
            }
            catch (System.Exception e)
            {
                Assert.Equal(typeof(Remoting.exception.RuntimeException), e.GetType());
            }
            client.registerUserProcessor(new SimpleServerUserProcessor());
            try
            {
                client.registerUserProcessor(new SimpleServerUserProcessor());
                string errMsg = "Can not register duplicate processor, should throw exception here";
                logger.LogError(errMsg);
                Assert.Null(errMsg);
            }
            catch (System.Exception e)
            {
                Assert.Equal(typeof(Remoting.exception.RuntimeException), e.GetType());
            }
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testSyncException()
        public virtual void testSyncException()
        {
            server.registerUserProcessor(new ExceptionUserProcessor());
            RequestBody b1 = new RequestBody(1, "Hello world!");
            try
            {
                client.invokeSync(addr, b1, 3000);
                string errMsg = "Should throw InvokeServerException!";
                logger.LogError(errMsg);
                Assert.Null(errMsg);
            }
            catch (InvokeServerException)
            {
                Assert.True(true);
            }
            catch (RemotingException)
            {
                string errMsg = "RemotingException in testSyncException ";
                logger.LogError(errMsg);
                Assert.Null(errMsg);
            }
            catch (ThreadInterruptedException)
            {
                string errMsg = "ThreadInterruptedException in testSyncException ";
                logger.LogError(errMsg);
                Assert.Null(errMsg);
            }
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testSyncException1()
        public virtual void testSyncException1()
        {
            server.registerUserProcessor(new AsyncExceptionUserProcessor());
            RequestBody b1 = new RequestBody(1, "Hello world!");
            try
            {
                client.invokeSync(addr, b1, 3000);
                string errMsg = "Should throw InvokeServerException!";
                logger.LogError(errMsg);
                Assert.Null(errMsg);
            }
            catch (InvokeServerException)
            {
                Assert.True(true);
            }
            catch (RemotingException)
            {
                string errMsg = "RemotingException in testSyncException ";
                logger.LogError(errMsg);
                Assert.Null(errMsg);
            }
            catch (ThreadInterruptedException)
            {
                string errMsg = "ThreadInterruptedException in testSyncException ";
                logger.LogError(errMsg);
                Assert.Null(errMsg);
            }
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testFutureException()
        public virtual void testFutureException()
        {
            server.registerUserProcessor(new ExceptionUserProcessor());
            RequestBody b1 = new RequestBody(1, "Hello world!");
            try
            {
                RpcResponseFuture future = client.invokeWithFuture(addr, b1, 1000);
                future.get(1500);
                string errMsg = "Should throw InvokeServerException!";
                logger.LogError(errMsg);
                Assert.Null(errMsg);
            }
            catch (InvokeServerException)
            {
                Assert.True(true);
            }
            catch (RemotingException)
            {
                string errMsg = "RemotingException in testFutureException ";
                logger.LogError(errMsg);
                Assert.Null(errMsg);
            }
            catch (ThreadInterruptedException)
            {
                string errMsg = "ThreadInterruptedException in testFutureException ";
                logger.LogError(errMsg);
                Assert.Null(errMsg);
            }
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testFutureException1()
        public virtual void testFutureException1()
        {
            server.registerUserProcessor(new AsyncExceptionUserProcessor());
            RequestBody b1 = new RequestBody(1, "Hello world!");
            try
            {
                RpcResponseFuture future = client.invokeWithFuture(addr, b1, 1000);
                future.get(1500);
                string errMsg = "Should throw InvokeServerException!";
                logger.LogError(errMsg);
                Assert.Null(errMsg);
            }
            catch (InvokeServerException)
            {
                Assert.True(true);
            }
            catch (RemotingException)
            {
                string errMsg = "RemotingException in testFutureException ";
                logger.LogError(errMsg);
                Assert.Null(errMsg);
            }
            catch (ThreadInterruptedException)
            {
                string errMsg = "ThreadInterruptedException in testFutureException ";
                logger.LogError(errMsg);
                Assert.Null(errMsg);
            }
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testCallBackException() throws ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        public virtual void testCallBackException()
        {
            server.registerUserProcessor(new ExceptionUserProcessor());
            RequestBody b3 = new RequestBody(3, "Hello world!");
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final java.util.concurrent.CountdownEvent latch = new java.util.concurrent.CountdownEvent(1);
            CountdownEvent latch = new CountdownEvent(1);
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final java.util.List<Throwable> ret = new java.util.ArrayList<Throwable>(1);
            IList<System.Exception> ret = new List<System.Exception>(1);
            try
            {
                client.invokeWithCallback(addr, b3, new InvokeCallbackAnonymousInnerClass(this, latch, ret), 1000);

            }
            catch (InvokeServerException)
            {
                Assert.True(true);
            }
            catch (RemotingException)
            {
                string errMsg = "ThreadInterruptedException in testCallBackException";
                logger.LogError(errMsg);
                Assert.Null(errMsg);
            }
            latch.Wait();
            Assert.Equal(typeof(InvokeServerException), ret[0].GetType());
        }

        private class InvokeCallbackAnonymousInnerClass : InvokeCallback
        {
            private readonly ExceptionTest outerInstance;

            private CountdownEvent latch;
            private IList<System.Exception> ret;

            public InvokeCallbackAnonymousInnerClass(ExceptionTest outerInstance, CountdownEvent latch, IList<System.Exception> ret)
            {
                this.outerInstance = outerInstance;
                this.latch = latch;
                this.ret = ret;
            }


            public void onResponse(object result)
            {
                Assert.Null("Should not reach here!");
            }

            public void onException(System.Exception e)
            {
                logger.LogError("Error in callback", e);
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

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testCallBackException1() throws ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        public virtual void testCallBackException1()
        {
            server.registerUserProcessor(new AsyncExceptionUserProcessor());
            RequestBody b3 = new RequestBody(3, "Hello world!");
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final java.util.concurrent.CountdownEvent latch = new java.util.concurrent.CountdownEvent(1);
            CountdownEvent latch = new CountdownEvent(1);
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final java.util.List<Throwable> ret = new java.util.ArrayList<Throwable>(1);
            IList<System.Exception> ret = new List<System.Exception>(1);
            try
            {
                client.invokeWithCallback(addr, b3, new InvokeCallbackAnonymousInnerClass2(this, latch, ret), 1000);

            }
            catch (InvokeServerException)
            {
                Assert.True(true);
            }
            catch (RemotingException)
            {
                string errMsg = "ThreadInterruptedException in testCallBackException";
                logger.LogError(errMsg);
                Assert.Null(errMsg);
            }
            latch.Wait();
            Assert.Equal(typeof(InvokeServerException), ret[0].GetType());
        }

        private class InvokeCallbackAnonymousInnerClass2 : InvokeCallback
        {
            private readonly ExceptionTest outerInstance;

            private CountdownEvent latch;
            private IList<System.Exception> ret;

            public InvokeCallbackAnonymousInnerClass2(ExceptionTest outerInstance, CountdownEvent latch, IList<System.Exception> ret)
            {
                this.outerInstance = outerInstance;
                this.latch = latch;
                this.ret = ret;
            }


            public void onResponse(object result)
            {
                Assert.Null("Should not reach here!");
            }

            public void onException(System.Exception e)
            {
                logger.LogError("Error in callback", e);
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