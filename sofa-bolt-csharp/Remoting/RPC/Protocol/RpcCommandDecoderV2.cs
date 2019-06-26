using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting.Config.switches;
using Remoting.util;
using System;
using System.Collections.Generic;
using System.Net;

namespace Remoting.rpc.protocol
{
    /// <summary>
    /// Command decoder for Rpc v2.
    /// 
    /// @author jiangping
    /// @version $Id: RpcCommandDecoderV2.java, v 0.1 2017-05-27 PM5:15:26 tao Exp $
    /// </summary>
    public class RpcCommandDecoderV2 : CommandDecoder
	{
		private bool InstanceFieldsInitialized = false;

		public RpcCommandDecoderV2()
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			lessLen = RpcProtocolV2.ResponseHeaderLength < RpcProtocolV2.RequestHeaderLength ? RpcProtocolV2.ResponseHeaderLength : RpcProtocolV2.RequestHeaderLength;
		}


		private static readonly ILogger logger = NullLogger.Instance;

		private int lessLen;

		/// <seealso cref= CommandDecoder#decode(IChannelHandlerContext, IByteBuffer, List<object>) </seealso>
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
				if (protocol == RpcProtocolV2.PROTOCOL_CODE)
				{
					/*
					 * ver: version for protocol
					 * type: request/response/request oneway
					 * cmdcode: code for remoting command
					 * ver2:version for remoting command
					 * requestId: id of request
					 * codec: code for codec
					 * switch: function switch
					 * (req)timeout: request timeout
					 * (resp)respStatus: response status
					 * classLen: length of request or response class name
					 * headerLen: length of header
					 * contentLen: length of content
					 * className
					 * header
					 * content
					 */
					if (@in.ReadableBytes > 2 + 1)
					{
						int startIndex = @in.ReaderIndex;
						@in.MarkReaderIndex();
						@in.ReadByte(); //protocol code
						byte version = @in.ReadByte(); //protocol version
						byte type = @in.ReadByte(); //type
						if (type == RpcCommandType.REQUEST || type == RpcCommandType.REQUEST_ONEWAY)
						{
							//decode request
							if (@in.ReadableBytes >= RpcProtocolV2.RequestHeaderLength - 3)
							{
								short cmdCode = @in.ReadShort();
								byte ver2 = @in.ReadByte();
								int requestId = @in.ReadInt();
								byte serializer = @in.ReadByte();
								byte protocolSwitchValue = @in.ReadByte();
								int timeout = @in.ReadInt();
								short classLen = @in.ReadShort();
								short headerLen = @in.ReadShort();
								int contentLen = @in.ReadInt();
								byte[] clazz = null;
								byte[] header = null;
								byte[] content = null;

								// decide the at-least bytes length for each version
								int lengthAtLeastForV1 = classLen + headerLen + contentLen;
								bool crcSwitchOn = ProtocolSwitch.isOn(ProtocolSwitch.CRC_SWITCH_INDEX, protocolSwitchValue);
								int lengthAtLeastForV2 = classLen + headerLen + contentLen;
								if (crcSwitchOn)
								{
									lengthAtLeastForV2 += 4; // crc int
								}

								// continue read
								if ((version == RpcProtocolV2.PROTOCOL_VERSION_1 && @in.ReadableBytes >= lengthAtLeastForV1) || (version == RpcProtocolV2.PROTOCOL_VERSION_2 && @in.ReadableBytes >= lengthAtLeastForV2))
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
									if (version == RpcProtocolV2.PROTOCOL_VERSION_2 && crcSwitchOn)
									{
										checkCRC(@in, startIndex);
									}
								}
								else
								{ // not enough data
									@in.ResetReaderIndex();
									return;
								}
								RequestCommand command;
								if (cmdCode == CommandCode_Fields.HEARTBEAT_VALUE)
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
								command.ProtocolSwitch = ProtocolSwitch.create(protocolSwitchValue);
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
							if (@in.ReadableBytes >= RpcProtocolV2.ResponseHeaderLength - 3)
							{
								short cmdCode = @in.ReadShort();
								byte ver2 = @in.ReadByte();
								int requestId = @in.ReadInt();
								byte serializer = @in.ReadByte();
								byte protocolSwitchValue = @in.ReadByte();
								short status = @in.ReadShort();
								short classLen = @in.ReadShort();
								short headerLen = @in.ReadShort();
								int contentLen = @in.ReadInt();
								byte[] clazz = null;
								byte[] header = null;
								byte[] content = null;

								// decide the at-least bytes length for each version
								int lengthAtLeastForV1 = classLen + headerLen + contentLen;
								bool crcSwitchOn = ProtocolSwitch.isOn(ProtocolSwitch.CRC_SWITCH_INDEX, protocolSwitchValue);
								int lengthAtLeastForV2 = classLen + headerLen + contentLen;
								if (crcSwitchOn)
								{
									lengthAtLeastForV2 += 4; // crc int
								}

								// continue read
								if ((version == RpcProtocolV2.PROTOCOL_VERSION_1 && @in.ReadableBytes >= lengthAtLeastForV1) || (version == RpcProtocolV2.PROTOCOL_VERSION_2 && @in.ReadableBytes >= lengthAtLeastForV2))
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
									if (version == RpcProtocolV2.PROTOCOL_VERSION_2 && crcSwitchOn)
									{
										checkCRC(@in, startIndex);
									}
								}
								else
								{ // not enough data
									@in.ResetReaderIndex();
									return;
								}
								ResponseCommand command;
								if (cmdCode == CommandCode_Fields.HEARTBEAT_VALUE)
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
								command.ProtocolSwitch = ProtocolSwitch.create(protocolSwitchValue);
								command.ResponseStatus = (ResponseStatus)status;
								command.Clazz = clazz;
								command.Header = header;
								command.Content = content;
								command.ResponseTimeMillis = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
								command.ResponseHost = (IPEndPoint) ctx.Channel.RemoteAddress;

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

		private void checkCRC(IByteBuffer @in, int startIndex)
		{
			int endIndex = @in.ReaderIndex;
			int expectedCrc = @in.ReadInt();
			byte[] frame = new byte[endIndex - startIndex];
			@in.GetBytes(startIndex, frame, 0, endIndex - startIndex);
			int actualCrc = CrcUtil.crc32(frame);
			if (expectedCrc != actualCrc)
			{
				string err = "CRC check failed!";
				logger.LogError(err);
				throw new Exception(err);
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