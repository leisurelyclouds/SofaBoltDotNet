using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using System;

namespace Remoting.util
{
    /// <summary>
    /// Utils for netty EventLoop
    /// </summary>
    public class NettyEventLoopUtil
    {
        public static bool IsUseLibuv { get; set; }
        /// <summary>
        /// Create the right event loop according to current platform and system property, fallback to NIO when epoll not enabled.
        /// </summary>
        /// <param name="nThreads"> </param>
        /// <param name="threadFactory"> </param>
        /// <returns> an EventLoopGroup suitable for the current platform </returns>
        public static IEventLoopGroup newEventLoopGroup(int nThreads, Func<IEventLoopGroup, IEventLoop> threadFactory)
        {
            IEventLoopGroup eventLoopGroup = IsUseLibuv ? (IEventLoopGroup)new DispatcherEventLoopGroup() : new MultithreadEventLoopGroup(threadFactory, nThreads);
            return eventLoopGroup;
        }

        /// <returns> a SocketChannel class suitable for the given EventLoopGroup implementation </returns>
        public static Type ClientSocketChannelClass
        {
            get
            {
                return IsUseLibuv ? typeof(TcpSocketChannel) : typeof(TcpSocketChannel);
            }
        }

        /// <summary>
        /// Use <seealso cref="EpollMode#LEVEL_TRIGGERED"/> for server bootstrap if level trigger enabled by system properties,
        ///   otherwise use <seealso cref="EpollMode#EDGE_TRIGGERED"/>. </summary>
        /// <param name="serverBootstrap"> server bootstrap </param>
        public static void enableTriggeredMode(ServerBootstrap serverBootstrap)
        {
            if (IsUseLibuv)
            {
                //if (ConfigManager.netty_epoll_lt_enabled())
                //{
                //    serverBootstrap.ChildOption(ChannelOption.EPOLL_MODE, EpollMode.LEVEL_TRIGGERED);
                //}
                //else
                //{
                //    serverBootstrap.ChildOption(ChannelOption.EPOLL_MODE, EpollMode.EDGE_TRIGGERED);
                //}
            }
        }
    }

}