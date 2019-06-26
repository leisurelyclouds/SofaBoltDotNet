using Remoting.Config.configs;
using Remoting.Config.switches;

namespace Remoting.Config
{
	/// <summary>
	/// define an interface which can be used to implement configurable apis.
	/// </summary>
	public interface ConfigurableInstance : NettyConfigure
	{
		/// <summary>
		/// get the config container for current instance
		/// </summary>
		/// <returns> the config container </returns>
		ConfigContainer conf();

		/// <summary>
		/// get the global switch for current instance
		/// </summary>
		/// <returns> the global switch </returns>
		GlobalSwitch switches();
	}
}