using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting;
using System.Collections.Generic;
using com.alipay.remoting.rpc.common;
using Xunit;
using java.util.concurrent;
using java.util.concurrent.atomic;
using Remoting.rpc.protocol;
using System;
using System.Net;

namespace com.alipay.remoting.rpc.prehandle
{
    public class PreHandleUserProcessor : SyncUserProcessor
    {
        /// <summary>
        /// logger 
        /// </summary>
        private static readonly ILogger logger = NullLogger.Instance;

        /// <summary>
        /// executor 
        /// </summary>
        private ThreadPoolExecutor executor = new ThreadPoolExecutor(1, 3, 60, TimeUnit.SECONDS, new ArrayBlockingQueue(4), new NamedThreadFactory("Request-process-pool"));

        private AtomicInteger invokeTimes = new AtomicInteger();

        public override BizContext preHandleRequest(RemotingContext remotingCtx, object request)
        {
            BizContext ctx = new MyBizContext(this, remotingCtx);
            ctx.put("test", "test");
            return ctx;
        }

        public override object handleRequest(BizContext bizCtx, object request)
        {
            logger.LogWarning("Request received:" + request);
            invokeTimes.incrementAndGet();

            long waittime = ((long?)bizCtx.InvokeContext.get(InvokeContext.BOLT_PROCESS_WAIT_TIME)).Value;
            logger.LogWarning("PreHandleUserProcessor User processor process wait time [" + waittime + "].");

            Assert.Equal(typeof(RequestBody), request.GetType());
            Assert.Equal(IPAddress.Loopback.MapToIPv6().ToString(), bizCtx.RemoteHost);
            Assert.True(bizCtx.RemotePort != -1);
            return bizCtx.get("test");
        }

        public override Type interest()
        {
            return typeof(RequestBody);
        }

        public override Executor Executor
        {
            get
            {
                return executor;
            }
        }

        public virtual int InvokeTimes
        {
            get
            {
                return this.invokeTimes.get();
            }
        }

        internal class MyBizContext : DefaultBizContext, BizContext
        {
            private readonly PreHandleUserProcessor outerInstance;

            /// <summary>
            /// customerized context 
            /// </summary>
            internal IDictionary<string, string> custCtx = new Dictionary<string, string>();

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="remotingCtx"> </param>
            public MyBizContext(PreHandleUserProcessor outerInstance, RemotingContext remotingCtx) : base(remotingCtx)
            {
                this.outerInstance = outerInstance;
            }

            public override void put(string key, string value)
            {
                custCtx[key] = value;
            }

            public override string get(string key)
            {
                return custCtx[key];
            }
        }
    }
}