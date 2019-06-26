using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting.exception;
using Remoting.rpc.exception;
using Remoting.rpc.protocol;
using System;

namespace Remoting.rpc
{
    /// <summary>
    /// Resolve response object from response command.
    /// </summary>
    public class RpcResponseResolver
	{
		private static readonly ILogger logger = NullLogger.Instance;

		/// <summary>
		/// Analyze the response command and generate the response object.
		/// </summary>
		/// <param name="responseCommand"> response command </param>
		/// <param name="addr"> response address </param>
		/// <returns> response object </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static Object resolveResponseObject(ResponseCommand responseCommand, String addr) throws exception.RemotingException
		public static object resolveResponseObject(ResponseCommand responseCommand, string addr)
		{
			preProcess(responseCommand, addr);
			if (responseCommand.ResponseStatus == ResponseStatus.SUCCESS)
			{
				return toResponseObject(responseCommand);
			}
			else
			{
				string msg = string.Format("Rpc invocation exception: {0}, the address is {1}, id={2}", responseCommand.ResponseStatus, addr, responseCommand.Id);
				logger.LogWarning(msg);
				if (responseCommand.Cause != null)
				{
					throw new InvokeException(msg, responseCommand.Cause);
				}
				else
				{
					throw new InvokeException(msg + ", please check the server log for more.");
				}
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void preProcess(ResponseCommand responseCommand, String addr) throws exception.RemotingException
		private static void preProcess(ResponseCommand responseCommand, string addr)
		{
			RemotingException e = null;
			string msg = null;
			if (responseCommand == null)
			{
				msg = string.Format("Rpc invocation timeout[responseCommand null]! the address is {0}", addr);
				e = new InvokeTimeoutException(msg);
			}
			else
			{
				switch (responseCommand.ResponseStatus)
				{
					case ResponseStatus.TIMEOUT:
						msg = string.Format("Rpc invocation timeout[responseCommand TIMEOUT]! the address is {0}", addr);
						e = new InvokeTimeoutException(msg);
						break;
					case ResponseStatus.CLIENT_SEND_ERROR:
						msg = string.Format("Rpc invocation send failed! the address is {0}", addr);
						e = new InvokeSendFailedException(msg, responseCommand.Cause);
						break;
					case ResponseStatus.CONNECTION_CLOSED:
						msg = string.Format("Connection closed! the address is {0}", addr);
						e = new ConnectionClosedException(msg);
						break;
					case ResponseStatus.SERVER_THREADPOOL_BUSY:
						msg = string.Format("Server thread pool busy! the address is {0}, id={1}", addr, responseCommand.Id);
						e = new InvokeServerBusyException(msg);
						break;
					case ResponseStatus.CODEC_EXCEPTION:
						msg = string.Format("Codec exception! the address is {0}, id={1}", addr, responseCommand.Id);
						e = new CodecException(msg);
						break;
					case ResponseStatus.SERVER_SERIAL_EXCEPTION:
						msg = string.Format("Server serialize response exception! the address is {0}, id={1}, serverSide=true", addr, responseCommand.Id);
						e = new SerializationException(detailErrMsg(msg, responseCommand), toThrowable(responseCommand), true);
						break;
					case ResponseStatus.SERVER_DESERIAL_EXCEPTION:
						msg = string.Format("Server deserialize request exception! the address is {0}, id={1}, serverSide=true", addr, responseCommand.Id);
						e = new DeserializationException(detailErrMsg(msg, responseCommand), toThrowable(responseCommand), true);
						break;
					case ResponseStatus.SERVER_EXCEPTION:
						msg = string.Format("Server exception! Please check the server log, the address is {0}, id={1}", addr, responseCommand.Id);
						e = new InvokeServerException(detailErrMsg(msg, responseCommand), toThrowable(responseCommand));
						break;
					default:
						break;
				}
			}
			if (!string.IsNullOrWhiteSpace(msg))
			{
				logger.LogWarning(msg);
			}
			if (null != e)
			{
				throw e;
			}
		}

		/// <summary>
		/// Convert remoting response command to application response object.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static Object toResponseObject(ResponseCommand responseCommand) throws exception.CodecException
		private static object toResponseObject(ResponseCommand responseCommand)
		{
			RpcResponseCommand response = (RpcResponseCommand) responseCommand;
			response.deserialize();
			return response.ResponseObject;
		}

		/// <summary>
		/// Convert remoting response command to throwable if it is a throwable, otherwise return null.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static Throwable toThrowable(ResponseCommand responseCommand) throws exception.CodecException
		private static Exception toThrowable(ResponseCommand responseCommand)
		{
			RpcResponseCommand resp = (RpcResponseCommand) responseCommand;
			resp.deserialize();
			object ex = resp.ResponseObject;
			if (ex != null && ex is Exception)
			{
				return (Exception) ex;
			}
			return null;
		}

		/// <summary>
		/// Detail your error msg with the error msg returned from response command
		/// </summary>
		private static string detailErrMsg(string clientErrMsg, ResponseCommand responseCommand)
		{
			RpcResponseCommand resp = (RpcResponseCommand) responseCommand;
			if (!string.IsNullOrWhiteSpace(resp.ErrorMsg))
			{
				return string.Format("{0}, ServerErrorMsg:{1}", clientErrMsg, resp.ErrorMsg);
			}
			else
			{
				return string.Format("{0}, ServerErrorMsg:null", clientErrMsg);
			}
		}
	}

}