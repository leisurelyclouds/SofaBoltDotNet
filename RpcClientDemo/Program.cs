using com.alipay.remoting.rpc.common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting;
using Remoting.exception;
using Remoting.rpc;
using System;
using System.Threading;
using Xunit;

namespace RpcClientDemo
{
    class Program
    {

        internal static ILogger logger = NullLogger.Instance;

        internal static RpcClient client;

        internal static string addr = "rpcserverdemo:8999";

        internal SimpleClientUserProcessor clientUserProcessor = new SimpleClientUserProcessor();
        internal CONNECTEventProcessor clientConnectProcessor = new CONNECTEventProcessor();
        internal DISCONNECTEventProcessor clientDisConnectProcessor = new DISCONNECTEventProcessor();

        public Program()
        {
            // 1. create a rpc client
            client = new RpcClient();
            // 2. add processor for connect and close event if you need
            client.addConnectionEventProcessor(ConnectionEventType.CONNECT, clientConnectProcessor);
            client.addConnectionEventProcessor(ConnectionEventType.CLOSE, clientDisConnectProcessor);
            // 3. do init
            client.startup();
        }

        static void Main()
        {
            new Program();
            RequestBody req = new RequestBody(2, "hello world sync");
            try
            {
                string res = (string)client.invokeSync(addr, req, 3000);
                Console.WriteLine("invoke sync result = [" + res + "]");
            }
            catch (RemotingException e)
            {
                string errMsg = "RemotingException caught in oneway!";
                logger.LogError(errMsg, e);
                Assert.Null(errMsg);
            }
            catch (ThreadInterruptedException)
            {
                logger.LogError("interrupted!");
            }
            client.shutdown();
        }
    }
}
