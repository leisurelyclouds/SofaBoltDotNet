using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting.Config.switches;
using Remoting.util;
using System;

namespace Remoting.rpc.protocol
{
    /// <summary>
    /// Encode remoting command into IByteBuffer v2.
    /// </summary>
    public class RpcCommandEncoderV2 : CommandEncoder
    {
        /// <summary>
        /// logger
		/// </summary>
        private static readonly ILogger logger = NullLogger.Instance;

        /// <seealso cref= CommandEncoder#encode(IChannelHandlerContext, Serializable, IByteBuffer) </seealso>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void encode(io.netty.channel.IChannelHandlerContext ctx, java.io.Serializable msg, io.netty.buffer.IByteBuffer out) throws Exception
        public virtual void encode(IChannelHandlerContext ctx, object msg, IByteBuffer @out)
        {
            try
            {
                if (msg is RpcCommand)
                {
                    /*
					 * proto: magic code for protocol
					 * ver: version for protocol
					 * type: request/response/request oneway
					 * cmdcode: code for remoting command
					 * ver2:version for remoting command
					 * requestId: id of request
					 * codec: code for codec
					 * switch: function switch
					 * (req)timeout: request timeout.
					 * (resp)respStatus: response status
					 * classLen: length of request or response class name
					 * headerLen: length of header
					 * cotentLen: length of content
					 * className
					 * header
					 * content
					 * crc (optional)
					 */
                    int index = @out.WriterIndex;
                    RpcCommand cmd = (RpcCommand)msg;
                    @out.WriteByte(RpcProtocolV2.PROTOCOL_CODE);
                    var version = ctx.Channel.GetAttribute(Connection.VERSION);
                    byte ver = RpcProtocolV2.PROTOCOL_VERSION_1;
                    if (version != null && version.Get() != null)
                    {
                        ver = (byte)version.Get();
                    }
                    @out.WriteByte(ver);
                    @out.WriteByte(cmd.Type);
                    @out.WriteShort(((RpcCommand)msg).CmdCode.value());
                    @out.WriteByte(cmd.Version);
                    @out.WriteInt(cmd.Id);
                    @out.WriteByte(cmd.Serializer);
                    @out.WriteByte(cmd.ProtocolSwitch.toByte());
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
                    if (ver == RpcProtocolV2.PROTOCOL_VERSION_2 && cmd.ProtocolSwitch.isOn(ProtocolSwitch.CRC_SWITCH_INDEX))
                    {
                        // compute the crc32 and write to out
                        byte[] frame = new byte[@out.ReadableBytes];
                        @out.GetBytes(index, frame);
                        @out.WriteInt(CrcUtil.crc32(frame));
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