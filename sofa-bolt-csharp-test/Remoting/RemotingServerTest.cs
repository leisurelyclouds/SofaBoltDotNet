using Remoting;
using Remoting.rpc;
using System;
using Xunit;

namespace com.alipay.remoting
{
    /// <summary>
    /// test <seealso cref="AbstractRemotingServer"/> apis
    /// </summary>
    [Collection("Sequential")]
    public class RemotingServerTest
    {
        [Fact]
        public virtual void testStartRepeatedly()
        {
            RpcServer rpcServer = new RpcServer(1111);
            rpcServer.startup();

            try
            {
                rpcServer.startup();
                Assert.Null("Should not reach here!");
            }
            catch (Exception)
            {
                // expect IllegalStateException
            }
            rpcServer.shutdown();
        }

        [Fact]
        public virtual void testStopRepeatedly()
        {
            RpcServer rpcServer = new RpcServer(1111);
            try
            {
                rpcServer.startup();
            }
            catch (Exception e)
            {
                Assert.Null("Should not reach here!");
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
            rpcServer.shutdown();
            try
            {
                rpcServer.shutdown();
                Assert.Null("Should not reach here!");
            }
            catch (Exception)
            {
                // expect IllegalStateException
            }
        }
    }

}