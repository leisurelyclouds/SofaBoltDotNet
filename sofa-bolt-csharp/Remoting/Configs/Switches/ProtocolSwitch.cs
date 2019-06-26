using System.Collections;

namespace Remoting.Config.switches
{

    /// <summary>
    /// Switches used in protocol, this is runtime switch.
    /// </summary>
    public class ProtocolSwitch : Switch
	{

		// switch index
		public const int CRC_SWITCH_INDEX = 0x000;

		// default value
		public const bool CRC_SWITCH_DEFAULT_VALUE = true;

		/// <summary>
		/// protocol switches
		/// </summary>
		private BitArray bs = new BitArray(8);

		// ~~~ public methods

		public virtual void turnOn(int index)
		{
			bs.Set(index, true);
		}

		public virtual void turnOff(int index)
		{
			bs.Set(index, false);
		}

		public virtual bool isOn(int index)
		{
			return bs.Get(index);
		}

		/// <summary>
		/// generate byte value according to the bit set in ProtocolSwitchStatus
		/// </summary>
		public virtual byte toByte()
		{
			return toByte(bs);
		}

		//~~~ static methods

		/// <summary>
		/// check switch status whether on according to specified value
		/// </summary>
		/// <param name="switchIndex"> </param>
		/// <param name="value">
		/// @return </param>
		public static bool isOn(int switchIndex, int value)
		{
			return toBitSet(value).Get(switchIndex);
		}

		/// <summary>
		/// create an instance of <seealso cref="ProtocolSwitch"/> according to byte value
		/// </summary>
		/// <param name="value"> </param>
		/// <returns> ProtocolSwitchStatus with initialized bit set. </returns>
		public static ProtocolSwitch create(int value)
		{
			ProtocolSwitch status = new ProtocolSwitch();
			status.Bs = toBitSet(value);
			return status;
		}

		/// <summary>
		/// create an instance of <seealso cref="ProtocolSwitch"/> according to switch index
		/// </summary>
		/// <param name="index"> the switch index which you want to set true </param>
		/// <returns> ProtocolSwitchStatus with initialized bit set. </returns>
		public static ProtocolSwitch create(int[] index)
		{
			ProtocolSwitch status = new ProtocolSwitch();
			for (int i = 0; i < index.Length; ++i)
			{
				status.turnOn(index[i]);
			}
			return status;
		}

		/// <summary>
		/// from bit set to byte
		/// </summary>
		/// <param name="bs"> </param>
		/// <returns> byte represent the bit set </returns>
		public static byte toByte(BitArray bs)
		{
			int value = 0;
			for (int i = 0; i < bs.Count; ++i)
			{
				if (bs.Get(i))
				{
					value += 1 << i;
				}
			}
			if (bs.Count > 8)
			{
				throw new System.ArgumentOutOfRangeException("The byte value " + value + " generated according to bit set " + bs + " is out of range, should be limited between [" + byte.MinValue + "] to [" + byte.MaxValue + "]");
			}
			return (byte) value;
		}

		/// <summary>
		/// from byte to bit set
		/// </summary>
		/// <param name="value"> </param>
		/// <returns> bit set represent the byte </returns>
		public static BitArray toBitSet(int value)
		{
			if (value > byte.MaxValue || value < byte.MinValue)
			{
				throw new System.ArgumentOutOfRangeException("The value " + value + " is out of byte range, should be limited between [" + byte.MinValue + "] to [" + byte.MaxValue + "]");
			}
            BitArray bs = new BitArray(8);
			int index = 0;
			while (value != 0)
			{
				if (value % 2 != 0)
				{
					bs.Set(index, true);
				}
				++index;
				value = (byte)(value >> 1);
			}
			return bs;
		}

		// ~~~ getter and setters

		/// <summary>
		/// Setter method for property <tt>bs<tt>.
		/// </summary>
		/// <param name="bs"> value to be assigned to property bs </param>
		public virtual BitArray Bs
		{
			set
			{
				bs = value;
			}
		}
	}
}