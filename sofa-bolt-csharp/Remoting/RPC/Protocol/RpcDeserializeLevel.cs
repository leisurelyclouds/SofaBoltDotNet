
namespace Remoting.rpc.protocol
{

	/// <summary>
	/// Rpc deserialize level.
	/// 
	/// @author tsui
	/// @version $Id: RpcDeserializeLevel.java, v 0.1 2017-04-24 15:12 tsui Exp $
	/// </summary>
	public class RpcDeserializeLevel
	{
		/// <summary>
		/// deserialize clazz, header, contents all three parts of rpc command
		/// </summary>
		public const int DESERIALIZE_ALL = 0x02;
		/// <summary>
		/// deserialize both header and clazz parts of rpc command
		/// </summary>
		public const int DESERIALIZE_HEADER = 0x01;
		/// <summary>
		/// deserialize only the clazz part of rpc command
		/// </summary>
		public const int DESERIALIZE_CLAZZ = 0x00;

		/// <summary>
		/// Convert to String.
		/// </summary>
		public static string valueOf(int value)
		{
			switch (value)
			{
				case 0x00:
					return "DESERIALIZE_CLAZZ";
				case 0x01:
					return "DESERIALIZE_HEADER";
				case 0x02:
					return "DESERIALIZE_ALL";
			}
			throw new System.ArgumentException("Unknown deserialize level value ," + value);
		}
	}
}