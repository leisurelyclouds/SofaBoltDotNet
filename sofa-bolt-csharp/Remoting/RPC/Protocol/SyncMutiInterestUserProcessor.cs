using System;
using System.Collections.Generic;

namespace Remoting.rpc.protocol
{
    /// <summary>
    /// Extends this to process user defined request in SYNC way.<br>
    /// If you want process reqeuest in ASYNC way, please extends <seealso cref="AsynMultiInterestUserProcessor"/>.
    /// </summary>
    public abstract class SyncMutiInterestUserProcessor : AbstractMultiInterestUserProcessor
	{

        /// <seealso cref= UserProcessor#handleRequest(BizContext, java.lang.Object) </seealso>
        public override abstract object handleRequest(BizContext bizCtx, object request);

        /// <summary>
        /// unsupported here!
        /// </summary>
        /// <seealso cref= UserProcessor#handleRequest(BizContext, AsyncContext, java.lang.Object) </seealso>
        public override void handleRequest(BizContext bizCtx, AsyncContext asyncCtx, object request)
		{
			throw new System.NotSupportedException("ASYNC handle request is unsupported in SyncMutiInterestUserProcessor!");
		}

        /// <seealso cref= MultiInterestUserProcessor#multiInterest() </seealso>
        public override abstract List<Type> multiInterest();

	}

}