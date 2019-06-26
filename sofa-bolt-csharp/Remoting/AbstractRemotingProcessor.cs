using java.lang;
using java.util.concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;

namespace Remoting
{
    /// <summary>
    /// Processor to process remoting command.
    /// </summary>
    public abstract class AbstractRemotingProcessor : RemotingProcessor
    {
        private static readonly ILogger logger = NullLogger.Instance;
        private CommandFactory commandFactory;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AbstractRemotingProcessor()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public AbstractRemotingProcessor(CommandFactory commandFactory)
        {
            this.commandFactory = commandFactory;
        }

        /// <summary>
        /// Constructor.
		/// </summary>
        /// <param name="executor"> ExecutorService </param>
        public AbstractRemotingProcessor(ExecutorService executor)
        {
            this.Executor = executor;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="commandFactory"> CommandFactory </param>
        /// <param name="executor"> ExecutorService </param>
        public AbstractRemotingProcessor(CommandFactory commandFactory, ExecutorService executor)
        {
            this.commandFactory = commandFactory;
            this.Executor = executor;
        }

        /// <summary>
        /// Do the process.
        /// </summary>
        /// <param name="ctx"> RemotingContext </param>
        /// <param name="msg"> T </param>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public abstract void doProcess(RemotingContext ctx, T msg) throws Exception;
        public abstract void doProcess(RemotingContext ctx, RemotingCommand msg);

        /// <summary>
        /// Process the remoting command with its own executor or with the defaultExecutor if its own if null.
        /// </summary>
        /// <param name="ctx"> RemotingContext </param>
        /// <param name="msg"> T </param>
        /// <param name="defaultExecutor"> ExecutorService, default executor </param>
        public virtual void process(RemotingContext ctx, RemotingCommand msg, ExecutorService defaultExecutor)
        {
            ProcessTask task = new ProcessTask(this, ctx, msg);
            if (Executor != null)
            {
                Executor.execute(task);
            }
            else
            {
                defaultExecutor.execute(task);
            }
        }

        /// <summary>
        /// Getter method for property <tt>executor</tt>.
        /// </summary>
        /// <returns> property value of executor </returns>
        public ExecutorService Executor { get; set; }


        public virtual CommandFactory CommandFactory
        {
            get
            {
                return commandFactory;
            }
            set
            {
                commandFactory = value;
            }
        }


        /// <summary>
        /// Task for asynchronous process.
        /// </summary>
        class ProcessTask : Runnable
        {
            private readonly AbstractRemotingProcessor outerInstance;


            internal RemotingContext ctx;
            internal RemotingCommand msg;

            public ProcessTask(AbstractRemotingProcessor outerInstance, RemotingContext ctx, RemotingCommand msg)
            {
                this.outerInstance = outerInstance;
                this.ctx = ctx;
                this.msg = msg;
            }

            public void run()
            {
                try
                {
                    outerInstance.doProcess(ctx, msg);
                }
                catch (System.Exception e)
                {
                    //protect the thread running this task
                    string remotingAddress = ((IPEndPoint)ctx.ChannelContext.Channel?.RemoteAddress)?.ToString();
                    logger.LogError("Exception caught when process rpc request command in AbstractRemotingProcessor, Id=" + msg.Id + "! Invoke source address is [" + remotingAddress + "].", e);
                }
            }
        }
    }
}