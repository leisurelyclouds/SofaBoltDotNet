using Remoting;
using Remoting.rpc;
using java.util.concurrent.atomic;
using Remoting.rpc.protocol;
using System.Text;
using java.io;

namespace com.alipay.remoting.rpc.serializer
{
    /// <summary>
    /// a custom serialize demo
    /// </summary>
    public class NormalStringCustomSerializer : DefaultCustomSerializer
	{

		private AtomicBoolean serialFlag = new AtomicBoolean();
		private AtomicBoolean deserialFlag = new AtomicBoolean();

		private byte contentSerializer = 255;
		private byte contentDeserialier = 255;

		/// <seealso cref= CustomSerializer#serializeContent(ResponseCommand) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public <T extends com.alipay.remoting.rpc.ResponseCommand> boolean serializeContent(T response) throws com.alipay.remoting.exception.SerializationException
		public override bool serializeContent(ResponseCommand response)
		{
			serialFlag.set(true);
			RpcResponseCommand rpcResp = (RpcResponseCommand) response;
			string str = (string) rpcResp.ResponseObject;
			try
			{
				rpcResp.Content = Encoding.UTF8.GetBytes(str);
			}
			catch (UnsupportedEncodingException e)
			{
				System.Console.WriteLine(e.ToString());
				System.Console.Write(e.StackTrace);
			}
			contentSerializer = response.Serializer;
			return true;
		}

		/// <seealso cref= CustomSerializer#deserializeContent(ResponseCommand, InvokeContext) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public <T extends com.alipay.remoting.rpc.ResponseCommand> boolean deserializeContent(T response, com.alipay.remoting.InvokeContext invokeContext) throws com.alipay.remoting.exception.DeserializationException
		public override bool deserializeContent(ResponseCommand response, InvokeContext invokeContext)
		{
			deserialFlag.set(true);
			RpcResponseCommand rpcResp = (RpcResponseCommand) response;
			try
			{
				rpcResp.ResponseObject = Encoding.UTF8.GetString(rpcResp.Content)+ "RANDOM";
			}
			catch (UnsupportedEncodingException e)
			{
				System.Console.WriteLine(e.ToString());
				System.Console.Write(e.StackTrace);
			}
			contentDeserialier = response.Serializer;
			return true;
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

		public virtual byte ContentSerializer
		{
			get
			{
				return contentSerializer;
			}
		}

		public virtual byte ContentDeserialier
		{
			get
			{
				return contentDeserialier;
			}
		}

		public virtual void reset()
		{
			this.contentDeserialier = 255;
			this.contentDeserialier = 255;
			this.deserialFlag.set(false);
			this.serialFlag.set(false);
		}
	}

}