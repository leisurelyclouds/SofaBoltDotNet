using Remoting.Config.switches;

namespace Remoting
{
	/// <summary>
	/// Remoting command.
	/// </summary>
	public interface RemotingCommand
	{
		/// <summary>
		/// Get the code of the protocol that this command belongs to
		/// </summary>
		/// <returns> protocol code </returns>
		ProtocolCode ProtocolCode {get;}

		/// <summary>
		/// Get the command code for this command
		/// </summary>
		/// <returns> command code </returns>
		CommandCode CmdCode {get;}

		/// <summary>
		/// Get the id of the command
		/// </summary>
		/// <returns> an int value represent the command id </returns>
		int Id {get;}

		/// <summary>
		/// Get invoke context for this command
		/// </summary>
		/// <returns> context </returns>
		InvokeContext InvokeContext {get;}

		/// <summary>
		/// Get serializer type for this command
		/// 
		/// @return
		/// </summary>
		byte Serializer {get;}

		/// <summary>
		/// Get the protocol switch status for this command
		/// 
		/// @return
		/// </summary>
		ProtocolSwitch ProtocolSwitch {get;}

		/// <summary>
		/// Serialize all parts of remoting command
		/// </summary>
		/// <exception cref="SerializationException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void serialize() throws exception.SerializationException;
		void serialize();

		/// <summary>
		/// Deserialize all parts of remoting command
		/// </summary>
		/// <exception cref="DeserializationException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void deserialize() throws exception.DeserializationException;
		void deserialize();

		/// <summary>
		/// Serialize content of remoting command
		/// </summary>
		/// <param name="invokeContext"> </param>
		/// <exception cref="SerializationException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void serializeContent(InvokeContext invokeContext) throws exception.SerializationException;
		void serializeContent(InvokeContext invokeContext);

		/// <summary>
		/// Deserialize content of remoting command
		/// </summary>
		/// <param name="invokeContext"> </param>
		/// <exception cref="DeserializationException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void deserializeContent(InvokeContext invokeContext) throws exception.DeserializationException;
		void deserializeContent(InvokeContext invokeContext);
	}

}