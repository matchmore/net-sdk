using Matchmore.SDK;
using NUnit.Framework;
using System;
using static Matchmore.Tests.Utils;

namespace Matchmore.Tests
{
    [TestFixture]
    public class MatchByExplicitCall: MatchTestBase
    {
        [Test, MaxTime(20000)]
        [Timeout(20000)]
        public void GetMatchByExplicitCall()
        {
            var testMatch = SetupTestMatch();
            Match match = null;
            do
            {
                var matches = RunSync(() => _instance.GetMatchesAsync());
                match = matches.Find(m => m.Publication.Id == testMatch.Publication.Id && m.Subscription.Id == testMatch.Subscription.Id);
            } while (match == null);

            Assert.NotNull(match);
        }
    }
}
