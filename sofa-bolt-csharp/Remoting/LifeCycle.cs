namespace Remoting
{
	public interface LifeCycle
	{
		void startup();
		void shutdown();
		bool Started {get;}
	}
}