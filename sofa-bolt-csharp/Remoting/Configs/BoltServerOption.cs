namespace Remoting.Config
{
	/// <summary>
	/// Supported options in server side.
	/// </summary>
	public class BoltServerOption : BoltGenericOption
	{

		public static readonly BoltOption TCP_SO_BACKLOG = valueOf("bolt.tcp.so.backlog", 1024);

		public static readonly BoltOption NETTY_EPOLL_LT = valueOf("bolt.netty.epoll.lt", true);

		public static readonly BoltOption TCP_SERVER_IDLE = valueOf("bolt.tcp.server.idle.interval", 90 * 1000);

		private BoltServerOption(string name, object defaultValue) : base(name, defaultValue)
		{
		}
	}
}