using java.util.concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting.Config;
using System.Collections.Concurrent;

namespace Remoting
{
    /// <summary>
    /// Manager of processors<br>
    /// Maintains the relationship between command and command processor through command code.
    /// </summary>
    public class ProcessorManager
    {
        private static readonly ILogger logger = NullLogger.Instance;
        private ConcurrentDictionary<CommandCode, RemotingProcessor> cmd2processors = new ConcurrentDictionary<CommandCode, RemotingProcessor>();

        private RemotingProcessor defaultProcessor;

        /// <summary>
        /// The default executor, if no executor is set for processor, this one will be used
        /// </summary>
        private ExecutorService defaultExecutor;

        private int minPoolSize = ConfigManager.default_tp_min_size();

        private int maxPoolSize = ConfigManager.default_tp_max_size();

        private int queueSize = ConfigManager.default_tp_queue_size();

        private long keepAliveTime = ConfigManager.default_tp_keepalive_time();

        public ProcessorManager()
        {
            defaultExecutor = new ThreadPoolExecutor(minPoolSize, maxPoolSize, keepAliveTime, TimeUnit.SECONDS, new ArrayBlockingQueue(queueSize), new NamedThreadFactory("Bolt-default-executor", true));
        }

        /// <summary>
        /// Register processor to process command that has the command code of cmdCode.
        /// </summary>
        /// <param name="cmdCode"> </param>
        /// <param name="processor"> </param>
        public virtual void registerProcessor(CommandCode cmdCode, RemotingProcessor processor)
        {
            if (cmd2processors.ContainsKey(cmdCode))
            {
                //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
                cmd2processors.TryGetValue(cmdCode, out var remotingProcessor);
                logger.LogWarning("Processor for cmd={} is already registered, the processor is {}, and changed to {}", cmdCode, ikvm.extensions.ExtensionMethods.getClass(remotingProcessor).getName(), ikvm.extensions.ExtensionMethods.getClass(processor).getName());
            }
            cmd2processors.AddOrUpdate(cmdCode, processor, (commandCode, remotingProcessor) => processor);
        }

        /// <summary>
        /// Register the default processor to process command with no specific processor registered.
        /// </summary>
        /// <param name="processor"> </param>
        public virtual void registerDefaultProcessor(RemotingProcessor processor)
        {
            if (defaultProcessor == null)
            {
                defaultProcessor = processor;
            }
            else
            {
                throw new System.InvalidOperationException("The defaultProcessor has already been registered: " + defaultProcessor.GetType());
            }
        }

        /// <summary>
        /// Get the specific processor with command code of cmdCode if registered, otherwise the default processor is returned.
        /// </summary>
        /// <param name="cmdCode">
        /// @return </param>
        //JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
        //ORIGINAL LINE: public RemotingProcessor<?> getProcessor(CommandCode cmdCode)
        public virtual RemotingProcessor getProcessor(CommandCode cmdCode)
        {
            //JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
            //ORIGINAL LINE: RemotingProcessor<?> processor = this.cmd2processors.get(cmdCode);
            var isGetValue = cmd2processors.TryGetValue(cmdCode, out var processor);
            if (isGetValue)
            {
                return processor;
            }
            return defaultProcessor;
        }

        /// <summary>
        /// Getter method for property <tt>defaultExecutor</tt>.
        /// </summary>
        /// <returns> property value of defaultExecutor </returns>
        public virtual ExecutorService DefaultExecutor
        {
            get
            {
                return defaultExecutor;
            }
        }

        /// <summary>
        /// Set the default executor.
        /// </summary>
        /// <param name="executor"> </param>
        public virtual void registerDefaultExecutor(ExecutorService executor)
        {
            defaultExecutor = executor;
        }
    }
}