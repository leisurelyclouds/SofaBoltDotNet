using DotNetty.Buffers;
using Remoting.Codecs;

namespace Remoting.rpc.protocol
{
	/// <summary>
	/// Rpc protocol decoder.
	/// </summary>
	public class RpcProtocolDecoder : ProtocolCodeBasedDecoder
	{
		public const int MIN_PROTOCOL_CODE_WITH_VERSION = 2;

		public RpcProtocolDecoder(int protocolCodeLength) : base(protocolCodeLength)
		{
		}

		protected internal override byte decodeProtocolVersion(IByteBuffer @in)
		{
			@in.ResetReaderIndex();
			if (@in.ReadableBytes >= protocolCodeLength + DEFAULT_PROTOCOL_VERSION_LENGTH)
			{
				byte rpcProtocolCodeByte = @in.ReadByte();
				if (rpcProtocolCodeByte >= MIN_PROTOCOL_CODE_WITH_VERSION)
				{
					return @in.ReadByte();
				}
			}
			return DEFAULT_ILLEGAL_PROTOCOL_VERSION_LENGTH;
		}
	}
}