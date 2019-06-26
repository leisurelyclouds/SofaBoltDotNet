using Remoting.Config;
using Remoting.Config.switches;
using Remoting.rpc.protocol;
using System;


namespace Remoting.rpc
{
    /// <summary>
    /// Remoting command. <br>
    /// A remoting command stands for a kind of transfer object in the network communication layer.
    /// </summary>
    [Serializable]
	public abstract class RpcCommand : RemotingCommand
	{

		/// <summary>
		/// For serialization
		/// </summary>
		private const long serialVersionUID = -3570261012462596503L;

		/// <summary>
		/// Code which stands for the command.
		/// </summary>
		private CommandCode cmdCode;
		/* command version */
		private byte version = 0x1;
		private byte type;
		/// <summary>
		/// Serializer, see the Configs.SERIALIZER_DEFAULT for the default serializer.
		/// Notice: this can not be changed after initialized at runtime.
		/// </summary>
		private byte serializer = ConfigManager.serializer_Renamed;
		/// <summary>
		/// protocol switches
		/// </summary>
		private ProtocolSwitch protocolSwitch = new ProtocolSwitch();
		private int id;
		/// <summary>
		/// The length of clazz
		/// </summary>
		private short clazzLength = 0;
		private short headerLength = 0;
		private int contentLength = 0;
		/// <summary>
		/// The class of content
		/// </summary>
		private byte[] clazz;
		/// <summary>
		/// Header is used for transparent transmission.
		/// </summary>
		private byte[] header;
		/// <summary>
		/// The bytes format of the content of the command.
		/// </summary>
		private byte[] content;
		/// <summary>
		/// invoke context of each rpc command.
		/// </summary>
		private InvokeContext invokeContext;

		public RpcCommand()
		{
		}

		public RpcCommand(byte type) : this()
		{
			this.type = type;
		}

		public RpcCommand(CommandCode cmdCode) : this()
		{
			this.cmdCode = cmdCode;
		}

		public RpcCommand(byte type, CommandCode cmdCode) : this(cmdCode)
		{
			this.type = type;
		}

		public RpcCommand(byte version, byte type, CommandCode cmdCode) : this(type, cmdCode)
		{
			this.version = version;
		}

		/// <summary>
		/// Serialize  the class header and content.
		/// </summary>
		/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void serialize() throws exception.SerializationException
		public virtual void serialize()
		{
			serializeClazz();
			serializeHeader(invokeContext);
			serializeContent(invokeContext);
		}

		/// <summary>
		/// Deserialize the class header and content.
		/// </summary>
		/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void deserialize() throws exception.DeserializationException
		public virtual void deserialize()
		{
			deserializeClazz();
			deserializeHeader(invokeContext);
			deserializeContent(invokeContext);
		}

		/// <summary>
		/// Deserialize according to mask.
		/// <ol>
		///     <li>If mask <= <seealso cref="RpcDeserializeLevel#DESERIALIZE_CLAZZ"/>, only deserialize clazz - only one part.</li>
		///     <li>If mask <= <seealso cref="RpcDeserializeLevel#DESERIALIZE_HEADER"/>, deserialize clazz and header - two parts.</li>
		///     <li>If mask <= <seealso cref="RpcDeserializeLevel#DESERIALIZE_ALL"/>, deserialize clazz, header and content - all three parts.</li>
		/// </ol>
		/// </summary>
		/// <param name="mask"> </param>
		/// <exception cref="CodecException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void deserialize(long mask) throws exception.DeserializationException
		public virtual void deserialize(long mask)
		{
			if (mask <= RpcDeserializeLevel.DESERIALIZE_CLAZZ)
			{
				deserializeClazz();
			}
			else if (mask <= RpcDeserializeLevel.DESERIALIZE_HEADER)
			{
				deserializeClazz();
				deserializeHeader(InvokeContext);
			}
			else if (mask <= RpcDeserializeLevel.DESERIALIZE_ALL)
			{
				deserialize();
			}
		}

		/// <summary>
		/// Serialize content class.
		/// </summary>
		/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void serializeClazz() throws exception.SerializationException
		public virtual void serializeClazz()
		{

		}

		/// <summary>
		/// Deserialize the content class.
		/// </summary>
		/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void deserializeClazz() throws exception.DeserializationException
		public virtual void deserializeClazz()
		{

		}

		/// <summary>
		/// Serialize the header.
		/// </summary>
		/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void serializeHeader(InvokeContext invokeContext) throws exception.SerializationException
		public virtual void serializeHeader(InvokeContext invokeContext)
		{
		}

		/// <summary>
		/// Serialize the content.
		/// </summary>
		/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void serializeContent(InvokeContext invokeContext) throws exception.SerializationException
		public virtual void serializeContent(InvokeContext invokeContext)
		{
		}

		/// <summary>
		/// Deserialize the header.
		/// </summary>
		/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void deserializeHeader(InvokeContext invokeContext) throws exception.DeserializationException
		public virtual void deserializeHeader(InvokeContext invokeContext)
		{
		}

		/// <summary>
		/// Deserialize the content.
		/// </summary>
		/// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void deserializeContent(InvokeContext invokeContext) throws exception.DeserializationException
		public virtual void deserializeContent(InvokeContext invokeContext)
		{
		}

		public virtual ProtocolCode ProtocolCode
		{
			get
			{
				return ProtocolCode.fromBytes(RpcProtocol.PROTOCOL_CODE);
			}
		}

		public virtual CommandCode CmdCode
		{
			get
			{
				return cmdCode;
			}
			set
			{
				cmdCode = value;
			}
		}

		public virtual InvokeContext InvokeContext
		{
			get
			{
				return invokeContext;
			}
			set
			{
				invokeContext = value;
			}
		}

		public virtual byte Serializer
		{
			get
			{
				return serializer;
			}
			set
			{
				serializer = value;
			}
		}

		public virtual ProtocolSwitch ProtocolSwitch
		{
			get
			{
				return protocolSwitch;
			}
			set
			{
				protocolSwitch = value;
			}
		}


		public virtual byte Version
		{
			get
			{
				return version;
			}
			set
			{
				version = value;
			}
		}


		public virtual byte Type
		{
			get
			{
				return type;
			}
			set
			{
				type = value;
			}
		}




		public virtual int Id
		{
			get
			{
				return id;
			}
			set
			{
				id = value;
			}
		}


		public virtual byte[] Header
		{
			get
			{
				return header;
			}
			set
			{
				if (value != null)
				{
					header = value;
					headerLength = (short) value.Length;
				}
			}
		}


		public virtual byte[] Content
		{
			get
			{
				return content;
			}
			set
			{
				if (value != null)
				{
					content = value;
					contentLength = value.Length;
				}
			}
		}


		public virtual short HeaderLength
		{
			get
			{
				return headerLength;
			}
		}

		public virtual int ContentLength
		{
			get
			{
				return contentLength;
			}
		}

		public virtual short ClazzLength
		{
			get
			{
				return clazzLength;
			}
		}

		public virtual byte[] Clazz
		{
			get
			{
				return clazz;
			}
			set
			{
				if (value != null)
				{
					clazz = value;
					clazzLength = (short) value.Length;
				}
			}
		}


	}

}