using DotNetty.Transport.Channels;
using Remoting.Codecs;
using Remoting.Config;

namespace Remoting.Connections
{
	/// <summary>
	/// Default connection factory.
	/// </summary>
	public class DefaultConnectionFactory : AbstractConnectionFactory
	{
		public DefaultConnectionFactory(Codec codec, IChannelHandler heartbeatHandler, IChannelHandler handler, ConfigurableInstance configInstance) 
            : base(codec, heartbeatHandler, handler, configInstance)
		{
		}
	}
}