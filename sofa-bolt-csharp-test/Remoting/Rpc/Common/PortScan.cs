using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;
using System.Threading;
using Xunit;
using Xunit.Abstractions;
using System.IO;

namespace com.alipay.remoting.rpc.common
{
    public class PortScan
    {
        public PortScan(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }
        private static readonly ILogger logger = NullLogger.Instance;
        private readonly ITestOutputHelper testOutputHelper;

        public static int select()
        {
            int port = -1;
            TcpListener ss = null;
            try
            {
                ss = new TcpListener(IPAddress.Loopback, 0);
                ss.Start();
                port = ((IPEndPoint)ss.LocalEndpoint).Port;
            }
            catch (IOException ioe)
            {
                logger.LogError(ioe.ToString());
                logger.LogError(ioe.StackTrace);
            }
            finally
            {
                try
                {
                    ss.Stop();
                    logger.LogWarning($"Server socket close status: {ss}");
                }
                catch (IOException)
                {
                }
            }
            return port;
        }

        [Fact]
        public void TestSelect()
        {
            int port = select();
            TcpListener ss = new TcpListener(IPAddress.Any, port);
            ss.Start();
            logger.LogWarning("listening on port：{}", port);

            Thread.Sleep(100);
            var ipe = new IPEndPoint(IPAddress.Loopback, port);
            var socket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ipe);
            testOutputHelper.WriteLine($"{socket.Connected}");
            testOutputHelper.WriteLine("local port: " + ((IPEndPoint)socket.LocalEndPoint).Port);
            testOutputHelper.WriteLine("remote port: " + ((IPEndPoint)socket.RemoteEndPoint).Port);
        }
    }

}