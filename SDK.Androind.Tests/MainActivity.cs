using System.Reflection;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Matchmore.SDK.Persistence;
using Xamarin.Android.NUnitLite;

namespace SDK.Androind.Tests
{
    [Activity(Label = "SDK.Androind.Tests", MainLauncher = true)]
    public class MainActivity : TestSuiteActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            // tests can be inside the main assembly
            AddTest(Assembly.GetExecutingAssembly());

			Xamarin.Forms.Forms.Init(this, bundle);

			var r = Task.Run(async () =>
			{
				var stateManager = new AndroidStateManager("test", "test_state.data");
				stateManager.WipeData();

				await Matchmore.SDK.Matchmore.Configure(new Matchmore.SDK.Config
				{
					ApiKey = "eyJ0eXAiOiJKV1QiLCJhbGciOiJFUzI1NiJ9.eyJpc3MiOiJhbHBzIiwic3ViIjoiZDFhMDhkMjUtOGNjNi00ZjhhLWFlZjAtYjNiNjc5MDE2MjFmIiwiYXVkIjpbIlB1YmxpYyJdLCJuYmYiOjE1MjU3MDI3ODksImlhdCI6MTUyNTcwMjc4OSwianRpIjoiMSJ9.ht7KJrXGXkh8xqC9cFYAJV7NS0kSti3YidUB2nTyeHm7REsIhNKlwuDyfxSkeQZE6o0OHWegn7hZcHoAvW5QOw",
					Environment = "130.211.39.172",
					StateManager = stateManager
				});
			});

            Task.WaitAll(r);

            // or in any reference assemblies
            // AddTest (typeof (Your.Library.TestClass).Assembly);

            // Once you called base.OnCreate(), you cannot add more assemblies.
            base.OnCreate(bundle);
        }
    }
}
