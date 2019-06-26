using com.alipay.remoting.rpc.common;
using DotNetty.Transport.Channels;
using java.lang;
using System.Threading;
using java.net;
using java.util.concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting;
using Remoting.rpc;
using Remoting.rpc.protocol;
using System;
using System.Net;
using System.Net;
using System.Threading;
using Xunit;


namespace com.alipay.remoting.inner.utiltest
{
    /// <summary>
    /// remoting util test
    /// </summary>
    public class RemotingUtilTest: IDisposable
    {

        internal ILogger logger = NullLogger.Instance;

        internal Server server;
        internal RpcClient client;

        private const int port = 1111;
        private const string localIP = "127.0.0.1";

        private static readonly Url connAddress = new Url(localIP, port);
        internal RpcAddressParser parser = new RpcAddressParser();

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Before private void init()
        public virtual void init()
        {
            server = new Server(this);
            try
            {
                server.startServer();
                System.Threading.Thread.Sleep(100);
            }
            catch (InterruptedException e)
            {
                logger.LogError("Start server failed!", e);
            }
            client = new RpcClient();
            client.startup();
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @After public void stop()
        public void Dispose()
        {
            server.stopServer();
            client.closeConnection(connAddress);
            try
            {
                System.Threading.Thread.Sleep(100);
            }
            catch (InterruptedException e)
            {
                logger.LogError("Stop server failed!", e);
            }
        }

        /// <summary>
        /// parse channel to get address(format [ip:port])
        /// </summary>
        [Fact]
        public virtual void testParseRemoteAddress()
        {
            Connection conn;
            try
            {
                parser.initUrlArgs(connAddress);
                conn = client.getConnection(connAddress, 1000);
                IChannel channel = conn.Channel;
                string res = channel.RemoteAddress.ToString();
                Assert.Equal(connAddress.UniqueKey, res);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
                Assert.False(true);
            }

        }

        /// <summary>
        /// parse channel to get address(format [ip:port])
        /// </summary>
        [Fact]
        public virtual void testParseLocalAddress()
        {
            Connection conn;
            try
            {
                parser.initUrlArgs(connAddress);
                conn = client.getConnection(connAddress, 1000);
                IChannel channel = conn.Channel;
                string res = channel.LocalAddress.ToString();
                Assert.NotNull(res);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
                Assert.False(true);
            }

        }

        /// <summary>
        /// parse channel to get ip (format [ip])
        /// </summary>
        [Fact]
        public virtual void testParseRemoteHostIp()
        {
            Connection conn;
            try
            {
                parser.initUrlArgs(connAddress);
                conn = client.getConnection(connAddress, 1000);
                IChannel channel = conn.Channel;
                string res = RemotingUtil.parseRemoteIP(channel);
                Assert.Equal(localIP, res);
            }
            catch (System.Exception)
            {
                Assert.False(true);
            }
        }

        /// <summary>
        /// parse <seealso cref="InetSocketAddress"/> to get address (format [ip:port])
        /// </summary>
        [Fact]
        public virtual void testParseSocketAddressToString()
        {
            string localhostName;
            string localIP;
            try
            {
                InetAddress inetAddress = InetAddress.LocalHost;
                localhostName = inetAddress.HostName;
                localIP = inetAddress.HostAddress;
                if (null == localIP || string.IsNullOrWhiteSpace(localIP))
                {
                    return;
                }
            }
            catch (UnknownHostException)
            {
                localhostName = "localhost";
                localIP = "127.0.0.1";
            }
            IPEndPoint socketAddress = new IPEndPoint(localhostName, port);
            string res = RemotingUtil.parseSocketAddressToString(socketAddress);
            Assert.Equal(localIP + ":" + port, res);
        }

        /// <summary>
        /// parse InetSocketAddress to get address (format [ip:port])
        /// 
        /// e.g.1 /127.0.0.1:1234 -> 127.0.0.1:1234
        /// e.g.2 sofatest-2.stack.alipay.net/10.209.155.54:12200 -> 10.209.155.54:12200
        /// </summary>
        [Fact]
        public virtual void testParseSocketAddressToString_MuiltiFormatTest()
        {
            IPEndPoint socketAddress = new IPEndPoint(IPAddress.Parse("/127.0.0.1"), 1111);
            string res = RemotingUtil.parseSocketAddressToString(socketAddress);
            Assert.Equal("127.0.0.1:1111", res);

            socketAddress = new InetSocketAddress("sofatest-2.stack.alipay.net/127.0.0.1", 12200);
            res = RemotingUtil.parseSocketAddressToString(socketAddress);
            Assert.Equal("127.0.0.1:12200", res);
        }

        /// <summary>
        /// parse InetSocketAddress to get ip (format [ip])
        /// </summary>
        [Fact]
        public virtual void testParseSocketAddressToHostIp()
        {
            string localhostName;
            IPAddress localIP;
            try
            {
                var inetAddress = IPAddress.Loopback;
                localhostName = Dns.GetHostName();
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName()); // `Dns.Resolve()` method is deprecated.
                localIP = ipHostInfo.AddressList[0];
                if (null == localIP)
                {
                    return;
                }
            }
            catch (UnknownHostException)
            {
                localhostName = "localhost";
                localIP = IPAddress.Parse("127.0.0.1");
            }

            IPEndPoint socketAddress = new IPEndPoint(IPAddress.Parse(localhostName), port);
            string res = RemotingUtil.parseSocketAddressToHostIp(socketAddress);
            Assert.Equal(localIP, res);
        }

        internal class Server
        {
            private readonly RemotingUtilTest outerInstance;

            public Server(RemotingUtilTest outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            internal ILogger logger = NullLogger.Instance;
            internal RpcServer server;

            public virtual void startServer()
            {
                server = new RpcServer(port);
                server.registerUserProcessor(new SyncUserProcessorAnonymousInnerClass(this));
                server.start();
            }

            private class SyncUserProcessorAnonymousInnerClass : SyncUserProcessor
            {
                private readonly Server outerInstance;

                public SyncUserProcessorAnonymousInnerClass(Server outerInstance)
                {
                    this.outerInstance = outerInstance;
                    executor = new ThreadPoolExecutor(1, 3, 60, TimeUnit.SECONDS, new ArrayBlockingQueue(4), new NamedThreadFactory("Request-process-pool"));
                }

                internal ThreadPoolExecutor executor;

                public override object handleRequest(BizContext bizCtx, object request)
                {
                    outerInstance.logger.LogWarning("Request received:" + request);
                    return "Hello world!";
                }

                public override Type interest()
                {
                    return typeof(RequestBody).ToString();
                }


                public Executor Executor
                {
                    get
                    {
                        return executor;
                    }
                }

            }

            public virtual void stopServer()
            {
                server.stop();
            }
        }

    }

}