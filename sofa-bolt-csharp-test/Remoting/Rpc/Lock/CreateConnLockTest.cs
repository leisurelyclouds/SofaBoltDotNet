using java.lang;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting;
using Remoting.rpc;
using com.alipay.remoting.rpc.common;
using Xunit;
using Remoting.exception;
using System;
using System.Net;
using java.util.concurrent.atomic;

namespace com.alipay.remoting.rpc.@lock
{
    /// <summary>
    /// alipay-com/bolt#110
    /// </summary>
    [Collection("Sequential")]
    public class CreateConnLockTest : IDisposable
    {
        private bool InstanceFieldsInitialized = false;

        public CreateConnLockTest()
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
        internal string bad_ip = "127.0.0.2";
        internal string ip_prefix = "127.0.0.";
        internal string addr;

        internal int invokeTimes = 3;

        internal ConcurrentServerUserProcessor serverUserProcessor = new ConcurrentServerUserProcessor();
        internal SimpleClientUserProcessor clientUserProcessor = new SimpleClientUserProcessor();
        internal CONNECTEventProcessor clientConnectProcessor = new CONNECTEventProcessor();
        internal CONNECTEventProcessor serverConnectProcessor = new CONNECTEventProcessor();
        internal DISCONNECTEventProcessor clientDisConnectProcessor = new DISCONNECTEventProcessor();
        internal DISCONNECTEventProcessor serverDisConnectProcessor = new DISCONNECTEventProcessor();

        private AtomicBoolean whetherConnectTimeoutConsumedTooLong = new AtomicBoolean();

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Before private void init()
        private void init()
        {
            whetherConnectTimeoutConsumedTooLong.set(false);
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
                System.Threading.Thread.Sleep(100);
            }
            catch (ThreadInterruptedException e)
            {
                logger.LogError("Stop server failed!", e);
            }
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testSync_DiffAddressOnePort() throws ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        public virtual void testSync_DiffAddressOnePort()
        {
            for (int i = 0; i < invokeTimes; ++i)
            {
                Url url = new Url(IPAddress.Parse(ip_prefix + i), port); //127.0.0.1:12200, 127.0.0.2:12200, 127.0.0.3:12200...
                MyThread thread = new MyThread(this, url, 1, false);
                new java.lang.Thread(thread).start();
            }

            System.Threading.Thread.Sleep(5000);
            Assert.False(whetherConnectTimeoutConsumedTooLong.get());
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testSync_OneAddressDiffPort() throws ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        public virtual void testSync_OneAddressDiffPort()
        {
            for (int i = 0; i < invokeTimes; ++i)
            {
                Url url = new Url(ip, port++); //127.0.0.1:12200, 127.0.0.2:12201, 127.0.0.3:12202...
                MyThread thread = new MyThread(this, url, 1, false);
                new java.lang.Thread(thread).start();
            }

            System.Threading.Thread.Sleep(5000);
            Assert.False(whetherConnectTimeoutConsumedTooLong.get());
        }

        /// <summary>
        /// enable this case only when non-lock feature of ConnectionManager implemented
        /// </summary>
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testSync_OneAddressOnePort() throws ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        public virtual void testSync_OneAddressOnePort()
        {
            for (int i = 0; i < invokeTimes; ++i)
            {
                Url url = new Url(IPAddress.Parse(bad_ip), port); //127.0.0.2:12200
                MyThread thread = new MyThread(this, url, 1, false);
                new java.lang.Thread(thread).start();
            }

            System.Threading.Thread.Sleep(5000);
            Assert.False(whetherConnectTimeoutConsumedTooLong.get());
        }

        internal class MyThread : Runnable
        {
            private readonly CreateConnLockTest outerInstance;

            internal Url url;
            internal int connNum;
            internal bool warmup;
            internal RpcAddressParser parser;

            public MyThread(CreateConnLockTest outerInstance, Url url, int connNum, bool warmup)
            {
                this.outerInstance = outerInstance;
                this.url = url;
                this.connNum = connNum;
                this.warmup = warmup;
                this.parser = new RpcAddressParser();
            }

            public void run()
            {
                InvokeContext ctx = new InvokeContext();
                try
                {
                    RequestBody req = new RequestBody(1, "hello world sync");
                    url.ConnectTimeout = 100; // default to be 1000
                    url.ConnNum = connNum;
                    url.ConnWarmup = warmup;
                    this.parser.initUrlArgs(url);
                    outerInstance.client.invokeSync(url, req, ctx, 3000);
                    long time = getAndPrintCreateConnTime(ctx);
                    //                Assert.True(time < 1500);
                }
                catch (RemotingException e)
                {
                    logger.LogError("error!", e);
                    long time = getAndPrintCreateConnTime(ctx);
                    //                Assert.True(time < 1500);
                }
                catch (System.Exception e)
                {
                    logger.LogError("error!", e);
                    long time = getAndPrintCreateConnTime(ctx);
                    //                Assert.True(time < 1500);
                }
            }

            internal virtual long getAndPrintCreateConnTime(InvokeContext ctx)
            {
                long time = ctx.get(InvokeContext.CLIENT_CONN_CREATETIME) == null ? -1L : ((long?)ctx.get(InvokeContext.CLIENT_CONN_CREATETIME)).Value;
                if (time > 1500)
                {
                    outerInstance.whetherConnectTimeoutConsumedTooLong.set(true);
                }
                logger.LogWarning("CREATE CONN TIME CONSUMED: " + time);
                return time;
            }

        }
    }
}