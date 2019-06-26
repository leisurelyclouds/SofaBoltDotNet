using DotNetty.Transport.Channels;
using System.Net;

namespace Remoting
{
    /// <summary>
    /// default biz context
    /// </summary>
    public class DefaultBizContext : BizContext
    {
        /// <summary>
        /// remoting context
        /// </summary>
        private RemotingContext remotingCtx;

        /// <summary>
        /// Constructor with RemotingContext
        /// </summary>
        /// <param name="remotingCtx"> </param>
        public DefaultBizContext(RemotingContext remotingCtx)
        {
            this.remotingCtx = remotingCtx;
        }

        /// <summary>
        /// get remoting context
        /// </summary>
        /// <returns> RemotingContext </returns>
        protected internal virtual RemotingContext RemotingCtx
        {
            get
            {
                return remotingCtx;
            }
        }

        /// <seealso cref= BizContext#getRemoteAddress() </seealso>
        public virtual string RemoteAddress
        {
            get
            {
                if (null != remotingCtx)
                {
                    IChannelHandlerContext channelCtx = remotingCtx.ChannelContext;
                    IChannel channel = channelCtx.Channel;
                    if (null != channel)
                    {
                        return ((IPEndPoint)channel?.RemoteAddress)?.ToString();
                    }
                }
                return "UNKNOWN_ADDRESS";
            }
        }

        /// <seealso cref= BizContext#getRemoteHost() </seealso>
        public virtual string RemoteHost
        {
            get
            {
                if (null != remotingCtx)
                {
                    IChannelHandlerContext channelCtx = remotingCtx.ChannelContext;
                    IChannel channel = channelCtx.Channel;
                    if (null != channel)
                    {
                        return ((IPEndPoint)channel?.RemoteAddress).Address?.ToString();
                    }
                }
                return "UNKNOWN_HOST";
            }
        }

        /// <seealso cref= BizContext#getRemotePort() </seealso>
        public virtual int RemotePort
        {
            get
            {
                if (null != remotingCtx)
                {
                    IChannelHandlerContext channelCtx = remotingCtx.ChannelContext;
                    IChannel channel = channelCtx.Channel;
                    if (null != channel)
                    {
                        return ((IPEndPoint)channel.RemoteAddress).Port;
                    }
                }
                return -1;
            }
        }

        /// <seealso cref= BizContext#getConnection() </seealso>
        public virtual Connection Connection
        {
            get
            {
                if (null != remotingCtx)
                {
                    return remotingCtx.Connection;
                }
                return null;
            }
        }

        /// <seealso cref= BizContext#isRequestTimeout() </seealso>
        public virtual bool RequestTimeout
        {
            get
            {
                return remotingCtx.RequestTimeout;
            }
        }

        /// <summary>
        /// get the timeout value from rpc client.
        /// 
        /// @return
        /// </summary>
        public virtual int ClientTimeout
        {
            get
            {
                return remotingCtx.Timeout;
            }
        }

        /// <summary>
        /// get the arrive time stamp
        /// 
        /// @return
        /// </summary>
        public virtual long ArriveTimestamp
        {
            get
            {
                return remotingCtx.ArriveTimestamp;
            }
        }

        /// <seealso cref= BizContext#put(java.lang.String, java.lang.String) </seealso>
        public virtual void put(string key, string value)
        {
        }

        /// <seealso cref= BizContext#get(java.lang.String) </seealso>
        public virtual string get(string key)
        {
            return null;
        }

        /// <seealso cref= BizContext#getInvokeContext() </seealso>
        public virtual InvokeContext InvokeContext
        {
            get
            {
                return remotingCtx.InvokeContext;
            }
        }
    }

}