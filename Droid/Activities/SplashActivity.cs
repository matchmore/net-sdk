using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;

namespace TestApp.Droid
{
    [Activity(Label = "@string/app_name", Theme = "@style/SplashTheme", MainLauncher = true)]
    public class SplashActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
   
			Xamarin.Forms.Forms.Init(this, savedInstanceState);

			var r = Matchmore.SDK.Matchmore.ConfigureAsync(new Matchmore.SDK.GenericConfig
            {
                ApiKey = "eyJ0eXAiOiJKV1QiLCJhbGciOiJFUzI1NiJ9.eyJpc3MiOiJhbHBzIiwic3ViIjoiZDFhMDhkMjUtOGNjNi00ZjhhLWFlZjAtYjNiNjc5MDE2MjFmIiwiYXVkIjpbIlB1YmxpYyJdLCJuYmYiOjE1MjU3MDI3ODksImlhdCI6MTUyNTcwMjc4OSwianRpIjoiMSJ9.ht7KJrXGXkh8xqC9cFYAJV7NS0kSti3YidUB2nTyeHm7REsIhNKlwuDyfxSkeQZE6o0OHWegn7hZcHoAvW5QOw",
                Environment = "130.211.39.172"
            });
			Task.WaitAll(r);

			Matchmore.SDK.Matchmore.Instance.StartLocationService();

            var newIntent = new Intent(this, typeof(MainActivity));
            newIntent.AddFlags(ActivityFlags.ClearTop);
            newIntent.AddFlags(ActivityFlags.SingleTop);

            StartActivity(newIntent);
            Finish();
        }
    }
}
