using Remoting.Config;
using Remoting.rpc;
using Xunit;

namespace com.alipay.remoting.inner.utiltest
{
    [Collection("Sequential")]
    public class GlobalSwitchTest
    {
		private RpcClient client1;
		private RpcClient client2;

        [Fact]
		public virtual void testDefaultvalue()
        {
            java.lang.System.clearProperty(Configs.CONN_RECONNECT_SWITCH);
            java.lang.System.clearProperty(Configs.CONN_MONITOR_SWITCH);
			client1 = new RpcClient();
			client2 = new RpcClient();

			Assert.False(client1.ConnectionMonitorSwitchOn);
			Assert.False(client1.ReconnectSwitchOn);
			Assert.False(client2.ConnectionMonitorSwitchOn);
			Assert.False(client2.ReconnectSwitchOn);
		}

        [Fact]
		public virtual void testSystemSettings_takesEffect_before_defaultvalue()
        {
            java.lang.System.setProperty(Configs.CONN_RECONNECT_SWITCH, "true");
            java.lang.System.setProperty(Configs.CONN_MONITOR_SWITCH, "true");
			client1 = new RpcClient();
			client2 = new RpcClient();

			Assert.True(client1.ConnectionMonitorSwitchOn);
			Assert.True(client1.ReconnectSwitchOn);
			Assert.True(client2.ConnectionMonitorSwitchOn);
			Assert.True(client2.ReconnectSwitchOn);
		}

        [Fact]
		public virtual void testUserSettings_takesEffect_before_SystemSettingsFalse()
        {
            java.lang.System.setProperty(Configs.CONN_RECONNECT_SWITCH, "false");
            java.lang.System.setProperty(Configs.CONN_MONITOR_SWITCH, "false");
			client1 = new RpcClient();
			client2 = new RpcClient();

			client1.enableConnectionMonitorSwitch();
			client1.enableReconnectSwitch();
			Assert.True(client1.ConnectionMonitorSwitchOn);
			Assert.True(client1.ReconnectSwitchOn);
			Assert.False(client2.ConnectionMonitorSwitchOn);
			Assert.False(client2.ReconnectSwitchOn);

			client1.disableConnectionMonitorSwitch();
			client1.disableReconnectSwith();
			client2.enableConnectionMonitorSwitch();
			client2.enableReconnectSwitch();
			Assert.False(client1.ConnectionMonitorSwitchOn);
			Assert.False(client1.ReconnectSwitchOn);
			Assert.True(client2.ReconnectSwitchOn);
			Assert.True(client2.ConnectionMonitorSwitchOn);
		}

        [Fact]
		public virtual void testUserSettings_takesEffect_before_SystemSettingsTrue()
        {
			java.lang.System.setProperty(Configs.CONN_RECONNECT_SWITCH, "true");
            java.lang.System.setProperty(Configs.CONN_MONITOR_SWITCH, "true");
			client1 = new RpcClient();
			client2 = new RpcClient();

			client1.enableConnectionMonitorSwitch();
			client1.enableReconnectSwitch();

			Assert.True(client1.ConnectionMonitorSwitchOn);
			Assert.True(client1.ReconnectSwitchOn);
			Assert.True(client2.ConnectionMonitorSwitchOn);
			Assert.True(client2.ReconnectSwitchOn);

			client1.disableConnectionMonitorSwitch();
			client1.disableReconnectSwith();
			client2.disableReconnectSwith();
			client2.disableConnectionMonitorSwitch();
			Assert.False(client1.ConnectionMonitorSwitchOn);
			Assert.False(client1.ReconnectSwitchOn);
			Assert.False(client2.ReconnectSwitchOn);
			Assert.False(client2.ConnectionMonitorSwitchOn);
		}
	}
}