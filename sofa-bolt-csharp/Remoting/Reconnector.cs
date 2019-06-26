namespace Remoting
{
	/// <summary>
	/// Reconnect manager interface.
	/// </summary>
	public interface Reconnector : LifeCycle
	{

		/// <summary>
		/// Do reconnecting in async mode.
		/// </summary>
		/// <param name="url"> target url </param>
		void reconnect(Url url);

		/// <summary>
		/// Disable reconnect to the target url.
		/// </summary>
		/// <param name="url"> target url </param>
		void disableReconnect(Url url);

		/// <summary>
		/// Enable reconnect to the target url.
		/// </summary>
		/// <param name="url"> target url </param>
		void enableReconnect(Url url);
	}

}