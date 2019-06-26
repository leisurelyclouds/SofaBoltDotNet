using System.Threading;
using java.util.concurrent;
using Microsoft.Extensions.Logging;
using Remoting.exception;

namespace Remoting.util
{
    /// <summary>
    /// Utils for future task
    /// </summary>
    public class FutureTaskUtil
    {
        /// <summary>
        /// get the result of a future task
        /// 
        /// Notice: the run method of this task should have been called at first.
        /// </summary>
        public static object getFutureTaskResult(RunStateRecordedFutureTask task, ILogger logger)
        {
            object t = default;
            if (null != task)
            {
                try
                {
                    t = task.AfterRun;
                }
                catch (ThreadInterruptedException e)
                {
                    logger.LogError("Future task interrupted!", e);
                }
                catch (ExecutionException e)
                {
                    logger.LogError("Future task execute failed!", e);
                }
                catch (FutureTaskNotRunYetException e)
                {
                    logger.LogError("Future task has not run yet!", e);
                }
                catch (FutureTaskNotCompleted e)
                {
                    logger.LogError("Future task has not completed!", e);
                }
            }
            return t;
        }

        /// <summary>
        /// launder the throwable
        /// </summary>
        /// <param name="t"> </param>
        public static void launderThrowable(System.Exception t)
        {
            if (t is RuntimeException)
            {
                throw t;
            }
            else if (t is java.lang.RuntimeException)
            {
                throw t;
            }
            else
            {
                throw new System.InvalidOperationException("Not unchecked!", t);
            }
        }
    }
}