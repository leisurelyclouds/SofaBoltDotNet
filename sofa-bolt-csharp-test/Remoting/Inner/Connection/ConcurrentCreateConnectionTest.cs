using com.alipay.remoting.rpc.common;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting;
using Remoting.Connections;
using Remoting.exception;
using Remoting.rpc;
using System;
using System.Net;
using Xunit;
using System.Collections.Concurrent;
using Remoting.rpc.protocol;

namespace com.alipay.remoting.inner.connection
{
    /// <summary>
    /// Concurrent create connection test
    /// </summary>
    [Collection("Sequential")]
    public class ConcurrentCreateConnectionTest: IDisposable
    {
		private bool InstanceFieldsInitialized = false;

		public ConcurrentCreateConnectionTest()
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

		internal CONNECTEventProcessor serverConnectProcessor = new CONNECTEventProcessor();

		private void init()
		{
			cm = new DefaultClientConnectionManager(connectionSelectStrategy, connectionFactory, connectionEventHandler, connectionEventListener);
			cm.AddressParser = addressParser;
			cm.startup();
			server = new BoltServer(port);
			server.start();
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

        [Fact]
		public virtual void testGetAndCheckConnection()
        {
			Url addr = new Url(ip, port);
			const int connNum = 1;
			const bool warmup = false;

			for (int i = 0; i < 10; ++i)
            {
                Thread thread = new Thread(() =>
                {
                    try
                    {
                        new RpcAddressParser().initUrlArgs(addr);
                        addr.ConnNum = connNum;
                        addr.ConnWarmup = warmup;
                        Connection conn = cm.getAndCreateIfAbsent(addr);
                        Assert.NotNull(conn);
                        Assert.True(conn.Fine);
                    }
                    catch (RemotingException e)
                    {
                        logger.LogError("error!", e);
                        Assert.True(false);
                    }
                    catch (Exception e)
                    {
                        logger.LogError("error!", e);
                        Assert.True(false);
                    }
                });
                thread.Start();
            }
			try
			{
                Thread.Sleep(1000);
			}
			catch (ThreadInterruptedException e)
			{
				logger.LogError("", e);
			}
			Assert.Equal(1, serverConnectProcessor.ConnectTimes);
		}

        [Fact]
		public virtual void testGetAndCheckConnectionMulti()
        {
			Url addr = new Url(ip, port);
			const int connNum = 10;
			const bool warmup = true;

			for (int i = 0; i < 10; ++i)
			{
                Thread thread = new Thread(()=>
                {
                    try
                    {
                        new RpcAddressParser().initUrlArgs(addr);
                        addr.ConnNum = connNum;
                        addr.ConnWarmup = warmup;
                        Connection conn = cm.getAndCreateIfAbsent(addr);
                        Assert.NotNull(conn);
                        Assert.True(conn.Fine);
                    }
                    catch (RemotingException e)
                    {
                        logger.LogError("error!", e);
                        Assert.True(false);
                    }
                    catch (Exception e)
                    {
                        logger.LogError("error!", e);
                        Assert.True(false);
                    }
                });
                thread.Start();
			}
			try
			{
                Thread.Sleep(3000);
			}
			catch (ThreadInterruptedException e)
			{
				logger.LogError("", e);
			}
			Assert.Equal(10, serverConnectProcessor.ConnectTimes);
		}
	}
}