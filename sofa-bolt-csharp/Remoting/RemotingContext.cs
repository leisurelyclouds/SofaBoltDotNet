using Remoting.rpc.protocol;
using DotNetty.Transport.Channels;
using Remoting.util;
using System.Threading.Tasks;
using System;
using System.Collections.Concurrent;

namespace Remoting
{
    /// <summary>
    /// Wrap the IChannelHandlerContext.
    /// </summary>
    public class RemotingContext
    {

        private IChannelHandlerContext channelContext;

        private bool serverSide = false;

        /// <summary>
        /// whether need handle request timeout, if true, request will be discarded. The default value is true
		/// </summary>
        private bool timeoutDiscard = true;

        /// <summary>
        /// request arrive time stamp
		/// </summary>
        private long arriveTimestamp;

        /// <summary>
        /// request timeout setting by invoke side
		/// </summary>
        private int timeout;

        /// <summary>
        /// rpc command type
		/// </summary>
        private int rpcCommandType;

        //JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
        //ORIGINAL LINE: private java.util.concurrent.ConcurrentHashMap<String, rpc.protocol.UserProcessor<?>> userProcessors;
        private ConcurrentDictionary<Type, UserProcessor> userProcessors;

        private InvokeContext invokeContext;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ctx"> </param>
        public RemotingContext(IChannelHandlerContext ctx)
        {
            channelContext = ctx;
        }

        /// <summary>
        /// Constructor.
		/// </summary>
        /// <param name="ctx"> </param>
        /// <param name="serverSide"> </param>
        public RemotingContext(IChannelHandlerContext ctx, bool serverSide)
        {
            channelContext = ctx;
            this.serverSide = serverSide;
        }

        /// <summary>
        /// Constructor.
		/// </summary>
        /// <param name="ctx"> </param>
        /// <param name="serverSide"> </param>
        /// <param name="userProcessors"> </param>
        public RemotingContext(IChannelHandlerContext ctx, bool serverSide, ConcurrentDictionary<Type, UserProcessor> userProcessors)
        {
            channelContext = ctx;
            this.serverSide = serverSide;
            this.userProcessors = userProcessors;
        }

        /// <summary>
        /// Constructor.
		/// </summary>
        /// <param name="ctx"> </param>
        /// <param name="invokeContext"> </param>
        /// <param name="serverSide"> </param>
        /// <param name="userProcessors"> </param>
        public RemotingContext(IChannelHandlerContext ctx, InvokeContext invokeContext, bool serverSide, ConcurrentDictionary<Type, UserProcessor> userProcessors)
        {
            channelContext = ctx;
            this.serverSide = serverSide;
            this.userProcessors = userProcessors;
            this.invokeContext = invokeContext;
        }

        /// <summary>
        /// Wrap the writeAndFlush method.
        /// </summary>
        /// <param name="msg">
        /// @return </param>
        public virtual Task writeAndFlush(RemotingCommand msg)
        {
            return channelContext.WriteAndFlushAsync(msg);
        }

        /// <summary>
        /// whether this request already timeout
        /// 
        /// @return
        /// </summary>
        public virtual bool RequestTimeout
        {
            get
            {
                if (timeout > 0 && (rpcCommandType != rpc.RpcCommandType.REQUEST_ONEWAY) && (new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() - arriveTimestamp) > timeout)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// The server side
        /// 
        /// @return
        /// </summary>
        public virtual bool ServerSide
        {
            get
            {
                return serverSide;
            }
        }

        /// <summary>
        /// Get user processor for class name.
        /// </summary>
        /// <param name="className">
        /// @return </param>
        //JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
        //ORIGINAL LINE: public rpc.protocol.UserProcessor<?> getUserProcessor(String className)
        public virtual UserProcessor getUserProcessor(Type className)
        {
            if (className == null)
            {
                return null;
            }
            else
            {
                userProcessors.TryGetValue(className, out var result);
                return result;
            }
        }

        /// <summary>
        /// Get connection from channel
        /// 
        /// @return
        /// </summary>
        public virtual Connection Connection
        {
            get
            {
                return ConnectionUtil.getConnectionFromChannel(channelContext.Channel);
            }
        }

        /// <summary>
        /// Get the channel handler context.
        /// 
        /// @return
        /// </summary>
        public virtual IChannelHandlerContext ChannelContext
        {
            get
            {
                return channelContext;
            }
            set
            {
                channelContext = value;
            }
        }


        /// <summary>
        /// Getter method for property <tt>invokeContext</tt>.
        /// </summary>
        /// <returns> property value of invokeContext </returns>
        public virtual InvokeContext InvokeContext
        {
            get
            {
                return invokeContext;
            }
        }

        /// <summary>
        /// Setter method for property <tt>arriveTimestamp<tt>.
        /// </summary>
        /// <param name="arriveTimestamp"> value to be assigned to property arriveTimestamp </param>
        public virtual long ArriveTimestamp
        {
            set
            {
                arriveTimestamp = value;
            }
            get
            {
                return arriveTimestamp;
            }
        }


        /// <summary>
        /// Setter method for property <tt>timeout<tt>.
        /// </summary>
        /// <param name="timeout"> value to be assigned to property timeout </param>
        public virtual int Timeout
        {
            set
            {
                timeout = value;
            }
            get
            {
                return timeout;
            }
        }


        /// <summary>
        /// Setter method for property <tt>rpcCommandType<tt>.
        /// </summary>
        /// <param name="rpcCommandType"> value to be assigned to property rpcCommandType </param>
        public virtual int RpcCommandType
        {
            set
            {
                rpcCommandType = value;
            }
        }

        public virtual bool TimeoutDiscard
        {
            get
            {
                return timeoutDiscard;
            }
        }

        public virtual RemotingContext setTimeoutDiscard(bool failFastEnabled)
        {
            timeoutDiscard = failFastEnabled;
            return this;
        }
    }

}