using Remoting.util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Remoting
{
    /// <summary>
    /// The strategy of connection monitor
    /// </summary>
    public interface ConnectionMonitorStrategy
	{
		/// <summary>
		/// Filter connections to monitor
		/// 
		/// Deprecated this method, this should be a private method.
		/// </summary>
		/// <param name="connections"> connections from a connection pool </param>
		[Obsolete]
        ConcurrentDictionary<string, List<Connection>> filter(List<Connection> connections);

		/// <summary>
		/// Add a set of connections to monitor.
		/// <para>
		/// The previous connections in monitor of this protocol,
		/// will be dropped by monitor automatically.
		/// 
		/// </para>
		/// </summary>
		/// <param name="connPools"> connection pools </param>
		void monitor(ConcurrentDictionary<string, RunStateRecordedFutureTask> connPools);
	}

}