using NUnit.Framework;
using System;
using System.Linq;
using static Matchmore.Tests.Utils;
using Matchmore.SDK;
using System.Threading;

namespace Matchmore.Tests
{
	[TestFixture]
	public class ThirdPartyMatchProviderTest : TestBase
	{
		Match match;
		[SetUp]
		public void Setup()
		{
			Matchmore.SDK.Matchmore.Reset();
			SetupDevInstance();
			_instance = Matchmore.SDK.Matchmore.Instance;
			Assert.NotNull(_instance);

			var testMatch = SetupTestMatch();
			match = null;
			do
			{
				var matches = RunSync(() => _instance.GetMatchesAsync());
				match = matches.Find(m => m.Publication.Id == testMatch.Publication.Id && m.Subscription.Id == testMatch.Subscription.Id);
			} while (match == null);

			Assert.NotNull(match);
		}

		[TearDown]
		public void Tear()
		{
			Matchmore.SDK.Matchmore.Reset();
		}

		[Test]
		public void ThirdPartyMatchProvider()
		{
			Assume.That(match != null, "Match is prepared");
			var matchProviderMonitor = _instance.SubscribeMatchesWithThirdParty();

			MatchReceivedEventArgs matchReceivedEvent = null;
			_instance.MatchReceived += (sender, e) => matchReceivedEvent = e;

			RunSync(() => matchProviderMonitor.ProvideMatchIdAsync(MatchId.Make(match.Id)));

			while (matchReceivedEvent == null) { }

			Assert.AreEqual(MatchChannel.ThirdParty, matchReceivedEvent.Channel);
			var receivedMatch = matchReceivedEvent.Matches.FirstOrDefault(m => m.Id == match.Id);
			Assert.NotNull(receivedMatch);
		}
	}
}
