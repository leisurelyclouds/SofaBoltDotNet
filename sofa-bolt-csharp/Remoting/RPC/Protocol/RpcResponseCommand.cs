using Remoting.Config;
using Remoting.exception;
using System;
using System.Text;

namespace Remoting.rpc.protocol
{
    /// <summary>
    /// Response command for Rpc.
    /// 
    /// @author jiangping
    /// @version $Id: RpcResponseCommand.java, v 0.1 2015-9-25 PM2:15:41 tao Exp $
    /// </summary>
    [Serializable]
	public class RpcResponseCommand : ResponseCommand
	{
		/// <summary>
		/// For serialization
		/// </summary>
		private const long serialVersionUID = 5667111367880018776L;
		private object responseObject;

		private Type responseClass;

		private CustomSerializer customSerializer;
		private object responseHeader;

		private string errorMsg;

		public RpcResponseCommand() : base(RpcCommandCode.RPC_RESPONSE)
		{
		}

		public RpcResponseCommand(object response) : base(RpcCommandCode.RPC_RESPONSE)
		{
			responseObject = response;
		}

		public RpcResponseCommand(int id, object response) : base(RpcCommandCode.RPC_RESPONSE, id)
		{
			responseObject = response;
		}

		/// <summary>
		/// Getter method for property <tt>responseObject</tt>.
		/// </summary>
		/// <returns> property value of responseObject </returns>
		public virtual object ResponseObject
		{
			get
			{
				return responseObject;
			}
			set
			{
				responseObject = value;
			}
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void serializeClazz() throws exception.SerializationException
		public override void serializeClazz()
		{
			if (!ReferenceEquals(ResponseClass, null))
			{
				try
				{
                    byte[] clz = Encoding.GetEncoding(Configs.DEFAULT_CHARSET).GetBytes(ResponseClass.AssemblyQualifiedName.ToString());
                    Clazz = clz;
				}
				catch (NotSupportedException e)
				{
					throw new SerializationException("Unsupported charset: " + Configs.DEFAULT_CHARSET, e);
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void deserializeClazz() throws exception.DeserializationException
		public override void deserializeClazz()
		{
			if (Clazz != null && ReferenceEquals(ResponseClass, null))
			{
				try
				{
                    ResponseClass = System.Type.GetType(Encoding.GetEncoding(Configs.DEFAULT_CHARSET).GetString((byte[])(object)Clazz));
				}
				catch (NotSupportedException e)
				{
					throw new DeserializationException("Unsupported charset: " + Configs.DEFAULT_CHARSET, e);
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void serializeContent(InvokeContext invokeContext) throws exception.SerializationException
		public override void serializeContent(InvokeContext invokeContext)
		{
			if (ResponseObject != null)
			{
				try
				{
					if (CustomSerializer != null && CustomSerializer.serializeContent(this))
					{
						return;
					}

                    Content = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(responseObject));
                }
				catch (SerializationException)
                {
					throw;
				}
				catch (Exception e)
				{
					throw new SerializationException("Exception caught when serialize content of rpc response command!", e);
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void deserializeContent(InvokeContext invokeContext) throws exception.DeserializationException
		public override void deserializeContent(InvokeContext invokeContext)
		{
			if (ResponseObject == null)
			{
				try
				{
					if (CustomSerializer != null && CustomSerializer.deserializeContent(this, invokeContext))
					{
						return;
					}
					if (Content != null)
					{
						ResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject(Encoding.UTF8.GetString(Content), responseClass);
					}
				}
				catch (DeserializationException)
                {
					throw;
				}
				catch (Exception e)
				{
					throw new DeserializationException("Exception caught when deserialize content of rpc response command!", e);
				}
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void serializeHeader(InvokeContext invokeContext) throws exception.SerializationException
		public override void serializeHeader(InvokeContext invokeContext)
		{
			if (CustomSerializer != null)
			{
				try
				{
					CustomSerializer.serializeHeader(this);
				}
				catch (SerializationException)
                {
					throw;
				}
				catch (Exception e)
				{
					throw new SerializationException("Exception caught when serialize header of rpc response command!", e);
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void deserializeHeader(InvokeContext invokeContext) throws exception.DeserializationException
		public override void deserializeHeader(InvokeContext invokeContext)
		{
			if (Header != null && ResponseHeader == null)
			{
				if (CustomSerializer != null)
				{
					try
					{
						CustomSerializer.deserializeHeader(this, invokeContext);
					}
					catch (DeserializationException)
                    {
						throw;
					}
					catch (Exception e)
					{
						throw new DeserializationException("Exception caught when deserialize header of rpc response command!", e);
					}
				}
			}
		}

		/// <summary>
		/// Getter method for property <tt>responseClass</tt>.
		/// </summary>
		/// <returns> property value of responseClass </returns>
		public virtual Type ResponseClass
		{
			get
			{
				return responseClass;
			}
			set
			{
				responseClass = value;
			}
		}


		/// <summary>
		/// Getter method for property <tt>responseHeader</tt>.
		/// </summary>
		/// <returns> property value of responseHeader </returns>
		public virtual object ResponseHeader
		{
			get
			{
				return responseHeader;
			}
			set
			{
				responseHeader = value;
			}
		}


		/// <summary>
		/// Getter method for property <tt>customSerializer</tt>.
		/// </summary>
		/// <returns> property value of customSerializer </returns>
		public virtual CustomSerializer CustomSerializer
		{
			get
			{
				if (customSerializer != null)
				{
					return customSerializer;
				}
				if (!ReferenceEquals(responseClass, null))
				{
					customSerializer = CustomSerializerManager.getCustomSerializer(responseClass);
				}
				if (customSerializer == null)
				{
					customSerializer = CustomSerializerManager.getCustomSerializer(CmdCode);
				}
				return customSerializer;
			}
		}

		/// <summary>
		/// Getter method for property <tt>errorMsg</tt>.
		/// </summary>
		/// <returns> property value of errorMsg </returns>
		public virtual string ErrorMsg
		{
			get
			{
				return errorMsg;
			}
			set
			{
				errorMsg = value;
			}
		}

	}

}