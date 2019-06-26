using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;

namespace Remoting.rpc.protocol
{
    /// <summary>
    /// Encode remoting command into IByteBuffer.
    /// </summary>
    public class RpcCommandEncoder : CommandEncoder
    {
        /// <summary>
        /// logger
		/// </summary>
        private static readonly ILogger logger = NullLogger.Instance;

        /// <seealso cref= CommandEncoder#encode(io.netty.channel.IChannelHandlerContext, java.io.Serializable, io.netty.buffer.IByteBuffer) </seealso>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void encode(io.netty.channel.IChannelHandlerContext ctx, java.io.Serializable msg, io.netty.buffer.IByteBuffer out) throws Exception
        public virtual void encode(IChannelHandlerContext ctx, object msg, IByteBuffer @out)
        {
            try
            {
                if (msg is RpcCommand)
                {
                    /*
					 * ver: version for protocol
					 * type: request/response/request oneway
					 * cmdcode: code for remoting command
					 * ver2:version for remoting command
					 * requestId: id of request
					 * codec: code for codec
					 * (req)timeout: request timeout.
					 * (resp)respStatus: response status
					 * classLen: length of request or response class name
					 * headerLen: length of header
					 * cotentLen: length of content
					 * className
					 * header
					 * content
					 */
                    RpcCommand cmd = (RpcCommand)msg;
                    @out.WriteByte(RpcProtocol.PROTOCOL_CODE);
                    @out.WriteByte(cmd.Type);
                    @out.WriteShort(((RpcCommand)msg).CmdCode.value());
                    @out.WriteByte(cmd.Version);
                    @out.WriteInt(cmd.Id);
                    @out.WriteByte(cmd.Serializer);
                    if (cmd is RequestCommand)
                    {
                        //timeout
                        @out.WriteInt(((RequestCommand)cmd).Timeout);
                    }
                    if (cmd is ResponseCommand)
                    {
                        //response status
                        ResponseCommand response = (ResponseCommand)cmd;
                        @out.WriteShort((short)response.ResponseStatus);
                    }
                    @out.WriteShort(cmd.ClazzLength);
                    @out.WriteShort(cmd.HeaderLength);
                    @out.WriteInt(cmd.ContentLength);
                    if (cmd.ClazzLength > 0)
                    {
                        @out.WriteBytes(cmd.Clazz);
                    }
                    if (cmd.HeaderLength > 0)
                    {
                        @out.WriteBytes(cmd.Header);
                    }
                    if (cmd.ContentLength > 0)
                    {
                        @out.WriteBytes(cmd.Content);
                    }
                }
                else
                {
                    string warnMsg = "msg type [" + msg.GetType() + "] is not subclass of RpcCommand";
                    logger.LogWarning(warnMsg);
                }
            }
            catch (Exception e)
            {
                logger.LogError("Exception caught!", e);
                throw;
            }
        }
    }

}