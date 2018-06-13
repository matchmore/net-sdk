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
    public class MatchByWebsocketMonitor : MatchTestBase
	{
		[Test, MaxTime(40000)]
		[Timeout(20000)]
		public void GetMatchByWebsocketMonitor()
		{
			_monitor = _instance.SubscribeMatches(MatchChannel.Websocket);
			Assert.That(_monitor, Is.InstanceOf<WebsocketMatchMonitor>());
			RunSync(_monitor.Start);
			List<Match> matches = null;
			_monitor.MatchReceived += (object sender, MatchReceivedEventArgs e) => matches = e.Matches;

			var testMatch = SetupTestMatch();
			Match match = null;
            var count = 10L;
			do
			{
				RunSync(() => _instance.UpdateLocationAsync(new Location
				{
					Latitude = 54.414662,
					Longitude = 18.625498
				}));
				if (matches != null)
				{
					match = matches.Find(m => m.Publication.Id == testMatch.Publication.Id && m.Subscription.Id == testMatch.Subscription.Id);
				}
                count--;
                System.Threading.Thread.Sleep(1000);
            } while (match == null || count > 0);

			Assert.NotNull(match);
		}
	}
}
