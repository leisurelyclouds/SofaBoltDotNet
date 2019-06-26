using System;

namespace Remoting.rpc.protocol
{
	/// <summary>
	/// Extends this to process user defined request in ASYNC way.<br>
	/// If you want process reqeuest in SYNC way, please extends <seealso cref="SyncUserProcessor"/>.
	/// </summary>
	public abstract class AsyncUserProcessor : AbstractUserProcessor
	{
        /// <summary>
        /// unsupported here!
        /// </summary>
        /// <seealso cref= UserProcessor#handleRequest(BizContext, java.lang.Object) </seealso>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public Object handleRequest(BizContext bizCtx, T request) throws Exception
        public override object handleRequest(BizContext bizCtx, object request)
		{
			throw new System.NotSupportedException("SYNC handle request is unsupported in AsyncUserProcessor!");
		}

        /// <seealso cref= UserProcessor#handleRequest(BizContext, AsyncContext, java.lang.Object) </seealso>
        public override abstract void handleRequest(BizContext bizCtx, AsyncContext asyncCtx, object request);

        /// <seealso cref= UserProcessor#interest() </seealso>
        public override abstract Type interest();
	}
}