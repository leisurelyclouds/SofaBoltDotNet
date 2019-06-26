using Remoting.rpc;
using Xunit;

namespace com.alipay.remoting.rpc
{
    [Collection("Sequential")]
    public class RpcConfigManagerTest
    {

        [Fact]
		public virtual void testSystemSettings()
		{
			Assert.True(RpcConfigManager.dispatch_msg_list_in_default_executor());
		}
	}
}