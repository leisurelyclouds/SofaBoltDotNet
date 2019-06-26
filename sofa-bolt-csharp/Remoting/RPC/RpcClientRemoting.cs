using System;
using System.Net;

namespace Remoting.rpc
{
    /// <summary>
    /// Rpc client remoting
    /// 
    /// @author xiaomin.cxm
    /// @version $Id: RpcClientRemoting.java, v 0.1 Apr 14, 2016 11:58:56 AM xiaomin.cxm Exp $
    /// </summary>
    public class RpcClientRemoting : RpcRemoting
	{

		public RpcClientRemoting(CommandFactory commandFactory, RemotingAddressParser addressParser, DefaultConnectionManager connectionManager) : base(commandFactory, addressParser, connectionManager)
		{
		}

        /// <seealso cref= RpcRemoting#oneway(Url, java.lang.Object, InvokeContext) </seealso>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void oneway(Url url, Object request, InvokeContext invokeContext) throws exception.RemotingException, ThreadInterruptedException
        public override void oneway(Url url, object request, InvokeContext invokeContext)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Connection conn = getConnectionAndInitInvokeContext(url, invokeContext);
			Connection conn = getConnectionAndInitInvokeContext(url, invokeContext);
			connectionManager.check(conn);
			oneway(conn, request, invokeContext);
		}

        /// <seealso cref= RpcRemoting#invokeSync(Url, java.lang.Object, InvokeContext, int) </seealso>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public Object invokeSync(Url url, Object request, InvokeContext invokeContext, int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        public override object invokeSync(Url url, object request, InvokeContext invokeContext, int timeoutMillis)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Connection conn = getConnectionAndInitInvokeContext(url, invokeContext);
			Connection conn = getConnectionAndInitInvokeContext(url, invokeContext);
			connectionManager.check(conn);
			return invokeSync(conn, request, invokeContext, timeoutMillis);
		}

        /// <seealso cref= RpcRemoting#invokeWithFuture(Url, java.lang.Object, InvokeContext, int) </seealso>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public RpcResponseFuture invokeWithFuture(Url url, Object request, InvokeContext invokeContext, int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        public override RpcResponseFuture invokeWithFuture(Url url, object request, InvokeContext invokeContext, int timeoutMillis)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Connection conn = getConnectionAndInitInvokeContext(url, invokeContext);
			Connection conn = getConnectionAndInitInvokeContext(url, invokeContext);
			connectionManager.check(conn);
			return invokeWithFuture(conn, request, invokeContext, timeoutMillis);
		}

        /// <seealso cref= RpcRemoting#invokeWithCallback(Url, java.lang.Object, InvokeContext, InvokeCallback, int) </seealso>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void invokeWithCallback(Url url, Object request, InvokeContext invokeContext, InvokeCallback invokeCallback, int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException
        public override void invokeWithCallback(Url url, object request, InvokeContext invokeContext, InvokeCallback invokeCallback, int timeoutMillis)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Connection conn = getConnectionAndInitInvokeContext(url, invokeContext);
			Connection conn = getConnectionAndInitInvokeContext(url, invokeContext);
			connectionManager.check(conn);
			invokeWithCallback(conn, request, invokeContext, invokeCallback, timeoutMillis);
		}

		/// <seealso cref= RpcRemoting#preProcessInvokeContext(InvokeContext, RemotingCommand, Connection) </seealso>
		protected internal override void preProcessInvokeContext(InvokeContext invokeContext, RemotingCommand cmd, Connection connection)
		{
			if (null != invokeContext)
			{
				invokeContext.putIfAbsent(InvokeContext.CLIENT_LOCAL_IP, ((IPEndPoint)connection.Channel.LocalAddress).Address);
				invokeContext.putIfAbsent(InvokeContext.CLIENT_LOCAL_PORT, ((IPEndPoint)connection.Channel.LocalAddress).Port);
				invokeContext.putIfAbsent(InvokeContext.CLIENT_REMOTE_IP, ((IPEndPoint)connection.Channel.RemoteAddress).Address);
				invokeContext.putIfAbsent(InvokeContext.CLIENT_REMOTE_PORT, ((IPEndPoint)connection.Channel.RemoteAddress).Port);
				invokeContext.putIfAbsent(InvokeContext.BOLT_INVOKE_REQUEST_ID, cmd.Id);
			}
		}

		/// <summary>
		/// Get connection and set init invokeContext if invokeContext not {@code null}
		/// </summary>
		/// <param name="url"> target url </param>
		/// <param name="invokeContext"> invoke context to set </param>
		/// <returns> connection </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected Connection getConnectionAndInitInvokeContext(Url url, InvokeContext invokeContext) throws exception.RemotingException, ThreadInterruptedException
		protected internal virtual Connection getConnectionAndInitInvokeContext(Url url, InvokeContext invokeContext)
		{
			long start = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
			Connection conn;
			try
			{
				conn = connectionManager.getAndCreateIfAbsent(url);
			}
			finally
			{
				if (null != invokeContext)
				{
					invokeContext.putIfAbsent(InvokeContext.CLIENT_CONN_CREATETIME, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() - start);
				}
			}
			return conn;
		}
	}

}