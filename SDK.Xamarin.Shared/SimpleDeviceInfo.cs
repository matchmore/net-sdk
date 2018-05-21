using System;
using Plugin.DeviceInfo;
namespace Matchmore.SDK.Xamarin.Shared
{
	public class SimpleDeviceInfo : IDeviceInfoProvider
    {
        public string DeviceName => CrossDeviceInfo.Current.DeviceName;

        public string Platform => CrossDeviceInfo.Current.Platform.ToString();
    }
}
