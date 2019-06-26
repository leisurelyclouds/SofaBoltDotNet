using Remoting.Config;
using Remoting.exception;
using Remoting.util;
using System;
using System.Text;

namespace Remoting.rpc.protocol
{
    /// <summary>
    /// Request command for Rpc.
    /// </summary>
    [Serializable]
    public class RpcRequestCommand : RequestCommand
    {
        /// <summary>
        /// For serialization
		/// </summary>
        private const long serialVersionUID = -4602613826188210946L;
        private object requestObject;
        private Type requestClass;

        private CustomSerializer customSerializer;
        private object requestHeader;

        [NonSerialized]
        private long arriveTime = -1;

        /// <summary>
        /// create request command without id
        /// </summary>
        public RpcRequestCommand() : base(RpcCommandCode.RPC_REQUEST)
        {
        }

        /// <summary>
        /// create request command with id and request object
		/// </summary>
        /// <param name="request"> request object </param>
        public RpcRequestCommand(object request) : base(RpcCommandCode.RPC_REQUEST)
        {
            requestObject = request;
            Id = IDGenerator.nextId();
        }

        public override void serializeClazz()
        {
            if (!ReferenceEquals(requestClass, null))
            {
                try
                {
                    byte[] clz = Encoding.GetEncoding(Configs.DEFAULT_CHARSET).GetBytes(requestClass.AssemblyQualifiedName.ToString());
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
            if (Clazz != null && ReferenceEquals(RequestClass, null))
            {
                try
                {
                    RequestClass = System.Type.GetType(Encoding.GetEncoding(Configs.DEFAULT_CHARSET).GetString(Clazz));
                }
                catch (NotSupportedException e)
                {
                    throw new DeserializationException("Unsupported charset: " + Configs.DEFAULT_CHARSET, e);
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
                    CustomSerializer.serializeHeader(this, invokeContext);
                }
                catch (SerializationException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new SerializationException("Exception caught when serialize header of rpc request command!", e);
                }
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void deserializeHeader(InvokeContext invokeContext) throws exception.DeserializationException
        public override void deserializeHeader(InvokeContext invokeContext)
        {
            if (Header != null && RequestHeader == null)
            {
                if (CustomSerializer != null)
                {
                    try
                    {
                        CustomSerializer.deserializeHeader(this);
                    }
                    catch (DeserializationException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        throw new DeserializationException("Exception caught when deserialize header of rpc request command!", e);
                    }
                }
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void serializeContent(InvokeContext invokeContext) throws exception.SerializationException
        public override void serializeContent(InvokeContext invokeContext)
        {
            if (requestObject != null)
            {
                try
                {
                    if (CustomSerializer != null && CustomSerializer.serializeContent(this, invokeContext))
                    {
                        return;
                    }

                    Content = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(requestObject));
                }
                catch (SerializationException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new SerializationException("Exception caught when serialize content of rpc request command!", e);
                }
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void deserializeContent(InvokeContext invokeContext) throws exception.DeserializationException
        public override void deserializeContent(InvokeContext invokeContext)
        {
            if (RequestObject == null)
            {
                try
                {
                    if (CustomSerializer != null && CustomSerializer.deserializeContent(this))
                    {
                        return;
                    }
                    if (Content != null)
                    {
                        RequestObject = Newtonsoft.Json.JsonConvert.DeserializeObject(Encoding.UTF8.GetString(Content), requestClass);
                    }
                }
                catch (DeserializationException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new DeserializationException("Exception caught when deserialize content of rpc request command!", e);
                }
            }
        }

        /// <summary>
        /// Getter method for property <tt>requestObject</tt>.
        /// </summary>
        /// <returns> property value of requestObject </returns>
        public virtual object RequestObject
        {
            get
            {
                return requestObject;
            }
            set
            {
                requestObject = value;
            }
        }


        /// <summary>
        /// Getter method for property <tt>requestHeader</tt>.
        /// </summary>
        /// <returns> property value of requestHeader </returns>
        public virtual object RequestHeader
        {
            get
            {
                return requestHeader;
            }
            set
            {
                requestHeader = value;
            }
        }


        /// <summary>
        /// Getter method for property <tt>requestClass</tt>.
        /// </summary>
        /// <returns> property value of requestClass </returns>
        public virtual Type RequestClass
        {
            get
            {
                return requestClass;
            }
            set
            {
                requestClass = value;
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
                if (!ReferenceEquals(requestClass, null))
                {
                    customSerializer = CustomSerializerManager.getCustomSerializer(requestClass);
                }
                if (customSerializer == null)
                {
                    customSerializer = CustomSerializerManager.getCustomSerializer(CmdCode);
                }
                return customSerializer;
            }
        }

        /// <summary>
        /// Getter method for property <tt>arriveTime</tt>.
        /// </summary>
        /// <returns> property value of arriveTime </returns>
        public virtual long ArriveTime
        {
            get
            {
                return arriveTime;
            }
            set
            {
                arriveTime = value;
            }
        }

    }

}