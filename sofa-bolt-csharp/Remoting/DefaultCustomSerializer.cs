using Remoting.rpc;

namespace Remoting
{
	/// <summary>
	/// The default custom serializer, which does nothing. 
	/// Extend this class and override the methods you want to custom.
	/// 
	/// @author jiangping
	/// @version $Id: DefaultCustomSerializer.java, v 0.1 2015-10-8 AM11:09:49 tao Exp $
	/// </summary>
	public class DefaultCustomSerializer : CustomSerializer
	{

		/// <seealso cref= CustomSerializer#serializeHeader(rpc.RequestCommand, InvokeContext) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public <RequestCommand extends rpc.RequestCommand> boolean serializeHeader(RequestCommand request, InvokeContext invokeContext) throws exception.SerializationException
		public virtual bool serializeHeader(RequestCommand request, InvokeContext invokeContext)
		{
			return false;
		}

		/// <seealso cref= CustomSerializer#serializeHeader(rpc.ResponseCommand) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public <RequestCommand extends rpc.ResponseCommand> boolean serializeHeader(RequestCommand response) throws exception.SerializationException
		public virtual bool serializeHeader(ResponseCommand response)
		{
			return false;
		}

		/// <seealso cref= CustomSerializer#deserializeHeader(rpc.RequestCommand) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public <RequestCommand extends rpc.RequestCommand> boolean deserializeHeader(RequestCommand request) throws exception.DeserializationException
		public virtual bool deserializeHeader(RequestCommand request)
		{
			return false;
		}

		/// <seealso cref= CustomSerializer#deserializeHeader(rpc.ResponseCommand, InvokeContext) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public <RequestCommand extends rpc.ResponseCommand> boolean deserializeHeader(RequestCommand response, InvokeContext invokeContext) throws exception.DeserializationException
		public virtual bool deserializeHeader(ResponseCommand response, InvokeContext invokeContext)
		{
			return false;
		}

		/// <seealso cref= CustomSerializer#serializeContent(rpc.RequestCommand, InvokeContext) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public <RequestCommand extends rpc.RequestCommand> boolean serializeContent(RequestCommand request, InvokeContext invokeContext) throws exception.SerializationException
		public virtual bool serializeContent(RequestCommand request, InvokeContext invokeContext)
		{
			return false;
		}

		/// <seealso cref= CustomSerializer#serializeContent(rpc.ResponseCommand) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public <RequestCommand extends rpc.ResponseCommand> boolean serializeContent(RequestCommand response) throws exception.SerializationException
		public virtual bool serializeContent(ResponseCommand response)
		{
			return false;
		}

		/// <seealso cref= CustomSerializer#deserializeContent(rpc.RequestCommand) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public <RequestCommand extends rpc.RequestCommand> boolean deserializeContent(RequestCommand request) throws exception.DeserializationException
		public virtual bool deserializeContent(RequestCommand request)
		{
			return false;
		}

		/// <seealso cref= CustomSerializer#deserializeContent(rpc.ResponseCommand, InvokeContext) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public <RequestCommand extends rpc.ResponseCommand> boolean deserializeContent(RequestCommand response, InvokeContext invokeContext) throws exception.DeserializationException
		public virtual bool deserializeContent(ResponseCommand response, InvokeContext invokeContext)
		{
			return false;
		}
	}
}