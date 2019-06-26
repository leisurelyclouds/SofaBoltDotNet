using DotNetty.Common.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Net;

namespace Remoting
{
    /// <summary>
    /// Base remoting capability.
    /// </summary>
    public abstract class BaseRemoting
    {
        /// <summary>
        /// logger
		/// </summary>
        private static readonly ILogger logger = NullLogger.Instance;

        protected internal CommandFactory commandFactory;

        public BaseRemoting(CommandFactory commandFactory)
        {
            this.commandFactory = commandFactory;
        }

        /// <summary>
        /// Synchronous invocation
        /// </summary>
        /// <param name="conn"> </param>
        /// <param name="request"> </param>
        /// <param name="timeoutMillis">
        /// @return </param>
        /// <exception cref="ThreadInterruptedException"> </exception>
        /// <exception cref="RemotingException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: protected RemotingCommand invokeSync(final Connection conn, final RemotingCommand request, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        protected internal virtual RemotingCommand invokeSync(Connection conn, RemotingCommand request, int timeoutMillis)
        {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final InvokeFuture future = createInvokeFuture(request, request.getInvokeContext());
            InvokeFuture future = createInvokeFuture(request, request.InvokeContext);
            conn.addInvokeFuture(future);
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final int requestId = request.getId();
            int requestId = request.Id;
            try
            {
                var writeFlushTask = conn.Channel.WriteAndFlushAsync(request);
                writeFlushTask.ContinueWith((task) =>
                {
                    if (!task.IsCompletedSuccessfully)
                    {
                        conn.removeInvokeFuture(requestId);
                        future.putResponse(commandFactory.createSendFailedResponse(conn.RemoteAddress, task.Exception));
                        logger.LogError("Invoke send failed, id={}", requestId, task.Exception);
                    }
                });
            }
            catch (Exception e)
            {
                conn.removeInvokeFuture(requestId);
                future.putResponse(commandFactory.createSendFailedResponse(conn.RemoteAddress, e));
                logger.LogError("Exception caught when sending invocation, id={}", requestId, e);
            }
            RemotingCommand response = future.waitResponse(timeoutMillis);

            if (response == null)
            {
                conn.removeInvokeFuture(requestId);
                response = commandFactory.createTimeoutResponse(conn.RemoteAddress);
                logger.LogWarning("Wait response, request id={} timeout!", requestId);
            }

            return response;
        }

        /// <summary>
        /// Invocation with callback.
        /// </summary>
        /// <param name="conn"> </param>
        /// <param name="request"> </param>
        /// <param name="invokeCallback"> </param>
        /// <param name="timeoutMillis"> </param>
        /// <exception cref="ThreadInterruptedException"> </exception>
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        //ORIGINAL LINE: protected void invokeWithCallback(final Connection conn, final RemotingCommand request, final InvokeCallback invokeCallback, final int timeoutMillis)
        protected internal virtual void invokeWithCallback(Connection conn, RemotingCommand request, InvokeCallback invokeCallback, int timeoutMillis)
        {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final InvokeFuture future = createInvokeFuture(conn, request, request.getInvokeContext(), invokeCallback);
            InvokeFuture future = createInvokeFuture(conn, request, request.InvokeContext, invokeCallback);
            conn.addInvokeFuture(future);
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final int requestId = request.getId();
            int requestId = request.Id;
            try
            {
                ITimeout timeout = TimerHolder.Timer.NewTimeout(new TimerTaskAnonymousInnerClass(this, conn, future, requestId), TimeSpan.FromMilliseconds(timeoutMillis));
                future.addTimeout(timeout);
                var writeFlushTask = conn.Channel.WriteAndFlushAsync(request);
                writeFlushTask.ContinueWith((task) =>
                {
                    if (!task.IsCompletedSuccessfully)
                    {
                        InvokeFuture f = conn.removeInvokeFuture(requestId);
                        if (f != null)
                        {
                            f.cancelTimeout();
                            f.putResponse(commandFactory.createSendFailedResponse(conn.RemoteAddress, task.Exception));
                            f.tryAsyncExecuteInvokeCallbackAbnormally();
                        }
                        logger.LogError("Invoke send failed. The address is {}", ((IPEndPoint)conn.Channel.RemoteAddress).ToString(), task.Exception);
                    }
                });
            }
            catch (Exception e)
            {
                InvokeFuture f = conn.removeInvokeFuture(requestId);
                if (f != null)
                {
                    f.cancelTimeout();
                    f.putResponse(commandFactory.createSendFailedResponse(conn.RemoteAddress, e));
                    f.tryAsyncExecuteInvokeCallbackAbnormally();
                }
                logger.LogError("Exception caught when sending invocation. The address is {}", ((IPEndPoint)conn.Channel.RemoteAddress).ToString(), e);
            }
        }

        private class TimerTaskAnonymousInnerClass : ITimerTask
        {
            private readonly BaseRemoting outerInstance;

            private Connection conn;
            private InvokeFuture future;
            private int requestId;

            public TimerTaskAnonymousInnerClass(BaseRemoting outerInstance, Connection conn, InvokeFuture future, int requestId)
            {
                this.outerInstance = outerInstance;
                this.conn = conn;
                this.future = future;
                this.requestId = requestId;
            }

            public void Run(ITimeout timeout)
            {
                InvokeFuture future = conn.removeInvokeFuture(requestId);
                if (future != null)
                {
                    future.putResponse(outerInstance.commandFactory.createTimeoutResponse(conn.RemoteAddress));
                    future.tryAsyncExecuteInvokeCallbackAbnormally();
                }
            }

        }


        /// <summary>
        /// Invocation with future returned.
        /// </summary>
        /// <param name="conn"> </param>
        /// <param name="request"> </param>
        /// <param name="timeoutMillis">
        /// @return </param>
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        //ORIGINAL LINE: protected InvokeFuture invokeWithFuture(final Connection conn, final RemotingCommand request, final int timeoutMillis)
        protected internal virtual InvokeFuture invokeWithFuture(Connection conn, RemotingCommand request, int timeoutMillis)
        {

            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final InvokeFuture future = createInvokeFuture(request, request.getInvokeContext());
            InvokeFuture future = createInvokeFuture(request, request.InvokeContext);
            conn.addInvokeFuture(future);
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final int requestId = request.getId();
            int requestId = request.Id;
            try
            {
                ITimeout timeout = TimerHolder.Timer.NewTimeout(new TimerTaskAnonymousInnerClass2(this, conn, future, requestId), TimeSpan.FromMilliseconds(timeoutMillis));
                future.addTimeout(timeout);

                var writeFlushTask = conn.Channel.WriteAndFlushAsync(request);
                writeFlushTask.ContinueWith((task) =>
                {
                    if (!task.IsCompletedSuccessfully)
                    {
                        InvokeFuture f = conn.removeInvokeFuture(requestId);
                        if (f != null)
                        {
                            f.cancelTimeout();
                            f.putResponse(commandFactory.createSendFailedResponse(conn.RemoteAddress, task.Exception));
                        }
                        logger.LogError("Invoke send failed. The address is {}", ((IPEndPoint)conn.Channel.RemoteAddress).ToString(), task.Exception);
                    }
                });
            }
            catch (Exception e)
            {
                InvokeFuture f = conn.removeInvokeFuture(requestId);
                if (f != null)
                {
                    f.cancelTimeout();
                    f.putResponse(commandFactory.createSendFailedResponse(conn.RemoteAddress, e));
                }
                logger.LogError("Exception caught when sending invocation. The address is {}", ((IPEndPoint)conn.Channel.RemoteAddress).ToString(), e);
            }
            return future;
        }

        private class TimerTaskAnonymousInnerClass2 : ITimerTask
        {
            private readonly BaseRemoting outerInstance;

            private Connection conn;
            private InvokeFuture future;
            private int requestId;

            public TimerTaskAnonymousInnerClass2(BaseRemoting outerInstance, Connection conn, InvokeFuture future, int requestId)
            {
                this.outerInstance = outerInstance;
                this.conn = conn;
                this.future = future;
                this.requestId = requestId;
            }

            public void Run(ITimeout timeout)
            {
                InvokeFuture future = conn.removeInvokeFuture(requestId);
                if (future != null)
                {
                    future.putResponse(outerInstance.commandFactory.createTimeoutResponse(conn.RemoteAddress));
                }
            }

        }


        /// <summary>
        /// Oneway invocation.
        /// </summary>
        /// <param name="conn"> </param>
        /// <param name="request"> </param>
        /// <exception cref="ThreadInterruptedException"> </exception>
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        //ORIGINAL LINE: protected void oneway(final Connection conn, final RemotingCommand request)
        protected internal virtual void oneway(Connection conn, RemotingCommand request)
        {
            try
            {
                var writeFlushTask = conn.Channel.WriteAndFlushAsync(request);
                writeFlushTask.ContinueWith((task) =>
                {
                    if (!task.IsCompletedSuccessfully)
                    {
                        logger.LogError("Invoke send failed. The address is {}", ((IPEndPoint)conn.Channel.RemoteAddress).ToString(), task.Exception);
                    }
                });
            }
            catch (Exception e)
            {
                if (null == conn)
                {
                    logger.LogError("Conn is null");
                }
                else
                {
                    logger.LogError("Exception caught when sending invocation. The address is {}", ((IPEndPoint)conn.Channel.RemoteAddress).ToString(), e);
                }
            }
        }


        /// <summary>
        /// Create invoke future with <seealso cref="InvokeContext"/>. </summary>
        /// <param name="request"> </param>
        /// <param name="invokeContext">
        /// @return </param>
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        //ORIGINAL LINE: protected abstract InvokeFuture createInvokeFuture(final RemotingCommand request, final InvokeContext invokeContext);
        protected internal abstract InvokeFuture createInvokeFuture(RemotingCommand request, InvokeContext invokeContext);

        /// <summary>
        /// Create invoke future with <seealso cref="InvokeContext"/>. </summary>
        /// <param name="conn"> </param>
        /// <param name="request"> </param>
        /// <param name="invokeContext"> </param>
        /// <param name="invokeCallback">
        /// @return </param>
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        //ORIGINAL LINE: protected abstract InvokeFuture createInvokeFuture(final Connection conn, final RemotingCommand request, final InvokeContext invokeContext, final InvokeCallback invokeCallback);
        protected internal abstract InvokeFuture createInvokeFuture(Connection conn, RemotingCommand request, InvokeContext invokeContext, InvokeCallback invokeCallback);

        protected internal virtual CommandFactory CommandFactory
        {
            get
            {
                return commandFactory;
            }
        }
    }
}