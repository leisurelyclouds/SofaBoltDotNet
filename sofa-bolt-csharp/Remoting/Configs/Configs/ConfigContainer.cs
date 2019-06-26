namespace Remoting.Config.configs
{
	/// <summary>
	/// the interface of a config container
	/// 
	/// Mainly used to manage config by user api, this is instance related, not globally.
	/// That is to say, different remoting instance client or server hold different ConfigContainer.
	/// </summary>
	public interface ConfigContainer
	{
		/// <summary>
		/// check whether a config item of a certain config type exist.
		/// </summary>
		/// <param name="configType"> config types in the config container, different config type can hold the same config item key </param>
		/// <param name="configItem"> config items in the config container </param>
		/// <returns> exist then return true, not exist return false </returns>
		bool contains(ConfigType configType, ConfigItem configItem);

		/// <summary>
		/// try to get config value using config type and config item.
		/// </summary>
		/// <param name="configType"> config types in the config container, different config type can hold the same config item key </param>
		/// <param name="configItem"> config items in the config container </param>
		/// @param <T> the generics of return value </param>
		/// <returns> the right value and cast to type T, if no mappings, then return null </returns>
		object get(ConfigType configType, ConfigItem configItem);

		/// <summary>
		/// init a config item with certain config type, and the value can be any type.
		/// Notice: the value can be overwrite if you set more than once.
		/// </summary>
		/// <param name="configType"> config types in the config container, different config type can hold the same config item key </param>
		/// <param name="configItem"> config items in the config container </param>
		void set(ConfigType configType, ConfigItem configItem, object value);
	}
}