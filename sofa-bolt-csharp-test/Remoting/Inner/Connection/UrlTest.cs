using java.lang.@ref;
using Remoting;
using Remoting.rpc;
using System;
using System.Net;
using Xunit;
using Xunit.Abstractions;
using System.Reflection;

namespace com.alipay.remoting.inner.connection
{
    /// <summary>
    /// Test basic usage of url
    /// </summary>
    public class UrlTest
    {
        public UrlTest(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }
        private readonly ITestOutputHelper testOutputHelper;

        [Fact]
        public virtual void testUrlArgs()
        {
            Url url = new Url(IPAddress.Parse("127.0.0.1"), 1111);
            try
            {
                url.ConnNum = -1;
            }
            catch (Exception e)
            {
                testOutputHelper.WriteLine(e.Message);
                //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
                Assert.Equal(e.GetType().FullName, typeof(ArgumentException).FullName);
            }

            try
            {
                url.ConnectTimeout = -1;
            }
            catch (Exception e)
            {
                testOutputHelper.WriteLine(e.Message);
                Assert.Equal(e.GetType().FullName, typeof(ArgumentException).FullName);
            }
        }

        [Fact]
        public virtual void testUrlArgsEquals()
        {
            RpcAddressParser parser = new RpcAddressParser();

            string urlA = "localhost:3333?key1=value1";
            Url urlObjA = parser.parse(urlA);
            string urlB = "localhost:3333?key1=value1";
            Url urlObjB = parser.parse(urlB);
            string urlC = "localhost:3333?key1=value2";
            Url urlObjC = parser.parse(urlC);

            Assert.Equal(urlObjA, urlObjB);
            Assert.Equal(urlObjA.GetHashCode(), urlObjB.GetHashCode());
            Assert.False(urlObjA.Equals(urlObjC));
            Assert.False(urlObjA.GetHashCode() == urlObjC.GetHashCode());
        }

        [Fact]
        public virtual void testGC()
        {
            string url = "localhost:3333?k1=v1&k2=v2";

            var con = typeof(Url).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(string) }, null);

            long start = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            long MAX_TIME_ELAPSED = 10 * 1000;
            while (!Url.isCollected)
            {
                Url urlObject = (Url)con.Invoke(new object[] { url });
                Url.parsedUrls.AddOrUpdate(url, new SoftReference(urlObject), (key, oldValue) => new SoftReference(urlObject));

                if ((new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() - start) > MAX_TIME_ELAPSED)
                {
                    Assert.Null("GC should have already been called!");
                    break;
                }
            }
        }

        /// <summary>
        /// -server -Xms20m -Xmx20m -Xmn3m -Xss256k -XX:PermSize=20m -XX:MaxPermSize=20m
        /// -XX:+UseConcMarkSweepGC -XX:+UseParNewGC -XX:+CMSClassUnloadingEnabled
        /// -XX:+DisableExplicitGC -XX:+UseCMSInitiatingOccupancyOnly
        /// -XX:CMSInitiatingOccupancyFraction=68 -verbose:gc -XX:+PrintGCDetails
        /// -XX:+PrintGCDateStamps -XX:+HeapDumpOnOutOfMemoryError -XX:HeapDumpPath=/Users/tsui/logs/
        /// </summary>
        /// <param name="args"> </param>
        //public static void Main(string[] args)
        //{
        //        ConcurrentHashMap<String, SoftReference<String>> parsedUrls = new ConcurrentHashMap<String, SoftReference<String>>();
        //        int MAX = Integer.valueOf(args[0]);
        //        for (int i = 0; i < MAX; i++) {
        //            parsedUrls.put("test" + i, new SoftReference<String>(i + "hehe"));
        //        }
        //        Collection<SoftReference<String>> urls = parsedUrls.values();
        //        for (SoftReference<String> url : urls) {
        //            System.out.println(url.get());
        //        }
        //}
    }

}