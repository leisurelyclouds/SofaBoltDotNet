using Remoting;
using Remoting.rpc;
using Remoting.exception;
using java.util.concurrent.atomic;

namespace com.alipay.remoting.rpc.serializer
{
    /// <summary>
    /// a String serializer throw exception
    /// </summary>
    public class ExceptionStringCustomSerializer : DefaultCustomSerializer
	{

		private AtomicBoolean serialFlag = new AtomicBoolean();
		private AtomicBoolean deserialFlag = new AtomicBoolean();
		private bool serialException = false;
		private bool serialRuntimeException = true;
		private bool deserialException = false;
		private bool deserialRuntimeException = true;

		public ExceptionStringCustomSerializer(bool serialException, bool deserialException)
		{
			this.serialException = serialException;
			this.deserialException = deserialException;
		}

		public ExceptionStringCustomSerializer(bool serialException, bool serialRuntimeException, bool deserialException, bool deserialRuntimeException)
		{
			this.serialException = serialException;
			this.serialRuntimeException = serialRuntimeException;
			this.deserialException = deserialException;
			this.deserialRuntimeException = deserialRuntimeException;
		}

		/// <seealso cref= CustomSerializer#serializeContent(ResponseCommand) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public <T extends com.alipay.remoting.rpc.ResponseCommand> boolean serializeContent(T response) throws com.alipay.remoting.exception.SerializationException
		public override bool serializeContent(ResponseCommand response)
		{
			serialFlag.set(true);
			if (serialRuntimeException)
			{
				throw new System.Exception("serialRuntimeException in ExceptionStringCustomSerializer!");
			}
			else if (serialException)
			{
				throw new SerializationException("serialException in ExceptionStringCustomSerializer!");
			}
			else
			{
				return false; // use default codec
			}
		}

		/// <seealso cref= CustomSerializer#deserializeContent(ResponseCommand, InvokeContext) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public <T extends com.alipay.remoting.rpc.ResponseCommand> boolean deserializeContent(T response, com.alipay.remoting.InvokeContext invokeContext) throws com.alipay.remoting.exception.DeserializationException
		public override bool deserializeContent(ResponseCommand response, InvokeContext invokeContext)
		{
			deserialFlag.set(true);
			if (deserialRuntimeException)
			{
				throw new System.Exception("deserialRuntimeException in ExceptionStringCustomSerializer!");
			}
			else if (deserialException)
			{
				throw new DeserializationException("deserialException in ExceptionStringCustomSerializer!");
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