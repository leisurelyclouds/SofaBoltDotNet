using java.util.concurrent;

namespace Remoting
{
	/// <summary>
	/// Command handler.
	/// </summary>
	public interface CommandHandler
	{
		/// <summary>
		/// Handle the command.
		/// </summary>
		/// <param name="ctx"> </param>
		/// <param name="msg"> </param>
		/// <exception cref="Exception"> </exception>
		void handleCommand(RemotingContext ctx, object msg);

		/// <summary>
		/// Register processor for command with specified code.
		/// </summary>
		/// <param name="cmd"> </param>
		/// <param name="processor"> </param>
		void registerProcessor(CommandCode cmd, RemotingProcessor processor);

		/// <summary>
		/// Register default executor for the handler.
		/// </summary>
		/// <param name="executor"> </param>
		void registerDefaultExecutor(ExecutorService executor);

		/// <summary>
		/// Get default executor for the handler.
		/// </summary>
		ExecutorService DefaultExecutor {get;}
	}
}