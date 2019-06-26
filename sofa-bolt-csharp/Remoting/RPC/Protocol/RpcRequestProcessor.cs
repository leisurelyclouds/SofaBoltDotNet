using DotNetty.Transport.Channels;
using java.lang;
using java.util.concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting.exception;
using System;
using System.Net;

namespace Remoting.rpc.protocol
{
    /// <summary>
    /// Process Rpc request.
    /// </summary>
    public class RpcRequestProcessor : AbstractRemotingProcessor
    {
        /// <summary>
        /// logger
		/// </summary>
        private static readonly ILogger logger = NullLogger.Instance;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public RpcRequestProcessor()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public RpcRequestProcessor(CommandFactory commandFactory) : base(commandFactory)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public RpcRequestProcessor(ExecutorService executor) : base(executor)
        {
        }

        /// <seealso cref= AbstractRemotingProcessor#process(RemotingContext, RemotingCommand, java.util.concurrent.ExecutorService) </seealso>
        public override void process(RemotingContext ctx, RemotingCommand cmd, ExecutorService defaultExecutor)
        {
            var rpcRequestCommand = (RpcRequestCommand)cmd;
            if (!deserializeRequestCommand(ctx, rpcRequestCommand, RpcDeserializeLevel.DESERIALIZE_CLAZZ))
            {
                return;
            }
            UserProcessor userProcessor = ctx.getUserProcessor(rpcRequestCommand.RequestClass);
            if (userProcessor == null)
            {
                string errMsg = "No user processor found for request: " + rpcRequestCommand.RequestClass;
                logger.LogError(errMsg);
                sendResponseIfNecessary(ctx, rpcRequestCommand.Type, CommandFactory.createExceptionResponse(cmd.Id, errMsg));
                return; // must end process
            }

            // set timeout check state from user's processor
            ctx.setTimeoutDiscard(userProcessor.timeoutDiscard());

            // to check whether to process in io thread
            if (userProcessor.processInIOThread())
            {
                if (!deserializeRequestCommand(ctx, rpcRequestCommand, RpcDeserializeLevel.DESERIALIZE_ALL))
                {
                    return;
                }
                // process in io thread
                new ProcessTask(this, ctx, rpcRequestCommand).run();
                return; // end
            }

            Executor executor;
            // to check whether get executor using executor selector
            if (null == userProcessor.ExecutorSelector)
            {
                executor = userProcessor.Executor;
            }
            else
            {
                // in case haven't deserialized in io thread
                // it need to deserialize clazz and header before using executor dispath strategy
                if (!deserializeRequestCommand(ctx, rpcRequestCommand, RpcDeserializeLevel.DESERIALIZE_HEADER))
                {
                    return;
                }
                //try get executor with strategy
                executor = userProcessor.ExecutorSelector.select(rpcRequestCommand.RequestClass, rpcRequestCommand.RequestHeader);
            }

            // Till now, if executor still null, then try default
            if (executor == null)
            {
                executor = Executor ?? defaultExecutor;
            }

            // use the final executor dispatch process task
            executor.execute(new ProcessTask(this, ctx, rpcRequestCommand));
        }

        /// <seealso cref= AbstractRemotingProcessor#doProcess(RemotingContext, RemotingCommand) </seealso>
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @SuppressWarnings({ "rawtypes", "unchecked" }) @Override public void doProcess(final RemotingContext ctx, RpcRequestCommand cmd) throws Exception
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual void doProcess(RemotingContext ctx, RpcRequestCommand cmd)
        {
            long currentTimestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();

            preProcessRemotingContext(ctx, cmd, currentTimestamp);
            if (ctx.TimeoutDiscard && ctx.RequestTimeout)
            {
                timeoutLog(cmd, currentTimestamp, ctx); // do some log
                return; // then, discard this request
            }
            debugLog(ctx, cmd, currentTimestamp);
            // decode request all
            if (!deserializeRequestCommand(ctx, cmd, RpcDeserializeLevel.DESERIALIZE_ALL))
            {
                return;
            }
            dispatchToUserProcessor(ctx, cmd);
        }

        public override void doProcess(RemotingContext ctx, RemotingCommand msg)
        {
            doProcess(ctx, (RpcRequestCommand)msg);
        }

        /// <summary>
        /// Send response using remoting context if necessary.<br>
        /// If request type is oneway, no need to send any response nor exception.
        /// </summary>
        /// <param name="ctx"> remoting context </param>
        /// <param name="type"> type code </param>
        /// <param name="response"> remoting command </param>
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        //ORIGINAL LINE: public void sendResponseIfNecessary(final RemotingContext ctx, byte type, final RemotingCommand response)
        public virtual void sendResponseIfNecessary(RemotingContext ctx, byte type, RemotingCommand response)
        {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final int id = response.getId();
            int id = response.Id;
            if (type != RpcCommandType.REQUEST_ONEWAY)
            {
                RemotingCommand serializedResponse = response;
                try
                {
                    response.serialize();
                }
                catch (SerializationException e)
                {
                    string errMsg = "SerializationException occurred when sendResponseIfNecessary in RpcRequestProcessor, id=" + id;
                    logger.LogError(errMsg, e);
                    serializedResponse = CommandFactory.createExceptionResponse(id, ResponseStatus.SERVER_SERIAL_EXCEPTION, e);
                    try
                    {
                        serializedResponse.serialize(); // serialize again for exception response
                    }
                    catch (SerializationException)
                    {
                        // should not happen
                        logger.LogError("serialize SerializationException response failed!");
                    }
                }
                catch (System.Exception t)
                {
                    string errMsg = "Serialize RpcResponseCommand failed when sendResponseIfNecessary in RpcRequestProcessor, id=" + id;
                    logger.LogError(errMsg, t);
                    serializedResponse = CommandFactory.createExceptionResponse(id, t, errMsg);
                }

                var writeFlushTask = ctx.writeAndFlush(serializedResponse);
                writeFlushTask.ContinueWith((task) =>
                {
                    if (logger.IsEnabled(LogLevel.Debug))
                    {
                        logger.LogDebug("Rpc response sent! requestId=" + id + ". The address is " + ctx.ChannelContext.Channel.RemoteAddress.ToString());
                    }
                    if (!task.IsCompletedSuccessfully)
                    {
                        logger.LogError("Rpc response send failed! id=" + id + ". The address is " + ctx.ChannelContext.Channel.RemoteAddress.ToString(), task.Exception);
                    }
                });
                //.addListener(new ChannelFutureListenerAnonymousInnerClass(this, ctx, id));
            }
            else
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("Oneway rpc request received, do not send response, id=" + id + ", the address is " + ctx.ChannelContext.Channel.RemoteAddress.ToString());
                }
            }
        }

        /// <summary>
        /// dispatch request command to user processor
		/// </summary>
        /// <param name="ctx"> remoting context </param>
        /// <param name="cmd"> rpc request command </param>
        private void dispatchToUserProcessor(RemotingContext ctx, RpcRequestCommand cmd)
        {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final int id = cmd.getId();
            int id = cmd.Id;
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final byte type = cmd.getType();
            byte type = cmd.Type;
            // processor here must not be null, for it have been checked before
            UserProcessor processor = ctx.getUserProcessor(cmd.RequestClass);
            if (processor is AsyncUserProcessor)
            {
                try
                {
                    processor.handleRequest(processor.preHandleRequest(ctx, cmd.RequestObject), new RpcAsyncContext(ctx, cmd, this), cmd.RequestObject);
                }
                catch (RejectedExecutionException)
                {
                    logger.LogWarning("RejectedExecutionException occurred when do ASYNC process in RpcRequestProcessor");
                    sendResponseIfNecessary(ctx, type, CommandFactory.createExceptionResponse(id, ResponseStatus.SERVER_THREADPOOL_BUSY));
                }
                catch (System.Exception t)
                {
                    string errMsg = "AYSNC process rpc request failed in RpcRequestProcessor, id=" + id;
                    logger.LogError(errMsg, t);
                    sendResponseIfNecessary(ctx, type, CommandFactory.createExceptionResponse(id, t, errMsg));
                }
            }
            else
            {
                try
                {
                    object responseObject = processor.handleRequest(processor.preHandleRequest(ctx, cmd.RequestObject), cmd.RequestObject);

                    sendResponseIfNecessary(ctx, type, CommandFactory.createResponse(responseObject, cmd));
                }
                catch (RejectedExecutionException)
                {
                    logger.LogWarning("RejectedExecutionException occurred when do SYNC process in RpcRequestProcessor");
                    sendResponseIfNecessary(ctx, type, CommandFactory.createExceptionResponse(id, ResponseStatus.SERVER_THREADPOOL_BUSY));
                }
                catch (System.Exception t)
                {
                    string errMsg = "SYNC process rpc request failed in RpcRequestProcessor, id=" + id;
                    logger.LogError(errMsg, t);
                    sendResponseIfNecessary(ctx, type, CommandFactory.createExceptionResponse(id, t, errMsg));
                }
            }
        }

        /// <summary>
        /// deserialize request command
        /// </summary>
        /// <returns> true if deserialize success; false if exception catched </returns>
        private bool deserializeRequestCommand(RemotingContext ctx, RpcRequestCommand cmd, int level)
        {
            bool result;
            try
            {
                cmd.deserialize(level);
                result = true;
            }
            catch (DeserializationException e)
            {
                logger.LogError("DeserializationException occurred when process in RpcRequestProcessor, id={}, deserializeLevel={}", cmd.Id, RpcDeserializeLevel.valueOf(level), e);
                sendResponseIfNecessary(ctx, cmd.Type, CommandFactory.createExceptionResponse(cmd.Id, ResponseStatus.SERVER_DESERIAL_EXCEPTION, e));
                result = false;
            }
            catch (System.Exception t)
            {
                string errMsg = "Deserialize RpcRequestCommand failed in RpcRequestProcessor, id=" + cmd.Id + ", deserializeLevel=" + level;
                logger.LogError(errMsg, t);
                sendResponseIfNecessary(ctx, cmd.Type, CommandFactory.createExceptionResponse(cmd.Id, t, errMsg));
                result = false;
            }
            return result;
        }

        /// <summary>
        /// pre process remoting context, initial some useful infos and pass to biz
        /// </summary>
        /// <param name="ctx"> remoting context </param>
        /// <param name="cmd"> rpc request command </param>
        /// <param name="currentTimestamp"> current timestamp </param>
        private void preProcessRemotingContext(RemotingContext ctx, RpcRequestCommand cmd, long currentTimestamp)
        {
            ctx.ArriveTimestamp = cmd.ArriveTime;
            ctx.Timeout = cmd.Timeout;
            ctx.RpcCommandType = cmd.Type;
            ctx.InvokeContext.putIfAbsent(InvokeContext.BOLT_PROCESS_WAIT_TIME, currentTimestamp - cmd.ArriveTime);
        }

        /// <summary>
        /// print some log when request timeout and discarded in io thread.
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        //ORIGINAL LINE: private void timeoutLog(final RpcRequestCommand cmd, long currentTimestamp, RemotingContext ctx)
        private void timeoutLog(RpcRequestCommand cmd, long currentTimestamp, RemotingContext ctx)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("request id [{}] currenTimestamp [{}] - arriveTime [{}] = server cost [{}] >= timeout value [{}].", cmd.Id, currentTimestamp, cmd.ArriveTime, currentTimestamp - cmd.ArriveTime, cmd.Timeout);
            }

            string remoteAddr = "UNKNOWN";
            if (null != ctx)
            {
                IChannelHandlerContext channelCtx = ctx.ChannelContext;
                IChannel channel = channelCtx.Channel;
                if (null != channel)
                {
                    remoteAddr = ((IPEndPoint)channel.RemoteAddress).ToString();
                }
            }
            logger.LogWarning("Rpc request id[{}], from remoteAddr[{}] stop process, total wait time in queue is [{}], client timeout setting is [{}].", cmd.Id, remoteAddr, currentTimestamp - cmd.ArriveTime, cmd.Timeout);
        }

        /// <summary>
        /// print some debug log when receive request
        /// </summary>
        private void debugLog(RemotingContext ctx, RpcRequestCommand cmd, long currentTimestamp)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Rpc request received! requestId={}, from {}", cmd.Id, ctx.ChannelContext.Channel.RemoteAddress.ToString());
                logger.LogDebug("request id {} currenTimestamp {} - arriveTime {} = server cost {} < timeout {}.", cmd.Id, currentTimestamp, cmd.ArriveTime, currentTimestamp - cmd.ArriveTime, cmd.Timeout);
            }
        }

        /// <summary>
        /// Inner process task
        /// 
        /// @author xiaomin.cxm
        /// @version $Id: RpcRequestProcessor.java, v 0.1 May 19, 2016 4:01:28 PM xiaomin.cxm Exp $
        /// </summary>
        class ProcessTask : Runnable
        {
            private readonly RpcRequestProcessor outerInstance;

            internal RemotingContext ctx;
            internal RpcRequestCommand msg;

            public ProcessTask(RpcRequestProcessor outerInstance, RemotingContext ctx, RpcRequestCommand msg)
            {
                this.outerInstance = outerInstance;
                this.ctx = ctx;
                this.msg = msg;
            }

            /// <seealso cref= Runnable#run() </seealso>
            public void run()
            {
                try
                {
                    outerInstance.doProcess(ctx, msg);
                }
                catch (System.Exception e)
                {
                    //protect the thread running this task
                    string remotingAddress = ctx.ChannelContext.Channel.RemoteAddress.ToString();
                    logger.LogError("Exception caught when process rpc request command in RpcRequestProcessor, Id=" + msg.Id + "! Invoke source address is [" + remotingAddress + "].", e);
                }
            }
        }
    }
}