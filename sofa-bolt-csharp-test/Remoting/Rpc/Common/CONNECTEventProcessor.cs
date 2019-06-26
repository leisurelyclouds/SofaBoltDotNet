
using java.util.concurrent;
using java.util.concurrent.atomic;
using Remoting;
using System.Threading;
using Xunit;

namespace com.alipay.remoting.rpc.common
{
	/// <summary>
	/// ConnectionEventProcessor for ConnectionEventType.CONNECT
	/// </summary>
	public class CONNECTEventProcessor : ConnectionEventProcessor
	{

		private AtomicBoolean connected = new AtomicBoolean();
		private AtomicInteger connectTimes = new AtomicInteger();
		private Connection connection;
		private string remoteAddr;
		private CountdownEvent latch = new CountdownEvent(1);

		public void onEvent(string remoteAddr, Connection conn)
		{
			Assert.NotNull(remoteAddr);
			doCheckConnection(conn);
			this.remoteAddr = remoteAddr;
			this.connection = conn;
			connected.set(true);
			connectTimes.incrementAndGet();
			if (!latch.IsSet)
            {
                latch.Signal();
            }
		}

		/// <summary>
		/// do check connection
		/// </summary>
		/// <param name="conn"> </param>
		private void doCheckConnection(Connection conn)
		{
			Assert.NotNull(conn);
			Assert.NotNull(conn.PoolKeys);
			Assert.True(conn.PoolKeys.Count > 0);
			Assert.NotNull(conn.Channel);
			Assert.NotNull(conn.Url);
			Assert.NotNull(conn.Channel.GetAttribute(Connection.CONNECTION).Get());
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public boolean isConnected() throws ThreadInterruptedException
		public virtual bool Connected
		{
			get
			{
				latch.Wait();
				return this.connected.get();
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int getConnectTimes() throws ThreadInterruptedException
		public virtual int ConnectTimes
		{
			get
			{
				latch.Wait();
				return this.connectTimes.get();
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public com.alipay.remoting.Connection getConnection() throws ThreadInterruptedException
		public virtual Connection Connection
		{
			get
			{
				latch.Wait();
				return this.connection;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String getRemoteAddr() throws ThreadInterruptedException
		public virtual string RemoteAddr
		{
			get
			{
				latch.Wait();
				return this.remoteAddr;
			}
		}

		public virtual void reset()
		{
			this.connectTimes.set(0);
			this.connected.set(false);
			this.connection = null;
		}
	}

}