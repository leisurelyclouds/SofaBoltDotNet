using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting.Config.switches;
using Remoting.rpc.protocol;
using System.Net;

namespace Remoting.rpc
{
    /// <summary>
    /// Rpc remoting capability.
    /// </summary>
    public abstract class RpcRemoting : BaseRemoting
    {
        static RpcRemoting()
        {
            RpcProtocolManager.initProtocols();
        }
        /// <summary>
        /// logger
		/// </summary>
        private static readonly ILogger logger = NullLogger.Instance;

        /// <summary>
        /// address parser to get custom args
		/// </summary>
        protected internal RemotingAddressParser addressParser;

        /// <summary>
        /// connection manager
		/// </summary>
        protected internal DefaultConnectionManager connectionManager;

        /// <summary>
        /// default constructor
        /// </summary>
        public RpcRemoting(CommandFactory commandFactory) : base(commandFactory)
        {
        }

        /// <param name="addressParser"> </param>
        /// <param name="connectionManager"> </param>
        public RpcRemoting(CommandFactory commandFactory, RemotingAddressParser addressParser, DefaultConnectionManager connectionManager) : this(commandFactory)
        {
            this.addressParser = addressParser;
            this.connectionManager = connectionManager;
        }

        /// <summary>
        /// Oneway rpc invocation.<br>
        /// Notice! DO NOT modify the request object concurrently when this method is called.
        /// </summary>
        /// <param name="addr"> </param>
        /// <param name="request"> </param>
        /// <param name="invokeContext"> </param>
        /// <exception cref="RemotingException"> </exception>
        /// <exception cref="ThreadInterruptedException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void oneway(final String addr, final Object request, final InvokeContext invokeContext) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual void oneway(string addr, object request, InvokeContext invokeContext)
        {
            Url url = addressParser.parse(addr);
            oneway(url, request, invokeContext);
        }

        /// <summary>
        /// Oneway rpc invocation.<br>
        /// Notice! DO NOT modify the request object concurrently when this method is called.
        /// </summary>
        /// <param name="url"> </param>
        /// <param name="request"> </param>
        /// <param name="invokeContext"> </param>
        /// <exception cref="RemotingException"> </exception>
        /// <exception cref="ThreadInterruptedException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract void oneway(final Url url, final Object request, final InvokeContext invokeContext) throws exception.RemotingException, ThreadInterruptedException;
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public abstract void oneway(Url url, object request, InvokeContext invokeContext);

        /// <summary>
        /// Oneway rpc invocation.<br>
        /// Notice! DO NOT modify the request object concurrently when this method is called.
        /// </summary>
        /// <param name="conn"> </param>
        /// <param name="request"> </param>
        /// <param name="invokeContext"> </param>
        /// <exception cref="RemotingException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void oneway(final Connection conn, final Object request, final InvokeContext invokeContext) throws exception.RemotingException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual void oneway(Connection conn, object request, InvokeContext invokeContext)
        {
            RequestCommand requestCommand = (RequestCommand)toRemotingCommand(request, conn, invokeContext, -1);
            requestCommand.Type = RpcCommandType.REQUEST_ONEWAY;
            preProcessInvokeContext(invokeContext, requestCommand, conn);
            base.oneway(conn, requestCommand);
        }

        /// <summary>
        /// Synchronous rpc invocation.<br>
        /// Notice! DO NOT modify the request object concurrently when this method is called.
        /// </summary>
        /// <param name="addr"> </param>
        /// <param name="request"> </param>
        /// <param name="invokeContext"> </param>
        /// <param name="timeoutMillis">
        /// @return </param>
        /// <exception cref="RemotingException"> </exception>
        /// <exception cref="ThreadInterruptedException">  </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public Object invokeSync(final String addr, final Object request, final InvokeContext invokeContext, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual object invokeSync(string addr, object request, InvokeContext invokeContext, int timeoutMillis)
        {
            Url url = addressParser.parse(addr);
            return invokeSync(url, request, invokeContext, timeoutMillis);
        }

        /// <summary>
        /// Synchronous rpc invocation.<br>
        /// Notice! DO NOT modify the request object concurrently when this method is called.
        /// </summary>
        /// <param name="url"> </param>
        /// <param name="request"> </param>
        /// <param name="invokeContext"> </param>
        /// <param name="timeoutMillis">
        /// @return </param>
        /// <exception cref="RemotingException"> </exception>
        /// <exception cref="ThreadInterruptedException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract Object invokeSync(final Url url, final Object request, final InvokeContext invokeContext, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException;
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public abstract object invokeSync(Url url, object request, InvokeContext invokeContext, int timeoutMillis);

        /// <summary>
        /// Synchronous rpc invocation.<br>
        /// Notice! DO NOT modify the request object concurrently when this method is called.
        /// </summary>
        /// <param name="conn"> </param>
        /// <param name="request"> </param>
        /// <param name="invokeContext"> </param>
        /// <param name="timeoutMillis">
        /// @return </param>
        /// <exception cref="RemotingException"> </exception>
        /// <exception cref="ThreadInterruptedException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public Object invokeSync(final Connection conn, final Object request, final InvokeContext invokeContext, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual object invokeSync(Connection conn, object request, InvokeContext invokeContext, int timeoutMillis)
        {
            RemotingCommand requestCommand = toRemotingCommand(request, conn, invokeContext, timeoutMillis);
            preProcessInvokeContext(invokeContext, requestCommand, conn);
            ResponseCommand responseCommand = (ResponseCommand)base.invokeSync(conn, requestCommand, timeoutMillis);
            responseCommand.InvokeContext = invokeContext;

            object responseObject = RpcResponseResolver.resolveResponseObject(responseCommand, ((IPEndPoint)conn.Channel?.RemoteAddress)?.ToString());
            return responseObject;
        }

        /// <summary>
        /// Rpc invocation with future returned.<br>
        /// Notice! DO NOT modify the request object concurrently when this method is called.
        /// </summary>
        /// <param name="addr"> </param>
        /// <param name="request"> </param>
        /// <param name="invokeContext"> </param>
        /// <param name="timeoutMillis">
        /// @return </param>
        /// <exception cref="RemotingException"> </exception>
        /// <exception cref="ThreadInterruptedException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public RpcResponseFuture invokeWithFuture(final String addr, final Object request, final InvokeContext invokeContext, int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual RpcResponseFuture invokeWithFuture(string addr, object request, InvokeContext invokeContext, int timeoutMillis)
        {
            Url url = addressParser.parse(addr);
            return invokeWithFuture(url, request, invokeContext, timeoutMillis);
        }

        /// <summary>
        /// Rpc invocation with future returned.<br>
        /// Notice! DO NOT modify the request object concurrently when this method is called.
        /// </summary>
        /// <param name="url"> </param>
        /// <param name="request"> </param>
        /// <param name="invokeContext"> </param>
        /// <param name="timeoutMillis">
        /// @return </param>
        /// <exception cref="RemotingException"> </exception>
        /// <exception cref="ThreadInterruptedException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract RpcResponseFuture invokeWithFuture(final Url url, final Object request, final InvokeContext invokeContext, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException;
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public abstract RpcResponseFuture invokeWithFuture(Url url, object request, InvokeContext invokeContext, int timeoutMillis);

        /// <summary>
        /// Rpc invocation with future returned.<br>
        /// Notice! DO NOT modify the request object concurrently when this method is called.
        /// </summary>
        /// <param name="conn"> </param>
        /// <param name="request"> </param>
        /// <param name="invokeContext"> </param>
        /// <param name="timeoutMillis">
        /// @return </param>
        /// <exception cref="RemotingException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public RpcResponseFuture invokeWithFuture(final Connection conn, final Object request, final InvokeContext invokeContext, final int timeoutMillis) throws exception.RemotingException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual RpcResponseFuture invokeWithFuture(Connection conn, object request, InvokeContext invokeContext, int timeoutMillis)
        {

            RemotingCommand requestCommand = toRemotingCommand(request, conn, invokeContext, timeoutMillis);

            preProcessInvokeContext(invokeContext, requestCommand, conn);
            InvokeFuture future = base.invokeWithFuture(conn, requestCommand, timeoutMillis);
            return new RpcResponseFuture(((IPEndPoint)conn.Channel.RemoteAddress).ToString(), future);
        }

        /// <summary>
        /// Rpc invocation with callback.<br>
        /// Notice! DO NOT modify the request object concurrently when this method is called.
        /// </summary>
        /// <param name="addr"> </param>
        /// <param name="request"> </param>
        /// <param name="invokeContext"> </param>
        /// <param name="invokeCallback"> </param>
        /// <param name="timeoutMillis"> </param>
        /// <exception cref="RemotingException"> </exception>
        /// <exception cref="ThreadInterruptedException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void invokeWithCallback(String addr, Object request, final InvokeContext invokeContext, InvokeCallback invokeCallback, int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual void invokeWithCallback(string addr, object request, InvokeContext invokeContext, InvokeCallback invokeCallback, int timeoutMillis)
        {
            Url url = addressParser.parse(addr);
            invokeWithCallback(url, request, invokeContext, invokeCallback, timeoutMillis);
        }

        /// <summary>
        /// Rpc invocation with callback.<br>
        /// Notice! DO NOT modify the request object concurrently when this method is called.
        /// </summary>
        /// <param name="url"> </param>
        /// <param name="request"> </param>
        /// <param name="invokeContext"> </param>
        /// <param name="invokeCallback"> </param>
        /// <param name="timeoutMillis"> </param>
        /// <exception cref="RemotingException"> </exception>
        /// <exception cref="ThreadInterruptedException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract void invokeWithCallback(final Url url, final Object request, final InvokeContext invokeContext, final InvokeCallback invokeCallback, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException;
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public abstract void invokeWithCallback(Url url, object request, InvokeContext invokeContext, InvokeCallback invokeCallback, int timeoutMillis);

        /// <summary>
        /// Rpc invocation with callback.<br>
        /// Notice! DO NOT modify the request object concurrently when this method is called.
        /// </summary>
        /// <param name="conn"> </param>
        /// <param name="request"> </param>
        /// <param name="invokeContext"> </param>
        /// <param name="invokeCallback"> </param>
        /// <param name="timeoutMillis"> </param>
        /// <exception cref="RemotingException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void invokeWithCallback(final Connection conn, final Object request, final InvokeContext invokeContext, final InvokeCallback invokeCallback, final int timeoutMillis) throws exception.RemotingException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual void invokeWithCallback(Connection conn, object request, InvokeContext invokeContext, InvokeCallback invokeCallback, int timeoutMillis)
        {
            RemotingCommand requestCommand = toRemotingCommand(request, conn, invokeContext, timeoutMillis);
            preProcessInvokeContext(invokeContext, requestCommand, conn);
            base.invokeWithCallback(conn, requestCommand, invokeCallback, timeoutMillis);
        }

        /// <summary>
        /// Convert application request object to remoting request command.
        /// </summary>
        /// <param name="request"> </param>
        /// <param name="conn"> </param>
        /// <param name="timeoutMillis">
        /// @return </param>
        /// <exception cref="CodecException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: protected RemotingCommand toRemotingCommand(Object request, Connection conn, InvokeContext invokeContext, int timeoutMillis) throws exception.SerializationException
        protected internal virtual RemotingCommand toRemotingCommand(object request, Connection conn, InvokeContext invokeContext, int timeoutMillis)
        {
            RpcRequestCommand command = (RpcRequestCommand)CommandFactory.createRequestCommand(request);

            if (null != invokeContext)
            {
                // set client custom serializer for request command if not null
                object clientCustomSerializer = invokeContext.get(InvokeContext.BOLT_CUSTOM_SERIALIZER);
                if (null != clientCustomSerializer)
                {
                    try
                    {
                        command.Serializer = ((byte?)clientCustomSerializer).Value;
                    }
                    catch (System.InvalidCastException)
                    {
                        //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
                        throw new System.ArgumentException("Illegal custom serializer [" + clientCustomSerializer + "], the type of value should be [byte], but now is [" + clientCustomSerializer.GetType().FullName + "].");
                    }
                }

                // enable crc by default, user can disable by set invoke context `false` for key `InvokeContext.BOLT_CRC_SWITCH`
                bool? crcSwitch = (bool)invokeContext.get(InvokeContext.BOLT_CRC_SWITCH, ProtocolSwitch.CRC_SWITCH_DEFAULT_VALUE);
                if (null != crcSwitch && crcSwitch.HasValue && crcSwitch.Value)
                {
                    command.ProtocolSwitch = ProtocolSwitch.create(new int[] { ProtocolSwitch.CRC_SWITCH_INDEX });
                }
            }
            else
            {
                // enable crc by default, if there is no invoke context.
                command.ProtocolSwitch = ProtocolSwitch.create(new int[] { ProtocolSwitch.CRC_SWITCH_INDEX });
            }
            command.Timeout = timeoutMillis;
            //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            command.RequestClass = request.GetType();
            command.InvokeContext = invokeContext;
            command.serialize();
            logDebugInfo(command);
            return command;
        }

        protected internal abstract void preProcessInvokeContext(InvokeContext invokeContext, RemotingCommand cmd, Connection connection);

        /// <param name="requestCommand"> </param>
        private void logDebugInfo(RemotingCommand requestCommand)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Send request, requestId=" + requestCommand.Id);
            }
        }

        /// <seealso cref= BaseRemoting#createInvokeFuture(RemotingCommand, InvokeContext) </seealso>
        protected internal override InvokeFuture createInvokeFuture(RemotingCommand request, InvokeContext invokeContext)
        {
            return new DefaultInvokeFuture(request.Id, null, null, request.ProtocolCode.FirstByte, CommandFactory, invokeContext);
        }

        /// <seealso cref= BaseRemoting#createInvokeFuture(Connection, RemotingCommand, InvokeContext, InvokeCallback) </seealso>
        protected internal override InvokeFuture createInvokeFuture(Connection conn, RemotingCommand request, InvokeContext invokeContext, InvokeCallback invokeCallback)
        {
            return new DefaultInvokeFuture(request.Id, new RpcInvokeCallbackListener(((IPEndPoint)conn.Channel?.RemoteAddress)?.ToString()), invokeCallback, request.ProtocolCode.FirstByte, CommandFactory, invokeContext);
        }
    }

}