namespace Remoting.Config.configs
{
	/// <summary>
	/// Items of config.
	/// 
	/// Mainly used to define some config items managed by <seealso cref="ConfigContainer"/>.
	/// You can define new config items based on this if need.
	/// </summary>
	public enum ConfigItem
	{
		// ~~~ netty related
		NETTY_BUFFER_LOW_WATER_MARK, // netty writer buffer low water mark
		NETTY_BUFFER_HIGH_WATER_MARK // netty writer buffer high water mark
	}
}