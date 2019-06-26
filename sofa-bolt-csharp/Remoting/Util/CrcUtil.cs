using java.util.zip;

namespace Remoting.util
{
    /// <summary>
    /// CRC32 utility.
    /// </summary>
    public class CrcUtil
    {
        /// <summary>
        /// Compute CRC32 code for byte[].
        /// </summary>
        /// <param name="array">
        /// @return </param>
        public static int crc32(byte[] array)
        {
            if (array != null)
            {
                return crc32(array, 0, array.Length);
            }

            return 0;
        }

        /// <summary>
        /// Compute CRC32 code for byte[].
        /// </summary>
        /// <param name="array"> </param>
        /// <param name="offset"> </param>
        /// <param name="length">
        /// @return </param>
        public static int crc32(byte[] array, int offset, int length)
        {
            CRC32 cRC32 = new CRC32();
            cRC32.update(array, offset, length);
            int value = (int)cRC32.getValue();
            cRC32.reset();
            return value;
        }
    }
}