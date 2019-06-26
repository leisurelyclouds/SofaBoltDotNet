namespace Remoting.rpc
{
	/// <summary>
	/// Constants for rpc.
	/// </summary>
	public class RpcConfigs
	{
		/// <summary>
		/// Protocol key in url.
		/// </summary>
		public const string URL_PROTOCOL = "_PROTOCOL";

		/// <summary>
		/// Version key in url.
		/// </summary>
		public const string URL_VERSION = "_VERSION";

		/// <summary>
		/// Connection timeout key in url.
		/// </summary>
		public const string CONNECT_TIMEOUT_KEY = "_CONNECTTIMEOUT";

		/// <summary>
		/// Connection number key of each address
		/// </summary>
		public const string CONNECTION_NUM_KEY = "_CONNECTIONNUM";

		/// <summary>
		/// whether need to warm up connections
		/// </summary>
		public const string CONNECTION_WARMUP_KEY = "_CONNECTIONWARMUP";

		/// <summary>
		/// Whether to dispatch message list in default executor.
		/// </summary>
		public const string DISPATCH_MSG_LIST_IN_DEFAULT_EXECUTOR = "bolt.rpc.dispatch-msg-list-in-default-executor";
		public const string DISPATCH_MSG_LIST_IN_DEFAULT_EXECUTOR_DEFAULT = "true";
	}
}