namespace Remoting.Config.configs
{

	/// <summary>
	/// type of config
	/// </summary>
	public enum ConfigType
	{
		CLIENT_SIDE, // configs of this type can only be used in client side
		SERVER_SIDE // configs of this type can only be used in server side
	}
}