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
//default params of .SubscribeMatches() use your main device and Polling as a channel delivery mechanism
var monitor = Matchmore.SDK.Matchmore.Instance.SubscribeMatches();
monitor.MatchReceived += (object sender, MatchReceivedEventArgs e) => {
    //handle your match
};

//if  you don't have access to your monitor, you can attach the event handler on the Matchmore Instance
Matchmore.SDK.Matchmore.Instance.MatchReceived += (object sender, MatchReceivedEventArgs e) => {
    //handle your match, the sender will be your monitor
};
```

## Third party match providers(APNS and FCM)

To use your match provider of your choice, you need to wire it differently depending on the platform.

First you need to provide the token for the platform(FCM or APNS) to Matchmore so we know where to route the match id

```csharp
//fcm
Matchmore.SDK.Matchmore.Instance.UpdateDeviceCommunicationAsync(new Matchmore.SDK.Communication.FCMTokenUpdate("token taken from FCM"));
//basing of https://docs.microsoft.com/en-us/xamarin/android/data-cloud/google-messaging/remote-notifications-with-fcm?tabs=vswin

[Service]
[IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
public class MyFirebaseIIDService : FirebaseInstanceIdService
{
    const string TAG = "MyFirebaseIIDService";
    public override void OnTokenRefresh()
    {
        var refreshedToken = FirebaseInstanceId.Instance.Token;
        Log.Debug(TAG, "Refreshed token: " + refreshedToken);
        Matchmore.SDK.Matchmore.Instance.UpdateDeviceCommunicationAsync(new Matchmore.SDK.Communication.FCMTokenUpdate(refreshedToken));
    }
}

//apns
Matchmore.SDK.Matchmore.Instance.UpdateDeviceCommunicationAsync(new Matchmore.SDK.Communication.APNSTokenUpdate("token taken from APNS"));
//basing of https://github.com/xamarin/ios-samples/blob/master/Notifications/AppDelegate.cs
public override void RegisteredForRemoteNotifications (UIApplication application, NSData deviceToken)
{
	Matchmore.SDK.Matchmore.Instance.UpdateDeviceCommunicationAsync(new Matchmore.SDK.Communication.APNSTokenUpdate(deviceToken.ToString()));
}

```

Then you need to tell Matchmore whenever you will get a match id to retrieve.

```csharp
//android fcm
[Service]
[IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
public class MyFirebaseMessagingService : FirebaseMessagingService
{

    IMatchProviderMonitor matchProviderMonitor = Matchmore.SDK.Matchmore.Instance.SubscribeMatchesWithThirdParty();
    const string TAG = "MyFirebaseMsgService";
    public override void OnMessageReceived(RemoteMessage message)
    {
        matchProviderMonitor.ProvideMatchIdAsync(MatchId.Make(message.GetNotification().Body));
    }
}

//ios apns in your AppDelegate
public override void ReceivedRemoteNotification (UIApplication application, NSDictionary userInfo)
{
    IMatchProviderMonitor matchProviderMonitor = Matchmore.SDK.Matchmore.Instance.SubscribeMatchesWithThirdParty();
    var matchId = userInfo["matchId"].ToString();
    matchProviderMonitor.ProvideMatchIdAsync(MatchId.Make(matchId));
}
```

Additional info you might find useful
[how to setup APNS](https://github.com/matchmore/alps-ios-sdk/blob/master/ApnsSetup.md).
[fcm in xamarin](https://docs.microsoft.com/en-us/xamarin/android/data-cloud/google-messaging/remote-notifications-with-fcm?tabs=vswin)
[apns in xamarin](https://docs.microsoft.com/en-us/xamarin/ios/platform/user-notifications/deprecated/remote-notifications-in-ios)

## Documentation

See the [http://matchmore.io/documentation/api](http://matchmore.io/documentation/api) or consult our website for further information [http://matchmore.io/](http://matchmore.io/)

## Authors

- @lmlynik, lukasz.mlynik@matchmore.com


## License

`Matchmore` is available under the MIT license. See the LICENSE file for more info.