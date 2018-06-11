using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Matchmore.SDK;

namespace ConsoleExample
{
    class Program
    {
        static void Main(string[] args)
        {
            MatchmoreExample().GetAwaiter().GetResult();
            Console.ReadKey();
            Matchmore.SDK.Matchmore.Reset();
        }

        static async Task MatchmoreExample()
        {
            // Configure Matchmore with your credentials
            await Matchmore.SDK.Matchmore.ConfigureAsync("eyJ0eXAiOiJKV1QiLCJhbGciOiJFUzI1NiJ9.eyJpc3MiOiJhbHBzIiwic3ViIjoiMzIzMmY3NTYtMzkzYi00OWM2LTgxMjItMzBlNTI5NDZiOWVkIiwiYXVkIjpbIlB1YmxpYyJdLCJuYmYiOjE1MjIxNTk3NTUsImlhdCI6MTUyMjE1OTc1NSwianRpIjoiMSJ9.6Ay0Ollaf1Wl1WXSyvb6B0_62fr74TYYFV0VykmORAL0sELzAMBvAXukFEeVYwwGv5W99AL-qVwzTj2UWn05ig");

            //you can access the device later by calling Matchmore.SDK.Matchmore.Instance.MainDevice
            await Matchmore.SDK.Matchmore.Instance.SetupMainDeviceAsync();

            // Create the subscription
            var sub = new Subscription
            {
                Topic = "Test Topic",
                Duration = 30,
                Range = 100,
                Selector = "test = true and price <= 200"
            };

            // Start the creation of your subscription in Matchmore service
            var createdSub = await Matchmore.SDK.Matchmore.Instance.CreateSubscriptionAsync(sub);
            Console.WriteLine($"created subscription {createdSub.Id}");

            var pubDevice = await Matchmore.SDK.Matchmore.Instance.CreateDeviceAsync(new PinDevice
            {
                Name = "Publisher"
            });

            var pub = new Publication
            {
                Topic = "Test Topic",
                Duration = 30,
                Range = 100,
                Properties = new Dictionary<string, object>{
                {"test", true},
                {"price", 199}
            }
            };
            //you can pass explicitly the device you would want to use, here we use a "virtual" second device for publication, like a pin
            var createdPub = await Matchmore.SDK.Matchmore.Instance.CreatePublicationAsync(pub, pubDevice);
            Console.WriteLine($"created publication {createdPub.Id}");

            // To receive matches, you need to create a monitor
            //default params of .SubscribeMatches() use your main device, Polling and Websocket as a channel delivery mechanism
            var monitor = Matchmore.SDK.Matchmore.Instance.SubscribeMatches();

            monitor.MatchReceived += (object sender, MatchReceivedEventArgs e) =>
            {
                Console.WriteLine($"Got match from {e.Channel}, {e.Matches[0].Id}");
            };

            //if  you don't have access to your monitor, you can attach the event handler on the Matchmore Instance
            Matchmore.SDK.Matchmore.Instance.MatchReceived += (object sender, MatchReceivedEventArgs e) =>
            {
                Console.WriteLine($"Got match using global event handler {e.Matches[0].Id}");
            };

            //lets update locations for both devices, in real life the published would pass
            await Matchmore.SDK.Matchmore.Instance.UpdateLocationAsync(new Location
            {
                Latitude = 54.414662,
                Longitude = 18.625498
            });

            await Matchmore.SDK.Matchmore.Instance.UpdateLocationAsync(new Location
            {
                Latitude = 54.414662,
                Longitude = 18.625498
            }, pubDevice);

        }
    }
}
