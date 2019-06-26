using java.lang;
using java.util.concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections;
using System.Collections.Generic;


namespace Remoting.rpc.protocol
{
    /// <summary>
    /// Rpc command handler.
    /// </summary>
    public class RpcCommandHandler : CommandHandler
    {
        public bool IsSharable => true;

        private static readonly ILogger logger = NullLogger.Instance;
        /// <summary>
        /// All processors
		/// </summary>
        internal ProcessorManager processorManager;

        internal CommandFactory commandFactory;

        /// <summary>
        /// Constructor. Initialize the processor manager and register processors.
        /// </summary>
        public RpcCommandHandler(CommandFactory commandFactory)
        {
            this.commandFactory = commandFactory;
            processorManager = new ProcessorManager();
            //process request
            processorManager.registerProcessor(RpcCommandCode.RPC_REQUEST, new RpcRequestProcessor(this.commandFactory));
            //process response
            processorManager.registerProcessor(RpcCommandCode.RPC_RESPONSE, new RpcResponseProcessor());

            processorManager.registerProcessor(CommonCommandCode.HEARTBEAT, new RpcHeartBeatProcessor());

            processorManager.registerDefaultProcessor(new AbstractRemotingProcessorAnonymousInnerClass(this));
        }

        private class AbstractRemotingProcessorAnonymousInnerClass : AbstractRemotingProcessor
        {
            private readonly RpcCommandHandler outerInstance;

            public AbstractRemotingProcessorAnonymousInnerClass(RpcCommandHandler outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
            //ORIGINAL LINE: @Override public void doProcess(RemotingContext ctx, RemotingCommand msg) throws Exception
            public override void doProcess(RemotingContext ctx, RemotingCommand msg)
            {
                logger.LogError("No processor available for command code {}, msgId {}", msg.CmdCode, msg.Id);
            }
        }

        /// <seealso cref= CommandHandler#handleCommand(RemotingContext, Object) </seealso>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void handleCommand(RemotingContext ctx, Object msg) throws Exception
        public virtual void handleCommand(RemotingContext ctx, object msg)
        {
            handle(ctx, msg);
        }

        /// <summary>
        /// Handle the request(s).
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="msg"></param>
        private void handle(RemotingContext ctx, object msg)
        {
            try
            {
                if (msg is IList)
                {
                    //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                    //ORIGINAL LINE: final Runnable handleTask = new Runnable()
                    Runnable handleTask = new TempRunnable(this, ctx, msg);
                    if (RpcConfigManager.dispatch_msg_list_in_default_executor())
                    {
                        // If msg is list ,then the batch submission to biz threadpool can save io thread.
                        // See decoder.ProtocolDecoder
                        processorManager.DefaultExecutor.execute(handleTask);
                    }
                    else
                    {
                        handleTask.run();
                    }
                }
                else
                {
                    process(ctx, msg);
                }
            }
            catch (System.Exception t)
            {
                processException(ctx, msg, t);
            }
        }

        public class TempRunnable : Runnable
        {
            private readonly RemotingContext remotingContext;
            private readonly object msg;
            private readonly RpcCommandHandler rpcCommandHandler;

            public TempRunnable(RpcCommandHandler rpcCommandHandler, RemotingContext remotingContext, object msg)
            {
                this.rpcCommandHandler = rpcCommandHandler;
                this.remotingContext = remotingContext;
                this.msg = msg;
            }
            public void run()
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("Batch message! size={}", ((List<object>)msg).Count);
                }
                foreach (var m in (List<object>)msg)
                {
                    rpcCommandHandler.process(remotingContext, m);
                }
            }
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @SuppressWarnings({ "rawtypes", "unchecked" }) private void process(RemotingContext ctx, Object msg)
        private void process(RemotingContext ctx, object msg)
        {
            try
            {
                //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                //ORIGINAL LINE: final rpc.RpcCommand cmd = (rpc.RpcCommand) msg;
                RpcCommand cmd = (RpcCommand)msg;
                //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                //ORIGINAL LINE: final RemotingProcessor processor = processorManager.getProcessor(cmd.getCmdCode());
                var processor = processorManager.getProcessor(cmd.CmdCode);
                processor.process(ctx, cmd, processorManager.DefaultExecutor);
            }
            //JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
            //ORIGINAL LINE: catch (final Throwable t)
            catch (System.Exception t)
            {
                processException(ctx, msg, t);
            }
        }

        private void processException(RemotingContext ctx, object msg, System.Exception t)
        {
            if (msg is IList)
            {
                //JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
                //ORIGINAL LINE: for (final Object m : (java.util.List<object><?>) msg)
                foreach (var m in (IList<object>)msg)
                {
                    processExceptionForSingleCommand(ctx, m, t);
                }
            }
            else
            {
                processExceptionForSingleCommand(ctx, msg, t);
            }
        }

        /*
		 * Return error command if necessary.
		 */
        private void processExceptionForSingleCommand(RemotingContext ctx, object msg, System.Exception t)
        {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final int id = ((rpc.RpcCommand) msg).getId();
            int id = ((RpcCommand)msg).Id;
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final String emsg = "Exception caught when processing " + ((msg instanceof rpc.RequestCommand) ? "request, id=" : "response, id=");
            string emsg = "Exception caught when processing " + ((msg is RequestCommand) ? "request, id=" : "response, id=");
            logger.LogWarning(emsg + id, t);
            if (msg is RequestCommand)
            {
                //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                //ORIGINAL LINE: final rpc.RequestCommand cmd = (rpc.RequestCommand) msg;
                RequestCommand cmd = (RequestCommand)msg;
                if (cmd.Type != RpcCommandType.REQUEST_ONEWAY)
                {
                    if (t is RejectedExecutionException)
                    {
                        ResponseCommand response = (ResponseCommand)commandFactory.createExceptionResponse(id, ResponseStatus.SERVER_THREADPOOL_BUSY);
                        var task = ctx.ChannelContext.WriteAndFlushAsync(response);
                        task.ContinueWith((writeFlushTask) =>
                        {
                            if (writeFlushTask.IsCompletedSuccessfully)
                            {
                                if (logger.IsEnabled(LogLevel.Information))
                                {
                                    logger.LogInformation("Write back exception response done, requestId={}, status={}", id, response.ResponseStatus);
                                }
                            }
                            else
                            {
                                logger.LogError("Write back exception response failed, requestId={}", id, writeFlushTask.Exception);
                            }
                        });
                    }
                }
            }
        }

        public void registerProcessor(CommandCode cmd, RemotingProcessor processor)
        {
            processorManager.registerProcessor(cmd, processor);
        }

        public void registerDefaultExecutor(ExecutorService executor)
        {
            processorManager.registerDefaultExecutor(executor);
        }

        public ExecutorService DefaultExecutor
        {
            get
            {
                return processorManager.DefaultExecutor;
            }
        }
    }
}