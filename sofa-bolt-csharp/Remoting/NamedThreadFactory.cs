using java.lang;
using java.util.concurrent;
using java.util.concurrent.atomic;

namespace Remoting
{
    /// <summary>
    /// Thread factory to name the thread purposely
    /// </summary>
    public class NamedThreadFactory : ThreadFactory
    {

        private static readonly AtomicInteger poolNumber = new AtomicInteger(1);
        private readonly AtomicInteger threadNumber = new AtomicInteger(1);
        private readonly ThreadGroup group;
        private readonly string namePrefix;
        private readonly bool isDaemon;

        public NamedThreadFactory() : this("ThreadPool")
        {
        }

        public NamedThreadFactory(string name) : this(name, false)
        {
        }

        public NamedThreadFactory(string preffix, bool daemon)
        {
            SecurityManager s = java.lang.System.getSecurityManager();
            group = (s != null) ? s.getThreadGroup() : Thread.currentThread().getThreadGroup();
            namePrefix = preffix + "-" + poolNumber.getAndIncrement() + "-thread-";
            isDaemon = daemon;
        }

        /// <summary>
        /// Create a thread.
        /// </summary>
        /// <seealso cref= ThreadFactory#newThread(java.lang.Runnable) </seealso>
        public virtual java.lang.Thread newThread(Runnable r)
        {
            java.lang.Thread thread = new java.lang.Thread(group, r, namePrefix + threadNumber.getAndIncrement(), 0);
            thread.setDaemon(isDaemon);
            if (thread.getPriority() != Thread.NORM_PRIORITY)
            {
                thread.setPriority(Thread.NORM_PRIORITY);
            }
            return thread;
        }
    }
}