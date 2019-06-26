using System;
using System.Net;
using System.Collections.Generic;
using com.alipay.remoting.rpc.common;
using System.Threading;
using java.util.concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting;
using Remoting.exception;
using Remoting.rpc;
using Xunit;


namespace com.alipay.remoting.rpc.addressargs
{
    /// <summary>
    /// address args test [_CONNECTIONNUM]
    /// </summary>
    [Collection("Sequential")]
    public class AddressArgs_CONNECTIONNUM_Test : IDisposable
    {
        private bool InstanceFieldsInitialized = false;

        public AddressArgs_CONNECTIONNUM_Test()
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

        /// <summary>
        /// url: url with different args
        /// invokeTimes: rpc invoke times
        /// expectConnTimes: expect connection times for client and server
        /// expectMaxFirstInvokeTimeDuration: expect first invoke time cost, if assign -1, then will not do check
        /// </summary>
        /// <param name="url"> </param>
        /// <param name="invokeTimes"> </param>
        /// <param name="expectConnTimes"> </param>
        /// <param name="expectMaxFirstInvokeTimeDuration"> </param>
        private void doTest(string url, RequestBody.InvokeType type, int invokeTimes, int expectConnTimes, int expectMaxFirstInvokeTimeDuration)
        {
            try
            {
                RpcAddressParser parser = new RpcAddressParser();
                //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                //ORIGINAL LINE: final com.alipay.remoting.Url addr = parser.parse(url);
                Url addr = parser.parse(url);
                for (int i = 0; i < invokeTimes; i++)
                {
                    long start = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                    string ret = (string)doInvoke(type, url);
                    long end = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                    logger.LogWarning("WITH WARMUP, first invoke cost ->" + (end - start));
                    if ((end - start) > expectMaxFirstInvokeTimeDuration && expectMaxFirstInvokeTimeDuration != -1)
                    {
                        Assert.Null("Should not reach here, First invoke cost too much time [" + (end - start) + "ms], expect limit in [" + expectMaxFirstInvokeTimeDuration + "ms]!");
                    }
                    if (!type.Equals(RequestBody.InvokeType.ONEWAY))
                    {
                        Assert.Equal(ret, RequestBody.DEFAULT_SERVER_RETURN_STR);
                    }
                }

                if (addr.ConnWarmup)
                {
                    Thread.Sleep(200); // must wait, to wait event finish
                    Assert.Equal(expectConnTimes, serverConnectProcessor.ConnectTimes);
                    Assert.Equal(expectConnTimes, clientConnectProcessor.ConnectTimes);

                    client.closeConnection(addr);
                    Thread.Sleep(200); // must wait, to wait event finish
                    Assert.Equal(expectConnTimes, serverDisConnectProcessor.DisConnectTimes);
                    Assert.Equal(expectConnTimes, clientDisConnectProcessor.DisConnectTimes);
                }
                else
                {
                    Thread.Sleep(200); // must wait, to wait event finish
                    Assert.True(serverConnectProcessor.ConnectTimes >= expectConnTimes);
                    Assert.True(clientConnectProcessor.ConnectTimes >= expectConnTimes);

                    client.closeConnection(addr);
                    Thread.Sleep(200); // must wait, to wait event finish
                    Assert.True(serverDisConnectProcessor.DisConnectTimes >= expectConnTimes);
                    Assert.True(clientDisConnectProcessor.DisConnectTimes >= expectConnTimes);
                }
            }
            catch (RemotingException e)
            {
                logger.LogError("Exception caught in sync!", e);
                Assert.Null("Should not reach here!");
            }
            catch (ThreadInterruptedException e)
            {
                logger.LogError("ThreadInterruptedException in sync", e);
                Assert.Null("Should not reach here!");
            }
        }

        /// <summary>
        /// do invoke
        /// </summary>
        /// <param name="type"> </param>
        /// <param name="url">
        /// @return </param>
        /// <exception cref="RemotingException"> </exception>
        /// <exception cref="ThreadInterruptedException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: private Object doInvoke(com.alipay.remoting.rpc.common.RequestBody.InvokeType type, String url) throws com.alipay.remoting.exception.RemotingException, ThreadInterruptedException
        private object doInvoke(RequestBody.InvokeType type, string url)
        {
            RequestBody b1 = new RequestBody(1, "hello world");
            object obj = null;
            if (type.Equals(RequestBody.InvokeType.ONEWAY))
            {
                client.oneway(url, b1);
            }
            else if (type.Equals(RequestBody.InvokeType.SYNC))
            {
                obj = client.invokeSync(url, b1, 3000);
            }
            else if (type.Equals(RequestBody.InvokeType.FUTURE))
            {
                RpcResponseFuture future = client.invokeWithFuture(url, b1, 3000);
                obj = future.get(3000);
            }
            else if (type.Equals(RequestBody.InvokeType.CALLBACK))
            {
                //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                //ORIGINAL LINE: final java.util.List<String> rets = new java.util.ArrayList<String>(1);
                IList<string> rets = new List<string>(1);
                //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                //ORIGINAL LINE: final java.util.concurrent.CountdownEvent latch = new java.util.concurrent.CountdownEvent(1);
                CountdownEvent latch = new CountdownEvent(1);
                client.invokeWithCallback(url, b1, new InvokeCallbackAnonymousInnerClass(this, rets, latch), 3000);
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
                    Assert.Null("No result of callback! Maybe exception caught!");
                }
                obj = rets[0];
            }
            return obj;

        }

        private class InvokeCallbackAnonymousInnerClass : InvokeCallback
        {
            private readonly AddressArgs_CONNECTIONNUM_Test outerInstance;

            private IList<string> rets;
            private CountdownEvent latch;

            public InvokeCallbackAnonymousInnerClass(AddressArgs_CONNECTIONNUM_Test outerInstance, IList<string> rets, CountdownEvent latch)
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

        /// <summary>
        /// reset all times
        /// </summary>
        private void doResetTimes()
        {
            this.clientConnectProcessor.reset();
            this.serverConnectProcessor.reset();
            this.clientDisConnectProcessor.reset();
            this.serverDisConnectProcessor.reset();
        }

        [Fact]
        public virtual void test_connNum_10_warmup_True_invoke_1times()
        {
            string url = addr + "?_CONNECTTIMEOUT=1000&_TIMEOUT=5000&_CONNECTIONNUM=10&_CONNECTIONWARMUP=true";
            foreach (RequestBody.InvokeType type in Enum.GetValues(typeof(RequestBody.InvokeType)))
            {
                doResetTimes();
                doTest(url, type, 1, 10, -1);
            }
        }

        [Fact]
        public virtual void test_connNum_10_warmup_False_invoke_1times()
        {
            string url = addr + "?_CONNECTTIMEOUT=1000&_TIMEOUT=5000&_CONNECTIONNUM=10&_CONNECTIONWARMUP=false";
            foreach (RequestBody.InvokeType type in Enum.GetValues(typeof(RequestBody.InvokeType)))
            {
                doResetTimes();
                doTest(url, type, 1, 1, -1);
            }
        }

        [Fact]
        public virtual void test_connNum_1_warmup_True_invoke_1times()
        {
            string url = addr + "?_CONNECTTIMEOUT=1000&_TIMEOUT=5000&_CONNECTIONNUM=1&_CONNECTIONWARMUP=true";
            foreach (RequestBody.InvokeType type in Enum.GetValues(typeof(RequestBody.InvokeType)))
            {
                doResetTimes();
                doTest(url, type, 1, 1, -1);
            }
        }

        [Fact]
        public virtual void test_connNum_1_warmup_False_invoke_3times()
        {
            string url = addr + "?_CONNECTTIMEOUT=1000&_TIMEOUT=5000&_CONNECTIONNUM=1&_CONNECTIONWARMUP=false";
            foreach (RequestBody.InvokeType type in Enum.GetValues(typeof(RequestBody.InvokeType)))
            {
                doResetTimes();
                doTest(url, type, 1, 1, -1);
            }
        }

        [Fact]
        public virtual void test_connNum_2_warmup_False_invoke_3times()
        {
            string url = addr + "?_CONNECTTIMEOUT=1000&_TIMEOUT=5000&_CONNECTIONNUM=2&_CONNECTIONWARMUP=false";
            foreach (RequestBody.InvokeType type in Enum.GetValues(typeof(RequestBody.InvokeType)))
            {
                doResetTimes();
                doTest(url, type, 1, 1, -1);
            }
        }

        [Fact]
        public virtual void test_connNum_2_warmup_True_invoke_3times()
        {
            string url = addr + "?_CONNECTTIMEOUT=1000&_TIMEOUT=5000&_CONNECTIONNUM=2&_CONNECTIONWARMUP=true";
            foreach (RequestBody.InvokeType type in Enum.GetValues(typeof(RequestBody.InvokeType)))
            {
                doResetTimes();
                doTest(url, type, 1, 2, -1);
            }
        }
    }

}