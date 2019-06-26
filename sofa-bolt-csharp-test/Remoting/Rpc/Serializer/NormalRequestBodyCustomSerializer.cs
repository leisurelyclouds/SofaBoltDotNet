using com.alipay.remoting.rpc.common;
using DotNetty.Buffers;
using java.io;
using java.util.concurrent.atomic;
using Remoting;
using Remoting.rpc;
using Remoting.rpc.protocol;
using System.Text;

namespace com.alipay.remoting.rpc.serializer
{
	/// <summary>
	/// a custom serialize demo
	/// </summary>
	public class NormalRequestBodyCustomSerializer : DefaultCustomSerializer
	{

		private AtomicBoolean serialFlag = new AtomicBoolean();
		private AtomicBoolean deserialFlag = new AtomicBoolean();

		private byte contentSerializer = 255;
		private byte contentDeserializer = 255;

		/// <seealso cref= CustomSerializer#serializeContent(RequestCommand, InvokeContext) </seealso>
		public override bool serializeContent(RequestCommand req, InvokeContext invokeContext)
		{
			serialFlag.set(true);
			RpcRequestCommand rpcReq = (RpcRequestCommand) req;
			RequestBody bd = (RequestBody) rpcReq.RequestObject;
			int id = bd.Id;
			byte[] msg;
			try
			{
				msg = Encoding.UTF8.GetBytes(bd.Msg);
				IByteBuffer bb = UnpooledByteBufferAllocator.Default.Buffer(4 + msg.Length);
				bb.WriteInt(id);
				bb.WriteBytes(msg);
				rpcReq.Content = bb.Array;
			}
			catch (UnsupportedEncodingException e)
			{
				System.Console.WriteLine(e.ToString());
                System.Console.Write(e.StackTrace);
			}

			contentSerializer = rpcReq.Serializer;
			return true;
		}

		/// <seealso cref= CustomSerializer#deserializeContent(RequestCommand) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public <T extends com.alipay.remoting.rpc.RequestCommand> boolean deserializeContent(T req) throws com.alipay.remoting.exception.DeserializationException
		public override bool deserializeContent(RequestCommand req)
		{
			deserialFlag.set(true);
			RpcRequestCommand rpcReq = (RpcRequestCommand) req;
			byte[] content = rpcReq.Content;
			IByteBuffer bb = Unpooled.WrappedBuffer(content);
			int a = bb.ReadInt();
			byte[] dst = new byte[content.Length - 4];
			bb.ReadBytes(dst, 0, dst.Length);
			try
			{
                string b = Encoding.UTF8.GetString(dst);
				RequestBody bd = new RequestBody(a, b);
				rpcReq.RequestObject = bd;
			}
			catch (UnsupportedEncodingException e)
			{
				System.Console.WriteLine(e.ToString());
                System.Console.Write(e.StackTrace);
			}

			contentDeserializer = rpcReq.Serializer;
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

		public virtual byte ContentDeserializer
		{
			get
			{
				return contentDeserializer;
			}
		}

		public virtual void reset()
		{
			this.contentDeserializer = 255;
			this.contentSerializer = 255;
			this.deserialFlag.set(false);
			this.serialFlag.set(false);
		}
	}

}