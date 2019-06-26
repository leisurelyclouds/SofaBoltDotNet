using java.lang.@ref;
using java.util;
using Remoting.Config;
using Remoting.rpc.protocol;
using SofaBoltCsharp;
using System;
using System.Net;

namespace Remoting.rpc
{
    /// <summary>
    /// This is address parser for RPC.
    /// <h3>Normal format</h3>
    /// <pre>host:port?paramkey1=paramvalue1&amp;paramkey2=paramvalue2</pre>
    /// 
    /// <h4>Normal format example</h4>
    /// <pre>127.0.0.1:12200?KEY1=VALUE1&KEY2=VALUE2</pre>
    /// 
    /// <h4>Illegal format example</h4>
    /// <pre>
    /// 127.0.0.1
    /// 127.0.0.1:
    /// 127.0.0.1:12200?
    /// 127.0.0.1:12200?key1=
    /// 127.0.0.1:12200?key1=value1&
    /// </pre>
    /// </summary>
    public class RpcAddressParser : RemotingAddressParser
    {
        public virtual Url parse(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("Illegal format address string [" + url + "], should not be blank! ");
            }
            Url parsedUrl = tryGet(url);
            if (null != parsedUrl)
            {
                return parsedUrl;
            }
            IPAddress ip = null;
            int port = 0;
            Properties properties = null;

            int size = url.Length;
            int pos = 0;
            //for (int i = 0; i < size; ++i)
            //{
            //    if (RemotingAddressParser_Fields.COLON == url[i])
            //    {
            //        ip = url.Substring(pos, i - pos);
            //        pos = i;
            //        // should not end with COLON
            //        if (i == size - 1)
            //        {
            //            throw new ArgumentException("Illegal format address string [" + url + "], should not end with COLON[:]! ");
            //        }
            //        break;
            //    }
            //    // must have one COLON
            //    if (i == size - 1)
            //    {
            //        throw new ArgumentException("Illegal format address string [" + url + "], must have one COLON[:]! ");
            //    }
            //}

            for (int i = 0; i < size; ++i)
            {
                if (RemotingAddressParser_Fields.QUES == url[i])
                {
                    var ipEndPointString = url.Substring(pos, i - pos);
                    IPEndPointParser.TryParse(ipEndPointString, out var ipEndPoint);
                    ip = ipEndPoint.Address;
                    port = ipEndPoint.Port;
                    pos = i;
                    if (i == size - 1)
                    {
                        // should not end with QUES
                        throw new ArgumentException("Illegal format address string [" + url + "], should not end with QUES[?]! ");
                    }
                    break;
                }
                // end without a QUES
                if (i == size - 1)
                {
                    var ipEndPointString = url.Substring(pos, i - pos + 1);
                    var ipOrHostAndPortSplitIndex = ipEndPointString.LastIndexOf(':');

                    var ipOrHost = ipEndPointString.Substring(0, ipOrHostAndPortSplitIndex);
                    var portString = ipEndPointString.Substring(ipOrHostAndPortSplitIndex+1);
                    if (int.TryParse(portString, out var portParsed))
                    {
                        port = portParsed;
                    }
                    else
                    {
                        throw new ArgumentException("Illegal format address string [" + url + "], must have a valid port! ");
                    }

                    if (port <= 0)
                    {
                        throw new ArgumentException("Illegal format address string [" + url + "], must have a valid port! ");
                    }

                    if (IPAddress.TryParse(ipOrHost, out var ipAddress))
                    {
                        ip = ipAddress;
                    }
                    else
                    {
                        var addresses = Dns.GetHostAddresses(ipOrHost);
                        if (addresses.Length == 0)
                        {
                            throw new ArgumentException("Unable to retrieve address from specified host name!");
                        }
                        ip = addresses[0];
                    }

                    pos = size;
                }
            }

            if (pos < (size - 1))
            {
                properties = new Properties();
                while (pos < (size - 1))
                {
                    string key = null;
                    string value = null;
                    for (int i = pos; i < size; ++i)
                    {
                        if (RemotingAddressParser_Fields.EQUAL == url[i])
                        {
                            key = url.Substring(pos + 1, i - (pos + 1));
                            pos = i;
                            if (i == size - 1)
                            {
                                // should not end with EQUAL
                                throw new ArgumentException("Illegal format address string [" + url + "], should not end with EQUAL[=]! ");
                            }
                            break;
                        }
                        if (i == size - 1)
                        {
                            // must have one EQUAL
                            throw new ArgumentException("Illegal format address string [" + url + "], must have one EQUAL[=]! ");
                        }
                    }
                    for (int i = pos; i < size; ++i)
                    {
                        if (RemotingAddressParser_Fields.AND == url[i])
                        {
                            value = url.Substring(pos + 1, i - (pos + 1));
                            pos = i;
                            if (i == size - 1)
                            {
                                // should not end with AND
                                throw new ArgumentException("Illegal format address string [" + url + "], should not end with AND[&]! ");
                            }
                            break;
                        }
                        // end without more AND
                        if (i == size - 1)
                        {
                            value = url.Substring(pos + 1, i + 1 - (pos + 1));
                            pos = size;
                        }
                    }
                    properties.put(key, value);
                }
            }
            parsedUrl = new Url(url, ip, port, properties);
            initUrlArgs(parsedUrl);
            Url.parsedUrls.AddOrUpdate(url, new SoftReference(parsedUrl), (key, oldValue) => new SoftReference(parsedUrl));
            return parsedUrl;
        }

        /// <seealso cref= RemotingAddressParser#parseUniqueKey(java.lang.String) </seealso>
        public virtual string parseUniqueKey(string url)
        {
            bool illegal = false;
            if (string.IsNullOrWhiteSpace(url))
            {
                illegal = true;
            }

            string uniqueKey = string.Empty;
            string addr = url.Trim();
            string[] sectors = addr.Split(RemotingAddressParser_Fields.QUES);
            if (!illegal && sectors.Length == 2 && !string.IsNullOrWhiteSpace(sectors[0]))
            {
                uniqueKey = sectors[0].Trim();
            }
            else
            {
                illegal = true;
            }

            if (illegal)
            {
                throw new ArgumentException("Illegal format address string: " + url);
            }
            return uniqueKey;
        }

        /// <seealso cref= RemotingAddressParser#parseProperty(java.lang.String, java.lang.String) </seealso>
        public virtual string parseProperty(string addr, string propKey)
        {
            if (addr.Contains("?") && !addr.EndsWith("?", StringComparison.Ordinal))
            {
                string part = addr.Split(@"\?")[1];
                foreach (string item in part.Split('&'))
                {
                    string[] kv = item.Split('=');
                    string k = kv[0];
                    if (k.Equals(propKey))
                    {
                        return kv[1];
                    }
                }
            }
            return null;
        }

        /// <seealso cref= RemotingAddressParser#initUrlArgs(Url) </seealso>
        public virtual void initUrlArgs(Url url)
        {
            string connTimeoutStr = url.getProperty(RpcConfigs.CONNECT_TIMEOUT_KEY);
            int connTimeout = Configs.DEFAULT_CONNECT_TIMEOUT;
            if (!string.IsNullOrWhiteSpace(connTimeoutStr))
            {
                if (int.TryParse(connTimeoutStr, out connTimeout))
                {
                }
                else
                {
                    throw new ArgumentException("Url args illegal value of key [" + RpcConfigs.CONNECT_TIMEOUT_KEY + "] must be positive integer! The origin url is [" + url.OriginUrl + "]");
                }
            }
            url.ConnectTimeout = connTimeout;

            string protocolStr = url.getProperty(RpcConfigs.URL_PROTOCOL);
            byte protocol = RpcProtocol.PROTOCOL_CODE;
            if (!string.IsNullOrWhiteSpace(protocolStr))
            {
                if (byte.TryParse(protocolStr, out protocol))
                {
                }
                else
                {
                    throw new ArgumentException("Url args illegal value of key [" + RpcConfigs.URL_PROTOCOL + "] must be positive integer! The origin url is [" + url.OriginUrl + "]");
                }
            }
            url.Protocol = protocol;

            string versionStr = url.getProperty(RpcConfigs.URL_VERSION);
            byte version = RpcProtocolV2.PROTOCOL_VERSION_1;
            if (!string.IsNullOrWhiteSpace(versionStr))
            {
                if (byte.TryParse(versionStr, out version))
                {
                }
                else
                {
                    throw new ArgumentException("Url args illegal value of key [" + RpcConfigs.URL_VERSION + "] must be positive integer! The origin url is [" + url.OriginUrl + "]");
                }
            }
            url.Version = version;

            string connNumStr = url.getProperty(RpcConfigs.CONNECTION_NUM_KEY);
            int connNum = Configs.DEFAULT_CONN_NUM_PER_URL;
            if (!string.IsNullOrWhiteSpace(connNumStr))
            {
                if (int.TryParse(connNumStr, out connNum))
                {
                }
                else
                {
                    throw new ArgumentException("Url args illegal value of key [" + RpcConfigs.CONNECTION_NUM_KEY + "] must be positive integer! The origin url is [" + url.OriginUrl + "]");
                }
            }
            url.ConnNum = connNum;

            string connWarmupStr = url.getProperty(RpcConfigs.CONNECTION_WARMUP_KEY);
            bool connWarmup = false;
            if (!string.IsNullOrWhiteSpace(connWarmupStr))
            {
                connWarmup = bool.Parse(connWarmupStr);
            }
            url.ConnWarmup = connWarmup;
        }

        /// <summary>
        /// try get from cache
        /// </summary>
        private Url tryGet(string url)
        {
            Url.parsedUrls.TryGetValue(url, out var softReference);
            return (null == softReference) ? null : (Url)softReference.get();
        }
    }
}