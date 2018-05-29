# Matchmore .NET SDK

`Matchmore` is a contextualized publish/subscribe model which can be used to model any geolocated or proximity based mobile applications. Save time and make development easier by using our SDK.

## Usage

Please refer to documentation "tutorial" to get a full explanation on this example:

Setup application API key and world, get it for free from [http://matchmore.io/](http://matchmore.io/).
```csharp
async Task SetupMatchmore()
{
    await Matchmore.SDK.Matchmore.ConfigureAsync("YOUR_API_KEY");
}
```

Create first device, publication and subscription. Please note that we're not caring about errors right now.
```csharp
await Matchmore.SDK.Matchmore.Instance.SetupMainDeviceAsync();
//you can access the device later by calling Matchmore.SDK.Matchmore.Instance.MainDevice
var sub = new Subscription
        {
            Topic = "Test Topic",
            Duration = 30,
            Range = 100,
            Selector = "test = true and price <= 200"
        };

var createdPub = await Matchmore.SDK.Matchmore.Instance.CreateSubscriptionAsync(sub);

var pubDevice = await Matchmore.SDK.Matchmore.Instance.CreateDeviceAsync(new MobileDevice
            {
                Name = "Publisher"
            });

var pub = new Publication
		{
			Topic = "Test Topic",
			Duration = 30,
			Range = 100,
			Properties = new Dictionary<string, object>(){
				{"test", true},
				{"price", 199}
			}
		};
//you can pass explicitly the device you would want to use
var createdPub = await Matchmore.SDK.Matchmore.Instance.CreatePublicationAsync(pub, pubDevice);
```

To receive matches, you need to create a monitor
```csharp
var monitor = Matchmore.SDK.Matchmore.Instance.SubscribeMatches(MatchChannel.Polling);

```

Start listening for main device matches changes.
```swift
let exampleMatchHandler = ExampleMatchHandler { matches, _ in
    print(matches)
}
Matchmore.matchDelegates += exampleMatchHandler
```

## Set up APNS: Certificates for push notifications

Matchmore iOS SDK uses Apple Push Notification Service (APNS) to deliver notifications to your iOS users.

If you already know how to enable APNS, don't forget to upload the certificate in our portal.

Also, you need to add the following lines to your project `AppDelegate`.

These callbacks allow the SDK to get the device token.

```swift
// Called when APNS has assigned the device a unique token
func application(_ application: UIApplication, didRegisterForRemoteNotificationsWithDeviceToken deviceToken: Data) {
    // Convert token to string
    let deviceTokenString = deviceToken.reduce("", {$0 + String(format: "%02X", $1)})
    Matchmore.registerDeviceToken(deviceToken: deviceTokenString)
}

// Called when APNS failed to register the device for push notifications
func application(_ application: UIApplication, didReceiveRemoteNotification userInfo: [AnyHashable : Any]) {
    Matchmore.processPushNotification(pushNotification: userInfo)
}
```

Else, you can find help on [how to setup APNS](https://github.com/matchmore/alps-ios-sdk/blob/master/ApnsSetup.md).

## Example

In `MatchmoreExample/` you will find working simple example.

For more complex solution please check [Ticketing App](https://github.com/matchmore/alps-ios-TicketingApp):

## Documentation

See the [http://matchmore.io/documentation/api](http://matchmore.io/documentation/api) or consult our website for further information [http://matchmore.io/](http://matchmore.io/)

## Authors

- @tharpa, rk@matchmore.com
- @wenjdu, jean-marc.du@matchmore.com
- @maciejburda, maciej.burda@matchmore.com


## License

`Matchmore` is available under the MIT license. See the LICENSE file for more info.