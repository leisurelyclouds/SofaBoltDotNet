using java.util.concurrent;

namespace Remoting
{
	/// <summary>
	/// Remoting processor processes remoting commands.
	/// </summary>
	public interface RemotingProcessor
	{
		/// <summary>
		/// Process the remoting command.
		/// </summary>
		/// <param name="ctx"> </param>
		/// <param name="msg"> </param>
		/// <param name="defaultExecutor"> </param>
		/// <exception cref="Exception"> </exception>
		void process(RemotingContext ctx, RemotingCommand msg, ExecutorService defaultExecutor);

		/// <summary>
		/// Get the executor.
		/// </summary>
		ExecutorService Executor {get;set;}
	}
}