
namespace Remoting.rpc.protocol
{
	/// <summary>
	/// Request command protocol for v2
	/// 0     1     2           4           6           8          10     11     12          14         16
	/// +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+------+-----+-----+-----+-----+
	/// |proto| ver1|type | cmdcode   |ver2 |   requestId           |codec|switch|   timeout             |
	/// +-----------+-----------+-----------+-----------+-----------+------------+-----------+-----------+
	/// |classLen   |headerLen  |contentLen             |           ...                                  |
	/// +-----------+-----------+-----------+-----------+                                                +
	/// |               className + header  + content  bytes                                             |
	/// +                                                                                                +
	/// |                               ... ...                                  | CRC32(optional)       |
	/// +------------------------------------------------------------------------------------------------+
	/// 
	/// proto: code for protocol
	/// ver1: version for protocol
	/// type: request/response/request oneway
	/// cmdcode: code for remoting command
	/// ver2:version for remoting command
	/// requestId: id of request
	/// codec: code for codec
	/// switch: function switch for protocol
	/// headerLen: length of header
	/// contentLen: length of content
	/// CRC32: CRC32 of the frame(Exists when ver1 > 1)
	/// 
	/// Response command protocol for v2
	/// 0     1     2     3     4           6           8          10     11    12          14          16
	/// +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+------+-----+-----+-----+-----+
	/// |proto| ver1| type| cmdcode   |ver2 |   requestId           |codec|switch|respstatus |  classLen |
	/// +-----------+-----------+-----------+-----------+-----------+------------+-----------+-----------+
	/// |headerLen  | contentLen            |                      ...                                   |
	/// +-----------------------------------+                                                            +
	/// |               className + header  + content  bytes                                             |
	/// +                                                                                                +
	/// |                               ... ...                                  | CRC32(optional)       |
	/// +------------------------------------------------------------------------------------------------+
	/// respstatus: response status
	/// </summary>
	public class RpcProtocolV2 : Protocol
	{
		/* because the design defect, the version is neglected in RpcProtocol, so we design RpcProtocolV2 and add protocol version. */
		public static readonly byte PROTOCOL_CODE = (byte) 2;
		/// <summary>
		/// version 1, is the same with RpcProtocol
		/// </summary>
		public static readonly byte PROTOCOL_VERSION_1 = (byte) 1;
		/// <summary>
		/// version 2, is the protocol version for RpcProtocolV2
		/// </summary>
		public static readonly byte PROTOCOL_VERSION_2 = (byte) 2;

		/// <summary>
		/// in contrast to protocol v1,
		/// one more byte is used as protocol version,
		/// and another one is userd as protocol switch
		/// </summary>
		private const int REQUEST_HEADER_LEN = 22 + 2;
		private const int RESPONSE_HEADER_LEN = 20 + 2;
		private CommandEncoder encoder;
		private CommandDecoder decoder;
		private HeartbeatTrigger heartbeatTrigger;
		private CommandHandler commandHandler;
		private CommandFactory commandFactory;

		public RpcProtocolV2()
		{
			encoder = new RpcCommandEncoderV2();
			decoder = new RpcCommandDecoderV2();
			commandFactory = new RpcCommandFactory();
			heartbeatTrigger = new RpcHeartbeatTrigger(commandFactory);
			commandHandler = new RpcCommandHandler(commandFactory);
		}

		public static int RequestHeaderLength
		{
			get
			{
				return REQUEST_HEADER_LEN;
			}
		}

		public static int ResponseHeaderLength
		{
			get
			{
				return RESPONSE_HEADER_LEN;
			}
		}

		public virtual CommandEncoder Encoder
		{
			get
			{
				return encoder;
			}
		}

		public virtual CommandDecoder Decoder
		{
			get
			{
				return decoder;
			}
		}

		public virtual HeartbeatTrigger HeartbeatTrigger
		{
			get
			{
				return heartbeatTrigger;
			}
		}

		public virtual CommandHandler CommandHandler
		{
			get
			{
				return commandHandler;
			}
		}

		public virtual CommandFactory CommandFactory
		{
			get
			{
				return commandFactory;
			}
		}
	}

}