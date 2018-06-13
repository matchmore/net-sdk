using Matchmore.SDK;
using Matchmore.SDK.Monitors;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using static Matchmore.Tests.Utils;

namespace Matchmore.Tests
{
    [TestFixture]
    public class MatchByPollingMonitor: MatchTestBase
    {        
		[Test, MaxTime(20000)]
		[Timeout(20000)]
		public void GetMatchByPollingMonitor()
		{
			_monitor = _instance.SubscribeMatches(MatchChannel.Polling);
			Assert.That(_monitor, Is.InstanceOf<PollingMatchMonitor>());
			RunSync(_monitor.Start);
			List<Match> matches = null;
			_monitor.MatchReceived += (object sender, MatchReceivedEventArgs e) => matches = e.Matches;

			var testMatch = SetupTestMatch();
			Match match = null;
			do
			{
				if (matches != null)
				{
					match = matches.Find(m => m.Publication.Id == testMatch.Publication.Id && m.Subscription.Id == testMatch.Subscription.Id);
				}
			} while (match == null);

			Assert.NotNull(match);
		}
    }
}
