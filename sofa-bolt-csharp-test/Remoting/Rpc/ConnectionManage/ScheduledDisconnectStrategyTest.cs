using Remoting;
using Remoting.rpc;
using com.alipay.remoting.rpc.common;
using Xunit;
using System;
using Remoting.Config;

namespace com.alipay.remoting.rpc.connectionmanage
{
    [Collection("Sequential")]
    public class ScheduledDisconnectStrategyTest: IDisposable
    {

		internal BoltServer server;
		internal RpcClient client;

        internal int port = PortScan.select();

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

		public void Dispose()
		{
			client.shutdown();
			server.stop();
		}

        [Fact]
		public virtual void testConnectionMonitorBySystemSetting()
		{
			java.lang.System.setProperty(Configs.CONN_MONITOR_INITIAL_DELAY, "2000");
			java.lang.System.setProperty(Configs.CONN_MONITOR_PERIOD, "100");
			doInit(true, false);
			string addr = "127.0.0.1:" + port + "?zone=RZONE&_CONNECTIONNUM=8&_CONNECTIONWARMUP=false";
			Url url = addressParser.parse(addr);

			for (int i = 0; i < 8; ++i)
			{
				client.getConnection(url, 1000);
			}

		    System.Threading.Thread.Sleep(2150);
			Assert.True(1 <= clientDisConnectProcessor.DisConnectTimes);
            Assert.Equal(9, clientConnectProcessor.ConnectTimes);
            System.Threading.Thread.Sleep(200);
			Assert.True(2 <= clientDisConnectProcessor.DisConnectTimes);
			Assert.True(9 <= clientConnectProcessor.ConnectTimes);
		    System.Threading.Thread.Sleep(400);
			Assert.True(4 <= clientDisConnectProcessor.DisConnectTimes);
			Assert.True(9 <= clientConnectProcessor.ConnectTimes);
		    System.Threading.Thread.Sleep(200);
			Assert.True(5 <= clientDisConnectProcessor.DisConnectTimes);
		    System.Threading.Thread.Sleep(200);
			Assert.True(5 <= clientDisConnectProcessor.DisConnectTimes);
		    System.Threading.Thread.Sleep(100);
			Assert.True(6 <= clientDisConnectProcessor.DisConnectTimes);
			Assert.True(10 <= clientConnectProcessor.ConnectTimes);
		}

        [Fact]
		public virtual void testConnectionMonitorByUserSetting()
		{
			java.lang.System.setProperty(Configs.CONN_MONITOR_INITIAL_DELAY, "2000");
			java.lang.System.setProperty(Configs.CONN_MONITOR_PERIOD, "100");
			doInit(false, true);
			string addr = "127.0.0.1:" + port + "?zone=RZONE&_CONNECTIONNUM=8&_CONNECTIONWARMUP=false";
			Url url = addressParser.parse(addr);

			for (int i = 0; i < 8; ++i)
			{
				client.getConnection(url, 1000);
			}

		    System.Threading.Thread.Sleep(2150);
			Assert.True(1 <= clientDisConnectProcessor.DisConnectTimes);
			Assert.Equal(9, clientConnectProcessor.ConnectTimes);
		    System.Threading.Thread.Sleep(200);
			Assert.True(2 <= clientDisConnectProcessor.DisConnectTimes);
			Assert.True(9 <= clientConnectProcessor.ConnectTimes);
		    System.Threading.Thread.Sleep(400);
			Assert.True(4 <= clientDisConnectProcessor.DisConnectTimes);
			Assert.True(9 <= clientConnectProcessor.ConnectTimes);
		    System.Threading.Thread.Sleep(200);
			Assert.True(5 <= clientDisConnectProcessor.DisConnectTimes);
		    System.Threading.Thread.Sleep(200);
			Assert.True(5 <= clientDisConnectProcessor.DisConnectTimes);
		    System.Threading.Thread.Sleep(100);
			Assert.True(6 <= clientDisConnectProcessor.DisConnectTimes);
			Assert.True(10 <= clientConnectProcessor.ConnectTimes);
		}

        [Fact]
		public virtual void testCloseFreshSelectConnections_bySystemSetting()
		{
			java.lang.System.setProperty(Configs.RETRY_DETECT_PERIOD, "500");
			java.lang.System.setProperty(Configs.CONN_MONITOR_INITIAL_DELAY, "2000");
			java.lang.System.setProperty(Configs.CONN_MONITOR_PERIOD, "100");
			java.lang.System.setProperty(Configs.CONN_THRESHOLD, "0");
			doInit(true, false);

			string addr = "127.0.0.1:" + port + "?zone=RZONE&_CONNECTIONNUM=1";
			Url url = addressParser.parse(addr);

			Connection connection = client.getConnection(url, 1000);
			connection.addInvokeFuture(new DefaultInvokeFuture(1, null, null, RpcCommandType.REQUEST, null));
		    System.Threading.Thread.Sleep(2100);
			Assert.True(0 == clientDisConnectProcessor.DisConnectTimes);
			Assert.Equal(1, clientConnectProcessor.ConnectTimes);
			connection.removeInvokeFuture(1);
			/* Monitor task sleep 500ms*/
		    System.Threading.Thread.Sleep(100);
			Assert.True(0 <= clientDisConnectProcessor.DisConnectTimes);
		    System.Threading.Thread.Sleep(500);
			Assert.True(0 <= clientDisConnectProcessor.DisConnectTimes);
		}

        [Fact]
		public virtual void testCloseFreshSelectConnections_byUserSetting()
		{
			java.lang.System.setProperty(Configs.RETRY_DETECT_PERIOD, "500");
			java.lang.System.setProperty(Configs.CONN_MONITOR_INITIAL_DELAY, "2000");
			java.lang.System.setProperty(Configs.CONN_MONITOR_PERIOD, "100");
			java.lang.System.setProperty(Configs.CONN_THRESHOLD, "0");
			doInit(false, true);

			string addr = "127.0.0.1:" + port + "?zone=RZONE&_CONNECTIONNUM=1";
			Url url = addressParser.parse(addr);

			Connection connection = client.getConnection(url, 1000);
			connection.addInvokeFuture(new DefaultInvokeFuture(1, null, null, RpcCommandType.REQUEST, null));
		    System.Threading.Thread.Sleep(2100);
			Assert.True(0 == clientDisConnectProcessor.DisConnectTimes);
			Assert.Equal(1, clientConnectProcessor.ConnectTimes);
			connection.removeInvokeFuture(1);
			/* Monitor task sleep 500ms*/
		    System.Threading.Thread.Sleep(100);
			Assert.Equal(1, clientDisConnectProcessor.DisConnectTimes);
		    System.Threading.Thread.Sleep(500);
			Assert.True(0 <= clientDisConnectProcessor.DisConnectTimes);
		}

        [Fact]
		public virtual void testDisconnectStrategy_bySystemSetting()
		{
			java.lang.System.setProperty(Configs.CONN_MONITOR_SWITCH, "true");
			java.lang.System.setProperty(Configs.CONN_MONITOR_INITIAL_DELAY, "2000");
			java.lang.System.setProperty(Configs.CONN_MONITOR_PERIOD, "100");
			java.lang.System.setProperty(Configs.CONN_THRESHOLD, "0");
			doInit(true, false);
			string addr = "127.0.0.1:" + port + "?zone=RZONE&_CONNECTIONNUM=8";
			Url url = addressParser.parse(addr);

			for (int i = 0; i < 8; i++)
			{
				client.getConnection(url, 1000);
			}
		    System.Threading.Thread.Sleep(2100);
			Assert.True(0 <= clientDisConnectProcessor.DisConnectTimes);
		    System.Threading.Thread.Sleep(200);
			Assert.True(2 <= clientDisConnectProcessor.DisConnectTimes);
		    System.Threading.Thread.Sleep(400);
			Assert.True(4 <= clientDisConnectProcessor.DisConnectTimes);
		    System.Threading.Thread.Sleep(200);
			Assert.True(5 <= clientDisConnectProcessor.DisConnectTimes);
		    System.Threading.Thread.Sleep(200);
			Assert.True(5 <= clientDisConnectProcessor.DisConnectTimes);
		    System.Threading.Thread.Sleep(100);
			Assert.True(6 <= clientDisConnectProcessor.DisConnectTimes);
		}

        [Fact]
		public virtual void testDisconnectStrategy_byUserSetting()
		{
			java.lang.System.setProperty(Configs.CONN_MONITOR_SWITCH, "true");
			java.lang.System.setProperty(Configs.CONN_MONITOR_INITIAL_DELAY, "2000");
			java.lang.System.setProperty(Configs.CONN_MONITOR_PERIOD, "100");
			java.lang.System.setProperty(Configs.CONN_THRESHOLD, "0");
			doInit(false, true);
			string addr = "127.0.0.1:" + port + "?zone=RZONE&_CONNECTIONNUM=8";
			Url url = addressParser.parse(addr);

			for (int i = 0; i < 8; i++)
			{
				client.getConnection(url, 1000);
			}
		    System.Threading.Thread.Sleep(2100);
			Assert.True(0 <= clientDisConnectProcessor.DisConnectTimes);
		    System.Threading.Thread.Sleep(200);
			Assert.True(2 <= clientDisConnectProcessor.DisConnectTimes);
		    System.Threading.Thread.Sleep(400);
			Assert.True(4 <= clientDisConnectProcessor.DisConnectTimes);
		    System.Threading.Thread.Sleep(200);
			Assert.True(5 <= clientDisConnectProcessor.DisConnectTimes);
		    System.Threading.Thread.Sleep(200);
			Assert.True(5 <= clientDisConnectProcessor.DisConnectTimes);
		    System.Threading.Thread.Sleep(100);
			Assert.True(6 <= clientDisConnectProcessor.DisConnectTimes);
		}

		private void doInit(bool enableSystem, bool enableUser)
		{
			if (enableSystem)
			{
				java.lang.System.setProperty(Configs.CONN_MONITOR_SWITCH, "true");
				java.lang.System.setProperty(Configs.CONN_RECONNECT_SWITCH, "true");
			}
			else
			{
				java.lang.System.setProperty(Configs.CONN_MONITOR_SWITCH, "false");
				java.lang.System.setProperty(Configs.CONN_RECONNECT_SWITCH, "false");
			}
			server = new BoltServer(port, false, true);
			server.start();
            server.addConnectionEventProcessor(ConnectionEventType.CONNECT, serverConnectProcessor);
            server.addConnectionEventProcessor(ConnectionEventType.CLOSE, serverDisConnectProcessor);
			server.registerUserProcessor(serverUserProcessor);

			client = new RpcClient();
			if (enableUser)
			{
				client.enableReconnectSwitch();
				client.enableConnectionMonitorSwitch();
			}
			client.addConnectionEventProcessor(ConnectionEventType.CONNECT, clientConnectProcessor);
			client.addConnectionEventProcessor(ConnectionEventType.CLOSE, clientDisConnectProcessor);
			client.registerUserProcessor(clientUserProcessor);
			client.startup();
		}
	}
}