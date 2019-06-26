using Remoting.Config.switches;
using System.Collections;
using Xunit;


namespace com.alipay.remoting.inner.utiltest
{
    [Collection("Sequential")]
    public class ProtocolSwitchTest
    {
        [Fact]
        public virtual void test_toByte()
        {
            ProtocolSwitch protocolSwitch = new ProtocolSwitch();
            protocolSwitch.turnOn(0);
            Assert.Equal(1, protocolSwitch.toByte());

            protocolSwitch = new ProtocolSwitch();
            protocolSwitch.turnOn(1);
            Assert.Equal(2, protocolSwitch.toByte());

            protocolSwitch = new ProtocolSwitch();
            protocolSwitch.turnOn(2);
            Assert.Equal(4, protocolSwitch.toByte());

            protocolSwitch = new ProtocolSwitch();
            protocolSwitch.turnOn(3);
            Assert.Equal(8, protocolSwitch.toByte());

            protocolSwitch = new ProtocolSwitch();
            protocolSwitch.turnOn(0);
            protocolSwitch.turnOn(1);
            Assert.Equal(3, protocolSwitch.toByte());

            protocolSwitch = new ProtocolSwitch();
            protocolSwitch.turnOn(6);
            Assert.Equal(64, protocolSwitch.toByte());

            protocolSwitch = new ProtocolSwitch();
            for (int i = 0; i < 7; ++i)
            {
                protocolSwitch.turnOn(i);
            }
            Assert.Equal(127, protocolSwitch.toByte());

            protocolSwitch = new ProtocolSwitch();
            try
            {
                for (int i = 0; i < 9; ++i)
                {
                    protocolSwitch.turnOn(i);
                }

                protocolSwitch.toByte();
                Assert.Null("Should not reach here!");
            }
            catch (System.ArgumentOutOfRangeException)
            {
            }
        }

        [Fact]
        public virtual void test_createUsingIndex()
        {
            for (int i = 0; i < 7; ++i)
            {
                Assert.True(ProtocolSwitch.create(new int[] { i }).isOn(i));
            }

            int size = 7;
            int[] a = new int[size];
            for (int i = 0; i < size; ++i)
            {
                a[i] = i;
            }
            ProtocolSwitch status = ProtocolSwitch.create(a);
            for (int i = 0; i < size; ++i)
            {
                Assert.True(status.isOn(i));
            }
        }

        [Fact]
        public virtual void test_createUsingByte()
        {
            Assert.False(ProtocolSwitch.create(0).isOn(0));
            Assert.True(ProtocolSwitch.create(1).isOn(0));
            Assert.True(ProtocolSwitch.create(2).isOn(1));
            Assert.True(ProtocolSwitch.create(4).isOn(2));
            Assert.True(ProtocolSwitch.create(8).isOn(3));
            Assert.True(ProtocolSwitch.create(3).isOn(0));
            Assert.True(ProtocolSwitch.create(3).isOn(1));
            Assert.False(ProtocolSwitch.create(3).isOn(2));
            Assert.True(ProtocolSwitch.create(64).isOn(6));
            Assert.False(ProtocolSwitch.create(64).isOn(0));
            Assert.True(ProtocolSwitch.create(127).isOn(0));
            Assert.True(ProtocolSwitch.create(127).isOn(1));
            Assert.True(ProtocolSwitch.create(127).isOn(6));
            Assert.False(ProtocolSwitch.create(127).isOn(7));
            Assert.True(ProtocolSwitch.create(255).isOn(0));
            Assert.True(ProtocolSwitch.create(255).isOn(1));
            Assert.True(ProtocolSwitch.create(255).isOn(7));
            try
            {
                ProtocolSwitch.create(511);
                Assert.Null("Should not reach here!");
            }
            catch (System.ArgumentOutOfRangeException)
            {
            }
        }

        [Fact]
        public virtual void test_isSwitchOn()
        {
            for (int i = 0; i < 7; ++i)
            {
                Assert.True(ProtocolSwitch.isOn(i, 1 << i));
            }
            Assert.True(ProtocolSwitch.isOn(0, 1));
            Assert.True(ProtocolSwitch.isOn(1, 2));
            Assert.True(ProtocolSwitch.isOn(2, 4));
            Assert.True(ProtocolSwitch.isOn(3, 8));
            Assert.True(ProtocolSwitch.isOn(0, 3));
            Assert.True(ProtocolSwitch.isOn(1, 3));
            Assert.False(ProtocolSwitch.isOn(2, 3));
            Assert.True(ProtocolSwitch.isOn(6, 64));
            Assert.False(ProtocolSwitch.isOn(0, 64));
            Assert.True(ProtocolSwitch.isOn(0, 127));
            Assert.True(ProtocolSwitch.isOn(1, 127));
            Assert.False(ProtocolSwitch.isOn(7, 127));
            Assert.True(ProtocolSwitch.isOn(0, 255));
            Assert.True(ProtocolSwitch.isOn(1, 255));
            Assert.True(ProtocolSwitch.isOn(7, 255));
            try
            {
                ProtocolSwitch.isOn(7, 511);
                Assert.Null("Should not reach here!");
            }
            catch (System.ArgumentOutOfRangeException)
            {
            }
        }

        [Fact]
        public virtual void test_byteToBitSet()
        {
            var bs = ProtocolSwitch.toBitSet(1);
            Assert.True(bs.Get(0));

            bs = ProtocolSwitch.toBitSet(2);
            Assert.True(bs.Get(1));

            bs = ProtocolSwitch.toBitSet(4);
            Assert.True(bs.Get(2));

            bs = ProtocolSwitch.toBitSet(8);
            Assert.True(bs.Get(3));

            bs = ProtocolSwitch.toBitSet(3);
            Assert.True(bs.Get(0));
            Assert.True(bs.Get(1));

            bs = ProtocolSwitch.toBitSet(12);
            Assert.True(bs.Get(2));
            Assert.True(bs.Get(3));

            bs = ProtocolSwitch.toBitSet(64);
            Assert.True(bs.Get(6));

            bs = ProtocolSwitch.toBitSet(127);
            for (int i = 0; i <= 6; ++i)
            {
                Assert.True(bs.Get(i));
            }
        }

        [Fact]
        public virtual void test_bitSetToByte()
        {
            var bs = new BitArray(8);

            bs.Set(0, true);
            Assert.Equal(1, ProtocolSwitch.toByte(bs));

            bs.SetAll(false);
            bs.Set(1, true);
            Assert.Equal(2, ProtocolSwitch.toByte(bs));

            bs.SetAll(false);
            bs.Set(2, true);
            Assert.Equal(4, ProtocolSwitch.toByte(bs));

            bs.SetAll(false);
            bs.Set(3, true);
            Assert.Equal(8, ProtocolSwitch.toByte(bs));

            bs.SetAll(false);
            bs.Set(0, true);
            bs.Set(1, true);
            Assert.Equal(3, ProtocolSwitch.toByte(bs));

            bs.SetAll(false);
            bs.Set(2, true);
            bs.Set(3, true);
            Assert.Equal(12, ProtocolSwitch.toByte(bs));

            bs.SetAll(false);
            bs.Set(6, true);
            Assert.Equal(64, ProtocolSwitch.toByte(bs));

            bs.SetAll(false);
            for (int i = 0; i <= 6; ++i)
            {
                bs.Set(i, true);
            }
            Assert.Equal(127, ProtocolSwitch.toByte(bs));

            bs.SetAll(false);
            for (int i = 0; i <= 7; ++i)
            {
                bs.Set(i, true);
            }
            Assert.Equal(255, ProtocolSwitch.toByte(bs));


            bs.SetAll(false);
            try
            {
                for (int i = 0; i <= 8; ++i)
                {
                    bs.Set(i, true);
                }
                ProtocolSwitch.toByte(bs);
                Assert.Null("Should not reach here!");
            }
            catch (System.ArgumentOutOfRangeException)
            {
            }
        }
    }
}