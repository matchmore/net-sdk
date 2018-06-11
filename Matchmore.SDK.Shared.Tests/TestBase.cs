using System;
using System.Threading.Tasks;
using Matchmore.SDK;
using Matchmore.SDK.Persistence;
using static Matchmore.Tests.Utils;
using NUnit.Framework;
using System.Collections.Generic;
#if __ANDROID__ || __IOS__
using Matchmore.SDK.Xamarin.Shared;
#endif

namespace Matchmore.Tests
{
	public class TestBase
    {   public string apiKey = "eyJ0eXAiOiJKV1QiLCJhbGciOiJFUzI1NiJ9.eyJpc3MiOiJhbHBzIiwic3ViIjoiMzIzMmY3NTYtMzkzYi00OWM2LTgxMjItMzBlNTI5NDZiOWVkIiwiYXVkIjpbIlB1YmxpYyJdLCJuYmYiOjE1MjIxNTk3NTUsImlhdCI6MTUyMjE1OTc1NSwianRpIjoiMSJ9.6Ay0Ollaf1Wl1WXSyvb6B0_62fr74TYYFV0VykmORAL0sELzAMBvAXukFEeVYwwGv5W99AL-qVwzTj2UWn05ig";
		protected static Matchmore.SDK.Matchmore _instance;

		protected Subscription sub = new Subscription
        {
            Topic = "Unity",
            Duration = 30,
            Range = 100,
            Selector = "test = true and price <= 200"
        };

		protected Publication pub = new Publication
		{
			Topic = "Unity",
			Duration = 30,
			Range = 100,
			Properties = new Dictionary<string, object>(){
				{"test", true},
				{"price", 199}
			}
		};

		public void SetupDevInstance()
		{
			Matchmore.SDK.Matchmore.Reset();
			var r = Task.Run(async () =>
			{
				IStateRepository stateManager = null;
#if __ANDROID__ || __IOS__
				stateManager = new MobileStateManager("test", "test_state.data");
#else
                stateManager = new SimpleJsonStateRepository("test", "test_state.data");
#endif

				stateManager.WipeData();

				await Matchmore.SDK.Matchmore.ConfigureAsync(new Matchmore.SDK.GenericConfig
				{
					ApiKey = apiKey,
					Environment = "130.211.39.172",
					StateManager = stateManager
				});

				await Matchmore.SDK.Matchmore.Instance.SetupMainDeviceAsync().ConfigureAwait(false);
			});
			Task.WaitAll(r);
			TaskScheduler.UnobservedTaskException += (s, e) => {

			};

		}

		internal class TestMatchSetup
        {
            public Subscription Subscription { get; set; }
            public Publication Publication { get; set; }
            public Device PublishingDevice { get; set; }
        }


		internal TestMatchSetup SetupTestMatch()
        {
            return RunSync(() => SetupMatchAsync());
        }

		internal async Task<TestMatchSetup> SetupMatchAsync()
        {
            var pubDevice = await _instance.CreateDeviceAsync(new MobileDevice
            {
                Name = "Publisher"
            });


            Assert.NotNull(pubDevice);
            Assert.NotNull(pubDevice.Id);


			var _sub = await _instance.CreateSubscriptionAsync(sub);
            Assert.NotNull(_sub);
            Assert.NotNull(_sub.Id);

			var _pub = await _instance.CreatePublicationAsync(pub, pubDevice);

            Assert.NotNull(_pub);
            Assert.NotNull(_pub.Id);

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
                Subscription = _sub,
                Publication = _pub,
                PublishingDevice = pubDevice
            };

        }
	}
}
