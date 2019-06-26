using System.Collections.Generic;

namespace Remoting
{
    /// <summary>
    /// Select strategy from connection pool
    /// </summary>
    public interface ConnectionSelectStrategy
	{
		/// <summary>
		/// select strategy
		/// </summary>
		/// <param name="connections"> source connections </param>
		/// <returns> selected connection </returns>
		Connection select(List<Connection> connections);
	}
}