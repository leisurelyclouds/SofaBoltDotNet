namespace Remoting
{
	/// <summary>
	/// Connection heart beat manager, operate heart beat whether enabled for a certain connection at runtime
	/// </summary>
	public interface ConnectionHeartbeatManager
	{
		/// <summary>
		/// disable heart beat for a certain connection
		/// </summary>
		/// <param name="connection"> Connection </param>
		void disableHeartbeat(Connection connection);

		/// <summary>
		/// enable heart beat for a certain connection
		/// </summary>
		/// <param name="connection"> Connection </param>
		void enableHeartbeat(Connection connection);
	}

}