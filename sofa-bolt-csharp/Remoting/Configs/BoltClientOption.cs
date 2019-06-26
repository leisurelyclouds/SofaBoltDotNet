namespace Remoting.Config
{
	/// <summary>
	/// Supported options in client side.
	/// </summary>
	public class BoltClientOption : BoltGenericOption
	{
		public new static readonly BoltOption NETTY_IO_RATIO = valueOf("bolt.tcp.heartbeat.interval", 15 * 1000);
		public static readonly BoltOption TCP_IDLE_MAXTIMES = valueOf("bolt.tcp.heartbeat.maxtimes", 3);
		public static readonly BoltOption CONN_CREATE_TP_MIN_SIZE = valueOf("bolt.conn.create.tp.min", 3);
		public static readonly BoltOption CONN_CREATE_TP_MAX_SIZE = valueOf("bolt.conn.create.tp.max", 8);
		public static readonly BoltOption CONN_CREATE_TP_QUEUE_SIZE = valueOf("bolt.conn.create.tp.queue", 50);
		public static readonly BoltOption CONN_CREATE_TP_KEEPALIVE_TIME = valueOf("bolt.conn.create.tp.keepalive", 60);
        private BoltClientOption(string str, object obj) : base(str, obj)
        {
        }
    }
}