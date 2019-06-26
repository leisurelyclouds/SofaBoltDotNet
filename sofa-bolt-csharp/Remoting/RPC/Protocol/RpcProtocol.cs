namespace Remoting.rpc.protocol
{
	/// <summary>
	/// Request command protocol for v1
	/// 0     1     2           4           6           8          10           12          14         16
	/// +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
	/// |proto| type| cmdcode   |ver2 |   requestId           |codec|        timeout        |  classLen |
	/// +-----------+-----------+-----------+-----------+-----------+-----------+-----------+-----------+
	/// |headerLen  | contentLen            |                             ... ...                       |
	/// +-----------+-----------+-----------+                                                                                               +
	/// |               className + header  + content  bytes                                            |
	/// +                                                                                               +
	/// |                               ... ...                                                         |
	/// +-----------------------------------------------------------------------------------------------+
	/// 
	/// proto: code for protocol
	/// type: request/response/request oneway
	/// cmdcode: code for remoting command
	/// ver2:version for remoting command
	/// requestId: id of request
	/// codec: code for codec
	/// headerLen: length of header
	/// contentLen: length of content
	/// 
	/// Response command protocol for v1
	/// 0     1     2     3     4           6           8          10           12          14         16
	/// +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
	/// |proto| type| cmdcode   |ver2 |   requestId           |codec|respstatus |  classLen |headerLen  |
	/// +-----------+-----------+-----------+-----------+-----------+-----------+-----------+-----------+
	/// | contentLen            |                  ... ...                                              |
	/// +-----------------------+                                                                       +
	/// |                         className + header  + content  bytes                                  |
	/// +                                                                                               +
	/// |                               ... ...                                                         |
	/// +-----------------------------------------------------------------------------------------------+
	/// respstatus: response status
	/// </summary>
	public class RpcProtocol : Protocol
	{
		public static readonly byte PROTOCOL_CODE = (byte) 1;
		private const int REQUEST_HEADER_LEN = 22;
		private const int RESPONSE_HEADER_LEN = 20;
		private CommandEncoder encoder;
		private CommandDecoder decoder;
		private HeartbeatTrigger heartbeatTrigger;
		private CommandHandler commandHandler;
		private CommandFactory commandFactory;

		public RpcProtocol()
		{
			encoder = new RpcCommandEncoder();
			decoder = new RpcCommandDecoder();
			commandFactory = new RpcCommandFactory();
			heartbeatTrigger = new RpcHeartbeatTrigger(commandFactory);
			commandHandler = new RpcCommandHandler(commandFactory);
		}

		/// <summary>
		/// Get the length of request header.
		/// </summary>
		public static int RequestHeaderLength
		{
			get
			{
				return REQUEST_HEADER_LEN;
			}
		}

		/// <summary>
		/// Get the length of response header.
		/// </summary>
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