using DotNetty.Common.Utilities;
using System.Threading;
using System.Net;

namespace Remoting
{
    /// <summary>
    /// The future of an invocation.
    /// </summary>
    public interface InvokeFuture
	{
		/// <summary>
		/// Wait response with timeout.
		/// </summary>
		/// <param name="timeoutMillis"> time out in millisecond </param>
		/// <returns> remoting command </returns>
		/// <exception cref="ThreadInterruptedException"> if interrupted </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: RemotingCommand waitResponse(final long timeoutMillis) throws ThreadInterruptedException;
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		RemotingCommand waitResponse(long timeoutMillis);

		/// <summary>
		/// Wait response with unlimit timeout
		/// </summary>
		/// <returns> remoting command </returns>
		/// <exception cref="ThreadInterruptedException"> if interrupted </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: RemotingCommand waitResponse() throws ThreadInterruptedException;
		RemotingCommand waitResponse();

		/// <summary>
		/// Create a remoting command response when connection closed
		/// </summary>
		/// <param name="responseHost"> target host </param>
		/// <returns> remoting command </returns>
		RemotingCommand createConnectionClosedResponse(IPEndPoint responseHost);

		/// <summary>
		/// Put the response to the future.
		/// </summary>
		/// <param name="response"> remoting command </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: void putResponse(final RemotingCommand response);
		void putResponse(RemotingCommand response);

		/// <summary>
		/// Get the id of the invocation.
		/// </summary>
		/// <returns> invoke id </returns>
		int invokeId();

		/// <summary>
		/// Execute the callback.
		/// </summary>
		void executeInvokeCallback();

		/// <summary>
		/// Asynchronous execute the callback abnormally.
		/// </summary>
		void tryAsyncExecuteInvokeCallbackAbnormally();

        /// <summary>
        /// Set the cause if exception caught.
        /// </summary>
        System.Exception Cause {set;get;}


		/// <summary>
		/// Get the application callback of the future.
		/// </summary>
		/// <returns> get invoke callback </returns>
		InvokeCallback InvokeCallback {get;}

		/// <summary>
		/// Add timeout for the future.
		/// </summary>
		void addTimeout(ITimeout timeout);

		/// <summary>
		/// Cancel the timeout.
		/// </summary>
		void cancelTimeout();

		/// <summary>
		/// Whether the future is done.
		/// </summary>
		/// <returns> true if the future is done </returns>
		bool Done {get;}

		/// <summary>
		/// Get the protocol code of command.
		/// </summary>
		/// <returns> protocol code </returns>
		byte ProtocolCode {get;}

		/// <summary>
		/// set invoke context
		/// </summary>
		InvokeContext InvokeContext {set;get;}
	}
}