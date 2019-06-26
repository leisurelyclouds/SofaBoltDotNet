using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using System.Collections.Generic;

namespace Remoting
{
    /// <summary>
    /// Decode command.
    /// </summary>
    public interface CommandDecoder
	{
		/// <summary>
		/// Decode bytes into object.
		/// </summary>
		/// <param name="ctx"> </param>
		/// <param name="in"> </param>
		/// <param name="out"> </param>
		/// <exception cref="Exception"> </exception>
		void decode(IChannelHandlerContext ctx, IByteBuffer @in, List<object> @out);
	}

}