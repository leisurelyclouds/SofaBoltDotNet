using Remoting.Config;

namespace Remoting.rpc
{
	/// <summary>
	/// RPC framework config manager.
	/// @author dennis
	/// </summary>
	public class RpcConfigManager
	{
		public static bool dispatch_msg_list_in_default_executor()
		{
			return ConfigManager.getBool(RpcConfigs.DISPATCH_MSG_LIST_IN_DEFAULT_EXECUTOR, RpcConfigs.DISPATCH_MSG_LIST_IN_DEFAULT_EXECUTOR_DEFAULT);
		}
	}

}