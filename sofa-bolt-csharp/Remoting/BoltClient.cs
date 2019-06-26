using Remoting.Config;
using Remoting.rpc;
using Remoting.rpc.protocol;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;

namespace Remoting
{
    /// <summary>
    /// Bolt client interface.
    /// </summary>
    public interface BoltClient : Configurable, LifeCycle
	{

		/// <summary>
		/// One way invocation using a string address, address format example - 127.0.0.1:12200?key1=value1&key2=value2 <br>
		/// <para>
		/// Notice:<br>
		///   <ol>
		///   <li><b>DO NOT modify the request object concurrently when this method is called.</b></li>
		///   <li>When do invocation, use the string address to find a available connection, if none then create one.</li>
		///      <ul>
		///        <li>You can use <seealso cref="RpcConfigs#CONNECT_TIMEOUT_KEY"/> to specify connection timeout, time unit is milliseconds, e.g [127.0.0.1:12200?_CONNECTTIMEOUT=3000]
		///        <li>You can use <seealso cref="RpcConfigs#CONNECTION_NUM_KEY"/> to specify connection number for each ip and port, e.g [127.0.0.1:12200?_CONNECTIONNUM=30]
		///        <li>You can use <seealso cref="RpcConfigs#CONNECTION_WARMUP_KEY"/> to specify whether need warmup all connections for the first time you call this method, e.g [127.0.0.1:12200?_CONNECTIONWARMUP=false]
		///      </ul>
		///   <li>You should use <seealso cref="#closeConnection(String addr)"/> to close it if you want.
		///   </ol>
		/// 
		/// </para>
		/// </summary>
		/// <param name="address"> target address </param>
		/// <param name="request"> request </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void oneway(final String address, final Object request) throws exception.RemotingException, ThreadInterruptedException;
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		void oneway(string address, object request);

		/// <summary>
		/// Oneway invocation with a <seealso cref="InvokeContext"/>, common api notice please see <seealso cref="#oneway(Connection, Object)"/>
		/// </summary>
		/// <param name="address"> target address </param>
		/// <param name="request"> request </param>
		/// <param name="invokeContext"> invoke context </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void oneway(final String address, final Object request, final InvokeContext invokeContext) throws exception.RemotingException, ThreadInterruptedException;
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		void oneway(string address, object request, InvokeContext invokeContext);

		/// <summary>
		/// One way invocation using a parsed <seealso cref="Url"/> <br>
		/// <para>
		/// Notice:<br>
		///   <ol>
		///   <li><b>DO NOT modify the request object concurrently when this method is called.</b></li>
		///   <li>When do invocation, use the parsed <seealso cref="Url"/> to find a available connection, if none then create one.</li>
		///      <ul>
		///        <li>You can use <seealso cref="Url#setConnectTimeout"/> to specify connection timeout, time unit is milliseconds.
		///        <li>You can use <seealso cref="Url#setConnNum"/> to specify connection number for each ip and port.
		///        <li>You can use <seealso cref="Url#setConnWarmup"/> to specify whether need warmup all connections for the first time you call this method.
		///      </ul>
		///   <li>You should use <seealso cref="#closeConnection(Url url)"/> to close it if you want.
		///   </ol>
		/// 
		/// </para>
		/// </summary>
		/// <param name="url"> target url </param>
		/// <param name="request"> object </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void oneway(final Url url, final Object request) throws exception.RemotingException, ThreadInterruptedException;
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		void oneway(Url url, object request);

		/// <summary>
		/// Oneway invocation with a <seealso cref="InvokeContext"/>, common api notice please see <seealso cref="#oneway(Url, Object)"/>
		/// </summary>
		/// <param name="url"> url </param>
		/// <param name="request"> request </param>
		/// <param name="invokeContext"> invoke context </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void oneway(final Url url, final Object request, final InvokeContext invokeContext) throws exception.RemotingException, ThreadInterruptedException;
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		void oneway(Url url, object request, InvokeContext invokeContext);

		/// <summary>
		/// One way invocation using a <seealso cref="Connection"/> <br>
		/// <para>
		/// Notice:<br>
		///   <b>DO NOT modify the request object concurrently when this method is called.</b>
		/// 
		/// </para>
		/// </summary>
		/// <param name="conn"> target connection </param>
		/// <param name="request"> request </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void oneway(final Connection conn, final Object request) throws exception.RemotingException;
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		void oneway(Connection conn, object request);

		/// <summary>
		/// Oneway invocation with a <seealso cref="InvokeContext"/>, common api notice please see <seealso cref="#oneway(Connection, Object)"/>
		/// </summary>
		/// <param name="conn"> target connection </param>
		/// <param name="request"> request </param>
		/// <param name="invokeContext"> invoke context </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void oneway(final Connection conn, final Object request, final InvokeContext invokeContext) throws exception.RemotingException;
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		void oneway(Connection conn, object request, InvokeContext invokeContext);

		/// <summary>
		/// Synchronous invocation using a string address, address format example - 127.0.0.1:12200?key1=value1&key2=value2 <br>
		/// <para>
		/// Notice:<br>
		///   <ol>
		///   <li><b>DO NOT modify the request object concurrently when this method is called.</b></li>
		///   <li>When do invocation, use the string address to find a available connection, if none then create one.</li>
		///      <ul>
		///        <li>You can use <seealso cref="RpcConfigs#CONNECT_TIMEOUT_KEY"/> to specify connection timeout, time unit is milliseconds, e.g [127.0.0.1:12200?_CONNECTTIMEOUT=3000]
		///        <li>You can use <seealso cref="RpcConfigs#CONNECTION_NUM_KEY"/> to specify connection number for each ip and port, e.g [127.0.0.1:12200?_CONNECTIONNUM=30]
		///        <li>You can use <seealso cref="RpcConfigs#CONNECTION_WARMUP_KEY"/> to specify whether need warmup all connections for the first time you call this method, e.g [127.0.0.1:12200?_CONNECTIONWARMUP=false]
		///      </ul>
		///   <li>You should use <seealso cref="#closeConnection(String addr)"/> to close it if you want.
		///   </ol>
		/// 
		/// </para>
		/// </summary>
		/// <param name="address"> target address </param>
		/// <param name="request"> request </param>
		/// <param name="timeoutMillis"> timeout millisecond </param>
		/// <returns> result object </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Object invokeSync(final String address, final Object request, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException;
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		object invokeSync(string address, object request, int timeoutMillis);

		/// <summary>
		/// Synchronous invocation with a <seealso cref="InvokeContext"/>, common api notice please see <seealso cref="#invokeSync(String, Object, int)"/>
		/// </summary>
		/// <param name="address"> target address </param>
		/// <param name="request"> request </param>
		/// <param name="invokeContext"> invoke context </param>
		/// <param name="timeoutMillis"> timeout in millisecond </param>
		/// <returns> result object </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Object invokeSync(final String address, final Object request, final InvokeContext invokeContext, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException;
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		object invokeSync(string address, object request, InvokeContext invokeContext, int timeoutMillis);

		/// <summary>
		/// Synchronous invocation using a parsed <seealso cref="Url"/> <br>
		/// <para>
		/// Notice:<br>
		///   <ol>
		///   <li><b>DO NOT modify the request object concurrently when this method is called.</b></li>
		///   <li>When do invocation, use the parsed <seealso cref="Url"/> to find a available connection, if none then create one.</li>
		///      <ul>
		///        <li>You can use <seealso cref="Url#setConnectTimeout"/> to specify connection timeout, time unit is milliseconds.
		///        <li>You can use <seealso cref="Url#setConnNum"/> to specify connection number for each ip and port.
		///        <li>You can use <seealso cref="Url#setConnWarmup"/> to specify whether need warmup all connections for the first time you call this method.
		///      </ul>
		///   <li>You should use <seealso cref="#closeConnection(Url url)"/> to close it if you want.
		///   </ol>
		/// 
		/// </para>
		/// </summary>
		/// <param name="url"> target url </param>
		/// <param name="request"> request </param>
		/// <param name="timeoutMillis"> timeout in millisecond </param>
		/// <returns> Object </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Object invokeSync(final Url url, final Object request, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException;
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		object invokeSync(Url url, object request, int timeoutMillis);

		/// <summary>
		/// Synchronous invocation with a <seealso cref="InvokeContext"/>, common api notice please see <seealso cref="#invokeSync(Url, Object, int)"/>
		/// </summary>
		/// <param name="url"> target url </param>
		/// <param name="request"> request </param>
		/// <param name="invokeContext"> invoke context </param>
		/// <param name="timeoutMillis"> timeout in millisecond </param>
		/// <returns> result object </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Object invokeSync(final Url url, final Object request, final InvokeContext invokeContext, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException;
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		object invokeSync(Url url, object request, InvokeContext invokeContext, int timeoutMillis);

		/// <summary>
		/// Synchronous invocation using a <seealso cref="Connection"/> <br>
		/// <para>
		/// Notice:<br>
		///   <b>DO NOT modify the request object concurrently when this method is called.</b>
		/// 
		/// </para>
		/// </summary>
		/// <param name="conn"> target connection </param>
		/// <param name="request"> request </param>
		/// <param name="timeoutMillis"> timeout in millisecond </param>
		/// <returns> Object </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Object invokeSync(final Connection conn, final Object request, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException;
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		object invokeSync(Connection conn, object request, int timeoutMillis);

		/// <summary>
		/// Synchronous invocation with a <seealso cref="InvokeContext"/>, common api notice please see <seealso cref="#invokeSync(Connection, Object, int)"/>
		/// </summary>
		/// <param name="conn"> target connection </param>
		/// <param name="request"> request </param>
		/// <param name="invokeContext"> invoke context </param>
		/// <param name="timeoutMillis"> timeout in millis </param>
		/// <returns> Object </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Object invokeSync(final Connection conn, final Object request, final InvokeContext invokeContext, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException;
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		object invokeSync(Connection conn, object request, InvokeContext invokeContext, int timeoutMillis);

        /// <summary>
        /// Future invocation using a string address, address format example - 127.0.0.1:12200?key1=value1&key2=value2 <br>
        /// You can get result use the returned <seealso cref="RpcResponseFuture"/>.
        /// <para>
        /// Notice:<br>
        ///   <ol>
        ///   <li><b>DO NOT modify the request object concurrently when this method is called.</b></li>
        ///   <li>When do invocation, use the string address to find a available connection, if none then create one.</li>
        ///      <ul>
        ///        <li>You can use <seealso cref="RpcConfigs#CONNECT_TIMEOUT_KEY"/> to specify connection timeout, time unit is milliseconds, e.g [127.0.0.1:12200?_CONNECTTIMEOUT=3000]
        ///        <li>You can use <seealso cref="RpcConfigs#CONNECTION_NUM_KEY"/> to specify connection number for each ip and port, e.g [127.0.0.1:12200?_CONNECTIONNUM=30]
        ///        <li>You can use <seealso cref="RpcConfigs#CONNECTION_WARMUP_KEY"/> to specify whether need warmup all connections for the first time you call this method, e.g [127.0.0.1:12200?_CONNECTIONWARMUP=false]
        ///      </ul>
        ///   <li>You should use <seealso cref="#closeConnection(String addr)"/> to close it if you want.
        ///   </ol>
        /// 
        /// </para>
        /// </summary>
        /// <param name="address"> target address </param>
        /// <param name="request"> request </param>
        /// <param name="timeoutMillis"> timeout in millisecond </param>
        /// <returns> RpcResponseFuture </returns>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: rpc.RpcResponseFuture invokeWithFuture(final String address, final Object request, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException;
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        RpcResponseFuture invokeWithFuture(string address, object request, int timeoutMillis);

        /// <summary>
        /// Future invocation with a <seealso cref="InvokeContext"/>, common api notice please see <seealso cref="#invokeWithFuture(String, Object, int)"/>
        /// </summary>
        /// <param name="address"> target address </param>
        /// <param name="request"> request </param>
        /// <param name="invokeContext"> invoke context </param>
        /// <param name="timeoutMillis"> timeout in millisecond </param>
        /// <returns> RpcResponseFuture </returns>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: rpc.RpcResponseFuture invokeWithFuture(final String address, final Object request, final InvokeContext invokeContext, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException;
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        RpcResponseFuture invokeWithFuture(string address, object request, InvokeContext invokeContext, int timeoutMillis);

        /// <summary>
        /// Future invocation using a parsed <seealso cref="Url"/> <br>
        /// You can get result use the returned <seealso cref="RpcResponseFuture"/>.
        /// <para>
        /// Notice:<br>
        ///   <ol>
        ///   <li><b>DO NOT modify the request object concurrently when this method is called.</b></li>
        ///   <li>When do invocation, use the parsed <seealso cref="Url"/> to find a available connection, if none then create one.</li>
        ///      <ul>
        ///        <li>You can use <seealso cref="Url#setConnectTimeout"/> to specify connection timeout, time unit is milliseconds.
        ///        <li>You can use <seealso cref="Url#setConnNum"/> to specify connection number for each ip and port.
        ///        <li>You can use <seealso cref="Url#setConnWarmup"/> to specify whether need warmup all connections for the first time you call this method.
        ///      </ul>
        ///   <li>You should use <seealso cref="#closeConnection(Url url)"/> to close it if you want.
        ///   </ol>
        /// 
        /// </para>
        /// </summary>
        /// <param name="url"> target url </param>
        /// <param name="request"> request </param>
        /// <param name="timeoutMillis"> timeout in millisecond </param>
        /// <returns> RpcResponseFuture </returns>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: rpc.RpcResponseFuture invokeWithFuture(final Url url, final Object request, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException;
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        RpcResponseFuture invokeWithFuture(Url url, object request, int timeoutMillis);

        /// <summary>
        /// Future invocation with a <seealso cref="InvokeContext"/>, common api notice please see <seealso cref="#invokeWithFuture(Url, Object, int)"/>
        /// </summary>
        /// <param name="url"> target url </param>
        /// <param name="request"> request </param>
        /// <param name="invokeContext"> invoke context </param>
        /// <param name="timeoutMillis"> timeout in millis </param>
        /// <returns> RpcResponseFuture </returns>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: rpc.RpcResponseFuture invokeWithFuture(final Url url, final Object request, final InvokeContext invokeContext, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException;
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        RpcResponseFuture invokeWithFuture(Url url, object request, InvokeContext invokeContext, int timeoutMillis);

        /// <summary>
        /// Future invocation using a <seealso cref="Connection"/> <br>
        /// You can get result use the returned <seealso cref="RpcResponseFuture"/>.
        /// <para>
        /// Notice:<br>
        ///   <b>DO NOT modify the request object concurrently when this method is called.</b>
        /// 
        /// </para>
        /// </summary>
        /// <param name="conn"> target connection </param>
        /// <param name="request"> request </param>
        /// <param name="timeoutMillis"> timeout in millis </param>
        /// <returns> RpcResponseFuture </returns>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: rpc.RpcResponseFuture invokeWithFuture(final Connection conn, final Object request, int timeoutMillis) throws exception.RemotingException;
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        RpcResponseFuture invokeWithFuture(Connection conn, object request, int timeoutMillis);

        /// <summary>
        /// Future invocation with a <seealso cref="InvokeContext"/>, common api notice please see <seealso cref="#invokeWithFuture(Connection, Object, int)"/>
        /// </summary>
        /// <param name="conn"> target connection </param>
        /// <param name="request"> request </param>
        /// <param name="invokeContext"> context </param>
        /// <param name="timeoutMillis"> timeout millisecond </param>
        /// <returns> RpcResponseFuture </returns>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: rpc.RpcResponseFuture invokeWithFuture(final Connection conn, final Object request, final InvokeContext invokeContext, int timeoutMillis) throws exception.RemotingException;
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        RpcResponseFuture invokeWithFuture(Connection conn, object request, InvokeContext invokeContext, int timeoutMillis);

        /// <summary>
        /// Callback invocation using a string address, address format example - 127.0.0.1:12200?key1=value1&key2=value2 <br>
        /// You can specify an implementation of <seealso cref="InvokeCallback"/> to get the result.
        /// <para>
        /// Notice:<br>
        ///   <ol>
        ///   <li><b>DO NOT modify the request object concurrently when this method is called.</b></li>
        ///   <li>When do invocation, use the string address to find a available connection, if none then create one.</li>
        ///      <ul>
        ///        <li>You can use <seealso cref="RpcConfigs#CONNECT_TIMEOUT_KEY"/> to specify connection timeout, time unit is milliseconds, e.g [127.0.0.1:12200?_CONNECTTIMEOUT=3000]
        ///        <li>You can use <seealso cref="RpcConfigs#CONNECTION_NUM_KEY"/> to specify connection number for each ip and port, e.g [127.0.0.1:12200?_CONNECTIONNUM=30]
        ///        <li>You can use <seealso cref="RpcConfigs#CONNECTION_WARMUP_KEY"/> to specify whether need warmup all connections for the first time you call this method, e.g [127.0.0.1:12200?_CONNECTIONWARMUP=false]
        ///      </ul>
        ///   <li>You should use <seealso cref="#closeConnection(String addr)"/> to close it if you want.
        ///   </ol>
        /// 
        /// </para>
        /// </summary>
        /// <param name="addr"> target address </param>
        /// <param name="request"> request </param>
        /// <param name="invokeCallback"> callback </param>
        /// <param name="timeoutMillis"> timeout in millisecond </param>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: void invokeWithCallback(final String addr, final Object request, final InvokeCallback invokeCallback, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException;
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        void invokeWithCallback(string addr, object request, InvokeCallback invokeCallback, int timeoutMillis);

        /// <summary>
        /// Callback invocation with a <seealso cref="InvokeContext"/>, common api notice please see <seealso cref="#invokeWithCallback(String, Object, InvokeCallback, int)"/>
        /// </summary>
        /// <param name="addr"> target address </param>
        /// <param name="request"> request </param>
        /// <param name="invokeContext"> context </param>
        /// <param name="invokeCallback"> callback </param>
        /// <param name="timeoutMillis"> timeout in millisecond </param>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: void invokeWithCallback(final String addr, final Object request, final InvokeContext invokeContext, final InvokeCallback invokeCallback, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException;
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        void invokeWithCallback(string addr, object request, InvokeContext invokeContext, InvokeCallback invokeCallback, int timeoutMillis);

        /// <summary>
        /// Callback invocation using a parsed <seealso cref="Url"/> <br>
        /// You can specify an implementation of <seealso cref="InvokeCallback"/> to get the result.
        /// <para>
        /// Notice:<br>
        ///   <ol>
        ///   <li><b>DO NOT modify the request object concurrently when this method is called.</b></li>
        ///   <li>When do invocation, use the parsed <seealso cref="Url"/> to find a available connection, if none then create one.</li>
        ///      <ul>
        ///        <li>You can use <seealso cref="Url#setConnectTimeout"/> to specify connection timeout, time unit is milliseconds.
        ///        <li>You can use <seealso cref="Url#setConnNum"/> to specify connection number for each ip and port.
        ///        <li>You can use <seealso cref="Url#setConnWarmup"/> to specify whether need warmup all connections for the first time you call this method.
        ///      </ul>
        ///   <li>You should use <seealso cref="#closeConnection(Url url)"/> to close it if you want.
        ///   </ol>
        /// 
        /// </para>
        /// </summary>
        /// <param name="url"> target url </param>
        /// <param name="request"> request </param>
        /// <param name="invokeCallback"> callback </param>
        /// <param name="timeoutMillis"> timeout in millisecond </param>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: void invokeWithCallback(final Url url, final Object request, final InvokeCallback invokeCallback, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException;
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        void invokeWithCallback(Url url, object request, InvokeCallback invokeCallback, int timeoutMillis);

        /// <summary>
        /// Callback invocation with a <seealso cref="InvokeContext"/>, common api notice please see <seealso cref="#invokeWithCallback(Url, Object, InvokeCallback, int)"/>
        /// </summary>
        /// <param name="url"> target url </param>
        /// <param name="request"> request </param>
        /// <param name="invokeContext"> context </param>
        /// <param name="invokeCallback"> callback </param>
        /// <param name="timeoutMillis"> timeout in millisecond </param>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: void invokeWithCallback(final Url url, final Object request, final InvokeContext invokeContext, final InvokeCallback invokeCallback, final int timeoutMillis) throws exception.RemotingException, ThreadInterruptedException;
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        void invokeWithCallback(Url url, object request, InvokeContext invokeContext, InvokeCallback invokeCallback, int timeoutMillis);

        /// <summary>
        /// Callback invocation using a <seealso cref="Connection"/> <br>
        /// You can specify an implementation of <seealso cref="InvokeCallback"/> to get the result.
        /// <para>
        /// Notice:<br>
        ///   <b>DO NOT modify the request object concurrently when this method is called.</b>
        /// 
        /// </para>
        /// </summary>
        /// <param name="conn"> target connection </param>
        /// <param name="request"> request </param>
        /// <param name="invokeCallback"> callback </param>
        /// <param name="timeoutMillis"> timeout in millisecond </param>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: void invokeWithCallback(final Connection conn, final Object request, final InvokeCallback invokeCallback, final int timeoutMillis) throws exception.RemotingException;
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        void invokeWithCallback(Connection conn, object request, InvokeCallback invokeCallback, int timeoutMillis);

        /// <summary>
        /// Callback invocation with a <seealso cref="InvokeContext"/>, common api notice please see <seealso cref="#invokeWithCallback(Connection, Object, InvokeCallback, int)"/>
        /// </summary>
        /// <param name="conn"> target connection </param>
        /// <param name="request"> request </param>
        /// <param name="invokeContext"> invoke context </param>
        /// <param name="invokeCallback"> callback </param>
        /// <param name="timeoutMillis"> timeout in millisecond </param>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: void invokeWithCallback(final Connection conn, final Object request, final InvokeContext invokeContext, final InvokeCallback invokeCallback, final int timeoutMillis) throws exception.RemotingException;
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        void invokeWithCallback(Connection conn, object request, InvokeContext invokeContext, InvokeCallback invokeCallback, int timeoutMillis);

        /// <summary>
        /// Add processor to process connection event.
        /// </summary>
        /// <param name="type"> connection event type </param>
        /// <param name="processor"> connection event process </param>
        void addConnectionEventProcessor(ConnectionEventType type, ConnectionEventProcessor processor);

        /// <summary>
        /// Use UserProcessorRegisterHelper<seealso cref="UserProcessorRegisterHelper"/> to help register user processor for client side.
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
        //ORIGINAL LINE: void registerUserProcessor(rpc.protocol.UserProcessor<?> processor);
        void registerUserProcessor(UserProcessor processor);

        /// <summary>
        /// Create a stand alone connection using ip and port. <br>
        /// <para>
        /// Notice:<br>
        ///   <li>Each time you call this method, will create a new connection.
        ///   <li>Bolt will not control this connection.
        ///   <li>You should use <seealso cref="#closeStandaloneConnection"/> to close it.
        /// 
        /// </para>
        /// </summary>
        /// <param name="ip"> ip </param>
        /// <param name="port"> port </param>
        /// <param name="connectTimeout"> timeout in millisecond </param>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: Connection createStandaloneConnection(String ip, int port, int connectTimeout) throws exception.RemotingException;
        Connection createStandaloneConnection(IPAddress ip, int port, int connectTimeout);

		/// <summary>
		/// Create a stand alone connection using address, address format example - 127.0.0.1:12200 <br>
		/// <para>
		/// Notice:<br>
		///   <ol>
		///   <li>Each time you can this method, will create a new connection.
		///   <li>Bolt will not control this connection.
		///   <li>You should use <seealso cref="#closeStandaloneConnection"/> to close it.
		///   </ol>
		/// 
		/// </para>
		/// </summary>
		/// <param name="address"> target address </param>
		/// <param name="connectTimeout"> timeout in millisecond </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Connection createStandaloneConnection(String address, int connectTimeout) throws exception.RemotingException;
		Connection createStandaloneConnection(string address, int connectTimeout);

		/// <summary>
		/// Close a standalone connection
		/// </summary>
		/// <param name="conn"> target connection </param>
		void closeStandaloneConnection(Connection conn);

		/// <summary>
		/// Get a connection using address, address format example - 127.0.0.1:12200?key1=value1&key2=value2 <br>
		/// <para>
		/// Notice:<br>
		///   <ol>
		///   <li>Get a connection, if none then create.</li>
		///      <ul>
		///        <li>You can use <seealso cref="RpcConfigs#CONNECT_TIMEOUT_KEY"/> to specify connection timeout, time unit is milliseconds, e.g [127.0.0.1:12200?_CONNECTTIMEOUT=3000]
		///        <li>You can use <seealso cref="RpcConfigs#CONNECTION_NUM_KEY"/> to specify connection number for each ip and port, e.g [127.0.0.1:12200?_CONNECTIONNUM=30]
		///        <li>You can use <seealso cref="RpcConfigs#CONNECTION_WARMUP_KEY"/> to specify whether need warmup all connections for the first time you call this method, e.g [127.0.0.1:12200?_CONNECTIONWARMUP=false]
		///      </ul>
		///   <li>Bolt will control this connection in <seealso cref="ConnectionPool"/>
		///   <li>You should use <seealso cref="#closeConnection(String addr)"/> to close it.
		///   </ol>
		/// </para>
		/// </summary>
		/// <param name="addr"> target address </param>
		/// <param name="connectTimeout"> this is prior to url args <seealso cref="RpcConfigs#CONNECT_TIMEOUT_KEY"/> </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Connection getConnection(String addr, int connectTimeout) throws exception.RemotingException, ThreadInterruptedException;
		Connection getConnection(string addr, int connectTimeout);

		/// <summary>
		/// Get a connection using a <seealso cref="Url"/>.<br>
		/// <para>
		/// Notice:
		///   <ol>
		///   <li>Get a connection, if none then create.
		///   <li>Bolt will control this connection in <seealso cref="ConnectionPool"/>
		///   <li>You should use <seealso cref="#closeConnection(Url url)"/> to close it.
		///   </ol>
		/// 
		/// </para>
		/// </summary>
		/// <param name="url"> target url </param>
		/// <param name="connectTimeout"> this is prior to url args <seealso cref="RpcConfigs#CONNECT_TIMEOUT_KEY"/> </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Connection getConnection(Url url, int connectTimeout) throws exception.RemotingException, ThreadInterruptedException;
		Connection getConnection(Url url, int connectTimeout);

        /// <summary>
        /// get all connections managed by rpc client
        /// </summary>
        /// <returns> map key is ip+port, value is a list of connections of this key. </returns>
        ConcurrentDictionary<string, List<Connection>> AllManagedConnections {get;}

		/// <summary>
		/// check connection, the address format example - 127.0.0.1:12200?key1=value1&key2=value2
		/// </summary>
		/// <param name="address"> target address </param>
		/// <returns> true if and only if there is a connection, and the connection is active and writable;else return false </returns>
		bool checkConnection(string address);

		/// <summary>
		/// Close all connections of a address
		/// </summary>
		/// <param name="address"> target address </param>
		void closeConnection(string address);

		/// <summary>
		/// Close all connections of a <seealso cref="Url"/>
		/// </summary>
		/// <param name="url"> target url </param>
		void closeConnection(Url url);

		/// <summary>
		/// Enable heart beat for a certain connection.
		/// If this address not connected, then do nothing.
		/// <para>
		/// Notice: this method takes no effect on a stand alone connection.
		/// 
		/// </para>
		/// </summary>
		/// <param name="address"> target address </param>
		void enableConnHeartbeat(string address);

		/// <summary>
		/// Enable heart beat for a certain connection.
		/// If this <seealso cref="Url"/> not connected, then do nothing.
		/// <para>
		/// Notice: this method takes no effect on a stand alone connection.
		/// 
		/// </para>
		/// </summary>
		/// <param name="url"> target url </param>
		void enableConnHeartbeat(Url url);

		/// <summary>
		/// Disable heart beat for a certain connection.
		/// If this addr not connected, then do nothing.
		/// <para>
		/// Notice: this method takes no effect on a stand alone connection.
		/// 
		/// </para>
		/// </summary>
		/// <param name="address"> target address </param>
		void disableConnHeartbeat(string address);

		/// <summary>
		/// Disable heart beat for a certain connection.
		/// If this <seealso cref="Url"/> not connected, then do nothing.
		/// <para>
		/// Notice: this method takes no effect on a stand alone connection.
		/// 
		/// </para>
		/// </summary>
		/// <param name="url"> target url </param>
		void disableConnHeartbeat(Url url);

		/// <summary>
		/// enable connection reconnect switch on
		/// <para>
		/// Notice: This api should be called before <seealso cref="RpcClient#init()"/>
		/// </para>
		/// </summary>
		void enableReconnectSwitch();

		/// <summary>
		/// disable connection reconnect switch off
		/// <para>
		/// Notice: This api should be called before <seealso cref="RpcClient#init()"/>
		/// </para>
		/// </summary>
		void disableReconnectSwith();

		/// <summary>
		/// is reconnect switch on
		/// </summary>
		bool ReconnectSwitchOn {get;}

		/// <summary>
		/// enable connection monitor switch on
		/// </summary>
		void enableConnectionMonitorSwitch();

		/// <summary>
		/// disable connection monitor switch off
		/// <para>
		/// Notice: This api should be called before <seealso cref="RpcClient#init()"/>
		/// </para>
		/// </summary>
		void disableConnectionMonitorSwitch();

		/// <summary>
		/// is connection monitor switch on
		/// </summary>
		bool ConnectionMonitorSwitchOn {get;}

		/// <summary>
		/// Getter method for property <tt>connectionManager</tt>.
		/// </summary>
		/// <returns> property value of connectionManager </returns>
		DefaultConnectionManager ConnectionManager {get;}

		/// <summary>
		/// Getter method for property <tt>addressParser</tt>.
		/// </summary>
		/// <returns> property value of addressParser </returns>
		RemotingAddressParser AddressParser {get;set;}


        /// <summary>
        /// Setter method for property <tt>monitorStrategy<tt>.
        /// </summary>
        /// <param name="monitorStrategy"> value to be assigned to property monitorStrategy </param>
        ConnectionMonitorStrategy MonitorStrategy { set; }
    }

}