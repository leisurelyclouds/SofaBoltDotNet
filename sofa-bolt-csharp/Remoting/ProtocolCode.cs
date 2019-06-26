using System;
using System.Linq;

namespace Remoting
{
    /// <summary>
    /// Protocol code definition, you can define your own protocol code in byte array <seealso cref="ProtocolCode#version"/>
    /// We suggest to use just one byte for simplicity.
    /// </summary>
    public class ProtocolCode
    {
        /// <summary>
        /// bytes to represent protocol code
		/// </summary>
        internal byte[] Version { get; set; }

        private ProtocolCode(params byte[] version)
        {
            this.Version = version;
        }

        public static ProtocolCode fromBytes(params byte[] version)
        {
            return new ProtocolCode(version);
        }

        /// <summary>
        /// get the first single byte if your protocol code is single code.
        /// @return
        /// </summary>
        public virtual byte FirstByte
        {
            get
            {
                return Version[0];
            }
        }

        public virtual int length()
        {
            return Version.Length;
        }

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }
            if (o == null || GetType() != o.GetType())
            {
                return false;
            }
            ProtocolCode that = (ProtocolCode)o;
            return Version.SequenceEqual(that.Version);
        }

        public override int GetHashCode()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(Version).GetHashCode();
        }

        public override string ToString()
        {
            return "ProtocolVersion{" + "version=" + Newtonsoft.Json.JsonConvert.SerializeObject(Version) + '}';
        }
    }
}