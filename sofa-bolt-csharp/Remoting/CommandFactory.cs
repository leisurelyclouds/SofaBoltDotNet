using System;
using System.Net;

namespace Remoting
{
    /// <summary>
    /// Command factory
    /// </summary>
    public interface CommandFactory
	{
		// ~~~ create request command

		/// <summary>
		/// create a request command with request object
		/// </summary>
		/// <param name="requestObject"> the request object included in request command </param>
		RemotingCommand createRequestCommand(object requestObject);

		// ~~~ create response command

		/// <summary>
		/// create a normal response with response object 
        /// </summary>
		/// <param name="responseObject"> </param>
		/// <param name="requestCmd"> </param>
		RemotingCommand createResponse(object responseObject, RemotingCommand requestCmd);

		RemotingCommand createExceptionResponse(int id, string errMsg);

		RemotingCommand createExceptionResponse(int id, Exception t, string errMsg);

		RemotingCommand createExceptionResponse(int id, ResponseStatus status);

		RemotingCommand createExceptionResponse(int id, ResponseStatus status, Exception t);

        RemotingCommand createTimeoutResponse(IPEndPoint address);

		RemotingCommand createSendFailedResponse(IPEndPoint address, Exception throwable);

		RemotingCommand createConnectionClosedResponse(IPEndPoint address, string message);
	}

}