using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Matchmore.SDK;
using Matchmore.SDK.Persistence;
using NUnit.Framework;

namespace SDK.iOS.Tests
{
	[TestFixture]
	public class SdkTests
	{
		private Matchmore.SDK.Matchmore _instance;
		private IMatchMonitor _monitor;

		[SetUp]
		public void Setup()
		{
			var r = Task.Run(async () =>
			{
				IStateManager stateManager = null;
#if __ANDROID__
				stateManager = new AndroidStateManager("test", "test_state.data");
#else
#if __IOS__
				stateManager = new iOSStateManager("test", "test_state.data");
#else
				stateManager = new SimpleJsonStateManager("test", "test_state.data");
#endif
#endif

				stateManager.WipeData();

				await Matchmore.SDK.Matchmore.ConfigureAsync(new Matchmore.SDK.GenericConfig
				{
					ApiKey = "eyJ0eXAiOiJKV1QiLCJhbGciOiJFUzI1NiJ9.eyJpc3MiOiJhbHBzIiwic3ViIjoiZDFhMDhkMjUtOGNjNi00ZjhhLWFlZjAtYjNiNjc5MDE2MjFmIiwiYXVkIjpbIlB1YmxpYyJdLCJuYmYiOjE1MjU3MDI3ODksImlhdCI6MTUyNTcwMjc4OSwianRpIjoiMSJ9.ht7KJrXGXkh8xqC9cFYAJV7NS0kSti3YidUB2nTyeHm7REsIhNKlwuDyfxSkeQZE6o0OHWegn7hZcHoAvW5QOw",
					Environment = "130.211.39.172",
					StateManager = stateManager
				});
			});
			Task.WaitAll(r);
			_instance = Matchmore.SDK.Matchmore.Instance;
			Assert.NotNull(_instance);
		}

		[TearDown]
		public void Tear()
		{
			_monitor?.Stop();
			Matchmore.SDK.Matchmore.Reset();
		}

		[Test, MaxTime(60000)]
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

		[Test, MaxTime(60000)]
		public void GetMatchByPollingMonitor()
		{
			_monitor = _instance.SubscribeMatches(MatchChannel.Polling);
			RunSync(() => _monitor.Start());
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

		[Test, MaxTime(60000)]
		[Ignore("Websocket broken")]
		public void GetMatchByWebsocketMonitor()
		{
			_monitor = _instance.SubscribeMatches(MatchChannel.Websocket);
			RunSync(() => _monitor.Start());
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

		[Test, MaxTime(60000)]
		public void GetMatchByMultiChannelMonitor()
		{
			_monitor = _instance.SubscribeMatches(MatchChannel.Polling | MatchChannel.Websocket);
			RunSync(() => _monitor.Start());
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


		internal class TestMatchSetup
		{
			public Subscription Subscription { get; set; }
			public Publication Publication { get; set; }
			public Device PublishingDevice { get; set; }
		}


		private TestMatchSetup SetupTestMatch()
		{
			return RunSync(() => SetupMatchAsync());
		}

		private static T RunSync<T>(Func<Task<T>> task)
		{
			var r = Task.Run(async () => await task());
			Task.WaitAll(r);
			return r.Result;
		}

		private static void RunSync(Func<Task> task)
		{
			var r = Task.Run(async () => await task());
			Task.WaitAll(r);
		}

		private async Task<TestMatchSetup> SetupMatchAsync()
		{
			var pubDevice = await _instance.CreateDeviceAsync(new MobileDevice
			{
				Name = "Publisher"
			});


			Assert.NotNull(pubDevice);
			Assert.NotNull(pubDevice.Id);

			var sub = await _instance.CreateSubscriptionAsync(new Subscription
			{
				Topic = "Unity",
				Duration = 30,
				Range = 100,
				Selector = "test = true and price <= 200",
				Pushers = new List<string>() { "ws" }
			});
			Assert.NotNull(sub);
			Assert.NotNull(sub.Id);

			var pub = await _instance.CreatePublicationAsync(new Publication
			{
				Topic = "Unity",
				Duration = 30,
				Range = 100,
				Properties = new Dictionary<string, object>(){
				{"test", true},
				{"price", 199}
			}
			}, pubDevice);

			Assert.NotNull(pub);
			Assert.NotNull(pub.Id);

			await _instance.UpdateLocationAsync(new Location
			{
				Latitude = 54.414662,
				Longitude = 18.625498
			});

			await _instance.UpdateLocationAsync(new Location
			{
				Latitude = 54.414662,
				Longitude = 18.625498
			}, pubDevice);

			return new TestMatchSetup()
			{
				Subscription = sub,
				Publication = pub,
				PublishingDevice = pubDevice
			};

		}
	}
}
