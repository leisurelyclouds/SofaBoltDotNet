
using java.util.concurrent.atomic;
using Remoting;
using Xunit;

namespace com.alipay.remoting.rpc.common
{
	/// <summary>
	/// ConnectionEventProcessor for ConnectionEventType.CLOSE
	/// </summary>
	public class DISCONNECTEventProcessor : ConnectionEventProcessor
	{

		private AtomicBoolean dicConnected = new AtomicBoolean();
		private AtomicInteger disConnectTimes = new AtomicInteger();

		public void onEvent(string remoteAddr, Connection conn)
		{
			Assert.NotNull(conn);
			dicConnected.set(true);
			disConnectTimes.incrementAndGet();
		}

		public virtual bool DisConnected
		{
			get
			{
				return this.dicConnected.get();
			}
		}

		public virtual int DisConnectTimes
		{
			get
			{
				return this.disConnectTimes.get();
			}
		}

		public virtual void reset()
		{
			this.disConnectTimes.set(0);
			this.dicConnected.set(false);
		}
	}

}