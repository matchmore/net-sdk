using Matchmore.SDK;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using static Matchmore.Tests.Utils;

namespace Matchmore.Tests
{
    [TestFixture]
    public class MatchForOtherDevice: MatchTestBase
    {   

		[Test, MaxTime(20000)]
		[Timeout(20000)]
		public void GetMatchForOtherDevice()
		{
			var _beacon = new IBeaconDevice
			{
				Major = 13,
				Minor = 3,
				ProximityUUID = Guid.NewGuid().ToString(),
				Name = "bacon"
			};

			var (beacon, _monitor) = RunSync(() => _instance.CreateDeviceAndStartListening(_beacon, MatchChannel.Polling));

			RunSync(() => _instance.UpdateLocationAsync(new Location
			{
				Latitude = 54.414662,
				Longitude = 18.625498
			}, beacon));

			RunSync(_monitor.Start);
			List<Match> matches = null;
			_monitor.MatchReceived += (object sender, MatchReceivedEventArgs e) => matches = e.Matches;

			RunSync(() => _instance.CreateSubscriptionAsync(sub, beacon));

			var testMatch = SetupTestMatch();
			Match match = null;
			do
			{
				if (matches != null)
				{
					match = matches.Find(m => m.Subscription.DeviceId == beacon.Id);
				}
			} while (match == null);

			Assert.NotNull(match);
		}

    }
}
