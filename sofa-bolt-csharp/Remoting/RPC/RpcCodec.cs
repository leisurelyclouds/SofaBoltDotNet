using DotNetty.Transport.Channels;
using Remoting.Codecs;
using Remoting.rpc.protocol;

namespace Remoting.rpc
{
	public class RpcCodec : Codec
    {

		public virtual IChannelHandler newEncoder()
		{
			return new ProtocolCodeBasedEncoder(ProtocolCode.fromBytes(RpcProtocolV2.PROTOCOL_CODE));
		}

		public virtual IChannelHandler newDecoder()
		{
			return new RpcProtocolDecoder(RpcProtocolManager.DEFAULT_PROTOCOL_CODE_LENGTH);
		}
	}
}