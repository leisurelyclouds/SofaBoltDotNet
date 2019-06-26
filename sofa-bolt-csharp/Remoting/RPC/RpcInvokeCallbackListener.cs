using java.lang;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting.exception;
using Remoting.rpc.exception;
using Remoting.rpc.protocol;
using java.util.concurrent;

namespace Remoting.rpc
{
    /// <summary>
    /// Listener which listens the Rpc invoke result, and then invokes the call back.
    /// 
    /// @author jiangping
    /// @version $Id: RpcInvokeCallbackListener.java, v 0.1 2015-9-30 AM10:36:34 tao Exp $
    /// </summary>
    public class RpcInvokeCallbackListener : InvokeCallbackListener
	{

		private static readonly ILogger logger = NullLogger.Instance;

		private string address;

		public RpcInvokeCallbackListener()
		{

		}

		public RpcInvokeCallbackListener(string address)
		{
			this.address = address;
		}

		/// <seealso cref= InvokeCallbackListener#onResponse(InvokeFuture) </seealso>
		public virtual void onResponse(InvokeFuture future)
		{
			InvokeCallback callback = future.InvokeCallback;
			if (callback != null)
			{
				CallbackTask task = new CallbackTask(this, RemoteAddress, future);
				if (callback.Executor != null)
				{
					// There is no need to switch classloader, because executor is provided by user.
					try
					{
						callback.Executor.execute(task);
					}
					catch (RejectedExecutionException)
					{
						logger.LogWarning("Callback thread pool busy.");
					}
				}
				else
				{
					task.run();
				}
			}
		}

		internal class CallbackTask : Runnable
		{
			private readonly RpcInvokeCallbackListener outerInstance;


			internal InvokeFuture future;
			internal string remoteAddress;

			/// 
			public CallbackTask(RpcInvokeCallbackListener outerInstance, string remoteAddress, InvokeFuture future)
			{
				this.outerInstance = outerInstance;
				this.remoteAddress = remoteAddress;
				this.future = future;
			}

            /// <seealso cref= Runnable#run() </seealso>
            public void run()
			{
				InvokeCallback callback = future.InvokeCallback;
				// a lot of try-catches to protect thread pool
				ResponseCommand response = null;

				try
				{
					response = (ResponseCommand) future.waitResponse(0);
				}
				catch (ThreadInterruptedException e)
				{
					string msg = "Exception caught when getting response from InvokeFuture. The address is " + remoteAddress;
					logger.LogError(msg, e);
				}
				if (response == null || response.ResponseStatus != ResponseStatus.SUCCESS)
				{
					try
					{
                        System.Exception e;
						if (response == null)
						{
							e = new InvokeException("Exception caught in invocation. The address is " + remoteAddress + " responseStatus:" + ResponseStatus.UNKNOWN, future.Cause);
						}
						else
						{
							response.InvokeContext = future.InvokeContext;
							switch (response.ResponseStatus)
							{
								case ResponseStatus.TIMEOUT:
									e = new InvokeTimeoutException("Invoke timeout when invoke with callback.The address is " + remoteAddress);
									break;
								case ResponseStatus.CONNECTION_CLOSED:
									e = new ConnectionClosedException("Connection closed when invoke with callback.The address is " + remoteAddress);
									break;
								case ResponseStatus.SERVER_THREADPOOL_BUSY:
									e = new InvokeServerBusyException("Server thread pool busy when invoke with callback.The address is " + remoteAddress);
									break;
								case ResponseStatus.SERVER_EXCEPTION:
									string msg = "Server exception when invoke with callback.Please check the server log! The address is " + remoteAddress;
									RpcResponseCommand resp = (RpcResponseCommand) response;
									resp.deserialize();
									object ex = resp.ResponseObject;
									if (ex != null && ex is System.Exception)
									{
										e = new InvokeServerException(msg, (System.Exception) ex);
									}
									else
									{
										e = new InvokeServerException(msg);
									}
									break;
								default:
									e = new InvokeException("Exception caught in invocation. The address is " + remoteAddress + " responseStatus:" + response.ResponseStatus, future.Cause);

								break;
							}
						}
						callback.onException(e);
					}
					catch (System.Exception e)
					{
						logger.LogError("Exception occurred in user defined InvokeCallback#onException() logic, The address is {}", remoteAddress, e);
					}
				}
				else
				{
					try
					{
						response.InvokeContext = future.InvokeContext;
						RpcResponseCommand rpcResponse = (RpcResponseCommand) response;
						response.deserialize();
						try
						{
							callback.onResponse(rpcResponse.ResponseObject);
						}
						catch (System.Exception e)
						{
							logger.LogError("Exception occurred in user defined InvokeCallback#onResponse() logic.", e);
						}
					}
					catch (CodecException e)
					{
						logger.LogError("CodecException caught on when deserialize response in RpcInvokeCallbackListener. The address is {}.", remoteAddress, e);
					}
					catch (System.Exception e)
					{
						logger.LogError("Exception caught in RpcInvokeCallbackListener. The address is {}", remoteAddress, e);
					}
					finally
					{
					}
				} // enf of else
			} // end of run
		}

		/// <seealso cref= InvokeCallbackListener#getRemoteAddress() </seealso>
		public virtual string RemoteAddress
		{
			get
			{
				return address;
			}
		}
	}
}