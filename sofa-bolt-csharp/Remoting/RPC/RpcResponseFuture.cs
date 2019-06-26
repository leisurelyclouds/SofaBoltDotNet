using Remoting.rpc.exception;

namespace Remoting.rpc
{
	/// <summary>
	/// The future for response.
	/// </summary>
	public class RpcResponseFuture
	{
		/// <summary>
		/// rpc server address
		/// </summary>
		private string addr;

		/// <summary>
		/// rpc server port
		/// </summary>
		private InvokeFuture future;

		/// <summary>
		/// Constructor
		/// </summary>
		public RpcResponseFuture(string addr, InvokeFuture future)
		{
			this.addr = addr;
			this.future = future;
		}

		/// <summary>
		/// Whether the future is done.
		/// </summary>
		public virtual bool Done
		{
			get
			{
				return future.Done;
			}
		}

		/// <summary>
		/// get result with timeout specified
		/// 
		/// if request done, resolve normal responseObject
		/// if request not done, throws InvokeTimeoutException
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object get(int timeoutMillis) throws rpc.exception.InvokeTimeoutException, exception.RemotingException, ThreadInterruptedException
		public virtual object get(int timeoutMillis)
		{
			future.waitResponse(timeoutMillis);
			if (!Done)
			{
				throw new InvokeTimeoutException("Future get result timeout!");
			}
			ResponseCommand responseCommand = (ResponseCommand) future.waitResponse();
			responseCommand.InvokeContext = future.InvokeContext;
			return RpcResponseResolver.resolveResponseObject(responseCommand, addr);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object get() throws exception.RemotingException, ThreadInterruptedException
		public virtual object get()
		{
			ResponseCommand responseCommand = (ResponseCommand) future.waitResponse();
			responseCommand.InvokeContext = future.InvokeContext;
			return RpcResponseResolver.resolveResponseObject(responseCommand, addr);
		}

	}

}