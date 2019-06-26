using com.alipay.remoting.rpc.common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting;
using System;

namespace RpcServerDemo
{
    class Program
    {
        internal static ILogger logger = NullLogger.Instance;

        internal BoltServer server;

        internal int port = 8999;

        internal SimpleServerUserProcessor serverUserProcessor = new SimpleServerUserProcessor();
        internal CONNECTEventProcessor serverConnectProcessor = new CONNECTEventProcessor();
        internal DISCONNECTEventProcessor serverDisConnectProcessor = new DISCONNECTEventProcessor();

        public Program()
        {
            // 1. create a Rpc server with port assigned
            server = new BoltServer(port);
            // 2. add processor for connect and close event if you need
            server.addConnectionEventProcessor(ConnectionEventType.CONNECT, serverConnectProcessor);
            server.addConnectionEventProcessor(ConnectionEventType.CLOSE, serverDisConnectProcessor);
            // 3. register user processor for client request
            server.registerUserProcessor(serverUserProcessor);
            // 4. server start
            if (server.start())
            {
                Console.WriteLine("server start ok!");
            }
            else
            {
                Console.WriteLine("server start failed!");
            }
            //server.RpcServer.shutdown();
            Console.ReadLine();
        }

        static void Main()
        {
            new Program();
        }
    }
}
