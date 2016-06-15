using System;
using System.Threading;
using Xunit;

namespace Wheatech.UnitTest
{
    public class LazyTest
    {
        [Fact]
        public void TestPublicationResetLazy()
        {
            TestResetLazy(LazyThreadSafetyMode.PublicationOnly);
        }

        [Fact]
        public void TestNoneThreadSafetyResetLazy()
        {
            TestResetLazy(LazyThreadSafetyMode.None);
        }

        [Fact]
        public void TestExecutionAndPublicationResetLazy()
        {
            TestResetLazy(LazyThreadSafetyMode.ExecutionAndPublication);
        }

        private void TestResetLazy(LazyThreadSafetyMode mode)
        {
            var val = new Lazy<string>(() => "hola" + DateTime.Now.Ticks, mode);

            val.Reset(); //reset before initialized
            var str1 = val.Value;
            val.Reset(); //reset after initialized
            Thread.Sleep(1);
            var str2 = val.Value;

            Assert.NotEqual(str1, str2);
        }
    }
}
