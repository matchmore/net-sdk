using System;
using System.Threading.Tasks;
using Matchmore.SDK.Persistence;
#if __ANDROID__ || __IOS__
using Matchmore.SDK.Xamarin.Shared;
#endif

namespace Matchmore.Tests
{
	public class TestBase
	{
		public string apiKey = "eyJ0eXAiOiJKV1QiLCJhbGciOiJFUzI1NiJ9.eyJpc3MiOiJhbHBzIiwic3ViIjoiMzIyNzliODEtODBkMS00MmFkLTgyNjEtNDE1MDlmNjM0NWRlIiwiYXVkIjpbIlB1YmxpYyJdLCJuYmYiOjE1MjY5MTE3NDYsImlhdCI6MTUyNjkxMTc0NiwianRpIjoiMSJ9.9ppoTeyGwhk9tERCThmJgXcenMvnggxWq53QTMxokyRi9AU0BETYPoBttdjPaPWhCvJpnxpercLATqt9nwt9ow";

		public void SetupDevInstance()
		{
			Matchmore.SDK.Matchmore.Reset();
			var r = Task.Run(async () =>
			{
				IStateManager stateManager = null;
#if __ANDROID__ || __IOS__
				stateManager = new MobileStateManager("test", "test_state.data");
#else
                stateManager = new SimpleJsonStateManager("test", "test_state.data");
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
		}
	}
}
