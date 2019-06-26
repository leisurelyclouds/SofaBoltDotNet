using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting;
using Remoting.rpc;
using Xunit;
using java.util.concurrent;
using Remoting.rpc.protocol;
using System.Net;
using System;

namespace com.alipay.remoting.rpc.exception
{
    [Collection("Sequential")]
    public class BadServerIpTest
    {
        internal ILogger logger = NullLogger.Instance;

        [Fact]
        public virtual void cantAssignTest()
        {
            BadServer server = new BadServer(IPAddress.Parse("59.66.132.166"));
            try
            {
                server.startServer();
            }
            catch (Exception e)
            {
                logger.LogError("Start server failed!", e);
            }
        }

        internal class BadServer
        {

            internal ILogger logger = NullLogger.Instance;
            internal RpcServer server;
            internal IPAddress ip;

            public BadServer(IPAddress ip)
            {
                this.ip = ip;
            }

            public virtual void startServer()
            {
                server = new RpcServer(ip, 1111);
                server.registerUserProcessor(new SyncUserProcessorAnonymousInnerClass(this));
                server.startup();
            }

            private class SyncUserProcessorAnonymousInnerClass : SyncUserProcessor
            {
                private readonly BadServer outerInstance;

                public SyncUserProcessorAnonymousInnerClass(BadServer outerInstance)
                {
                    this.outerInstance = outerInstance;
                }

                public override object handleRequest(BizContext bizCtx, object request)
                {
                    outerInstance.logger.LogWarning("Request received:" + request);
                    return "hello world!";
                }

                public override Type interest()
                {
                    return typeof(string);
                }

                public override Executor Executor
                {
                    get
                    {
                        return null;
                    }
                }
            }
        }
    }
}