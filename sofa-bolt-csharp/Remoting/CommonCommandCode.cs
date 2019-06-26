using ikvm.@internal;
using java.lang;
using System;

namespace Remoting
{

    /// <summary>
    /// The common command code, especially for heart beat command.
    /// </summary>
    public sealed class CommonCommandCode : java.lang.Enum, CommandCode
    {
        internal static CommonCommandCode __HEARTBEAT;

        private short _value;

        private static CommonCommandCode[] _VALUES;

        public static CommonCommandCode HEARTBEAT
        {
            get
            {
                return __HEARTBEAT;
            }
        }

        static CommonCommandCode()
        {
            __HEARTBEAT = new CommonCommandCode("HEARTBEAT", 0, 0);
            _VALUES = new CommonCommandCode[] { __HEARTBEAT };
        }

        private CommonCommandCode(string str, int num, short value)
            : base(str, num)
        {
            _value = value;
            GC.KeepAlive(this);
        }


        public short value()
        {
            return _value;
        }

        public static CommonCommandCode valueOf(string name)
        {
            return (CommonCommandCode)valueOf(ClassLiteral<CommonCommandCode>.Value, name);
        }

        public static CommonCommandCode valueOf(short value)
        {
            int num = value;
            int num1 = num;
            if (num1 != 0)
            {
                string str = new StringBuilder().append("Unknown Rpc command code value ,").append(num).toString();
                throw new ArgumentException(str);
            }
            return __HEARTBEAT;
        }

        public static CommonCommandCode[] values()
        {
            return (CommonCommandCode[])_VALUES.Clone();
        }

        [Serializable]
        public enum __Enum : short
        {
            HEARTBEAT
        }
    }
}