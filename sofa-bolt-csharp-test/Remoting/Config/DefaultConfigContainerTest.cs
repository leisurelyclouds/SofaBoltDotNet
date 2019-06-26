using Remoting.Config.configs;
using Xunit;

namespace com.alipay.remoting.config
{
    [Collection("Sequential")]
    public class DefaultConfigContainerTest
    {
        [Fact]
		public virtual void testNormalArgs()
        {
			ConfigContainer configContainer = new DefaultConfigContainer();
			// test set one
			int expected_int = 123;
			configContainer.set(ConfigType.CLIENT_SIDE, ConfigItem.NETTY_BUFFER_LOW_WATER_MARK, expected_int);
			Assert.Equal(expected_int, configContainer.get(ConfigType.CLIENT_SIDE, ConfigItem.NETTY_BUFFER_LOW_WATER_MARK));
			Assert.Null(configContainer.get(ConfigType.CLIENT_SIDE, ConfigItem.NETTY_BUFFER_HIGH_WATER_MARK));
			Assert.Null(configContainer.get(ConfigType.SERVER_SIDE, ConfigItem.NETTY_BUFFER_LOW_WATER_MARK));
			Assert.Null(configContainer.get(ConfigType.SERVER_SIDE, ConfigItem.NETTY_BUFFER_HIGH_WATER_MARK));

			Assert.True(configContainer.contains(ConfigType.CLIENT_SIDE, ConfigItem.NETTY_BUFFER_LOW_WATER_MARK));
			Assert.False(configContainer.contains(ConfigType.CLIENT_SIDE, ConfigItem.NETTY_BUFFER_HIGH_WATER_MARK));
			Assert.False(configContainer.contains(ConfigType.SERVER_SIDE, ConfigItem.NETTY_BUFFER_LOW_WATER_MARK));
			Assert.False(configContainer.contains(ConfigType.SERVER_SIDE, ConfigItem.NETTY_BUFFER_HIGH_WATER_MARK));

			// test set all
			configContainer.set(ConfigType.CLIENT_SIDE, ConfigItem.NETTY_BUFFER_HIGH_WATER_MARK, expected_int);
			configContainer.set(ConfigType.SERVER_SIDE, ConfigItem.NETTY_BUFFER_LOW_WATER_MARK, expected_int);
			configContainer.set(ConfigType.SERVER_SIDE, ConfigItem.NETTY_BUFFER_HIGH_WATER_MARK, expected_int);

			Assert.True(configContainer.contains(ConfigType.CLIENT_SIDE, ConfigItem.NETTY_BUFFER_LOW_WATER_MARK));
			Assert.True(configContainer.contains(ConfigType.CLIENT_SIDE, ConfigItem.NETTY_BUFFER_HIGH_WATER_MARK));
			Assert.True(configContainer.contains(ConfigType.SERVER_SIDE, ConfigItem.NETTY_BUFFER_LOW_WATER_MARK));
			Assert.True(configContainer.contains(ConfigType.SERVER_SIDE, ConfigItem.NETTY_BUFFER_HIGH_WATER_MARK));

			// test overwrite
			int expected_int_overwrite = 456;
			configContainer.set(ConfigType.CLIENT_SIDE, ConfigItem.NETTY_BUFFER_LOW_WATER_MARK, expected_int_overwrite);
			Assert.Equal(expected_int_overwrite, configContainer.get(ConfigType.CLIENT_SIDE, ConfigItem.NETTY_BUFFER_LOW_WATER_MARK));
			Assert.Equal(expected_int, configContainer.get(ConfigType.CLIENT_SIDE, ConfigItem.NETTY_BUFFER_HIGH_WATER_MARK));
			Assert.Equal(expected_int, configContainer.get(ConfigType.SERVER_SIDE, ConfigItem.NETTY_BUFFER_LOW_WATER_MARK));
			Assert.Equal(expected_int, configContainer.get(ConfigType.SERVER_SIDE, ConfigItem.NETTY_BUFFER_HIGH_WATER_MARK));
		}

        [Fact]
		public virtual void testNullArgs()
        {
			ConfigContainer configContainer = new DefaultConfigContainer();

			try
			{
				configContainer.set(ConfigType.CLIENT_SIDE, ConfigItem.NETTY_BUFFER_LOW_WATER_MARK, null);
				Assert.Null("Should not reach here!");
			}
			catch (System.ArgumentException)
			{
			}
		}
	}
}