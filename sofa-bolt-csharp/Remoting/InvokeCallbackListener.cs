namespace Remoting
{

	/// <summary>
	/// Listener to listen response and invoke callback.
	/// </summary>
	public interface InvokeCallbackListener
	{
		/// <summary>
		/// Response arrived.
		/// </summary>
		/// <param name="future"> </param>
		void onResponse(InvokeFuture future);

		/// <summary>
		/// Get the remote address.
		/// </summary>
		string RemoteAddress {get;}
	}

}