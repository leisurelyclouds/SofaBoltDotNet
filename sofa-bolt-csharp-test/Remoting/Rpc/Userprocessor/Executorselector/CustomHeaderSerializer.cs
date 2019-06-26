using java.io;
using Remoting;
using Remoting.rpc;
using Remoting.rpc.protocol;
using System.Text;

namespace com.alipay.remoting.rpc.userprocessor.executorselector
{

    //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
    //	import static com.alipay.remoting.rpc.userprocessor.executorselector.DefaultExecutorSelector.EXECUTOR1;

    /// <summary>
    
    
    /// </summary>
    public class CustomHeaderSerializer : DefaultCustomSerializer
	{
        /// <seealso cref= remoting.CustomSerializer#serializeHeader(com.alipay.remoting.rpc.RequestCommand, InvokeContext) </seealso>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public <T extends com.alipay.remoting.rpc.RequestCommand> boolean serializeHeader(T request, com.alipay.remoting.InvokeContext invokeContext) throws com.alipay.remoting.exception.SerializationException
        public override bool serializeHeader(RequestCommand request, InvokeContext invokeContext)
		{
			if (request is RpcRequestCommand)
			{
				RpcRequestCommand requestCommand = (RpcRequestCommand) request;
				try
				{
					requestCommand.Header = Encoding.UTF8.GetBytes(DefaultExecutorSelector.EXECUTOR1);
				}
				catch (UnsupportedEncodingException)
				{
					System.Console.Error.WriteLine("UnsupportedEncodingException");
				}
				return true;
			}
			return false;
		}

        /// <seealso cref= remoting.CustomSerializer#deserializeHeader(com.alipay.remoting.rpc.RequestCommand) </seealso>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public <T extends com.alipay.remoting.rpc.RequestCommand> boolean deserializeHeader(T request) throws com.alipay.remoting.exception.DeserializationException
        public override bool deserializeHeader(RequestCommand request)
		{
			if (request is RpcRequestCommand)
			{
				RpcRequestCommand requestCommand = (RpcRequestCommand) request;
				byte[] header = requestCommand.Header;
				try
				{
					requestCommand.RequestHeader = Encoding.UTF8.GetString(header);
				}
				catch (UnsupportedEncodingException)
				{
					System.Console.Error.WriteLine("UnsupportedEncodingException");
				}
				return true;
			}
			return false;
		}

        /// <seealso cref= remoting.CustomSerializer#serializeHeader(com.alipay.remoting.rpc.ResponseCommand) </seealso>
        public override bool serializeHeader(ResponseCommand response)
		{
			if (response is RpcResponseCommand)
			{
				RpcResponseCommand responseCommand = (RpcResponseCommand) response;
				try
				{
					responseCommand.Header = Encoding.UTF8.GetBytes(DefaultExecutorSelector.EXECUTOR1);
				}
				catch (UnsupportedEncodingException)
				{
                    System.Console.Error.WriteLine("UnsupportedEncodingException");
				}
				return true;
			}
			return false;
		}

        /// <seealso cref= remoting.CustomSerializer#deserializeHeader(com.alipay.remoting.rpc.ResponseCommand, InvokeContext) </seealso>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public <T extends com.alipay.remoting.rpc.ResponseCommand> boolean deserializeHeader(T response, com.alipay.remoting.InvokeContext invokeContext) throws com.alipay.remoting.exception.DeserializationException
        public override bool deserializeHeader(ResponseCommand response, InvokeContext invokeContext)
		{
			if (response is RpcResponseCommand)
			{
				RpcResponseCommand responseCommand = (RpcResponseCommand) response;
				byte[] header = responseCommand.Header;
				try
				{
					responseCommand.ResponseHeader = Encoding.UTF8.GetString(header);
				}
				catch (UnsupportedEncodingException)
				{
					System.Console.Error.WriteLine("UnsupportedEncodingException");
				}
				return true;
			}
			return false;
		}
	}
}