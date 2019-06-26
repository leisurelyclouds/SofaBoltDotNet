using DotNetty.Codecs;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using DotNetty.Common.Utilities;

namespace Remoting.Codecs
{
    /// <summary>
    /// Protocol code based newEncoder, the main newEncoder for a certain protocol, which is lead by one or multi bytes (magic code).
    /// 
    /// Notice: this is stateless can be noted as <seealso cref="io.netty.channel.ChannelHandler.Sharable"/>
    /// </summary>
    //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
    //ORIGINAL LINE: @ChannelHandler.Sharable public class ProtocolCodeBasedEncoder extends io.netty.handler.codec.MessageToByteEncoder<java.io.Serializable>
    public class ProtocolCodeBasedEncoder : MessageToByteEncoder<object>
	{
		/// <summary>
		/// default protocol code 
        /// </summary>
		protected internal ProtocolCode defaultProtocolCode;

		public ProtocolCodeBasedEncoder(ProtocolCode defaultProtocolCode) : base()
		{
			this.defaultProtocolCode = defaultProtocolCode;
		}

		protected override void Encode(IChannelHandlerContext ctx, object msg, IByteBuffer @out)
		{
			IAttribute<ProtocolCode> att = ctx.Channel.GetAttribute(Connection.PROTOCOL);
			ProtocolCode protocolCode;
			if (att == null || att.Get() == null)
			{
				protocolCode = defaultProtocolCode;
			}
			else
			{
				protocolCode = att.Get();
			}
			Protocol protocol = ProtocolManager.getProtocol(protocolCode);
			protocol.Encoder.encode(ctx, msg, @out);
		}
	}
}