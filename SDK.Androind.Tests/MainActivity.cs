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

            Task.WaitAll(r);

            // or in any reference assemblies
            // AddTest (typeof (Your.Library.TestClass).Assembly);

            // Once you called base.OnCreate(), you cannot add more assemblies.
            base.OnCreate(bundle);
        }
    }
}
