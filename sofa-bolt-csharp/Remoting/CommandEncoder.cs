using DotNetty.Buffers;
using DotNetty.Transport.Channels;

namespace Remoting
{
    /// <summary>
    /// Encode command.
    /// </summary>
    public interface CommandEncoder
	{
		/// <summary>
		/// Encode object into bytes.
		/// </summary>
		/// <param name="ctx"> </param>
		/// <param name="msg"> </param>
		/// <param name="out"> </param>
		/// <exception cref="Exception"> </exception>
		void encode(IChannelHandlerContext ctx, object msg, IByteBuffer @out);
	}
}