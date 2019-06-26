using java.lang.@ref;
using java.util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting.Config;
using Remoting.rpc.protocol;
using System;
using System.Collections.Concurrent;
using System.Net;

namespace Remoting
{
    /// <summary>
    /// URL definition.
    /// </summary>
    public class Url
    {
        /// <summary>
        /// origin url
		/// </summary>
        private string originUrl;

        /// <summary>
        /// ip, can be number format or hostname format
		/// </summary>
        private IPAddress ip;

        /// <summary>
        /// port, should be integer between (0, 65535]
		/// </summary>
        private int port;

        /// <summary>
        /// unique key of this url
		/// </summary>
        private string uniqueKey;

        /// <summary>
        /// URL args: timeout value when do connect
		/// </summary>
        private int connectTimeout;

        /// <summary>
        /// URL args: protocol
		/// </summary>
        private byte protocol;

        /// <summary>
        /// URL args: version
		/// </summary>
        private byte version = RpcProtocolV2.PROTOCOL_VERSION_1;

        /// <summary>
        /// URL agrs: connection number
		/// </summary>
        private int connNum;

        /// <summary>
        /// URL agrs: whether need warm up connection
		/// </summary>
        private bool connWarmup;

        /// <summary>
        /// URL agrs: all parsed args of each originUrl
		/// </summary>
        private Properties properties;

        /// <summary>
        /// Constructor with originUrl
        /// </summary>
        /// <param name="originUrl"> </param>
        protected internal Url(string originUrl)
        {
            this.originUrl = originUrl;
        }

        /// <summary>
        /// Constructor with ip and port
        /// <ul>
        /// <li>Initialize ip:port as <seealso cref="Url#originUrl"/> </li>
        /// <li>Initialize <seealso cref="Url#originUrl"/> as <seealso cref="Url#uniqueKey"/> </li>
        /// </ul> 
        /// </summary>
        /// <param name="ip"> </param>
        /// <param name="port"> </param>
        public Url(IPAddress ip, int port) : this(ip + ":" + port)
        {
            this.ip = ip;
            this.port = port;
            uniqueKey = originUrl;
        }

        /// <summary>
        /// Constructor with originUrl, ip and port
        /// 
        /// <ul>
		/// </summary>
        /// <li>Initialize <param name="originUrl"> as <seealso cref="Url#originUrl"/> </li>
        /// <li>Initialize ip:port as <seealso cref="Url#uniqueKey"/> </li>
        /// </ul> 
        /// </param>
        /// <param name="originUrl"> </param>
        /// <param name="ip"> </param>
        /// <param name="port"> </param>
        public Url(string originUrl, IPAddress ip, int port) : this(originUrl)
        {
            this.ip = ip;
            this.port = port;
            uniqueKey = ip + ":" + port;
        }

        /// <summary>
        /// Constructor with originUrl, ip, port and properties
        /// 
        /// <ul>
		/// </summary>
        /// <li>Initialize <param name="originUrl"> as <seealso cref="Url#originUrl"/> </li>
        /// <li>Initialize ip:port as <seealso cref="Url#uniqueKey"/> </li> </param>
        /// <li>Initialize <param name="properties"> as <seealso cref="Url#properties"/> </li>
        /// </ul> 
        /// </param>
        /// <param name="originUrl"> </param>
        /// <param name="ip"> </param>
        /// <param name="port"> </param>
        /// <param name="properties"> </param>
        public Url(string originUrl, IPAddress ip, int port, Properties properties) : this(originUrl, ip, port)
        {
            this.properties = properties;
        }

        /// <summary>
        /// Constructor with originUrl, ip, port, uniqueKey and properties
        /// 
        /// <ul>
		/// </summary>
        /// <li>Initialize <param name="originUrl"> as <seealso cref="Url#originUrl"/> </li> </param>
        /// <li>Initialize <param name="uniqueKey"> as <seealso cref="Url#uniqueKey"/> </li> </param>
        /// <li>Initialize <param name="properties"> as <seealso cref="Url#properties"/> </li>
        /// </ul>
        /// </param>
        /// <param name="originUrl"> </param>
        /// <param name="ip"> </param>
        /// <param name="port"> </param>
        /// <param name="uniqueKey"> </param>
        /// <param name="properties"> </param>
        public Url(string originUrl, IPAddress ip, int port, string uniqueKey, Properties properties) : this(originUrl, ip, port)
        {
            this.uniqueKey = uniqueKey;
            this.properties = properties;
        }

        /// <summary>
        /// Get property value according to property key
        /// </summary>
        /// <param name="key"> property key </param>
        /// <returns> property value </returns>
        public virtual string getProperty(string key)
        {
            if (properties == null)
            {
                return null;
            }
            return properties.getProperty(key);
        }

        public virtual string OriginUrl
        {
            get
            {
                return originUrl;
            }
        }

        public virtual IPAddress Ip
        {
            get
            {
                return ip;
            }
        }

        public virtual int Port
        {
            get
            {
                return port;
            }
        }

        public virtual string UniqueKey
        {
            get
            {
                return uniqueKey;
            }
        }

        public virtual int ConnectTimeout
        {
            get
            {
                return connectTimeout;
            }
            set
            {
                if (value <= 0)
                {
                    throw new System.ArgumentException("Illegal value of connection number [" + connNum + "], must be a positive integer].");
                }
                connectTimeout = value;
            }
        }


        public virtual byte Protocol
        {
            get
            {
                return protocol;
            }
            set
            {
                protocol = value;
            }
        }


        public virtual int ConnNum
        {
            get
            {
                return connNum;
            }
            set
            {
                if (value <= 0 || value > Configs.MAX_CONN_NUM_PER_URL)
                {
                    throw new System.ArgumentException("Illegal value of connection number [" + value + "], must be an integer between [" + Configs.DEFAULT_CONN_NUM_PER_URL + ", " + Configs.MAX_CONN_NUM_PER_URL + "].");
                }
                connNum = value;
            }
        }


        public virtual bool ConnWarmup
        {
            get
            {
                return connWarmup;
            }
            set
            {
                connWarmup = value;
            }
        }


        public virtual Properties Properties
        {
            get
            {
                return properties;
            }
        }

        public override sealed bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj == null)
            {
                return false;
            }
            if (GetType() != obj.GetType())
            {
                return false;
            }
            Url url = (Url)obj;
            if (OriginUrl.Equals(url.OriginUrl))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            const int prime = 31;
            int result = 1;
            result = prime * result + (ReferenceEquals(OriginUrl, null) ? 0 : OriginUrl.GetHashCode());
            return result;
        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("Origin url [").Append(originUrl).Append("], Unique key [").Append(uniqueKey).Append("].");
            return sb.ToString();
        }

        /// <summary>
        /// Use <seealso cref="SoftReference"/> to cache parsed urls. Key is the original url. </summary>
        public static ConcurrentDictionary<string, SoftReference> parsedUrls = new ConcurrentDictionary<string, SoftReference>();

        /// <summary>
        /// for unit test only, indicate this object have already been GCed
		/// </summary>
        public static volatile bool isCollected = false;

        /// <summary>
        /// logger
		/// </summary>
        private static readonly ILogger logger = NullLogger.Instance;

        ~Url()
        {
            try
            {
                isCollected = true;
                parsedUrls.TryRemove(OriginUrl, out _);
            }
            catch (Exception e)
            {
                logger.LogError("Exception occurred when do finalize for Url [{}].", OriginUrl, e);
            }
        }

        public virtual byte Version
        {
            get
            {
                return version;
            }
            set
            {
                version = value;
            }
        }
    }
}