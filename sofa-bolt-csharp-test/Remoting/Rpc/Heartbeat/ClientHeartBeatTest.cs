using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting;
using Remoting.rpc;
using System;
using System.Net;
using com.alipay.remoting.rpc.common;
using Xunit;
using Remoting.exception;
using Remoting.rpc.protocol;
using Remoting.Config;

namespace com.alipay.remoting.rpc.heartbeat
{
    /// <summary>
    /// Client heart beat test
    /// </summary>
    [Collection("Sequential")]
    public class ClientHeartBeatTest: IDisposable
    {
		private bool InstanceFieldsInitialized = false;

		public ClientHeartBeatTest()
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

		internal CustomHeartBeatProcessor heartBeatProcessor = new CustomHeartBeatProcessor();

		private void init()
		{
			java.lang.System.setProperty(Configs.TCP_IDLE, "100");
			java.lang.System.setProperty(Configs.TCP_IDLE_SWITCH, Convert.ToString(true));
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
		///  test heartbeat trigger
		/// </summary>
        [Fact]
		public virtual void testClientHeartBeatTrigger()
		{
			server.RpcServer.registerProcessor(RpcProtocol.PROTOCOL_CODE, CommonCommandCode.HEARTBEAT, heartBeatProcessor);
			try
			{
				client.createStandaloneConnection(addr, 1000);
			}
			catch (RemotingException e)
			{
				logger.LogError("", e);
			}
            Thread.Sleep(5000);
			Assert.True(heartBeatProcessor.HeartBeatTimes > 1);
			Assert.Equal(1, clientConnectProcessor.ConnectTimes);
			Assert.Equal(1, serverConnectProcessor.ConnectTimes);
		}

		/// <summary>
		///  test heartbeat no response, close the connection from client side
		/// </summary>
        [Fact]
		public virtual void testClientHeartBeatTriggerExceed3Times()
		{
			server.RpcServer.registerProcessor(RpcProtocol.PROTOCOL_CODE, CommonCommandCode.HEARTBEAT, heartBeatProcessor);
			try
			{
				client.createStandaloneConnection(addr, 1000);
			}
			catch (RemotingException e)
			{
				logger.LogError("", e);
			}
            Thread.Sleep(3000);
			Assert.True(heartBeatProcessor.HeartBeatTimes > 1);
			Assert.Equal(1, clientConnectProcessor.ConnectTimes);
			Assert.Equal(1, serverConnectProcessor.ConnectTimes);
			Assert.Equal(1, clientDisConnectProcessor.DisConnectTimes);
			Assert.Equal(1, serverDisConnectProcessor.DisConnectTimes);
		}

		/// <summary>
		/// test basic heartbeat and ack
		/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        [Fact]
        //ORIGINAL LINE: @Test public void testClientHeartBeatAck() throws ThreadInterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testClientHeartBeatAck()
		{
			try
			{
				client.createStandaloneConnection(addr, 1000);
			}
			catch (RemotingException e)
			{
				logger.LogError("", e);
			}
            Thread.Sleep(1000);
			Assert.Equal(1, clientConnectProcessor.ConnectTimes);
			Assert.Equal(1, serverConnectProcessor.ConnectTimes);
			Assert.Equal(0, clientDisConnectProcessor.DisConnectTimes);
			Assert.Equal(0, serverDisConnectProcessor.DisConnectTimes);
		}
	}
}