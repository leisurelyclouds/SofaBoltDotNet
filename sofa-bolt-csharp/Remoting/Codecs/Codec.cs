using DotNetty.Transport.Channels;

namespace Remoting.Codecs
{
    /// <summary>
    /// Codec interface.
    /// </summary>
    public interface Codec
	{

        /// <summary>
        /// Create an encoder instance.
        /// </summary>
        /// <returns> new encoder instance </returns>
        IChannelHandler newEncoder();

		/// <summary>
		/// Create an decoder instance.
		/// </summary>
		/// <returns> new decoder instance </returns>
		IChannelHandler newDecoder();
	}

}