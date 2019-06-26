using System.Net;

namespace Remoting.Connections
{
    /// <summary>
    /// Factory that creates connections.
    /// </summary>
    public interface ConnectionFactory
	{
		/// <summary>
		/// Initialize the factory.
		/// </summary>
		void init(ConnectionEventHandler connectionEventHandler);

		/// <summary>
		/// Create a connection use #BoltUrl
		/// </summary>
		/// <param name="url"> target url </param>
		/// <returns> connection </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Connection createConnection(Url url) throws Exception;
		Connection createConnection(Url url);

        /// <summary>
        /// Create a connection according to the IP and port.
        /// Note: The default protocol is RpcProtocol.
        /// </summary>
        /// <param name="targetIP"> target ip </param>
        /// <param name="targetPort"> target port </param>
        /// <param name="connectTimeout"> connect timeout in millisecond </param>
        /// <returns> connection </returns>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: Connection createConnection(String targetIP, int targetPort, int connectTimeout) throws Exception;
        Connection createConnection(IPAddress targetIP, int targetPort, int connectTimeout);

        /// <summary>
        /// Create a connection according to the IP and port.
        /// 
        /// Note: The default protocol is RpcProtocolV2, and you can specify the version
        /// </summary>
        /// <param name="targetIP"> target ip </param>
        /// <param name="targetPort"> target port </param>
        /// <param name="version"> protocol version </param>
        /// <param name="connectTimeout"> connect timeout in millisecond </param>
        /// <returns> connection </returns>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: Connection createConnection(String targetIP, int targetPort, byte version, int connectTimeout) throws Exception;
        Connection createConnection(IPAddress targetIP, int targetPort, byte version, int connectTimeout);
	}

}