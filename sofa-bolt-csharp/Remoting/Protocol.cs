namespace Remoting
{
	/// <summary>
	/// A protocol contains a group of commands.
	/// </summary>
	public interface Protocol
	{
		/// <summary>
		/// Get the newEncoder for the protocol.
		/// 
		/// @return
		/// </summary>
		CommandEncoder Encoder {get;}

		/// <summary>
		/// Get the decoder for the protocol.
		/// 
		/// @return
		/// </summary>
		CommandDecoder Decoder {get;}

		/// <summary>
		/// Get the heartbeat trigger for the protocol.
		/// 
		/// @return
		/// </summary>
		HeartbeatTrigger HeartbeatTrigger {get;}

		/// <summary>
		/// Get the command handler for the protocol.
		/// </summary>
		CommandHandler CommandHandler {get;}

		/// <summary>
		/// Get the command factory for the protocol.
		/// @return
		/// </summary>
		CommandFactory CommandFactory {get;}
	}
}