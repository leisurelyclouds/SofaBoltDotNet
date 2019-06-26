using Remoting;
using com.alipay.remoting.rpc.common;
using Xunit;
using Remoting.rpc.protocol;
using com.alipay.remoting.rpc.userprocessor.multiinterestprocessor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace com.alipay.remoting.rpc.userprocessor
{
    [Collection("Sequential")]
    public class UserProcessorRegisterHelperTest
    {
        internal ConcurrentDictionary<Type, UserProcessor> userProcessors;

        public UserProcessorRegisterHelperTest()
        {
            userProcessors = new ConcurrentDictionary<Type, UserProcessor>();
        }

        [Fact]
        public virtual void testRegisterUserProcessor()
        {
            UserProcessor userProcessor = new SimpleServerUserProcessor();
            UserProcessorRegisterHelper.registerUserProcessor(userProcessor, userProcessors);
            Assert.Single(userProcessors);
        }

        [Fact]
        public virtual void testRegisterMultiInterestUserProcessor()
        {
            UserProcessor multiInterestUserProcessor = new SimpleServerMultiInterestUserProcessor();
            UserProcessorRegisterHelper.registerUserProcessor(multiInterestUserProcessor, userProcessors);
            Assert.Equal(((SimpleServerMultiInterestUserProcessor)multiInterestUserProcessor).multiInterest().Count, userProcessors.Count);
        }

        [Fact]
        public virtual void testInterestNullException()
        {
            UserProcessor userProcessor = new SyncUserProcessorAnonymousInnerClass(this);

            try
            {
                UserProcessorRegisterHelper.registerUserProcessor(userProcessor, userProcessors);
            }
            catch (Exception)
            {
            }

            Assert.Empty(userProcessors);
        }

        private class SyncUserProcessorAnonymousInnerClass : SyncUserProcessor
        {
            private readonly UserProcessorRegisterHelperTest outerInstance;

            public SyncUserProcessorAnonymousInnerClass(UserProcessorRegisterHelperTest outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public override object handleRequest(BizContext bizCtx, object request)
            {
                return request;
            }

            public override Type interest()
            {
                return null;
            }
        }

        [Fact]
        public virtual void testInterestEmptyException()
        {
            MultiInterestUserProcessor userProcessor = new SyncMutiInterestUserProcessorAnonymousInnerClass(this);

            try
            {
                UserProcessorRegisterHelper.registerUserProcessor(userProcessor, userProcessors);
            }
            catch (Exception)
            {
            }

            Assert.Empty(userProcessors);
        }

        private class SyncMutiInterestUserProcessorAnonymousInnerClass : SyncMutiInterestUserProcessor
        {
            private readonly UserProcessorRegisterHelperTest outerInstance;

            public SyncMutiInterestUserProcessorAnonymousInnerClass(UserProcessorRegisterHelperTest outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public override object handleRequest(BizContext bizCtx, object request)
            {
                return request;
            }

            public override List<Type> multiInterest()
            {
                return new List<Type>();
            }
        }

        [Fact]
        public virtual void testInterestRepeatException()
        {
            UserProcessor userProcessor = new SimpleServerUserProcessor();
            UserProcessor repeatedUserProcessor = new SimpleServerUserProcessor();
            try
            {
                UserProcessorRegisterHelper.registerUserProcessor(userProcessor, userProcessors);
                UserProcessorRegisterHelper.registerUserProcessor(repeatedUserProcessor, userProcessors);
            }
            catch (Exception)
            {
            }

            Assert.Single(userProcessors);
        }
    }
}