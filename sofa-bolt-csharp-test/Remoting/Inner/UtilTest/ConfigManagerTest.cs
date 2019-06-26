using Remoting.Config;
using Xunit;

namespace com.alipay.remoting.inner.utiltest
{
    /// <summary>
    /// test ConfigManager get config
    /// </summary>
    [Collection("Sequential")]
    public class ConfigManagerTest
    {
        [Fact]
		public virtual void testSystemSettings()
        {
			int low_default = int.Parse(Configs.NETTY_BUFFER_LOW_WATERMARK_DEFAULT);
			int high_default = int.Parse(Configs.NETTY_BUFFER_HIGH_WATERMARK_DEFAULT);
			Assert.Equal(low_default, ConfigManager.netty_buffer_low_watermark());
			Assert.Equal(high_default, ConfigManager.netty_buffer_high_watermark());
		}
	}
}