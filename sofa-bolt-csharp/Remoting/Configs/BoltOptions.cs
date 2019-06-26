using System.Collections.Concurrent;

namespace Remoting.Config
{
    /// <summary>
    /// Option carrier.
    /// </summary>
    public class BoltOptions
    {
        private ConcurrentDictionary<BoltOption, object> options;

        public BoltOptions()
        {
            options = new ConcurrentDictionary<BoltOption, object>();
        }

        /// <summary>
        /// Get the optioned value.
        /// Return default value if option does not exist.
        /// </summary>
        /// <param name="option"> target option </param>
        /// <returns> the optioned value of default value if option does not exist. </returns>
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @SuppressWarnings("unchecked") public <T> T option(BoltOption<T> option)
        public virtual object option(BoltOption option)
        {
            options.TryGetValue(option, out var obj);
            return obj ?? option.defaultValue();
        }

        /// <summary>
        /// Set up an new option with specific value.
        /// Use a value of {@code null} to remove a previous set <seealso cref="BoltOption"/>.
        /// </summary>
        /// <param name="option"> target option </param>
        /// <param name="value"> option value, null for remove a previous set <seealso cref="BoltOption"/>. </param>
        /// <returns> this BoltOptions instance </returns>
        public virtual BoltOptions option(BoltOption option, object value)
        {
            if (value == null)
            {
                options.TryRemove(option, out _);
                return this;
            }
            options.AddOrUpdate(option, value, (key, oldValue) => value);
            return this;
        }
    }

}