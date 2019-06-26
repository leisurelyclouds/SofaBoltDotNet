using DotNetty.Common.Utilities;
using System;

namespace Remoting
{
    /// <summary>
    /// A singleton holder of the timer for timeout.
    /// </summary>
    public class TimerHolder
    {

        private const long defaultTickDuration = 10;

        private class DefaultInstance
        {
            internal static readonly ITimer INSTANCE = new HashedWheelTimer(TimeSpan.FromMilliseconds(defaultTickDuration), 512, -1);
        }

        private TimerHolder()
        {
        }

        /// <summary>
        /// Get a singleton instance of <seealso cref="Timer"/>. <br>
        /// The tick duration is <seealso cref="#defaultTickDuration"/>.
        /// </summary>
        /// <returns> Timer </returns>
        public static ITimer Timer
        {
            get
            {
                return DefaultInstance.INSTANCE;
            }
        }
    }
}