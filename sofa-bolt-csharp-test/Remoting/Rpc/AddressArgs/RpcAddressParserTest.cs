using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remoting;
using Remoting.rpc;
using System.Net;
using Xunit;

namespace com.alipay.remoting.rpc.addressargs
{
    /// <summary>
    /// rpc address parser test
    /// </summary>
    [Collection("Sequential")]
    public class RpcAddressParserTest
    {
		internal static ILogger logger = NullLogger.Instance;

        [Fact]
		public virtual void testParserNonProtocol()
		{
			string url = "127.0.0.1:1111?_TIMEOUT=3000&_SERIALIZETYPE=hessian2";
			RpcAddressParser parser = new RpcAddressParser();
			Url btUrl = parser.parse(url);
			Assert.Equal(IPAddress.Parse("127.0.0.1"), btUrl.Ip);
			Assert.Equal(1111, btUrl.Port);
			Assert.Null(btUrl.getProperty(RpcConfigs.URL_PROTOCOL));

			url = "127.0.0.1:1111";
			btUrl = parser.parse(url);
			Assert.Equal(IPAddress.Parse("127.0.0.1"), btUrl.Ip);
			Assert.Equal(1111, btUrl.Port);
			Assert.Null(btUrl.getProperty(RpcConfigs.URL_PROTOCOL));
		}

        [Fact]
		public virtual void testParserWithProtocol()
		{
			string url = "127.0.0.1:1111?_TIMEOUT=3000&_SERIALIZETYPE=hessian2&_PROTOCOL=1";
			RpcAddressParser parser = new RpcAddressParser();
			Url btUrl = parser.parse(url);

			Assert.Equal(IPAddress.Parse("127.0.0.1"), btUrl.Ip);
			Assert.Equal(1111, btUrl.Port);
			Assert.Equal("1", btUrl.getProperty(RpcConfigs.URL_PROTOCOL));

			url = "127.0.0.1:1111?protocol=1";
			Assert.Equal(IPAddress.Parse("127.0.0.1"), btUrl.Ip);
			Assert.Equal(1111, btUrl.Port);
			Assert.Equal("1", btUrl.getProperty(RpcConfigs.URL_PROTOCOL));
		}

        [Fact]
		public virtual void testParserConnectTimeout()
		{
			string url = "127.0.0.1:1111?_CONNECTTIMEOUT=2000&_TIMEOUT=3000&_SERIALIZETYPE=hessian2&_PROTOCOL=1";
			RpcAddressParser parser = new RpcAddressParser();
			Url btUrl = parser.parse(url);
			Assert.Equal(IPAddress.Parse("127.0.0.1"), btUrl.Ip);
			Assert.Equal(1111, btUrl.Port);
			Assert.Equal("1", btUrl.getProperty(RpcConfigs.URL_PROTOCOL));
			Assert.Equal("2000", btUrl.getProperty(RpcConfigs.CONNECT_TIMEOUT_KEY));
		}

        [Fact]
		public virtual void testUrlIllegal()
		{
			Url btUrl = null;
			try
			{
				string url = "127.0.0.1";
				RpcAddressParser parser = new RpcAddressParser();
				btUrl = parser.parse(url);
				Assert.Null("Should not reach here!");
			}
			catch (System.ArgumentException e)
			{
				logger.LogError("URL FORMAT ERROR!", e);
				Assert.Null(btUrl);
			}

			btUrl = null;
			try
			{
				string url = "127.0.0.1:";
				RpcAddressParser parser = new RpcAddressParser();
				btUrl = parser.parse(url);
				Assert.Null("Should not reach here!");
			}
			catch (System.ArgumentException e)
			{
				logger.LogError("URL FORMAT ERROR!", e);
				Assert.Null(btUrl);
			}

			btUrl = null;
			try
			{
				string url = "127.0.0.1:1111?_CONNECTTIMEOUT";
				RpcAddressParser parser = new RpcAddressParser();
				btUrl = parser.parse(url);
				Assert.Null("Should not reach here!");
			}
			catch (System.ArgumentException e)
			{
				logger.LogError("URL FORMAT ERROR!", e);
				Assert.Null(btUrl);
			}

			btUrl = null;
			try
			{
				string url = "127.0.0.1:100?";
				RpcAddressParser parser = new RpcAddressParser();
				btUrl = parser.parse(url);
				Assert.Null("Should not reach here!");
			}
			catch (System.ArgumentException e)
			{
				logger.LogError("URL FORMAT ERROR!", e);
				Assert.Null(btUrl);
			}

			btUrl = null;
			try
			{
				string url = "127.0.1:100?A=B&";
				RpcAddressParser parser = new RpcAddressParser();
				btUrl = parser.parse(url);
				Assert.Null("Should not reach here!");
			}
			catch (System.ArgumentException e)
			{
				logger.LogError("URL FORMAT ERROR!", e);
				Assert.Null(btUrl);
			}

			btUrl = null;
			try
			{
				string url = "127.0.1:100?A=B&C";
				RpcAddressParser parser = new RpcAddressParser();
				btUrl = parser.parse(url);
				Assert.Null("Should not reach here!");
			}
			catch (System.ArgumentException e)
			{
				logger.LogError("URL FORMAT ERROR!", e);
				Assert.Null(btUrl);
			}

			btUrl = null;
			try
			{
				string url = "127.0.1:100?A=B&C=";
				RpcAddressParser parser = new RpcAddressParser();
				btUrl = parser.parse(url);
				Assert.Null("Should not reach here!");
			}
			catch (System.ArgumentException e)
			{
				logger.LogError("URL FORMAT ERROR!", e);
				Assert.Null(btUrl);
			}
		}

        [Fact]
		public virtual void testArgsIllegal()
		{
            Url btUrl;
            try
			{
				string url = "127.0.0.1:1111?_CONNECTTIMEOUT=-1&_TIMEOUT=3000&_SERIALIZETYPE=hessian2&_PROTOCOL=1";
				RpcAddressParser parser = new RpcAddressParser();
				btUrl = parser.parse(url);
				parser.initUrlArgs(btUrl);
				Assert.Null("Should not reach here!");
			}
			catch (System.ArgumentException e)
			{
				logger.LogError("URL ARGS ILLEAGL!", e);
			}

			try
			{
				string url = "127.0.0.1:1111?_CONNECTTIMEOUT=1000&_TIMEOUT=3000&_SERIALIZETYPE=hessian2&_PROTOCOL=-11";
				RpcAddressParser parser = new RpcAddressParser();
				btUrl = parser.parse(url);
				parser.initUrlArgs(btUrl);
				Assert.Null("Should not reach here!");
			}
			catch (System.ArgumentException e)
			{
				logger.LogError("URL ARGS ILLEAGL!", e);
			}

            try
            {
				string url = "127.0.0.1:1111?_CONNECTTIMEOUT=1000&_TIMEOUT=3000&_SERIALIZETYPE=hessian2&_CONNECTIONNUM=-11";
				RpcAddressParser parser = new RpcAddressParser();
				btUrl = parser.parse(url);
				parser.initUrlArgs(btUrl);
				Assert.Null("Should not reach here!");
			}
			catch (System.ArgumentException e)
			{
				logger.LogError("URL ARGS ILLEAGL!", e);
			}
		}
	}

}