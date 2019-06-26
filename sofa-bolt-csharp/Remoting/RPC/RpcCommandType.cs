namespace Remoting.rpc
{
	/// <summary>
	/// The type of command in the request/response model.
	/// </summary>
	public class RpcCommandType
	{
		/// <summary>
		/// rpc response
		/// </summary>
		public static readonly byte RESPONSE = (byte) 0x00;
		/// <summary>
		/// rpc request
		/// </summary>
		public static readonly byte REQUEST = (byte) 0x01;
		/// <summary>
		/// rpc oneway request
		/// </summary>
		public static readonly byte REQUEST_ONEWAY = (byte) 0x02;
	}
}