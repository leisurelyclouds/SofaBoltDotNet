using System;
using System.Collections.Generic;

namespace Remoting.rpc.protocol
{
    /// <summary>
    /// Support multi-interests feature based on UserProcessor
    /// 
    /// The implementations of this interface don't need to implement the <seealso cref="UserProcessor#interest() interest()"/> method;
    /// </summary>
    public interface MultiInterestUserProcessor : UserProcessor
	{

        /// <summary>
        /// A list of the class names of user request.
        /// Use String type to avoid classloader problem.
        /// </summary>
        List<Type> multiInterest();
	}
}