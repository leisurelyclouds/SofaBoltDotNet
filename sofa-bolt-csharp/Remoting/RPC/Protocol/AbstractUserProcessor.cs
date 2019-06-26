using java.util.concurrent;
using System;

namespace Remoting.rpc.protocol
{


    /// <summary>
    /// Implements common function and provide default value.
    /// </summary>
    public abstract class AbstractUserProcessor : UserProcessor
    {
        public abstract Type interest();
        public abstract object handleRequest(BizContext bizCtx, object request);
        public abstract void handleRequest(BizContext bizCtx, AsyncContext asyncCtx, object request);

        /// <summary>
        /// executor selector, default null unless provide one using its setter method
		/// </summary>
        protected internal UserProcessor_ExecutorSelector executorSelector;

        /// <summary>
        /// Provide a default - <seealso cref="DefaultBizContext"/> implementation of <seealso cref="BizContext"/>.
        /// </summary>
        /// <seealso cref= UserProcessor#preHandleRequest(RemotingContext, java.lang.Object) </seealso>
        public virtual BizContext preHandleRequest(RemotingContext remotingCtx, object request)
        {
            return new DefaultBizContext(remotingCtx);
        }

        /// <summary>
        /// By default return null.
        /// </summary>
        /// <seealso cref= UserProcessor#getExecutor() </seealso>
        public virtual Executor Executor
        {
            get
            {
                return null;
            }
        }

        /// <seealso cref= UserProcessor#getExecutorSelector() </seealso>
        public virtual UserProcessor_ExecutorSelector ExecutorSelector
        {
            get
            {
                return executorSelector;
            }
            set
            {
                executorSelector = value;
            }
        }


        /// <summary>
        /// By default, return false, means not deserialize and process biz logic in io thread
        /// </summary>
        /// <seealso cref= UserProcessor#processInIOThread()  </seealso>
        public virtual bool processInIOThread()
        {
            return false;
        }

        /// <summary>
        /// By default, return true, means discard requests which timeout already.
        /// </summary>
        public virtual bool timeoutDiscard()
        {
            return true;
        }
    }
}