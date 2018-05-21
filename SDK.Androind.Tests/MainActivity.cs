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
   
			const string p1 = Android.Manifest.Permission.AccessFineLocation;
			const string p2 = Android.Manifest.Permission.AccessCoarseLocation;
			RequestPermissions(new string[] { p1, p2 }, 0);

			var x1 = CheckSelfPermission(p1);
			var x2 = CheckSelfPermission(p2);
            // tests can be inside the main assembly
            AddTest(Assembly.GetExecutingAssembly());

            // or in any reference assemblies
            // AddTest (typeof (Your.Library.TestClass).Assembly);

            // Once you called base.OnCreate(), you cannot add more assemblies.
            base.OnCreate(bundle);
        }
    }
}
