using System;
using Xunit;
using TriviaGameServer;

namespace TriviaGameServerTests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1ShouldPass()
        {
            Assert.Equal(3, Program.foo());
        }

        [Fact]
        public void Test2ShouldFail()
        {
            Assert.Equal(4, Program.foo());
        }
    }
}
