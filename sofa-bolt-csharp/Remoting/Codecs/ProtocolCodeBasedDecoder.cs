using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using System.Collections.Generic;

namespace Remoting.Codecs
{
    /// <summary>
    /// Protocol code based decoder, the main decoder for a certain protocol, which is lead by one or multi bytes (magic code).
    /// 
    /// Notice: this is not stateless, can not be noted as <seealso cref="io.netty.channel.ChannelHandler.Sharable"/>
    /// </summary>
    public class ProtocolCodeBasedDecoder : AbstractBatchDecoder
	{
		/// <summary>
		/// by default, suggest design a single byte for protocol version.
		/// </summary>
		public const int DEFAULT_PROTOCOL_VERSION_LENGTH = 1;
		/// <summary>
		/// protocol version should be a positive number, we use -1 to represent illegal
		/// </summary>
		public const int DEFAULT_ILLEGAL_PROTOCOL_VERSION_LENGTH = 255;

		/// <summary>
		/// the length of protocol code
		/// </summary>
		protected internal int protocolCodeLength;

		public ProtocolCodeBasedDecoder(int protocolCodeLength) : base()
		{
			this.protocolCodeLength = protocolCodeLength;
		}

		/// <summary>
		/// decode the protocol code
		/// </summary>
		/// <param name="in"> input byte buf </param>
		/// <returns> an instance of ProtocolCode </returns>
		protected internal virtual ProtocolCode decodeProtocolCode(IByteBuffer @in)
		{
			if (@in.ReadableBytes >= protocolCodeLength)
			{
				byte[] protocolCodeBytes = new byte[protocolCodeLength];
				@in.ReadBytes(protocolCodeBytes);
				return ProtocolCode.fromBytes(protocolCodeBytes);
			}
			return null;
		}

		/// <summary>
		/// decode the protocol version
		/// </summary>
		/// <param name="in"> input byte buf </param>
		/// <returns> a byte to represent protocol version </returns>
		protected internal virtual byte decodeProtocolVersion(IByteBuffer @in)
		{
			if (@in.ReadableBytes >= DEFAULT_PROTOCOL_VERSION_LENGTH)
			{
				return @in.ReadByte();
			}
			return DEFAULT_ILLEGAL_PROTOCOL_VERSION_LENGTH;
		}

		protected internal override void decode(IChannelHandlerContext ctx, IByteBuffer @in, List<object> @out)
		{
			@in.MarkReaderIndex();
			ProtocolCode protocolCode = decodeProtocolCode(@in);
			if (null != protocolCode)
			{
				byte protocolVersion = decodeProtocolVersion(@in);
				if (ctx.Channel.GetAttribute(Connection.PROTOCOL).Get() == null)
				{
					ctx.Channel.GetAttribute(Connection.PROTOCOL).Set(protocolCode);
					if (DEFAULT_ILLEGAL_PROTOCOL_VERSION_LENGTH != protocolVersion)
					{
						ctx.Channel.GetAttribute(Connection.VERSION).Set(protocolVersion);
					}
				}
				Protocol protocol = ProtocolManager.getProtocol(protocolCode);
				if (null != protocol)
				{
					@in.ResetReaderIndex();
					protocol.Decoder.decode(ctx, @in, @out);
				}
				else
				{
					throw new CodecException("Unknown protocol code: [" + protocolCode + "] while decode in ProtocolDecoder.");
				}
			}
		}
	}

}