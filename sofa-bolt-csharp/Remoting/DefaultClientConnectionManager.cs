using Remoting.Config.switches;
using Remoting.Connections;

namespace Remoting
{
	/// <summary>
	/// Do some preparatory work in order to refactor the ConnectionManager in the next version.
	/// 
	/// @author chengyi (mark.lx@antfin.com) 2019-03-07 14:27
	/// </summary>
	public class DefaultClientConnectionManager : DefaultConnectionManager, ClientConnectionManager
	{

		public DefaultClientConnectionManager(ConnectionSelectStrategy connectionSelectStrategy, ConnectionFactory connectionFactory, ConnectionEventHandler connectionEventHandler, ConnectionEventListener connectionEventListener)
            : base(connectionSelectStrategy, connectionFactory, connectionEventHandler, connectionEventListener)
		{
		}

		public DefaultClientConnectionManager(ConnectionSelectStrategy connectionSelectStrategy, ConnectionFactory connectionFactory, ConnectionEventHandler connectionEventHandler, ConnectionEventListener connectionEventListener, GlobalSwitch globalSwitch) : base(connectionSelectStrategy, connectionFactory, connectionEventHandler, connectionEventListener, globalSwitch)
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void startup() throws LifeCycleException
		public override void startup()
		{
			base.startup();

			connectionEventHandler.ConnectionManager = this;
			connectionEventHandler.ConnectionEventListener = connectionEventListener;
			connectionFactory.init(connectionEventHandler);
		}

	}

}