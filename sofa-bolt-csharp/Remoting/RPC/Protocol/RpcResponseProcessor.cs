using java.util.concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Remoting.rpc.protocol
{
    /// <summary>
    /// Processor to process RpcResponse.
    /// </summary>
    public class RpcResponseProcessor : AbstractRemotingProcessor
    {

        private static readonly ILogger logger = NullLogger.Instance;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public RpcResponseProcessor()
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public RpcResponseProcessor(ExecutorService executor) : base(executor)
        {
        }

        /// <seealso cref= AbstractRemotingProcessor#doProcess </seealso>
        public override void doProcess(RemotingContext ctx, RemotingCommand cmd)
        {

            Connection conn = ctx.ChannelContext.Channel.GetAttribute(Connection.CONNECTION).Get();
            InvokeFuture future = conn.removeInvokeFuture(cmd.Id);
            try
            {
                if (future != null)
                {
                    future.putResponse(cmd);
                    future.cancelTimeout();
                    try
                    {
                        future.executeInvokeCallback();
                    }
                    catch (System.Exception e)
                    {
                        logger.LogError("Exception caught when executing invoke callback, id={}", cmd.Id, e);
                    }
                }
                else
                {
                    logger.LogWarning("Cannot find InvokeFuture, maybe already timeout, id={}, from={} ", cmd.Id, ctx.ChannelContext.Channel.RemoteAddress.ToString());
                }
            }
            finally
            {
            }
        }
    }
}