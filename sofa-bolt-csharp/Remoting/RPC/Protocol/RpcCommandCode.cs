using ikvm.@internal;
using System;
using System.Text;

namespace Remoting.rpc.protocol
{
    /// <summary>
    /// Command code for rpc remoting command.
    /// </summary>
    public sealed class RpcCommandCode : java.lang.Enum, CommandCode
    {
        internal static RpcCommandCode __RPC_REQUEST;

        internal static RpcCommandCode __RPC_RESPONSE;

        private short _value;

        private static RpcCommandCode[] _VALUES;

        public static RpcCommandCode RPC_REQUEST
        {
            get
            {
                return __RPC_REQUEST;
            }
        }

        public static RpcCommandCode RPC_RESPONSE
        {
            get
            {
                return __RPC_RESPONSE;
            }
        }

        static RpcCommandCode()
        {
            __RPC_REQUEST = new RpcCommandCode("RPC_REQUEST", 0, 1);
            __RPC_RESPONSE = new RpcCommandCode("RPC_RESPONSE", 1, 2);
            _VALUES = new RpcCommandCode[] { __RPC_REQUEST, __RPC_RESPONSE };
        }

        private RpcCommandCode(string str, int num, short value)
            :base(str, num)
        {
            _value = value;
            GC.KeepAlive(this);
        }

        public static void __<clinit>()
        {
        }

        public short @value()
        {
            return _value;
        }

        public static RpcCommandCode valueOf(string name)
        {
            return (RpcCommandCode)valueOf(ClassLiteral<RpcCommandCode>.Value, name);
        }

        public static RpcCommandCode valueOf(short value)
        {
            int num = value;
            int num1 = num;
            if (num1 == 1)
            {
                return __RPC_REQUEST;
            }
            if (num1 != 2)
            {
                string str = new StringBuilder().Append("Unknown Rpc command code value: ").Append(num).ToString();
                throw new ArgumentException(str);
            }
            return __RPC_RESPONSE;
        }

        public static RpcCommandCode[] values()
        {
            return (RpcCommandCode[])_VALUES.Clone();
        }

        public enum __Enum : short
        {
            RPC_REQUEST = 1,
            RPC_RESPONSE = 2
        }
	}
}