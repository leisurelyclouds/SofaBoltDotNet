using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Net;

namespace Remoting.rpc.protocol
{
    /// <summary>
    /// Command decoder for Rpc.
    /// </summary>
    public class RpcCommandDecoder : CommandDecoder
    {
        private bool InstanceFieldsInitialized = false;

        public RpcCommandDecoder()
        {
            if (!InstanceFieldsInitialized)
            {
                InitializeInstanceFields();
                InstanceFieldsInitialized = true;
            }
        }

        private void InitializeInstanceFields()
        {
            lessLen = RpcProtocol.ResponseHeaderLength < RpcProtocol.RequestHeaderLength ? RpcProtocol.ResponseHeaderLength : RpcProtocol.RequestHeaderLength;
        }


        private static readonly ILogger logger = NullLogger.Instance;

        private int lessLen;

        /// <seealso cref= CommandDecoder#decode(io.netty.channel.IChannelHandlerContext, io.netty.buffer.IByteBuffer, java.util.List<object>) </seealso>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void decode(io.netty.channel.IChannelHandlerContext ctx, io.netty.buffer.IByteBuffer in, java.util.List<object><Object> out) throws Exception
        public virtual void decode(IChannelHandlerContext ctx, IByteBuffer @in, List<object> @out)
        {
            // the less length between response header and request header
            if (@in.ReadableBytes >= lessLen)
            {
                @in.MarkReaderIndex();
                byte protocol = @in.ReadByte();
                @in.ResetReaderIndex();
                if (protocol == RpcProtocol.PROTOCOL_CODE)
                {
                    /*
					 * ver: version for protocol
					 * type: request/response/request oneway
					 * cmdcode: code for remoting command
					 * ver2:version for remoting command
					 * requestId: id of request
					 * codec: code for codec
					 * (req)timeout: request timeout
					 * (resp)respStatus: response status
					 * classLen: length of request or response class name
					 * headerLen: length of header
					 * contentLen: length of content
					 * className
					 * header
					 * content
					 */
                    if (@in.ReadableBytes > 2)
                    {
                        @in.MarkReaderIndex();
                        @in.ReadByte(); //version
                        byte type = @in.ReadByte(); //type
                        if (type == RpcCommandType.REQUEST || type == RpcCommandType.REQUEST_ONEWAY)
                        {
                            //decode request
                            if (@in.ReadableBytes >= RpcProtocol.RequestHeaderLength - 2)
                            {
                                short cmdCode = @in.ReadShort();
                                byte ver2 = @in.ReadByte();
                                int requestId = @in.ReadInt();
                                byte serializer = @in.ReadByte();
                                int timeout = @in.ReadInt();
                                short classLen = @in.ReadShort();
                                short headerLen = @in.ReadShort();
                                int contentLen = @in.ReadInt();
                                byte[] clazz = null;
                                byte[] header = null;
                                byte[] content = null;
                                if (@in.ReadableBytes >= classLen + headerLen + contentLen)
                                {
                                    if (classLen > 0)
                                    {
                                        clazz = new byte[classLen];
                                        @in.ReadBytes(clazz);
                                    }
                                    if (headerLen > 0)
                                    {
                                        header = new byte[headerLen];
                                        @in.ReadBytes(header);
                                    }
                                    if (contentLen > 0)
                                    {
                                        content = new byte[contentLen];
                                        @in.ReadBytes(content);
                                    }
                                }
                                else
                                { // not enough data
                                    @in.ResetReaderIndex();
                                    return;
                                }
                                RequestCommand command;
                                if (cmdCode == (short)CommonCommandCode.__Enum.HEARTBEAT)
                                {
                                    command = new HeartbeatCommand();
                                }
                                else
                                {
                                    command = createRequestCommand(cmdCode);
                                }
                                command.Type = type;
                                command.Version = ver2;
                                command.Id = requestId;
                                command.Serializer = serializer;
                                command.Timeout = timeout;
                                command.Clazz = clazz;
                                command.Header = header;
                                command.Content = content;
                                @out.Add(command);

                            }
                            else
                            {
                                @in.ResetReaderIndex();
                            }
                        }
                        else if (type == RpcCommandType.RESPONSE)
                        {
                            //decode response
                            if (@in.ReadableBytes >= RpcProtocol.ResponseHeaderLength - 2)
                            {
                                short cmdCode = @in.ReadShort();
                                byte ver2 = @in.ReadByte();
                                int requestId = @in.ReadInt();
                                byte serializer = @in.ReadByte();
                                short status = @in.ReadShort();
                                short classLen = @in.ReadShort();
                                short headerLen = @in.ReadShort();
                                int contentLen = @in.ReadInt();
                                byte[] clazz = null;
                                byte[] header = null;
                                byte[] content = null;
                                if (@in.ReadableBytes >= classLen + headerLen + contentLen)
                                {
                                    if (classLen > 0)
                                    {
                                        clazz = new byte[classLen];
                                        @in.ReadBytes(clazz);
                                    }
                                    if (headerLen > 0)
                                    {
                                        header = new byte[headerLen];
                                        @in.ReadBytes(header);
                                    }
                                    if (contentLen > 0)
                                    {
                                        content = new byte[contentLen];
                                        @in.ReadBytes(content);
                                    }
                                }
                                else
                                { // not enough data
                                    @in.ResetReaderIndex();
                                    return;
                                }
                                ResponseCommand command;
                                if (cmdCode == (short)CommonCommandCode.__Enum.HEARTBEAT)
                                {

                                    command = new HeartbeatAckCommand();
                                }
                                else
                                {
                                    command = createResponseCommand(cmdCode);
                                }
                                command.Type = type;
                                command.Version = ver2;
                                command.Id = requestId;
                                command.Serializer = serializer;
                                command.ResponseStatus = (ResponseStatus)status;
                                command.Clazz = clazz;
                                command.Header = header;
                                command.Content = content;
                                command.ResponseTimeMillis = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                                command.ResponseHost = (IPEndPoint)ctx.Channel.RemoteAddress;
                                @out.Add(command);
                            }
                            else
                            {
                                @in.ResetReaderIndex();
                            }
                        }
                        else
                        {
                            string emsg = "Unknown command type: " + type;
                            logger.LogError(emsg);
                            throw new Exception(emsg);
                        }
                    }

                }
                else
                {
                    string emsg = "Unknown protocol: " + protocol;
                    logger.LogError(emsg);
                    throw new Exception(emsg);
                }

            }
        }

        private ResponseCommand createResponseCommand(short cmdCode)
        {
            ResponseCommand command = new RpcResponseCommand();
            command.CmdCode = RpcCommandCode.valueOf(cmdCode);
            return command;
        }

        private RpcRequestCommand createRequestCommand(short cmdCode)
        {
            RpcRequestCommand command = new RpcRequestCommand();
            command.CmdCode = RpcCommandCode.valueOf(cmdCode);
            command.ArriveTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            return command;
        }

    }

}