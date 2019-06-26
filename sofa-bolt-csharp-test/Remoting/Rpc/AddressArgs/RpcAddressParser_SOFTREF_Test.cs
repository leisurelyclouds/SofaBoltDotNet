using java.lang;
using Remoting;
using Remoting.rpc;
using System;
using Xunit;
using Xunit.Abstractions;

namespace com.alipay.remoting.rpc.addressargs
{
    /// <summary>
    /// rpc address parser
    /// test soft reference
    /// </summary>
    [Collection("Sequential")]
    public class RpcAddressParser_SOFTREF_Test
    {
        private readonly ITestOutputHelper testOutputHelper;

        public RpcAddressParser_SOFTREF_Test(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public virtual void testParserNonProtocol()
        {
            string url = "127.0.0.1:1111?_TIMEOUT=3000&_SERIALIZETYPE=hessian2";
            RpcAddressParser parser = new RpcAddressParser();
            int MAX = 1000000;

            printMemory();
            long start1 = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            for (int i = 0; i < MAX; ++i)
            {
                Url btUrl = parser.parse(url);
                Assert.Equal("127.0.0.1:1111", btUrl.UniqueKey);
            }
            long end1 = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            long time1 = end1 - start1;
            testOutputHelper.WriteLine("time1:" + time1);
            printMemory();
        }

        private void printMemory()
        {
            int mb = 1024 * 1024;
            Runtime rt = Runtime.getRuntime();
            long total = rt.totalMemory();
            long max = rt.maxMemory();
            long free = rt.freeMemory();
            testOutputHelper.WriteLine($"total[{ total / mb }mb] max[{max / mb}mb] free[{free / mb }mb]");
        }
    }
}