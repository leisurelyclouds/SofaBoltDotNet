using DotNetty.Common.Utilities;
using java.lang;
using java.util.concurrent;
using java.util.concurrent.atomic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;

namespace Remoting.rpc
{
    /// <summary>
    /// The default implementation of InvokeFuture.
    /// </summary>
    public class DefaultInvokeFuture : InvokeFuture
    {

        private static readonly ILogger logger = NullLogger.Instance;

        //JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
        private int invokeId_Renamed;

        private InvokeCallbackListener callbackListener;

        private InvokeCallback callback;

        private volatile ResponseCommand responseCommand;

        private readonly CountDownLatch countDownLatch = new CountDownLatch(1);

        private readonly AtomicBoolean executeCallbackOnlyOnce = new AtomicBoolean(false);

        private ITimeout timeout;

        private System.Exception cause;

        private byte protocol;

        private InvokeContext invokeContext;

        private CommandFactory commandFactory;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="invokeId"> invoke id </param>
        /// <param name="callbackListener"> callback listener </param>
        /// <param name="callback"> callback </param>
        /// <param name="protocol"> protocol code </param>
        /// <param name="commandFactory"> command factory </param>
        public DefaultInvokeFuture(int invokeId, InvokeCallbackListener callbackListener, InvokeCallback callback, byte protocol, CommandFactory commandFactory)
        {
            invokeId_Renamed = invokeId;
            this.callbackListener = callbackListener;
            this.callback = callback;
            this.protocol = protocol;
            this.commandFactory = commandFactory;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="invokeId"> invoke id </param>
        /// <param name="callbackListener"> callback listener </param>
        /// <param name="callback"> callback </param>
        /// <param name="protocol"> protocol </param>
        /// <param name="commandFactory"> command factory </param>
        /// <param name="invokeContext"> invoke context </param>
        public DefaultInvokeFuture(int invokeId, InvokeCallbackListener callbackListener, InvokeCallback callback, byte protocol, CommandFactory commandFactory, InvokeContext invokeContext) : this(invokeId, callbackListener, callback, protocol, commandFactory)
        {
            this.invokeContext = invokeContext;
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public ResponseCommand waitResponse(long timeoutMillis) throws ThreadInterruptedException
        public virtual ResponseCommand waitResponse(long timeoutMillis)
        {
            countDownLatch.await(timeoutMillis, TimeUnit.MILLISECONDS);
            return responseCommand;
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public ResponseCommand waitResponse() throws ThreadInterruptedException
        public virtual ResponseCommand waitResponse()
        {
            countDownLatch.await();
            return responseCommand;
        }

        RemotingCommand InvokeFuture.waitResponse(long timeoutMillis)
        {
            return waitResponse(timeoutMillis);
        }

        RemotingCommand InvokeFuture.waitResponse()
        {
            return waitResponse();
        }

        public virtual RemotingCommand createConnectionClosedResponse(IPEndPoint responseHost)
        {
            return commandFactory.createConnectionClosedResponse(responseHost, null);
        }

        /// <seealso cref= InvokeFuture#putResponse(RemotingCommand) </seealso>
        public virtual void putResponse(RemotingCommand response)
        {
            responseCommand = (ResponseCommand)response;
            countDownLatch.countDown();
        }

        /// 
        /// <seealso cref= InvokeFuture#isDone() </seealso>
        public virtual bool Done
        {
            get
            {
                return countDownLatch.getCount() <= 0;
            }
        }

        /// <seealso cref= InvokeFuture#invokeId() </seealso>
        public virtual int invokeId()
        {
            return invokeId_Renamed;
        }

        public virtual void executeInvokeCallback()
        {
            if (callbackListener != null)
            {
                if (executeCallbackOnlyOnce.compareAndSet(false, true))
                {
                    callbackListener.onResponse(this);
                }
            }
        }

        /// <seealso cref= InvokeFuture#getInvokeCallback() </seealso>
        public virtual InvokeCallback InvokeCallback
        {
            get
            {
                return callback;
            }
        }

        /// <seealso cref= InvokeFuture#addTimeout(io.netty.util.Timeout) </seealso>
        public virtual void addTimeout(ITimeout timeout)
        {
            this.timeout = timeout;
        }

        /// <seealso cref= InvokeFuture#cancelTimeout() </seealso>
        public virtual void cancelTimeout()
        {
            if (timeout != null)
            {
                timeout.Cancel();
            }
        }

        /// <seealso cref= InvokeFuture#setCause(java.lang.Throwable) </seealso>
        public virtual System.Exception Cause
        {
            set
            {
                cause = value;
            }
            get
            {
                return cause;
            }
        }


        /// <seealso cref= InvokeFuture#getProtocolCode() </seealso>
        public virtual byte ProtocolCode
        {
            get
            {
                return protocol;
            }
        }

        /// <seealso cref= InvokeFuture#getInvokeContext() </seealso>
        public virtual InvokeContext InvokeContext
        {
            set
            {
                invokeContext = value;
            }
            get
            {
                return invokeContext;
            }
        }

        public class TempRunnable : Runnable
        {
            private readonly DefaultInvokeFuture defaultInvokeFuture;

            public TempRunnable(DefaultInvokeFuture defaultInvokeFuture)
            {
                this.defaultInvokeFuture = defaultInvokeFuture;
            }
            public void run()
            {
                try
                {
                    defaultInvokeFuture.executeInvokeCallback();
                }
                finally
                {
                }
            }
        }


        /// <seealso cref= InvokeFuture#tryAsyncExecuteInvokeCallbackAbnormally() </seealso>
        public virtual void tryAsyncExecuteInvokeCallbackAbnormally()
        {
            try
            {
                Protocol protocol = ProtocolManager.getProtocol(Remoting.ProtocolCode.fromBytes(this.protocol));
                if (null != protocol)
                {
                    CommandHandler commandHandler = protocol.CommandHandler;
                    if (null != commandHandler)
                    {
                        ExecutorService executor = commandHandler.DefaultExecutor;
                        if (null != executor)
                        {
                            executor.execute(new TempRunnable(this));
                        }
                    }
                    else
                    {
                        logger.LogError("Executor null in commandHandler of protocolCode [{}].", this.protocol);
                    }
                }
                else
                {
                    logger.LogError("protocolCode [{}] not registered!", this.protocol);
                }
            }
            catch (System.Exception e)
            {
                logger.LogError("Exception caught when executing invoke callback abnormally.", e);
            }
        }
    }
}