using Xunit;
using Remoting;
using Remoting.rpc.protocol;

namespace com.alipay.remoting
{
    /// <summary>
    /// test processor manager
    /// </summary>
    [Collection("Sequential")]
    public class ProcessorManagerTest
    {
        /// <summary>
        /// test it should be override if register twice for the same command code
        /// </summary>
        [Fact]
        public virtual void testRegisterProcessor()
        {
            ProcessorManager processorManager = new ProcessorManager();
            CommandCode cmd1 = RpcCommandCode.RPC_REQUEST;
            CommandCode cmd2 = RpcCommandCode.RPC_REQUEST;
            RpcRequestProcessor rpcRequestProcessor1 = new RpcRequestProcessor();
            RpcRequestProcessor rpcRequestProcessor2 = new RpcRequestProcessor();
            processorManager.registerProcessor(cmd1, rpcRequestProcessor1);
            processorManager.registerProcessor(cmd2, rpcRequestProcessor2);
            Assert.Equal(processorManager.getProcessor(cmd1), rpcRequestProcessor2);
            Assert.Equal(processorManager.getProcessor(cmd2), rpcRequestProcessor2);
        }
    }
}