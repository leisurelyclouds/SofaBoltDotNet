using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;

namespace Remoting
{
    /// <summary>
    /// Connection manager of connection pool
    /// </summary>
    public interface ConnectionManager : Scannable, LifeCycle
    {
        /// <summary>
        /// Deprecated, use startup instead.
        /// </summary>
        [Obsolete]
        void init();

        /// <summary>
        /// Add a connection to <seealso cref="ConnectionPool"/>.
        /// If it contains multiple pool keys, this connection will be added to multiple <seealso cref="ConnectionPool"/> too.
        /// </summary>
        /// <param name="connection"> an available connection, you should <seealso cref="#check(Connection)"/> this connection before add </param>
        void add(Connection connection);

        /// <summary>
        /// Add a connection to <seealso cref="ConnectionPool"/> with the specified poolKey.
        /// </summary>
        /// <param name="connection"> an available connection, you should <seealso cref="#check(Connection)"/> this connection before add </param>
        /// <param name="poolKey"> unique key of a <seealso cref="ConnectionPool"/> </param>
        void add(Connection connection, string poolKey);

        /// <summary>
        /// Get a connection from <seealso cref="ConnectionPool"/> with the specified poolKey.
        /// </summary>
        /// <param name="poolKey"> unique key of a <seealso cref="ConnectionPool"/> </param>
        /// <returns> a <seealso cref="Connection"/> selected by <seealso cref="ConnectionSelectStrategy"/><br>
        ///   or return {@code null} if there is no <seealso cref="ConnectionPool"/> mapping with poolKey<br>
        ///   or return {@code null} if there is no <seealso cref="Connection"/> in <seealso cref="ConnectionPool"/>. </returns>
        Connection get(string poolKey);

        /// <summary>
        /// Get all connections from <seealso cref="ConnectionPool"/> with the specified poolKey.
        /// </summary>
        /// <param name="poolKey"> unique key of a <seealso cref="ConnectionPool"/> </param>
        /// <returns> a list of <seealso cref="Connection"/><br>
        ///   or return an empty list if there is no <seealso cref="ConnectionPool"/> mapping with poolKey. </returns>
        List<Connection> getAll(string poolKey);

        /// <summary>
        /// Get all connections of all poolKey.
        /// </summary>
        /// <returns> a map with poolKey as key and a list of connections in ConnectionPool as value </returns>
        ConcurrentDictionary<string, List<Connection>> All { get; }

        /// <summary>
        /// Remove a <seealso cref="Connection"/> from all <seealso cref="ConnectionPool"/> with the poolKeys in <seealso cref="Connection"/>, and close it.
        /// </summary>
        void remove(Connection connection);

        /// <summary>
        /// Remove and close a <seealso cref="Connection"/> from <seealso cref="ConnectionPool"/> with the specified poolKey.
        /// </summary>
        /// <param name="connection"> target connection </param>
        /// <param name="poolKey"> unique key of a <seealso cref="ConnectionPool"/> </param>
        void remove(Connection connection, string poolKey);

        /// <summary>
        /// Remove and close all connections from <seealso cref="ConnectionPool"/> with the specified poolKey.
        /// </summary>
        /// <param name="poolKey"> unique key of a <seealso cref="ConnectionPool"/> </param>
        void remove(string poolKey);

        /// <summary>
        /// Remove and close all connections from all <seealso cref="ConnectionPool"/>.
        /// Deprecated, use shutdown instead.
        /// </summary>
        [Obsolete]
        void removeAll();

        /// <summary>
        /// check a connection whether available, if not, throw RemotingException
        /// </summary>
        /// <param name="connection"> target connection </param>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: void check(Connection connection) throws exception.RemotingException;
        void check(Connection connection);

        /// <summary>
        /// Get the number of <seealso cref="Connection"/> in <seealso cref="ConnectionPool"/> with the specified pool key
        /// </summary>
        /// <param name="poolKey"> unique key of a <seealso cref="ConnectionPool"/> </param>
        /// <returns> connection count </returns>
        int count(string poolKey);

        /// <summary>
        /// Get a connection using <seealso cref="Url"/>, if {@code null} then create and add into <seealso cref="ConnectionPool"/>.
        /// The connection number of <seealso cref="ConnectionPool"/> is decided by <seealso cref="Url#getConnNum()"/>
        /// </summary>
        /// <param name="url"> <seealso cref="Url"/> contains connect infos. </param>
        /// <returns> the created <seealso cref="Connection"/>. </returns>
        /// <exception cref="ThreadInterruptedException"> if interrupted </exception>
        /// <exception cref="RemotingException"> if create failed. </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: Connection getAndCreateIfAbsent(Url url) throws ThreadInterruptedException, exception.RemotingException;
        Connection getAndCreateIfAbsent(Url url);

        /// <summary>
        /// This method can create connection pool with connections initialized and check the number of connections.
        /// The connection number of <seealso cref="ConnectionPool"/> is decided by <seealso cref="Url#getConnNum()"/>.
        /// Each time call this method, will check the number of connection, if not enough, this will do the healing logic additionally.
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: void createConnectionAndHealIfNeed(Url url) throws ThreadInterruptedException, exception.RemotingException;
        void createConnectionAndHealIfNeed(Url url);

        // ~~~ create operation

        /// <summary>
        /// Create a connection using specified <seealso cref="Url"/>.
        /// </summary>
        /// <param name="url"> <seealso cref="Url"/> contains connect infos. </param>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: Connection create(Url url) throws exception.RemotingException;
        Connection create(Url url);

        /// <summary>
        /// Create a connection using specified <seealso cref="string"/> address.
        /// </summary>
        /// <param name="address"> a <seealso cref="string"/> address, e.g. 127.0.0.1:1111 </param>
        /// <param name="connectTimeout"> an int connect timeout value </param>
        /// <returns> the created <seealso cref="Connection"/> </returns>
        /// <exception cref="RemotingException"> if create failed </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: Connection create(String address, int connectTimeout) throws exception.RemotingException;
        Connection create(string address, int connectTimeout);

        /// <summary>
        /// Create a connection using specified ip and port.
        /// </summary>
        /// <param name="ip"> connect ip, e.g. 127.0.0.1 </param>
        /// <param name="port"> connect port, e.g. 1111 </param>
        /// <param name="connectTimeout"> an int connect timeout value </param>
        /// <returns> the created <seealso cref="Connection"/> </returns>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: Connection create(String ip, int port, int connectTimeout) throws exception.RemotingException;
        Connection create(IPAddress ip, int port, int connectTimeout);
    }
}