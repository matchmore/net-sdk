using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Matchmore.SDK;
using NUnit.Framework;

namespace SDK.Androind.Tests
{
	[TestFixture]
	public class SdkTests
	{
		private Matchmore.SDK.Matchmore _instance;
		private IMatchMonitor _monitor;

		[SetUp]
		public void Setup()
		{
			_instance = Matchmore.SDK.Matchmore.Instance;
			Assert.NotNull(_instance);
		}


		[TearDown]
		public void Tear() {
			_monitor?.Stop();
		}

		[Test, Timeout(20000)]
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

		[Test, Timeout(20000)]
		public void GetMatchByPollingMonitor()
		{
			_monitor = _instance.SubscribeMatches(MatchChannel.polling);
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

		[Test, Timeout(60000)]
		[Ignore("Websocket broken")]
        public void GetMatchByWebsocketMonitor()
        {
			_monitor = _instance.SubscribeMatches(MatchChannel.websocket);
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
