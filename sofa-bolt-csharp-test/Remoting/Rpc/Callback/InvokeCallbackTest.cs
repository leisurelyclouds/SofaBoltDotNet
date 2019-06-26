using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting;
using Remoting.rpc;
using com.alipay.remoting.rpc.common;
using Xunit;
using java.util.concurrent;
using java.util.concurrent.atomic;
using System;
using System.Net;

namespace com.alipay.remoting.rpc.callback
{
    /// <summary>
    /// Invoke call back test
    /// </summary>
    [Collection("Sequential")]
    public class InvokeCallbackTest : IDisposable
    {
        private bool InstanceFieldsInitialized = false;

        public InvokeCallbackTest()
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
        //ORIGINAL LINE: @Test public void testCallbackInvokeTimes()
        public virtual void testCallbackInvokeTimes()
        {
            RequestBody req = new RequestBody(1, "hello world sync");
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger callbackInvokTimes = new java.util.concurrent.atomic.AtomicInteger();
            AtomicInteger callbackInvokTimes = new AtomicInteger();
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final java.util.concurrent.CountdownEvent latch = new java.util.concurrent.CountdownEvent(1);
            CountdownEvent latch = new CountdownEvent(1);
            try
            {
                client.invokeWithCallback(addr, req, new InvokeCallbackAnonymousInnerClass(this, callbackInvokTimes, latch), 3000);
                Thread.Sleep(500); // wait callback execution
                if (latch.Wait(3000))
                {
                    Assert.Equal(1, callbackInvokTimes.get());
                }
            }
            catch (Exception e)
            {
                logger.LogError("Exception", e);
            }
        }

        private class InvokeCallbackAnonymousInnerClass : InvokeCallback
        {
            private readonly InvokeCallbackTest outerInstance;

            private AtomicInteger callbackInvokTimes;
            private CountdownEvent latch;

            public InvokeCallbackAnonymousInnerClass(InvokeCallbackTest outerInstance, AtomicInteger callbackInvokTimes, CountdownEvent latch)
            {
                this.outerInstance = outerInstance;
                this.callbackInvokTimes = callbackInvokTimes;
                this.latch = latch;
                executor = Executors.newCachedThreadPool();
            }

            internal Executor executor;

            public void onResponse(object result)
            {
                callbackInvokTimes.getAndIncrement();
                if (!latch.IsSet)
            {
                latch.Signal();
            }
                throw new Exception("Hehe Exception");
            }

            public void onException(Exception e)
            {
                callbackInvokTimes.getAndIncrement();
                if (!latch.IsSet)
            {
                latch.Signal();
            }
                throw new Exception("Hehe Exception");
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