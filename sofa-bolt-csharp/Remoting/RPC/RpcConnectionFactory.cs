using Remoting.Config;
using Remoting.Connections;
using Remoting.rpc.protocol;
using System;
using System.Collections.Concurrent;

namespace Remoting.rpc
{
    /// <summary>
    /// Default RPC connection factory impl.
    /// </summary>
    public class RpcConnectionFactory : DefaultConnectionFactory
	{
		public RpcConnectionFactory(ConcurrentDictionary<Type, UserProcessor> userProcessors, ConfigurableInstance configInstance) 
            : base(new RpcCodec(), new HeartbeatHandler(), new RpcHandler(userProcessors), configInstance)
		{
		}
	}
}