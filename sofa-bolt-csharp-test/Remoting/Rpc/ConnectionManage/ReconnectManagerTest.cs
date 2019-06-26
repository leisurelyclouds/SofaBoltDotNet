using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting;
using Remoting.rpc;
using com.alipay.remoting.rpc.common;
using Xunit;
using Remoting.Config;
using System;

namespace com.alipay.remoting.rpc.connectionmanage
{
    [Collection("Sequential")]
    public class ReconnectManagerTest: IDisposable
    {
		internal static ILogger logger = NullLogger.Instance;

		internal BoltServer server;
		internal RpcClient client;

		internal int port = PortScan.select();

		private bool InstanceFieldsInitialized = false;
        internal string ipPort;

        public ReconnectManagerTest()
        {
            if (!InstanceFieldsInitialized)
            {
                InitializeInstanceFields();
                InstanceFieldsInitialized = true;
            }
        }

        private void InitializeInstanceFields()
        {
            ipPort = "127.0.0.1:" + port;
        }

        internal SimpleServerUserProcessor serverUserProcessor = new SimpleServerUserProcessor();
		internal SimpleClientUserProcessor clientUserProcessor = new SimpleClientUserProcessor();
		internal CONNECTEventProcessor clientConnectProcessor = new CONNECTEventProcessor();
		internal CONNECTEventProcessor serverConnectProcessor = new CONNECTEventProcessor();
		internal DISCONNECTEventProcessor clientDisConnectProcessor = new DISCONNECTEventProcessor();
		internal DISCONNECTEventProcessor serverDisConnectProcessor = new DISCONNECTEventProcessor();

		/// <summary>
		/// parser
		/// </summary>
		private RemotingAddressParser addressParser = new RpcAddressParser();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void stop()
		public void Dispose()
		{
			try
			{
				server.stop();
				client.shutdown();
                Thread.Sleep(100);
			}
			catch (ThreadInterruptedException e)
			{
				logger.LogError("Stop server failed!", e);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testReconnectionBySysetmSetting() throws ThreadInterruptedException, com.alipay.remoting.exception.RemotingException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testReconnectionBySysetmSetting()
		{
			doInit(true, false);
			string addr = $"{ipPort}?zone=RZONE&_CONNECTIONNUM=1";
			Url url = addressParser.parse(addr);

			Connection connection = client.getConnection(url, 1000);
			Assert.Equal(0, clientDisConnectProcessor.DisConnectTimes);
			Assert.Equal(1, clientConnectProcessor.ConnectTimes);
			connection.close();
            Thread.Sleep(2000);
			Assert.Equal(1, clientDisConnectProcessor.DisConnectTimes);
			Assert.Equal(2, clientConnectProcessor.ConnectTimes);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testReconnectionByUserSetting() throws ThreadInterruptedException, com.alipay.remoting.exception.RemotingException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testReconnectionByUserSetting()
		{
			doInit(false, true);
			client.enableReconnectSwitch();

			string addr = $"{ipPort}?zone=RZONE&_CONNECTIONNUM=1";
			Url url = addressParser.parse(addr);

			Connection connection = client.getConnection(url, 1000);
			Assert.Equal(0, clientDisConnectProcessor.DisConnectTimes);
			Assert.Equal(1, clientConnectProcessor.ConnectTimes);
			connection.close();
            Thread.Sleep(2000);
			Assert.Equal(1, clientDisConnectProcessor.DisConnectTimes);
			Assert.Equal(2, clientConnectProcessor.ConnectTimes);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testCancelReConnection() throws ThreadInterruptedException, com.alipay.remoting.exception.RemotingException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testCancelReConnection()
		{
			doInit(false, true);
			client.enableReconnectSwitch();

			string addr = $"{ipPort}?zone=RZONE&_CONNECTIONNUM=1";
			Url url = addressParser.parse(addr);

			client.getConnection(url, 1000);
			Assert.Equal(0, clientDisConnectProcessor.DisConnectTimes);
			Assert.Equal(1, clientConnectProcessor.ConnectTimes);

			client.closeConnection(url);

            Thread.Sleep(1000);
			Assert.Equal(1, clientDisConnectProcessor.DisConnectTimes);
			Assert.Equal(1, clientConnectProcessor.ConnectTimes);
		}

		private void doInit(bool enableSystem, bool enableUser)
		{
			if (enableSystem)
			{
				java.lang.System.setProperty(Configs.CONN_RECONNECT_SWITCH, "true");
			}
			else
			{
				java.lang.System.setProperty(Configs.CONN_RECONNECT_SWITCH, "false");
			}
			server = new BoltServer(port);
			server.start();
			server.addConnectionEventProcessor(ConnectionEventType.CONNECT, serverConnectProcessor);
			server.addConnectionEventProcessor(ConnectionEventType.CLOSE, serverDisConnectProcessor);
			server.registerUserProcessor(serverUserProcessor);

			client = new RpcClient();
			if (enableUser)
			{
				client.enableReconnectSwitch();
			}
			client.addConnectionEventProcessor(ConnectionEventType.CONNECT, clientConnectProcessor);
			client.addConnectionEventProcessor(ConnectionEventType.CLOSE, clientDisConnectProcessor);
			client.registerUserProcessor(clientUserProcessor);
			client.startup();
		}
	}
}