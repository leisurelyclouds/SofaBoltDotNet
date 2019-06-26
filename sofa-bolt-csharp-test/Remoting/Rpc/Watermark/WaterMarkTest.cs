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
using Remoting.Config;

namespace com.alipay.remoting.rpc.watermark
{
    /// <summary>
    /// water mark normal test, set a large enough buffer mark, and not trigger write over flow.
    /// </summary>
    [Collection("Sequential")]
    public class WaterMarkTest : IDisposable
    {
        private bool InstanceFieldsInitialized = false;

        public WaterMarkTest()
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

        internal SimpleServerUserProcessor serverUserProcessor = new SimpleServerUserProcessor(0, 20, 20, 60, 100);
        internal SimpleClientUserProcessor clientUserProcessor = new SimpleClientUserProcessor();
        internal CONNECTEventProcessor clientConnectProcessor = new CONNECTEventProcessor();
        internal CONNECTEventProcessor serverConnectProcessor = new CONNECTEventProcessor();
        internal DISCONNECTEventProcessor clientDisConnectProcessor = new DISCONNECTEventProcessor();
        internal DISCONNECTEventProcessor serverDisConnectProcessor = new DISCONNECTEventProcessor();

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Before private void init()
        private void init()
        {
            java.lang.System.setProperty(Configs.NETTY_BUFFER_HIGH_WATERMARK, Convert.ToString(128 * 1024));
            java.lang.System.setProperty(Configs.NETTY_BUFFER_LOW_WATERMARK, Convert.ToString(32 * 1024));

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
                Thread.Sleep(100);
            }
            catch (ThreadInterruptedException e)
            {
                logger.LogError("Stop server failed!", e);
            }
        }

        [Fact]
        public virtual void testSync()
        {
            RequestBody req = new RequestBody(1, 1024);
            for (int i = 0; i < invokeTimes; i++)
            {
                Thread thread = new Thread(() =>
                {
                    string res = null;
                    try
                    {
                        for (int t = 0; t < invokeTimes; t++)
                        {
                            res = (string)client.invokeSync(addr, req, 3000);
                        }
                    }
                    catch (RemotingException e)
                    {
                        string errMsg = "RemotingException caught in sync!";
                        logger.LogError(errMsg, e);
                        Assert.Null(errMsg);
                    }
                    catch (ThreadInterruptedException e)
                    {
                        string errMsg = "ThreadInterruptedException caught in sync!";
                        logger.LogError(errMsg, e);
                        Assert.Null(errMsg);
                    }
                    logger.LogWarning("Result received in sync: " + res);
                    Assert.Equal(RequestBody.DEFAULT_SERVER_RETURN_STR, res);
                });
                thread.Start();
            }

            Thread.Sleep(5000);

            Assert.True(serverConnectProcessor.Connected);
            Assert.Equal(1, serverConnectProcessor.ConnectTimes);
            Assert.Equal(invokeTimes * invokeTimes, serverUserProcessor.InvokeTimes);
        }

        [Fact]
        public virtual void testServerSyncUsingConnection()
        {
            Connection clientConn = client.createStandaloneConnection(ip, port, 1000);

            RequestBody req1 = new RequestBody(1, RequestBody.DEFAULT_CLIENT_STR);
            string serverres = (string)client.invokeSync(clientConn, req1, 1000);
            Assert.Equal(serverres, RequestBody.DEFAULT_SERVER_RETURN_STR);
            for (int i = 0; i < invokeTimes; i++)
            {
                Thread thread = new Thread(() =>
                {
                    try
                    {
                        string remoteAddr = serverUserProcessor.RemoteAddr;
                        Assert.NotNull(remoteAddr);
                        RequestBody req = new RequestBody(1, 1024);
                        for (int t = 0; t < invokeTimes; t++)
                        {
                            string clientres = (string)server.RpcServer.invokeSync(remoteAddr, req, 1000);
                            Assert.Equal(clientres, RequestBody.DEFAULT_CLIENT_RETURN_STR);
                        }
                    }
                    catch (ThreadInterruptedException e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace);
                    }
                    catch (RemotingException e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace);
                    }
                });
                thread.Start();
            }
            Thread.Sleep(5000);
            Assert.True(serverConnectProcessor.Connected);
            Assert.Equal(1, serverConnectProcessor.ConnectTimes);
            Assert.Equal(invokeTimes * invokeTimes, clientUserProcessor.InvokeTimes);
        }
    }
}