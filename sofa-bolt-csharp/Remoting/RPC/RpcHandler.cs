using DotNetty.Transport.Channels;
using Remoting.rpc.protocol;
using System;
using System.Collections.Concurrent;

namespace Remoting.rpc
{
    /// <summary>
    /// Dispatch messages to corresponding protocol.
    /// </summary>
    public class RpcHandler : ChannelHandlerAdapter
    {
        public override bool IsSharable => true;
        private bool serverSide;

        private ConcurrentDictionary<Type, UserProcessor> userProcessors;

        public RpcHandler()
        {
            serverSide = false;
        }

        public RpcHandler(ConcurrentDictionary<Type, UserProcessor> userProcessors)
        {
            serverSide = false;
            this.userProcessors = userProcessors;
        }

        public RpcHandler(bool serverSide, ConcurrentDictionary<Type, UserProcessor> userProcessors)
        {
            this.serverSide = serverSide;
            this.userProcessors = userProcessors;
        }

        public override void ChannelRead(IChannelHandlerContext ctx, object msg)
        {
            ProtocolCode protocolCode = ctx.Channel.GetAttribute(Connection.PROTOCOL).Get();
            Protocol protocol = ProtocolManager.getProtocol(protocolCode);
            protocol.CommandHandler.handleCommand(new RemotingContext(ctx, new InvokeContext(), serverSide, userProcessors), msg);
        }
    }
}