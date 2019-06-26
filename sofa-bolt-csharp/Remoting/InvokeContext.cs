using System.Collections.Concurrent;

namespace Remoting
{
    /// <summary>
    /// Invoke context
    /// </summary>
    public class InvokeContext
    {
        // ~~~ invoke context keys of client side
        public const string CLIENT_LOCAL_IP = "bolt.client.local.ip";
        public const string CLIENT_LOCAL_PORT = "bolt.client.local.port";
        public const string CLIENT_REMOTE_IP = "bolt.client.remote.ip";
        public const string CLIENT_REMOTE_PORT = "bolt.client.remote.port";
        /// <summary>
        /// time consumed during connection creating, this is a timespan
		/// </summary>
        public const string CLIENT_CONN_CREATETIME = "bolt.client.conn.createtime";

        // ~~~ invoke context keys of server side
        public const string SERVER_LOCAL_IP = "bolt.server.local.ip";
        public const string SERVER_LOCAL_PORT = "bolt.server.local.port";
        public const string SERVER_REMOTE_IP = "bolt.server.remote.ip";
        public const string SERVER_REMOTE_PORT = "bolt.server.remote.port";

        // ~~~ invoke context keys of bolt client and server side
        public const string BOLT_INVOKE_REQUEST_ID = "bolt.invoke.request.id";
        /// <summary>
        /// time consumed start from the time when request arrive, to the time when request be processed, this is a timespan
		/// </summary>
        public const string BOLT_PROCESS_WAIT_TIME = "bolt.invoke.wait.time";
        public const string BOLT_CUSTOM_SERIALIZER = "bolt.invoke.custom.serializer";
        public const string BOLT_CRC_SWITCH = "bolt.invoke.crc.switch";

        // ~~~ constants
        public const int INITIAL_SIZE = 8;

        /// <summary>
        /// context
		/// </summary>
        private ConcurrentDictionary<string, object> context;

        /// <summary>
        /// default construct
        /// </summary>
        public InvokeContext()
        {
            context = new ConcurrentDictionary<string, object>();
        }

        /// <summary>
        /// put if absent
        /// </summary>
        /// <param name="key"> </param>
        /// <param name="value"> </param>
        public virtual void putIfAbsent(string key, object value)
        {
            if (!context.ContainsKey(key))
            {
                context.AddOrUpdate(key, value, (k, oldValue) => value);
            }
        }

        /// <summary>
        /// put
        /// </summary>
        /// <param name="key"> </param>
        /// <param name="value"> </param>
        public virtual void put(string key, object value)
        {
            context.AddOrUpdate(key, value, (k, oldValue) => value);
        }

        /// <summary>
        /// get
        /// </summary>
        /// <param name="key">
        /// @return </param>
        public virtual object get(string key)
        {
            context.TryGetValue(key, out var result);
            return result;
        }

        /// <summary>
        /// get and use default if not found
        /// </summary>
        /// <param name="key"> </param>
        /// <param name="defaultIfNotFound"> </param>
        /// @param <T>
        /// @return </param>
        public virtual object get(string key, object defaultIfNotFound)
        {
            context.TryGetValue(key, out var result);
            return result ?? defaultIfNotFound;
        }

        /// <summary>
        /// clear all mappings.
        /// </summary>
        public virtual void clear()
        {
            context.Clear();
        }
    }
}