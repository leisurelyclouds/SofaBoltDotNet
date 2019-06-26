namespace Remoting
{
    /// <summary>
    /// basic info for biz
    /// </summary>
    public interface BizContext
	{
        /// <summary>
        /// get remote address
        /// </summary>
        /// <returns> remote address </returns>
        string RemoteAddress {get;}

		/// <summary>
		/// get remote host ip
		/// </summary>
		/// <returns> remote host </returns>
		string RemoteHost {get;}

		/// <summary>
		/// get remote port
		/// </summary>
		/// <returns> remote port </returns>
		int RemotePort {get;}

		/// <summary>
		/// get the connection of this request
		/// </summary>
		/// <returns> connection </returns>
		Connection Connection {get;}

		/// <summary>
		/// check whether request already timeout
		/// </summary>
		/// <returns> true if already timeout, you can log some useful info and then discard this request. </returns>
		bool RequestTimeout {get;}

		/// <summary>
		/// get the timeout value from rpc client.
		/// </summary>
		/// <returns> client timeout </returns>
		int ClientTimeout {get;}

		/// <summary>
		/// get the arrive time stamp
		/// </summary>
		/// <returns> the arrive time stamp </returns>
		long ArriveTimestamp {get;}

		/// <summary>
		/// put a key and value
		/// </summary>
		void put(string key, string value);

		/// <summary>
		/// get value
		/// </summary>
		/// <param name="key"> target key </param>
		/// <returns> value </returns>
		string get(string key);

		/// <summary>
		/// get invoke context.
		/// </summary>
		/// <returns> InvokeContext </returns>
		InvokeContext InvokeContext {get;}
	}
}