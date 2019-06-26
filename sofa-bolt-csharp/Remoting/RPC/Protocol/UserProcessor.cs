using java.util.concurrent;
using System;

namespace Remoting.rpc.protocol
{
    /// <summary>
    /// Defined all functions for biz to process user defined request.
    /// </summary>
    public interface UserProcessor
    {

        /// <summary>
        /// Pre handle request, to avoid expose <seealso cref="RemotingContext"/> directly to biz handle request logic.
        /// </summary>
        /// <param name="remotingCtx"> remoting context </param>
        /// <param name="request"> request </param>
        /// <returns> BizContext </returns>
        BizContext preHandleRequest(RemotingContext remotingCtx, object request);

        /// <summary>
        /// Handle request with <seealso cref="AsyncContext"/>. </summary>
        /// <param name="bizCtx"> biz context </param>
        /// <param name="asyncCtx"> async context </param>
        /// <param name="request"> request </param>
        void handleRequest(BizContext bizCtx, AsyncContext asyncCtx, object request);

        /// <summary>
        /// Handle request in sync way.
		/// </summary>
        /// <param name="bizCtx"> biz context </param>
        /// <param name="request"> request </param>
        object handleRequest(BizContext bizCtx, object request);

        /// <summary>
        /// The class name of user request.
        /// Use String type to avoid classloader problem.
        /// </summary>
        /// <returns> interested request's class name </returns>
        Type interest();

        /// <summary>
        /// Get user executor.
		/// </summary>
        /// <returns> executor </returns>
        Executor Executor { get; }

        /// <summary>
        /// Whether deserialize and process biz logic in io thread.
        /// Notice: If return true, this will have a strong impact on performance.
		/// </summary>
        /// <returns> true for processing in io thread </returns>
        bool processInIOThread();

        /// <summary>
        /// Whether handle request timeout automatically, we call this fail fast processing when detect timeout.
        /// 
        /// Notice: If you do not want to enable this feature, you need to override this method to return false,
        /// and then call <seealso cref="BizContext#isRequestTimeout()"/> to check by yourself if you want.
        /// </summary>
        /// <returns> true if you want to enable fail fast processing, otherwise return false </returns>
        bool timeoutDiscard();

        /// <summary>
        /// Use this method to set executor selector.
		/// </summary>
        /// <param name="executorSelector"> executor selector </param>
        UserProcessor_ExecutorSelector ExecutorSelector { set; get; }


        /// <summary>
        /// Executor selector interface.
        /// You can implement this and then provide a <seealso cref="ExecutorSelector"/> using method <seealso cref="#setExecutorSelector(ExecutorSelector)"/>
        /// 
        /// @author xiaomin.cxm
        /// @version $Id: ExecutorSelector.java, v 0.1 April 24, 2017 17:16:13 PM xiaomin.cxm Exp $
        /// </summary>
    }

    public interface UserProcessor_ExecutorSelector
    {
        Executor select(Type requestClass, object requestHeader);
    }
}