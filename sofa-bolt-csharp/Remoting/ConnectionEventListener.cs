using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Remoting
{
    /// <summary>
    /// Listen and dispatch connection events.
    /// </summary>
    public class ConnectionEventListener
    {
        private ConcurrentDictionary<ConnectionEventType, List<ConnectionEventProcessor>> processors = new ConcurrentDictionary<ConnectionEventType, List<ConnectionEventProcessor>>();

        /// <summary>
        /// Dispatch events.
        /// </summary>
        /// <param name="type"> ConnectionEventType </param>
        /// <param name="remoteAddress"> remoting address </param>
        /// <param name="connection"> Connection </param>
        public virtual void onEvent(ConnectionEventType type, string remoteAddress, Connection connection)
        {
            var isGetValue = processors.TryGetValue(type, out var processorList);
            if (isGetValue)
            {
                foreach (ConnectionEventProcessor processor in processorList)
                {
                    processor.onEvent(remoteAddress, connection);
                }
            }
        }

        /// <summary>
        /// Add event processor.
        /// </summary>
        /// <param name="type"> ConnectionEventType </param>
        /// <param name="processor"> ConnectionEventProcessor </param>
        public virtual void addConnectionEventProcessor(ConnectionEventType type, ConnectionEventProcessor processor)
        {
            var isGetValue = processors.TryGetValue(type, out var processorList);
            if (!isGetValue)
            {
                if (!processors.ContainsKey(type))
                {
                    processors.AddOrUpdate(type, new List<ConnectionEventProcessor>(1), (key, oldValue) => new List<ConnectionEventProcessor>(1));
                }
                processors.TryGetValue(type, out processorList);
            }
            processorList.Add(processor);
        }
    }
}