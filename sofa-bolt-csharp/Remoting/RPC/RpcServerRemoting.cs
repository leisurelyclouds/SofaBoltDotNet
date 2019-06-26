using Remoting.exception;
using System.Net;

namespace Remoting.rpc
{
    /// <summary>
    /// Rpc server remoting
    /// </summary>
    public class RpcServerRemoting : RpcRemoting
	{

		/// <summary>
		/// default constructor
		/// </summary>
		public RpcServerRemoting(CommandFactory commandFactory) : base(commandFactory)
		{
		}

		/// <param name="addressParser"> </param>
		/// <param name="connectionManager"> </param>
		public RpcServerRemoting(CommandFactory commandFactory, RemotingAddressParser addressParser, DefaultConnectionManager connectionManager) : base(commandFactory, addressParser, connectionManager)
		{
		}

        /// <seealso cref= RpcRemoting#invokeSync(Url, java.lang.Object, InvokeContext, int) </seealso>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public Object invokeSync(Url url, Object request, InvokeContext invokeContext, int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        public override object invokeSync(Url url, object request, InvokeContext invokeContext, int timeoutMillis)
		{
			Connection conn = connectionManager.get(url.UniqueKey);
			if (null == conn)
			{
				throw new RemotingException("Client address [" + url.UniqueKey + "] not connected yet!");
			}
			connectionManager.check(conn);
			return invokeSync(conn, request, invokeContext, timeoutMillis);
		}

        /// <seealso cref= RpcRemoting#oneway(Url, java.lang.Object, InvokeContext) </seealso>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void oneway(Url url, Object request, InvokeContext invokeContext) throws exception.RemotingException
        public override void oneway(Url url, object request, InvokeContext invokeContext)
		{
			Connection conn = connectionManager.get(url.UniqueKey);
			if (null == conn)
			{
				throw new RemotingException("Client address [" + url.OriginUrl + "] not connected yet!");
			}
			connectionManager.check(conn);
			oneway(conn, request, invokeContext);
		}

        /// <seealso cref= RpcRemoting#invokeWithFuture(Url, java.lang.Object, InvokeContext, int) </seealso>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public RpcResponseFuture invokeWithFuture(Url url, Object request, InvokeContext invokeContext, int timeoutMillis) throws exception.RemotingException
        public override RpcResponseFuture invokeWithFuture(Url url, object request, InvokeContext invokeContext, int timeoutMillis)
		{
			Connection conn = connectionManager.get(url.UniqueKey);
			if (null == conn)
			{
				throw new RemotingException("Client address [" + url.UniqueKey + "] not connected yet!");
			}
			connectionManager.check(conn);
			return invokeWithFuture(conn, request, invokeContext, timeoutMillis);
		}

        /// <seealso cref= RpcRemoting#invokeWithCallback(Url, java.lang.Object, InvokeContext, InvokeCallback, int) </seealso>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void invokeWithCallback(Url url, Object request, InvokeContext invokeContext, InvokeCallback invokeCallback, int timeoutMillis) throws exception.RemotingException
        public override void invokeWithCallback(Url url, object request, InvokeContext invokeContext, InvokeCallback invokeCallback, int timeoutMillis)
		{
			Connection conn = connectionManager.get(url.UniqueKey);
			if (null == conn)
			{
				throw new RemotingException("Client address [" + url.UniqueKey + "] not connected yet!");
			}
			connectionManager.check(conn);
			invokeWithCallback(conn, request, invokeContext, invokeCallback, timeoutMillis);
		}

		protected internal override void preProcessInvokeContext(InvokeContext invokeContext, RemotingCommand cmd, Connection connection)
		{
			if (null != invokeContext)
			{
				invokeContext.putIfAbsent(InvokeContext.SERVER_REMOTE_IP, ((IPEndPoint)connection.Channel.RemoteAddress).Address);
				invokeContext.putIfAbsent(InvokeContext.SERVER_REMOTE_PORT, ((IPEndPoint)connection.Channel.RemoteAddress).Port);
				invokeContext.putIfAbsent(InvokeContext.SERVER_LOCAL_IP, ((IPEndPoint)connection.Channel.LocalAddress).Address);
				invokeContext.putIfAbsent(InvokeContext.SERVER_LOCAL_PORT, ((IPEndPoint)connection.Channel.LocalAddress).Port);
				invokeContext.putIfAbsent(InvokeContext.BOLT_INVOKE_REQUEST_ID, cmd.Id);
			}
		}
	}

}