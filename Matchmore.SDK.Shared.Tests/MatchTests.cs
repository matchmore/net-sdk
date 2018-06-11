﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Matchmore.SDK;
using Matchmore.SDK.Monitors;
using static Matchmore.Tests.Utils;

using NUnit.Framework;

namespace Matchmore.Tests
{
	[TestFixture]
	public class MatchTests : TestBase
	{

		private IMatchMonitor _monitor;

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
			} while (match == null);

			Assert.NotNull(match);
		}

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