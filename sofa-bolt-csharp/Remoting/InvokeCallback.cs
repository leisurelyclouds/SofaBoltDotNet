using java.util.concurrent;
using System;

namespace Remoting
{

    /// <summary>
    /// Invoke callback.
    /// </summary>
    public interface InvokeCallback
	{

		/// <summary>
		/// Response received.
		/// </summary>
		/// <param name="result"> </param>
		void onResponse(object result);

		/// <summary>
		/// Exception caught.
		/// </summary>
		/// <param name="e"> </param>
		void onException(Exception e);

		/// <summary>
		/// User defined executor.
		/// 
		/// @return
		/// </summary>
		Executor Executor {get;}
	}
}