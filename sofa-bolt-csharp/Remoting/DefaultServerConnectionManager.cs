namespace Remoting
{
	/// <summary>
	/// Do some preparatory work in order to refactor the ConnectionManager in the next version.
	/// </summary>
	public class DefaultServerConnectionManager : DefaultConnectionManager, ServerConnectionManager
	{
		public DefaultServerConnectionManager(ConnectionSelectStrategy connectionSelectStrategy) : base(connectionSelectStrategy)
		{
		}
	}
}