using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Matchmore.SDK;
using Matchmore.SDK.Monitors;
using static Matchmore.Tests.Utils;

using NUnit.Framework;

namespace Matchmore.Tests
{
	[TestFixture]
    public class MatchByMultiChannelMonitor : MatchTestBase
	{
		[Test, MaxTime(40000)]
		[Timeout(20000)]
		public void GetMatchByMultiChannelMonitor()
		{
			_monitor = _instance.SubscribeMatches(MatchChannel.Polling | MatchChannel.Websocket);
			Assert.That(_monitor, Is.InstanceOf<MultiChannelMatchMonitor>());
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
