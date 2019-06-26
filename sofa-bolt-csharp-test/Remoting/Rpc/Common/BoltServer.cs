using Remoting;
using Remoting.rpc;
using Remoting.rpc.protocol;

namespace com.alipay.remoting.rpc.common
{
    /// <summary>
    /// Demo for bolt server
    /// </summary>
    public class BoltServer
    {
        /// <summary>
        /// port 
        /// </summary>
        private int port;

        /// <summary>
        /// rpc server
		/// </summary>
        private RpcServer server;

        // ~~~ constructors
        public BoltServer(int port)
        {
            this.port = port;
            this.server = new RpcServer(this.port);
        }

        public BoltServer(int port, bool manageFeatureEnabled)
        {
            this.port = port;
            this.server = new RpcServer(this.port, manageFeatureEnabled);
        }

        public BoltServer(int port, bool manageFeatureEnabled, bool syncStop)
        {
            this.port = port;
            this.server = new RpcServer(this.port, manageFeatureEnabled, syncStop);
        }

        public virtual bool start()
        {
            this.server.startup();
            return true;
        }

        public virtual void stop()
        {
            this.server.shutdown();
        }

        public virtual RpcServer RpcServer
        {
            get
            {
                return this.server;
            }
        }

        public virtual void registerUserProcessor(UserProcessor processor)
        {
            this.server.registerUserProcessor(processor);
        }

        public virtual void addConnectionEventProcessor(ConnectionEventType type, ConnectionEventProcessor processor)
        {
            this.server.addConnectionEventProcessor(type, processor);
        }
    }

}