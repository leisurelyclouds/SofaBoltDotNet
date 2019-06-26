using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;

namespace Remoting.Config.configs
{
    /// <summary>
    /// default implementation for config container
    /// </summary>
    public class DefaultConfigContainer : ConfigContainer
    {
        /// <summary>
        /// logger
		/// </summary>
        private static readonly ILogger logger = NullLogger.Instance;

        /// <summary>
        /// use a hash map to store the user configs with different config types and config items.
        /// </summary>
        private IDictionary<ConfigType, IDictionary<ConfigItem, object>> userConfigs = new Dictionary<ConfigType, IDictionary<ConfigItem, object>>();

        public virtual bool contains(ConfigType configType, ConfigItem configItem)
        {
            return userConfigs.ContainsKey(configType) && null != userConfigs[configType] && userConfigs[configType].ContainsKey(configItem);
        }

        public virtual object get(ConfigType configType, ConfigItem configItem)
        {
            if (userConfigs.ContainsKey(configType))
            {
                if (userConfigs[configType].ContainsKey(configItem))
                {
                    return userConfigs[configType][configItem];
                }
            }
            return null;
        }

        public virtual void set(ConfigType configType, ConfigItem configItem, object value)
        {
            validate(value);

            userConfigs.TryGetValue(configType, out var items);
            if (null == items)
            {
                items = new Dictionary<ConfigItem, object>();
                userConfigs[configType] = items;
            }
            object prev = items[configItem] = value;
            if (null != prev)
            {
                logger.LogWarning("the value of ConfigType {}, ConfigItem {} changed from {} to {}", configType, configItem, prev.ToString(), value.ToString());
            }
        }


        private void validate(object value)
        {
            if (null == value)
            {
                throw new ArgumentException($"value { value?.ToString()} should not be null!");
            }
        }
    }
}