using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;

namespace Remoting.util
{
    /// <summary>
    /// Trace log util
    /// </summary>
    public class TraceLogUtil
    {
        /// <summary>
        /// print trace log
		/// </summary>
        /// <param name="traceId"> </param>
        /// <param name="invokeContext"> </param>
        public static void printConnectionTraceLog(ILogger logger, string traceId, InvokeContext invokeContext)
        {
            var sourceIp = (IPAddress)invokeContext.get(InvokeContext.CLIENT_LOCAL_IP);
            int? sourcePort = (int?)invokeContext.get(InvokeContext.CLIENT_LOCAL_PORT);
            var targetIp = (IPAddress)invokeContext.get(InvokeContext.CLIENT_REMOTE_IP);
            int? targetPort = (int?)invokeContext.get(InvokeContext.CLIENT_REMOTE_PORT);
            StringBuilder logMsg = new StringBuilder();
            logMsg.Append(traceId).Append(",");
            logMsg.Append(sourceIp).Append(",");
            logMsg.Append(sourcePort).Append(",");
            logMsg.Append(targetIp).Append(",");
            logMsg.Append(targetPort);
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(logMsg.ToString());
            }
        }
    }
}