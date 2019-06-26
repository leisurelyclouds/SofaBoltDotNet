using System;
using System.Collections.Generic;

namespace Remoting.rpc.protocol
{

    /// <summary>
    /// Implements common function and provide default value.
    /// more details in <seealso cref="AbstractUserProcessor"/>
    /// </summary>
    public abstract class AbstractMultiInterestUserProcessor : AbstractUserProcessor, MultiInterestUserProcessor
    {
        public abstract List<Type> multiInterest();

        /// <summary>
        /// do not need to implement this method because of the multiple interests
		/// </summary>
        /// <seealso cref= UserProcessor#interest()
        ///  </seealso>
        public override Type interest()
        {
            return null;
        }
    }
}