using System.Collections.Generic;
using System.Collections.Concurrent;
using DotNetty.Common.Utilities;
using Remoting.rpc.protocol;
using java.util.concurrent.atomic;
using DotNetty.Transport.Channels;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;

namespace Remoting
{
    /// <summary>
    /// An abstraction of socket channel.
    /// </summary>
    public class Connection
    {
        private static readonly ILogger logger = NullLogger.Instance;

        private IChannel channel;

        private readonly ConcurrentDictionary<int, InvokeFuture> invokeFutureMap = new ConcurrentDictionary<int, InvokeFuture>();

        /// <summary>
        /// Attribute key for connection
		/// </summary>
        public static readonly AttributeKey<Connection> CONNECTION = AttributeKey<Connection>.ValueOf("connection");
        /// <summary>
        /// Attribute key for heartbeat count
		/// </summary>
        public static readonly AttributeKey<object> HEARTBEAT_COUNT = AttributeKey<object>.ValueOf("heartbeatCount");

        /// <summary>
        /// Attribute key for heartbeat switch for each connection
		/// </summary>
        public static readonly AttributeKey<object> HEARTBEAT_SWITCH = AttributeKey<object>.ValueOf("heartbeatSwitch");

        /// <summary>
        /// Attribute key for protocol
		/// </summary>
        public static readonly AttributeKey<ProtocolCode> PROTOCOL = AttributeKey<ProtocolCode>.ValueOf("protocol");
        private ProtocolCode protocolCode;

        /// <summary>
        /// Attribute key for version
		/// </summary>
        public static readonly AttributeKey<object> VERSION = AttributeKey<object>.ValueOf("version");
        private byte version = RpcProtocolV2.PROTOCOL_VERSION_1;

        private Url url;

        private readonly ConcurrentDictionary<int?, string> id2PoolKey = new ConcurrentDictionary<int?, string>(); // poolKey -  id

        private ISet<string> poolKeys = new HashSet<string>();

        private AtomicBoolean closed = new AtomicBoolean(false);

        private readonly ConcurrentDictionary<string, object> attributes = new ConcurrentDictionary<string, object>(); //attr value -  attr key

        /// <summary>
        /// the reference count used for this connection. If equals 2, it means this connection has been referenced 2 times
		/// </summary>
        private readonly AtomicInteger referenceCount = new AtomicInteger();

        /// <summary>
        /// no reference of the current connection
		/// </summary>
        private const int NO_REFERENCE = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="channel"> associated channel </param>
        public Connection(IChannel channel)
        {
            this.channel = channel;
            this.channel.GetAttribute(CONNECTION).Set(this);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="channel"> associated channel </param>
        /// <param name="url"> associated url </param>
        public Connection(IChannel channel, Url url) : this(channel)
        {
            this.url = url;
            poolKeys.Add(url.UniqueKey);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="channel"> associated channel </param>
        /// <param name="protocolCode"> ProtocolCode </param>
        /// <param name="url"> associated url </param>
        public Connection(IChannel channel, ProtocolCode protocolCode, Url url) : this(channel, url)
        {
            this.protocolCode = protocolCode;
            init();
        }

        /// 
        /// <param name="channel"> associated channel </param>
        /// <param name="protocolCode"> ProtocolCode </param>
        /// <param name="version"> protocol version </param>
        /// <param name="url"> associated url </param>
        public Connection(IChannel channel, ProtocolCode protocolCode, byte version, Url url) : this(channel, url)
        {
            this.protocolCode = protocolCode;
            this.version = version;
            init();
        }

        /// <summary>
        /// Initialization.
        /// </summary>
        private void init()
        {
            channel.GetAttribute(HEARTBEAT_COUNT).Set(0);
            channel.GetAttribute(PROTOCOL).Set(protocolCode);
            channel.GetAttribute(VERSION).Set(version);
            channel.GetAttribute(HEARTBEAT_SWITCH).Set(true);
        }

        /// <summary>
        /// to check whether the connection is fine to use
        /// </summary>
        /// <returns> true if connection is fine </returns>
        public virtual bool Fine
        {
            get
            {
                return channel != null && channel.Active;
            }
        }

        /// <summary>
        /// increase the reference count
        /// </summary>
        public virtual void increaseRef()
        {
            referenceCount.getAndIncrement();
        }

        /// <summary>
        /// decrease the reference count
        /// </summary>
        public virtual void decreaseRef()
        {
            referenceCount.getAndDecrement();
        }

        /// <summary>
        /// to check whether the reference count is 0
        /// </summary>
        /// <returns> true if the reference count is 0 </returns>
        public virtual bool noRef()
        {
            return referenceCount.get() == NO_REFERENCE;
        }

        /// <summary>
        /// Get the address of the remote peer.
        /// </summary>
        /// <returns> remote address </returns>
        public virtual IPEndPoint RemoteAddress
        {
            get
            {
                return (IPEndPoint)channel.RemoteAddress;
            }
        }

        /// <summary>
        /// Get the remote IP.
        /// </summary>
        /// <returns> remote IP </returns>
        public virtual string RemoteIP
        {
            get
            {
                return ((IPEndPoint)channel.RemoteAddress).Address.ToString();
            }
        }

        /// <summary>
        /// Get the remote port.
        /// </summary>
        /// <returns> remote port </returns>
        public virtual int RemotePort
        {
            get
            {
                return ((IPEndPoint)channel.RemoteAddress).Port;
            }
        }

        /// <summary>
        /// Get the address of the local peer.
        /// </summary>
        /// <returns> local address </returns>
        public virtual IPEndPoint LocalAddress
        {
            get
            {
                return (IPEndPoint)channel.LocalAddress;
            }
        }

        /// <summary>
        /// Get the local IP.
        /// </summary>
        /// <returns> local IP </returns>
        public virtual string LocalIP
        {
            get
            {
                return ((IPEndPoint)channel.LocalAddress).Address.ToString();
            }
        }

        /// <summary>
        /// Get the local port.
        /// </summary>
        /// <returns> local port </returns>
        public virtual int LocalPort
        {
            get
            {
                return ((IPEndPoint)channel.LocalAddress).Port;
            }
        }

        /// <summary>
        /// Get the netty channel of the connection.
        /// </summary>
        /// <returns> IChannel </returns>
        public virtual IChannel Channel
        {
            get
            {
                return channel;
            }
        }

        /// <summary>
        /// Get the InvokeFuture with invokeId of id.
        /// </summary>
        /// <param name="id"> invoke id </param>
        /// <returns> InvokeFuture </returns>
        public virtual InvokeFuture getInvokeFuture(int id)
        {
            invokeFutureMap.TryGetValue(id, out var result);
            return result;
        }

        /// <summary>
        /// Add an InvokeFuture
        /// </summary>
        /// <param name="future"> InvokeFuture </param>
        /// <returns> previous InvokeFuture with same invoke id </returns>
        public virtual InvokeFuture addInvokeFuture(InvokeFuture future)
        {
            if (!invokeFutureMap.ContainsKey(future.invokeId()))
            {
                invokeFutureMap.AddOrUpdate(future.invokeId(), future, (key, value) => future);
                return null;
            }
            else
            {
                invokeFutureMap.TryGetValue(future.invokeId(), out var result);
                return result;
            }
        }

        /// <summary>
        /// Remove InvokeFuture who's invokeId is id
        /// </summary>
        /// <param name="id"> invoke id </param>
        /// <returns> associated InvokerFuture with the target id </returns>
        public virtual InvokeFuture removeInvokeFuture(int id)
        {
            invokeFutureMap.TryRemove(id, out var result);
            return result;
        }

        /// <summary>
        /// Do something when closing.
        /// </summary>
        public virtual void onClose()
        {
            foreach (var invokeFutureMapKeyValue in invokeFutureMap)
            {
                invokeFutureMap.TryRemove(invokeFutureMapKeyValue.Key, out _);
                var value = invokeFutureMapKeyValue.Value;
                if (value != null)
                {
                    value.putResponse(value.createConnectionClosedResponse(RemoteAddress));
                    value.cancelTimeout();
                    value.tryAsyncExecuteInvokeCallbackAbnormally();
                }
            }
        }

        /// <summary>
        /// Close the connection.
        /// </summary>
        public virtual void close()
        {
            if (closed.compareAndSet(false, true))
            {
                try
                {
                    if (Channel != null)
                    {
                        var task = Channel.CloseAsync();
                        task.ContinueWith((t) =>
                        {
                            if (logger.IsEnabled(LogLevel.Information))
                            {
                                logger.LogInformation($"Close the connection to remote address={((IPEndPoint)Channel?.RemoteAddress)?.ToString()}, result={t.IsCompletedSuccessfully}, cause={t.Exception}");
                            }
                        });
                    }
                }
                catch (Exception e)
                {
                    logger.LogWarning("Exception caught when closing connection {}", ((IPEndPoint)Channel?.RemoteAddress)?.ToString(), e);
                }
            }
        }

        /// <summary>
        /// Whether invokeFutures is completed
        /// 
        /// </summary>
        public virtual bool InvokeFutureMapFinish
        {
            get
            {
                return invokeFutureMap.IsEmpty;
            }
        }

        /// <summary>
        /// add a pool key to list
        /// </summary>
        /// <param name="poolKey"> connection pool key </param>
        public virtual void addPoolKey(string poolKey)
        {
            poolKeys.Add(poolKey);
        }

        /// <summary>
        /// get all pool keys
        /// </summary>
        public virtual ISet<string> PoolKeys
        {
            get
            {
                return new HashSet<string>(poolKeys);
            }
        }

        /// <summary>
        /// remove pool key
        /// </summary>
        /// <param name="poolKey"> connection pool key </param>
        public virtual void removePoolKey(string poolKey)
        {
            poolKeys.Remove(poolKey);
        }

        /// <summary>
        /// Getter method for property <tt>url</tt>.
        /// </summary>
        /// <returns> property value of url </returns>
        public virtual Url Url
        {
            get
            {
                return url;
            }
        }

        /// <summary>
        /// add Id to group Mapping
        /// </summary>
        /// <param name="id"> invoke id </param>
        /// <param name="poolKey"> connection pool key </param>
        public virtual void addIdPoolKeyMapping(int? id, string poolKey)
        {
            id2PoolKey[id] = poolKey;
        }

        /// <summary>
        /// remove id to group Mapping
        /// </summary>
        /// <param name="id"> connection pool key </param>
        /// <returns> connection pool key </returns>
        public virtual string removeIdPoolKeyMapping(int? id)
        {
            id2PoolKey.Remove(id, out var result);
            return result;
        }

        /// <summary>
        /// Set attribute key=value.
        /// </summary>
        /// <param name="key"> attribute key </param>
        /// <param name="value"> attribute value </param>
        public virtual void setAttribute(string key, object value)
        {
            attributes[key] = value;
        }

        /// <summary>
        /// set attribute if key absent.
        /// </summary>
        /// <param name="key"> attribute key </param>
        /// <param name="value"> attribute value </param>
        /// <returns> previous value </returns>
        public virtual object setAttributeIfAbsent(string key, object value)
        {
            if (!attributes.ContainsKey(key))
            {
                attributes.TryAdd(key, value);
                return null;
            }
            else
            {
                attributes.TryGetValue(key, out var result);
                return result;
            }
        }

        /// <summary>
        /// Remove attribute.
        /// </summary>
        /// <param name="key"> attribute key </param>
        public virtual void removeAttribute(string key)
        {
            attributes.Remove(key, out var _);
        }

        /// <summary>
        /// Get attribute.
        /// </summary>
        /// <param name="key"> attribute key </param>
        /// <returns> attribute value </returns>
        public virtual object getAttribute(string key)
        {
            if (attributes.ContainsKey(key))
            {
                return attributes[key];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Clear attribute.
        /// </summary>
        public virtual void clearAttributes()
        {
            attributes.Clear();
        }

        /// <summary>
        /// Getter method for property <tt>invokeFutureMap</tt>.
        /// </summary>
        /// <returns> property value of invokeFutureMap </returns>
        public virtual ConcurrentDictionary<int, InvokeFuture> InvokeFutureMap
        {
            get
            {
                return invokeFutureMap;
            }
        }
    }

}