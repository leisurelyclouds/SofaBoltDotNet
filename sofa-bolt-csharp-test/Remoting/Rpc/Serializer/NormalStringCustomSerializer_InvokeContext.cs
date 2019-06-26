using Remoting;
using Remoting.rpc;
using java.util.concurrent.atomic;
using Remoting.rpc.protocol;
using java.io;
using System.Text;

namespace com.alipay.remoting.rpc.serializer
{
    /// <summary>
    /// a custom serialize demo using invoke context
    /// </summary>
    public class NormalStringCustomSerializer_InvokeContext : DefaultCustomSerializer
	{

		private AtomicBoolean serialFlag = new AtomicBoolean();
		private AtomicBoolean deserialFlag = new AtomicBoolean();

		public const string UNIVERSAL_RESP = "UNIVERSAL RESPONSE";

		public const string SERIALTYPE_KEY = "serial.type";
		public const string SERIALTYPE1_value = "SERIAL1";
		public const string SERIALTYPE2_value = "SERIAL2";

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

			return true;
		}

		/// <seealso cref= CustomSerializer#deserializeContent(ResponseCommand, InvokeContext) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public <T extends com.alipay.remoting.rpc.ResponseCommand> boolean deserializeContent(T response, com.alipay.remoting.InvokeContext invokeContext) throws com.alipay.remoting.exception.DeserializationException
		public override bool deserializeContent(ResponseCommand response, InvokeContext invokeContext)
		{
			deserialFlag.set(true);
			RpcResponseCommand rpcResp = (RpcResponseCommand) response;

			if (string.Equals(SERIALTYPE1_value, (string) invokeContext.get(SERIALTYPE_KEY)))
			{
				try
				{
					rpcResp.ResponseObject = Encoding.UTF8.GetString(rpcResp.Content) + "RANDOM";
				}
				catch (UnsupportedEncodingException e)
				{
					System.Console.WriteLine(e.ToString());
					System.Console.Write(e.StackTrace);
				}
			}
			else
			{
				rpcResp.ResponseObject = UNIVERSAL_RESP;
			}
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
	}

}