namespace Remoting.Config
{
    /// <summary>
    /// Supported options both in client and server side.
    /// </summary>
    public class BoltGenericOption : BoltOption
    {
        /*------------ NETTY Config Start ------------*/
        public static readonly BoltOption TCP_NODELAY = valueOf("bolt.tcp.nodelay", true);
        public static readonly BoltOption TCP_SO_REUSEADDR = valueOf("bolt.tcp.so.reuseaddr", true);
        public static readonly BoltOption TCP_SO_KEEPALIVE = valueOf("bolt.tcp.so.keepalive", true);

        public static readonly BoltOption NETTY_IO_RATIO = valueOf("bolt.netty.io.ratio", 70);
        public static readonly BoltOption NETTY_BUFFER_POOLED = valueOf("bolt.netty.buffer.pooled", true);

        public static readonly BoltOption NETTY_BUFFER_HIGH_WATER_MARK = valueOf("bolt.netty.buffer.high.watermark", 64 * 1024);
        public static readonly BoltOption NETTY_BUFFER_LOW_WATER_MARK = valueOf("bolt.netty.buffer.low.watermark", 32 * 1024);

        public static readonly BoltOption NETTY_EPOLL_SWITCH = valueOf("bolt.netty.epoll.switch", true);

        public static readonly BoltOption TCP_IDLE_SWITCH = valueOf("bolt.tcp.heartbeat.switch", true);
        /*------------ NETTY Config End ------------*/

        /*------------ Thread Pool Config Start ------------*/
        public static readonly BoltOption TP_MIN_SIZE = valueOf("bolt.tp.min", 20);
        public static readonly BoltOption TP_MAX_SIZE = valueOf("bolt.tp.max", 400);
        public static readonly BoltOption TP_QUEUE_SIZE = valueOf("bolt.tp.queue", 600);
        public static readonly BoltOption TP_KEEPALIVE_TIME = valueOf("bolt.tp.keepalive", 60);

        /*------------ Thread Pool Config End ------------*/

        public static readonly BoltOption CONNECTION_SELECT_STRATEGY = valueOf("CONNECTION_SELECT_STRATEGY");

        protected internal BoltGenericOption(string name, object defaultValue) : base(name, defaultValue)
        {
        }
    }
}