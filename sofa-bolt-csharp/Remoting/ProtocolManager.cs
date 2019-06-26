using System;
using System.Collections.Concurrent;

namespace Remoting
{
    /// <summary>
    /// Manager of all protocols
    /// </summary>
    public class ProtocolManager
    {
        private static readonly ConcurrentDictionary<ProtocolCode, Protocol> protocols = new ConcurrentDictionary<ProtocolCode, Protocol>();

        public static Protocol getProtocol(ProtocolCode protocolCode)
        {
            protocols.TryGetValue(protocolCode, out var protocol);
            return protocol;
        }

        public static void registerProtocol(Protocol protocol, params byte[] protocolCodeBytes)
        {
            registerProtocol(protocol, ProtocolCode.fromBytes(protocolCodeBytes));
        }

        public static void registerProtocol(Protocol protocol, ProtocolCode protocolCode)
        {
            if (null == protocolCode || null == protocol)
            {
                throw new Exception("Protocol: " + protocol + " and protocol code:" + protocolCode + " should not be null!");
            }
            if (protocols.ContainsKey(protocolCode))
            {
                throw new Exception("Protocol for code: " + protocolCode + " already exists!");
            }
            else
            {
                protocols.TryAdd(protocolCode, protocol);
            }
        }

        public static Protocol unRegisterProtocol(byte protocolCode)
        {
            protocols.TryRemove(ProtocolCode.fromBytes(protocolCode), out var protocol);
            return protocol;
        }
    }
}