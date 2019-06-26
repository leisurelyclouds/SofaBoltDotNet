using System;

namespace Remoting.rpc.protocol
{
	/// <summary>
	/// Extends this to process user defined request in SYNC way.<br>
	/// If you want process reqeuest in ASYNC way, please extends <seealso cref="AsyncUserProcessor"/>.
	/// </summary>
	public abstract class SyncUserProcessor : AbstractUserProcessor
	{
        /// <seealso cref= UserProcessor#handleRequest(BizContext, java.lang.Object) </seealso>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public abstract Object handleRequest(BizContext bizCtx, T request) throws Exception;
        public override abstract object handleRequest(BizContext bizCtx, object request);

        /// <summary>
        /// unsupported here!
        /// </summary>
        /// <seealso cref= UserProcessor#handleRequest(BizContext, AsyncContext, java.lang.Object) </seealso>
        public override void handleRequest(BizContext bizCtx, AsyncContext asyncCtx, object request)
		{
			throw new System.NotSupportedException("ASYNC handle request is unsupported in SyncUserProcessor!");
		}

        /// <seealso cref= UserProcessor#interest() </seealso>
        public override abstract Type interest();
	}

}