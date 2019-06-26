using System;

namespace Remoting.Config
{
    /// <summary>
    /// Define the key for a certain config item using system property,
    ///   and provide the default value for that config item.
    /// </summary>
    public class Configs
	{
		// ~~~ configs and default values for bootstrap

		/// <summary>
		/// TCP_NODELAY option
		/// </summary>
		public const string TCP_NODELAY = "bolt.tcp.nodelay";
		public const string TCP_NODELAY_DEFAULT = "true";

		/// <summary>
		/// TCP SO_REUSEADDR option
		/// </summary>
		public const string TCP_SO_REUSEADDR = "bolt.tcp.so.reuseaddr";
		public const string TCP_SO_REUSEADDR_DEFAULT = "true";

		/// <summary>
		/// TCP SO_BACKLOG option
		/// </summary>
		public const string TCP_SO_BACKLOG = "bolt.tcp.so.backlog";
		public const string TCP_SO_BACKLOG_DEFAULT = "1024";

		/// <summary>
		/// TCP SO_KEEPALIVE option
		/// </summary>
		public const string TCP_SO_KEEPALIVE = "bolt.tcp.so.keepalive";
		public const string TCP_SO_KEEPALIVE_DEFAULT = "true";

		/// <summary>
		/// Netty ioRatio option
		/// </summary>
		public const string NETTY_IO_RATIO = "bolt.netty.io.ratio";
		public const string NETTY_IO_RATIO_DEFAULT = "70";

		/// <summary>
		/// Netty buffer allocator, enabled as default
		/// </summary>
		public const string NETTY_BUFFER_POOLED = "bolt.netty.buffer.pooled";
		public const string NETTY_BUFFER_POOLED_DEFAULT = "true";

		/// <summary>
		/// Netty buffer high watermark
		/// </summary>
		public const string NETTY_BUFFER_HIGH_WATERMARK = "bolt.netty.buffer.high.watermark";
		public static readonly string NETTY_BUFFER_HIGH_WATERMARK_DEFAULT = Convert.ToString(64 * 1024);

		/// <summary>
		/// Netty buffer low watermark
		/// </summary>
		public const string NETTY_BUFFER_LOW_WATERMARK = "bolt.netty.buffer.low.watermark";
		public static readonly string NETTY_BUFFER_LOW_WATERMARK_DEFAULT = Convert.ToString(32 * 1024);

		/// <summary>
		/// Netty epoll switch
		/// </summary>
		public const string NETTY_EPOLL_SWITCH = "bolt.netty.epoll.switch";
		public const string NETTY_EPOLL_SWITCH_DEFAULT = "true";

		/// <summary>
		/// Netty epoll level trigger enabled
		/// </summary>
		public const string NETTY_EPOLL_LT = "bolt.netty.epoll.lt";
		public const string NETTY_EPOLL_LT_DEFAULT = "true";

		// ~~~ configs and default values for idle

		/// <summary>
		/// TCP idle switch
		/// </summary>
		public const string TCP_IDLE_SWITCH = "bolt.tcp.heartbeat.switch";
		public const string TCP_IDLE_SWITCH_DEFAULT = "true";

		/// <summary>
		/// TCP idle interval for client
		/// </summary>
		public const string TCP_IDLE = "bolt.tcp.heartbeat.interval";
		public const string TCP_IDLE_DEFAULT = "15000";

		/// <summary>
		/// TCP idle triggered max times if no response
		/// </summary>
		public const string TCP_IDLE_MAXTIMES = "bolt.tcp.heartbeat.maxtimes";
		public const string TCP_IDLE_MAXTIMES_DEFAULT = "3";

		/// <summary>
		/// TCP idle interval for server
		/// </summary>
		public const string TCP_SERVER_IDLE = "bolt.tcp.server.idle.interval";
		public const string TCP_SERVER_IDLE_DEFAULT = "90000";

		// ~~~ configs and default values for connection manager

		/// <summary>
		/// Thread pool min size for the connection manager executor
		/// </summary>
		public const string CONN_CREATE_TP_MIN_SIZE = "bolt.conn.create.tp.min";
		public const string CONN_CREATE_TP_MIN_SIZE_DEFAULT = "3";

		/// <summary>
		/// Thread pool max size for the connection manager executor
		/// </summary>
		public const string CONN_CREATE_TP_MAX_SIZE = "bolt.conn.create.tp.max";
		public const string CONN_CREATE_TP_MAX_SIZE_DEFAULT = "8";

		/// <summary>
		/// Thread pool queue size for the connection manager executor
		/// </summary>
		public const string CONN_CREATE_TP_QUEUE_SIZE = "bolt.conn.create.tp.queue";
		public const string CONN_CREATE_TP_QUEUE_SIZE_DEFAULT = "50";

		/// <summary>
		/// Thread pool keep alive time for the connection manager executor
		/// </summary>
		public const string CONN_CREATE_TP_KEEPALIVE_TIME = "bolt.conn.create.tp.keepalive";
		public const string CONN_CREATE_TP_KEEPALIVE_TIME_DEFAULT = "60";

		/// <summary>
		/// Default connect timeout value, time unit: ms
		/// </summary>
		public const int DEFAULT_CONNECT_TIMEOUT = 1000;

		/// <summary>
		/// default connection number per url
		/// </summary>
		public const int DEFAULT_CONN_NUM_PER_URL = 1;

		/// <summary>
		/// max connection number of each url
		/// </summary>
		public const int MAX_CONN_NUM_PER_URL = 100 * 10000;

		// ~~~ configs for processor manager

		/// <summary>
		/// Thread pool min size for the default executor.
		/// </summary>
		public const string TP_MIN_SIZE = "bolt.tp.min";
		public const string TP_MIN_SIZE_DEFAULT = "20";

		/// <summary>
		/// Thread pool max size for the default executor.
		/// </summary>
		public const string TP_MAX_SIZE = "bolt.tp.max";
		public const string TP_MAX_SIZE_DEFAULT = "400";

		/// <summary>
		/// Thread pool queue size for the default executor.
		/// </summary>
		public const string TP_QUEUE_SIZE = "bolt.tp.queue";
		public const string TP_QUEUE_SIZE_DEFAULT = "600";

		/// <summary>
		/// Thread pool keep alive time for the default executor
		/// </summary>
		public const string TP_KEEPALIVE_TIME = "bolt.tp.keepalive";
		public const string TP_KEEPALIVE_TIME_DEFAULT = "60";

		// ~~~ configs and default values for reconnect manager

		/// <summary>
		/// Reconnection switch
		/// </summary>
		public const string CONN_RECONNECT_SWITCH = "bolt.conn.reconnect.switch";
		public const string CONN_RECONNECT_SWITCH_DEFAULT = "false";

		// ~~~ configs and default values for connection monitor

		/// <summary>
		/// Connection monitor switch
		/// <para>
		///   If switch on connection monitor, it may cause <seealso cref="rpc.RpcClient#oneway"/> to fail.
		///   Please try to use other types of RPC methods
		/// </para>
		/// </summary>
		public const string CONN_MONITOR_SWITCH = "bolt.conn.monitor.switch";
		public const string CONN_MONITOR_SWITCH_DEFAULT = "false";

		/// <summary>
		/// Initial delay to execute schedule task for connection monitor
		/// </summary>
		public const string CONN_MONITOR_INITIAL_DELAY = "bolt.conn.monitor.initial.delay";
		public const string CONN_MONITOR_INITIAL_DELAY_DEFAULT = "10000";

		/// <summary>
		/// Period of schedule task for connection monitor
		/// </summary>
		public const string CONN_MONITOR_PERIOD = "bolt.conn.monitor.period";
		public const string CONN_MONITOR_PERIOD_DEFAULT = "180000";

		/// <summary>
		/// Connection threshold
		/// </summary>
		public const string CONN_THRESHOLD = "bolt.conn.threshold";
		public const string CONN_THRESHOLD_DEFAULT = "3";

		/// <summary>
		/// Retry detect period for ScheduledDisconnectStrategy
		/// </summary>
		[Obsolete]
		public const string RETRY_DETECT_PERIOD = "bolt.retry.delete.period";
		public const string RETRY_DETECT_PERIOD_DEFAULT = "5000";

		/// <summary>
		/// Connection status
		/// </summary>
		public const string CONN_SERVICE_STATUS = "bolt.conn.service.status";
		public const string CONN_SERVICE_STATUS_OFF = "off";
		public const string CONN_SERVICE_STATUS_ON = "on";

		// ~~~ configs and default values for serializer

		/// <summary>
		/// Codec type
		/// </summary>
		public const string SERIALIZER = "bolt.serializer";
        public static readonly string SERIALIZER_DEFAULT = 1.ToString();

        /// <summary>
        /// Charset
		/// </summary>
        public const string DEFAULT_CHARSET = "UTF-8";
	}
}