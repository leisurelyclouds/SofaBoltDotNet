using Xunit;
using Remoting;
using Remoting.Connections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using com.alipay.remoting.rpc.common;
using Remoting.rpc;
using System.Threading;
using Remoting.exception;
using java.util;
using System;
using System.Net;
using Remoting.rpc.protocol;
using System.Collections.Concurrent;

namespace com.alipay.remoting.inner.connection
{
    /// <summary>
    /// Rpc connection manager test
    /// </summary>
    [Collection("Sequential")]
    public class RpcConnectionManagerTest: IDisposable
    {
		private bool InstanceFieldsInitialized = false;

        public RpcConnectionManagerTest()
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
            connectionFactory = new RpcConnectionFactory(userProcessors, new RpcClient());
            addr = ip + ":" + port;
            poolKey = ip + ":" + port;
            url = new Url(ip, port);
        }

        private static readonly ILogger logger = NullLogger.Instance;

        private ConcurrentDictionary<Type, UserProcessor> userProcessors = new ConcurrentDictionary<Type, UserProcessor>();

        private DefaultClientConnectionManager cm;
        private ConnectionSelectStrategy connectionSelectStrategy = new RandomSelectStrategy(null);
        private RemotingAddressParser addressParser = new RpcAddressParser();
        private ConnectionFactory connectionFactory;
        private ConnectionEventHandler connectionEventHandler = new RpcConnectionEventHandler();
        private ConnectionEventListener connectionEventListener = new ConnectionEventListener();

        private BoltServer server;

        private IPAddress ip = IPAddress.Parse("127.0.0.1");
        private int port = PortScan.select();
        private string addr;
        private string poolKey;
        private Url url;

        internal CONNECTEventProcessor serverConnectProcessor = new CONNECTEventProcessor();

        private void init()
        {
            cm = new DefaultClientConnectionManager(connectionSelectStrategy, connectionFactory, connectionEventHandler, connectionEventListener);
            cm.AddressParser = addressParser;
            cm.startup();
            server = new BoltServer(port);
            server.start();
            server.addConnectionEventProcessor(ConnectionEventType.CONNECT, serverConnectProcessor);
            this.addressParser.initUrlArgs(url);
        }

        public void Dispose()
        {
            try
            {
                server.stop();
                Thread.Sleep(1);
            }
            catch (ThreadInterruptedException e)
            {
                logger.LogError("Stop server failed!", e);
            }
        }

        [Fact]
        public virtual void testAdd()
        {
            Connection conn = AConn;
            cm.add(conn);
            Assert.Equal(1, cm.count(poolKey));
        }

        [Fact]
        public virtual void testAddWithPoolKey()
        {
            Connection conn = AConn;
            cm.add(conn, poolKey);
            Assert.Equal(1, cm.count(poolKey));
        }

        [Fact]
        public virtual void testAddWconnithPoolKey_multiPoolKey()
        {
            Connection conn = AConn;
            cm.add(conn, poolKey);
            cm.add(conn, "GROUP1");
            cm.add(conn, "GROUP2");
            Assert.Equal(1, cm.count(poolKey));
            Assert.Equal(1, cm.count("GROUP1"));
            Assert.Equal(1, cm.count("GROUP2"));

            cm.remove(conn, poolKey);
            Assert.True(conn.Fine);
            Assert.True(cm.get(poolKey) == null);
            Assert.True(cm.get("GROUP1").Fine);
            Assert.True(cm.get("GROUP2").Fine);

            cm.remove(conn, "GROUP1");
            cm.remove(conn, "GROUP2");
            Thread.Sleep(1000);
            Assert.False(conn.Fine);
            Assert.True(cm.get(poolKey) == null);
            Assert.True(cm.get("GROUP1") == null);
            Assert.True(cm.get("GROUP2") == null);
        }

        [Fact]
        public virtual void testGet()
        {
            Connection pool = cm.get(poolKey);
            Assert.Null(pool);
            cm.add(AConn);
            Assert.NotNull(cm.get(poolKey));
        }

        [Fact]
        public virtual void testGetAllWithPoolKey()
        {
            cm.add(AConn);
            cm.add(AConn);
            cm.add(AConn);
            Assert.Equal(3, cm.getAll(poolKey).Count);
        }

        [Fact]
        public virtual void testGetAll()
        {
			cm.add(AConn);
			cm.add(AConn);
			cm.add(AConn);
			cm.add(AConn);
			var conns = cm.All;
			Assert.Single(conns);
            conns.TryGetValue(poolKey, out var value);
            Assert.Equal(4, value.Count);
		}

        [Fact]
        public virtual void testRemoveConn()
        {
            Connection conn1 = AConn;
            conn1.addPoolKey("hehe");
            Connection conn2 = AConn;
            conn2.addPoolKey("hehe");
            cm.add(conn1);
            cm.add(conn2);
            Assert.Equal(2, cm.count(poolKey));
            cm.remove(conn1);
            Assert.Equal(1, cm.count(poolKey));
            cm.remove(conn2);
            Assert.Equal(0, cm.count(poolKey));
        }

        [Fact]
        public virtual void testRemoveConnWithSpecifiedPoolkey()
        {
            Connection conn1 = AConn;
            conn1.addPoolKey("hehe");
            Connection conn2 = AConn;
            conn2.addPoolKey("hehe");
            cm.add(conn1);
            cm.add(conn2);
            Assert.Equal(2, cm.count(poolKey));
            cm.remove(conn1, poolKey);
            Assert.Equal(1, cm.count(poolKey));
        }

        [Fact]
        public virtual void testRemoveAllConnsOfSpecifiedPoolKey()
        {
            Connection conn1 = AConn;
            conn1.addPoolKey("hehe");
            Connection conn2 = AConn;
            conn2.addPoolKey("hehe");
            cm.add(conn1);
            cm.add(conn2);
            Assert.Equal(2, cm.count(poolKey));
            cm.remove(poolKey);
            Assert.Equal(0, cm.count(poolKey));
        }

        [Fact]
        public virtual void testGetAndCreateIfAbsent()
        {
            try
            {
                Connection conn = cm.getAndCreateIfAbsent(url);
                Assert.NotNull(conn);
            }
            catch (RemotingException)
            {
                Assert.Null("should not reach here!");
            }
            catch (ThreadInterruptedException)
            {
                Assert.Null("should not reach here!");
            }

        }

        [Fact]
        public virtual void testCreateUsingUrl()
        {
            try
            {
                Connection conn = cm.create(url);
                Assert.NotNull(conn);
            }
            catch (RemotingException)
            {
                Assert.Null("should not reach here!");
            }

        }

        [Fact]
        public virtual void testCreateUsingAddress()
        {
            try
            {
                Connection conn = cm.create(addr, 1000);
                Assert.NotNull(conn);
            }
            catch (RemotingException)
            {
                Assert.Null("should not reach here!");
            }

        }

        [Fact]
        public virtual void testCreateUsingIpPort()
        {
            try
            {
                Connection conn = cm.create(ip, port, 1000);
                Assert.NotNull(conn);
            }
            catch (RemotingException)
            {
                Assert.Null("should not reach here!");
            }

        }

        // ~~~ combinations

        [Fact]
        public virtual void testGetAndCheckConnection()
        {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final com.alipay.remoting.Url addr = new com.alipay.remoting.Url(ip, port);
            Url addr = new Url(ip, port);

            try
            {
                this.addressParser.initUrlArgs(addr);
                Connection conn = cm.getAndCreateIfAbsent(addr);
                Assert.True(conn.Fine);
            }
            catch (RemotingException)
            {
                Assert.Null("RemotingException!");
            }
            catch (ThreadInterruptedException)
            {
                Assert.Null("ThreadInterruptedException!");
            }
        }

        [Fact]
        public virtual void testCheckConnectionException()
        {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final com.alipay.remoting.Url addr = new com.alipay.remoting.Url(ip, port);
            Url addr = new Url(ip, port);

            Connection conn = null;
            try
            {
                cm.check(conn);
                Assert.True(false);
            }
            catch (RemotingException e)
            {
                logger.LogError("Connection null", e);
                Assert.True(true);
            }

            try
            {
                this.addressParser.initUrlArgs(addr);
                conn = cm.getAndCreateIfAbsent(addr);
                Assert.Equal(1, cm.count(addr.UniqueKey));
                conn.close();
                Thread.Sleep(100);
                cm.check(conn);
                Assert.True(false);
            }
            catch (RemotingException)
            {
                // test  remove success when do check
                Assert.Equal(0, cm.count(addr.UniqueKey));
                Assert.True(true);
            }
            catch (ThreadInterruptedException e)
            {
                logger.LogError("", e);
            }
        }

        [Fact]
        public virtual void testAddAndRemoveConnection()
        {
            string poolKey = ip + ":" + port;
            int connectTimeoutMillis = 1000;

            Connection conn = null;
            try
            {
                conn = cm.create(ip, port, connectTimeoutMillis);
                conn.addPoolKey(poolKey);
            }
            catch (Exception e)
            {
                logger.LogError("", e);
            }
            cm.add(conn);
            Assert.Equal(1, cm.count(poolKey));
            cm.remove(conn);
            Assert.Equal(0, cm.count(poolKey));
        }

        [Fact]
        public virtual void testRemoveAddress()
        {
            string poolKey = ip + ":" + port;
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final com.alipay.remoting.Url addr = new com.alipay.remoting.Url(ip, port);
            Url addr = new Url(ip, port);

            try
            {
                this.addressParser.initUrlArgs(addr);
                Connection conn = cm.getAndCreateIfAbsent(addr);
                try
                {
                    cm.check(conn);
                }
                catch (Exception)
                {
                    Assert.True(false);
                }
                conn = cm.getAndCreateIfAbsent(addr);
                cm.check(conn);
                Assert.True(true);
                cm.remove(poolKey);
                Assert.Equal(0, cm.count(poolKey));
            }
            catch (Exception)
            {
                Assert.True(false);
            }
        }

        [Fact]
        public virtual void testConnectionCloseAndConnectionManagerRemove()
        {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final com.alipay.remoting.Url addr = new com.alipay.remoting.Url(ip, port);
            Url addr = new Url(ip, port);

            this.addressParser.initUrlArgs(addr);
            Connection conn = null;
            try
            {
                conn = cm.getAndCreateIfAbsent(addr);
            }
            catch (ThreadInterruptedException)
            {
                Assert.Null("ThreadInterruptedException!");
            }

            Assert.Equal(1, cm.count(addr.UniqueKey));
            conn.close();
            Thread.Sleep(100);
            Assert.Equal(0, cm.count(addr.UniqueKey));
        }

        /// <summary>
        /// get a connection
        /// 
        /// @return
        /// </summary>
        private Connection AConn
        {
            get
            {
                try
                {
                    return cm.create(ip, port, 1000);
                }
                catch (RemotingException)
                {
                    logger.LogError("Create connection failed!");
                }
                return null;
            }
        }
    }

}