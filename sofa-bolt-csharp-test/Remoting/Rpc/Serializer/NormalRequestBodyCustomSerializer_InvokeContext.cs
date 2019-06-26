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
    public class NormalRequestBodyCustomSerializer_InvokeContext : DefaultCustomSerializer
    {

        private AtomicBoolean serialFlag = new AtomicBoolean();
        private AtomicBoolean deserialFlag = new AtomicBoolean();

        public const string UNIVERSAL_REQ = "UNIVERSAL REQUEST";

        public const string SERIALTYPE_KEY = "serial.type";
        public const string SERIALTYPE1_value = "SERIAL1";
        public const string SERIALTYPE2_value = "SERIAL2";

        /// <seealso cref= CustomSerializer#serializeContent(RequestCommand, InvokeContext) </seealso>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public <T extends com.alipay.remoting.rpc.RequestCommand> boolean serializeContent(T req, com.alipay.remoting.InvokeContext invokeContext) throws com.alipay.remoting.exception.SerializationException
        public override bool serializeContent(RequestCommand req, InvokeContext invokeContext)
        {
            serialFlag.set(true);
            RpcRequestCommand rpcReq = (RpcRequestCommand)req;
            if (string.Equals(SERIALTYPE1_value, (string)invokeContext.get(SERIALTYPE_KEY)))
            {
                RequestBody bd = (RequestBody)rpcReq.RequestObject;
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
            }
            else
            {
                try
                {
                    rpcReq.Content = Encoding.UTF8.GetBytes(UNIVERSAL_REQ);
                }
                catch (UnsupportedEncodingException e)
                {
                    System.Console.WriteLine(e.ToString());
                    System.Console.Write(e.StackTrace);
                }
            }

            return true;
        }

        /// <seealso cref= CustomSerializer#deserializeContent(RequestCommand) </seealso>
        public override bool deserializeContent(RequestCommand req)
        {
            deserialFlag.set(true);
            RpcRequestCommand rpcReq = (RpcRequestCommand)req;
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