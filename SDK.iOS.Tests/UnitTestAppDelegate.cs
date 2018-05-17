using System;
using System.Linq;
using System.Collections.Generic;

using Foundation;
using UIKit;
using MonoTouch.NUnit.UI;
using System.Threading.Tasks;
using Matchmore.SDK.Persistence;

namespace SDK.iOS.Tests
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("UnitTestAppDelegate")]
    public partial class UnitTestAppDelegate : UIApplicationDelegate
    {
        // class-level declarations
        UIWindow window;
        TouchRunner runner;

        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
			Xamarin.Forms.Forms.Init();
			var r = Task.Run(async () =>
            {
				var stateManager = new iOSStateManager("test", "test_state.data");
                stateManager.WipeData();

                await Matchmore.SDK.Matchmore.ConfigureAsync(new Matchmore.SDK.Config
                {
                    ApiKey = "eyJ0eXAiOiJKV1QiLCJhbGciOiJFUzI1NiJ9.eyJpc3MiOiJhbHBzIiwic3ViIjoiZDFhMDhkMjUtOGNjNi00ZjhhLWFlZjAtYjNiNjc5MDE2MjFmIiwiYXVkIjpbIlB1YmxpYyJdLCJuYmYiOjE1MjU3MDI3ODksImlhdCI6MTUyNTcwMjc4OSwianRpIjoiMSJ9.ht7KJrXGXkh8xqC9cFYAJV7NS0kSti3YidUB2nTyeHm7REsIhNKlwuDyfxSkeQZE6o0OHWegn7hZcHoAvW5QOw",
                    Environment = "130.211.39.172",
                    StateManager = stateManager
                });
            });

            Task.WaitAll(r);
            // create a new window instance based on the screen size
            window = new UIWindow(UIScreen.MainScreen.Bounds);
            runner = new TouchRunner(window);

            // register every tests included in the main application/assembly
            runner.Add(System.Reflection.Assembly.GetExecutingAssembly());

            window.RootViewController = new UINavigationController(runner.GetViewController());

            // make the window visible
            window.MakeKeyAndVisible();

            return true;
        }
    }
}
