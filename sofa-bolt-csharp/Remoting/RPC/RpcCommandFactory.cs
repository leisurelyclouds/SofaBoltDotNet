using Remoting.rpc.exception;
using Remoting.rpc.protocol;
using System;
using System.Net;

namespace Remoting.rpc
{
    /// <summary>
    /// command factory for rpc protocol
    /// 
    /// @author tsui
    /// @version $Id: RpcCommandFactory.java, v 0.1 2018-03-27 21:37 tsui Exp $
    /// </summary>
    public class RpcCommandFactory : CommandFactory
    {
        public virtual RemotingCommand createRequestCommand(object requestObject)
        {
            return new RpcRequestCommand(requestObject);
        }

        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        //ORIGINAL LINE: @Override public rpc.protocol.RpcResponseCommand createResponse(final Object responseObject, final RemotingCommand requestCmd)
        public virtual RemotingCommand createResponse(object responseObject, RemotingCommand requestCmd)
        {
            RpcResponseCommand response = new RpcResponseCommand(requestCmd.Id, responseObject);
            if (null != responseObject)
            {
                //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
                response.ResponseClass = responseObject.GetType();
            }
            else
            {
                response.ResponseClass = null;
            }
            response.Serializer = requestCmd.Serializer;
            response.ProtocolSwitch = requestCmd.ProtocolSwitch;
            response.ResponseStatus = ResponseStatus.SUCCESS;
            return response;
        }

        public virtual RemotingCommand createExceptionResponse(int id, string errMsg)
        {
            return createExceptionResponse(id, null, errMsg);
        }

        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        //ORIGINAL LINE: @Override public rpc.protocol.RpcResponseCommand createExceptionResponse(int id, final Throwable t, String errMsg)
        public virtual RemotingCommand createExceptionResponse(int id, System.Exception t, string errMsg)
        {
            RpcResponseCommand response = null;
            if (null == t)
            {
                response = new RpcResponseCommand(id, createServerException(errMsg));
            }
            else
            {
                response = new RpcResponseCommand(id, createServerException(t, errMsg));
            }
            //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            response.ResponseClass = typeof(RpcServerException);
            response.ResponseStatus = ResponseStatus.SERVER_EXCEPTION;
            return response;
        }

        public virtual RemotingCommand createExceptionResponse(int id, ResponseStatus status)
        {
            RpcResponseCommand responseCommand = new RpcResponseCommand();
            responseCommand.Id = id;
            responseCommand.ResponseStatus = status;
            return responseCommand;
        }

        public virtual RemotingCommand createExceptionResponse(int id, ResponseStatus status, System.Exception t)
        {
            var responseCommand = (RpcResponseCommand)createExceptionResponse(id, status);
            responseCommand.ResponseObject = createServerException(t, null);
            //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            responseCommand.ResponseClass = typeof(RpcServerException);
            return responseCommand;
        }

        public virtual RemotingCommand createTimeoutResponse(IPEndPoint address)
        {
            ResponseCommand responseCommand = new ResponseCommand();
            responseCommand.ResponseStatus = ResponseStatus.TIMEOUT;
            responseCommand.ResponseTimeMillis = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            responseCommand.ResponseHost = address;
            return responseCommand;
        }

        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        //ORIGINAL LINE: @Override public RemotingCommand createSendFailedResponse(final java.net.IPEndPoint address, Throwable throwable)
        public virtual RemotingCommand createSendFailedResponse(IPEndPoint address, System.Exception throwable)
        {
            ResponseCommand responseCommand = new ResponseCommand();
            responseCommand.ResponseStatus = ResponseStatus.CLIENT_SEND_ERROR;
            responseCommand.ResponseTimeMillis = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            responseCommand.ResponseHost = address;
            responseCommand.Cause = throwable;
            return responseCommand;
        }

        public virtual RemotingCommand createConnectionClosedResponse(IPEndPoint address, string message)
        {
            ResponseCommand responseCommand = new ResponseCommand();
            responseCommand.ResponseStatus = ResponseStatus.CONNECTION_CLOSED;
            responseCommand.ResponseTimeMillis = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            responseCommand.ResponseHost = address;
            return responseCommand;
        }

        /// <summary>
        /// create server exception using error msg, no stack trace
		/// </summary>
        /// <param name="errMsg"> the assigned error message </param>
        /// <returns> an instance of RpcServerException </returns>
        private RpcServerException createServerException(string errMsg)
        {
            return new RpcServerException(errMsg);
        }

        /// <summary>
        /// create server exception using error msg and fill the stack trace using the stack trace of throwable.
        /// </summary>
        /// <param name="t"> the origin throwable to fill the stack trace of rpc server exception </param>
        /// <param name="errMsg"> additional error msg, <code>null</code> is allowed </param>
        /// <returns> an instance of RpcServerException </returns>
        private RpcServerException createServerException(System.Exception exception, string errMsg)
        {
            RpcServerException rpcServerException = new RpcServerException(java.lang.String.format("[Server]OriginErrorMsg: %s: %s. AdditionalErrorMsg: %s", new object[] { ikvm.extensions.ExtensionMethods.getClass(exception).getName(), ikvm.extensions.ExtensionMethods.getMessage(exception), errMsg }));
            ikvm.extensions.ExtensionMethods.setStackTrace(rpcServerException, ikvm.extensions.ExtensionMethods.getStackTrace(exception));
            return rpcServerException;
        }
    }
}