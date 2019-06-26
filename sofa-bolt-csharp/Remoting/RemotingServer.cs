using java.util.concurrent;
using Remoting.Config;
using Remoting.rpc.protocol;
using System.Net;

namespace Remoting
{
    public interface RemotingServer : Configurable, LifeCycle
	{
        /// <summary>
        /// Get the ip of the server.
        /// </summary>
        /// <returns> ip </returns>
        IPAddress ip();

		/// <summary>
		/// Get the port of the server.
		/// </summary>
		/// <returns> listened port </returns>
		int port();

		/// <summary>
		/// Register processor for command with the command code.
		/// </summary>
		/// <param name="protocolCode"> protocol code </param>
		/// <param name="commandCode"> command code </param>
		/// <param name="processor"> processor </param>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: void registerProcessor(byte protocolCode, CommandCode commandCode, RemotingProcessor<?> processor);
		void registerProcessor(byte protocolCode, CommandCode commandCode, RemotingProcessor processor);

		/// <summary>
		/// Register default executor service for server.
		/// </summary>
		/// <param name="protocolCode"> protocol code </param>
		/// <param name="executor"> the executor service for the protocol code </param>
		void registerDefaultExecutor(byte protocolCode, ExecutorService executor);

		/// <summary>
		/// Register user processor.
		/// </summary>
		/// <param name="processor"> user processor which can be a single-interest processor or a multi-interest processor </param>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: void registerUserProcessor(rpc.protocol.UserProcessor<?> processor);
		void registerUserProcessor(UserProcessor processor);
	}
}