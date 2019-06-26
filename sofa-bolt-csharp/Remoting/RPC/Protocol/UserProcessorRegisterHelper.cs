using System.Collections.Concurrent;
using System;
using Remoting.exception;

namespace Remoting.rpc.protocol
{
    public class UserProcessorRegisterHelper
    {
        /// <summary>
        /// Help register single-interest user processor.
        /// </summary>
        /// <param name="processor">  the processor need to be registered </param>
        /// <param name="userProcessors">   the map of user processors </param>
        public static void registerUserProcessor(UserProcessor processor, ConcurrentDictionary<Type, UserProcessor> userProcessors)
        {
            if (null == processor)
            {
                throw new RuntimeException("User processor should not be null!");
            }
            if (processor is MultiInterestUserProcessor)
            {
                registerUserProcessor((MultiInterestUserProcessor)processor, userProcessors);
            }
            else
            {
                if (processor.interest() == null)
                {
                    throw new RuntimeException("Processor interest should not be blank!");
                }

                if (userProcessors.ContainsKey(processor.interest()))
                {
                    string errMsg = "Processor with interest key [" + processor.interest() + "] has already been registered to rpc server, can not register again!";
                    throw new RuntimeException(errMsg);
                }
                else
                {
                    userProcessors.TryAdd(processor.interest(), processor);
                }
            }
        }

        /// <summary>
        /// Help register multi-interest user processor.
        /// </summary>
        /// <param name="processor">  the processor with multi-interest need to be registered </param>
        /// <param name="userProcessors">    the map of user processors </param>
        private static void registerUserProcessor(MultiInterestUserProcessor processor, ConcurrentDictionary<Type, UserProcessor> userProcessors)
        {
            if (null == processor.multiInterest() || processor.multiInterest().Count==0)
            {
                throw new RuntimeException("Processor interest should not be blank!");
            }
            foreach (var interest in processor.multiInterest())
            {
                if (userProcessors.ContainsKey(interest))
                {
                    string errMsg = "Processor with interest key [" + interest + "] has already been registered to rpc server, can not register again!";
                    throw new RuntimeException(errMsg);
                }
                else
                {
                    userProcessors.TryAdd(interest, processor);
                }
            }
        }
    }
}