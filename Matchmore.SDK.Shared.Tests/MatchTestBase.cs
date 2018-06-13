using Matchmore.SDK.Monitors;
using NUnit.Framework;
using System;
namespace Matchmore.Tests
{
    public class MatchTestBase : TestBase
    {
        protected IMatchMonitor _monitor;

        [SetUp]
        public void Setup()
        {
            Matchmore.SDK.Matchmore.Reset();
            SetupDevInstance();
            _instance = Matchmore.SDK.Matchmore.Instance;
            Assert.NotNull(_instance);
        }

        [TearDown]
        public void Tear()
        {
            _monitor?.Stop();
            Matchmore.SDK.Matchmore.Reset();
        }
    }
}
