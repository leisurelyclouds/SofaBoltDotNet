using java.util.concurrent.atomic;

namespace Remoting.rpc.protocol
{
	/// <summary>
	/// Async biz context of Rpc.
	/// </summary>
	public class RpcAsyncContext : AsyncContext
	{
		/// <summary>
		/// remoting context
		/// </summary>
		private RemotingContext ctx;

		/// <summary>
		/// rpc request command
		/// </summary>
		private RpcRequestCommand cmd;

		private RpcRequestProcessor processor;

		/// <summary>
		/// is response sent already
		/// </summary>
		private AtomicBoolean isResponseSentAlready = new AtomicBoolean();

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="ctx"> remoting context </param>
		/// <param name="cmd"> rpc request command </param>
		/// <param name="processor"> rpc request processor </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public RpcAsyncContext(final RemotingContext ctx, final RpcRequestCommand cmd, final RpcRequestProcessor processor)
		public RpcAsyncContext(RemotingContext ctx, RpcRequestCommand cmd, RpcRequestProcessor processor)
		{
			this.ctx = ctx;
			this.cmd = cmd;
			this.processor = processor;
		}

		/// <seealso cref= AsyncContext#sendResponse(java.lang.Object) </seealso>
		public virtual void sendResponse(object responseObject)
		{
			if (isResponseSentAlready.compareAndSet(false, true))
			{
				processor.sendResponseIfNecessary(ctx, cmd.Type, processor.CommandFactory.createResponse(responseObject, cmd));
			}
			else
			{
				throw new System.InvalidOperationException("Should not send rpc response repeatedly!");
			}
		}
	}
}