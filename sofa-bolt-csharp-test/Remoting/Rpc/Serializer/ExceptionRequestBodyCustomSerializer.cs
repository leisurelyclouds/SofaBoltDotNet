using Remoting;
using Remoting.rpc;
using Remoting.exception;
using java.util.concurrent.atomic;

namespace com.alipay.remoting.rpc.serializer
{
    /// <summary>
    /// a request body serializer throw exception
    /// </summary>
    public class ExceptionRequestBodyCustomSerializer : DefaultCustomSerializer
	{

		private AtomicBoolean serialFlag = new AtomicBoolean();
		private AtomicBoolean deserialFlag = new AtomicBoolean();
		private bool serialException = false;
		private bool serialRuntimeException = true;
		private bool deserialException = false;
		private bool deserialRuntimeException = true;

		public ExceptionRequestBodyCustomSerializer(bool serialRuntimeException, bool deserialRuntimeException)
		{
			this.serialRuntimeException = serialRuntimeException;
			this.deserialRuntimeException = deserialRuntimeException;
		}

		public ExceptionRequestBodyCustomSerializer(bool serialException, bool serialRuntimeException, bool deserialException, bool deserialRuntimeException)
		{
			this.serialException = serialException;
			this.serialRuntimeException = serialRuntimeException;
			this.deserialException = deserialException;
			this.deserialRuntimeException = deserialRuntimeException;
		}

		/// <seealso cref= CustomSerializer#serializeContent(RequestCommand, InvokeContext) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public <T extends com.alipay.remoting.rpc.RequestCommand> boolean serializeContent(T req, com.alipay.remoting.InvokeContext invokeContext) throws com.alipay.remoting.exception.SerializationException
		public override bool serializeContent(RequestCommand req, InvokeContext invokeContext)
		{
			serialFlag.set(true);
			if (serialRuntimeException)
			{
				throw new System.Exception("serialRuntimeException in ExceptionRequestBodyCustomSerializer!");
			}
			else if (serialException)
			{
				throw new SerializationException("serialException in ExceptionRequestBodyCustomSerializer!");
			}
			else
			{
				return false; // use default codec
			}
		}

		/// <seealso cref= CustomSerializer#deserializeContent(RequestCommand) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public <T extends com.alipay.remoting.rpc.RequestCommand> boolean deserializeContent(T req) throws com.alipay.remoting.exception.DeserializationException
		public override bool deserializeContent(RequestCommand req)
		{
			deserialFlag.set(true);
			if (deserialRuntimeException)
			{
				throw new System.Exception("deserialRuntimeException in ExceptionRequestBodyCustomSerializer!");
			}
			else if (deserialException)
			{
				throw new DeserializationException("deserialException in ExceptionRequestBodyCustomSerializer!");
			}
			else
			{
				return false; // use default codec
			}
		}

		public virtual bool Serialized
		{
			get
			{
				return this.serialFlag.get();
			}
		}

		public virtual bool Deserialized
		{
			get
			{
				return this.deserialFlag.get();
			}
		}
	}

}