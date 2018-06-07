using Matchmore.SDK;
using Plugin.DeviceInfo;
using Xamarin.Forms;


[assembly: Dependency(typeof(iOSDeviceInfo))]
namespace Matchmore.SDK
{
	public class iOSDeviceInfo : IDeviceInfoProvider
	{
		public string DeviceName => CrossDeviceInfo.Current.DeviceName;

		public string Platform => CrossDeviceInfo.Current.Platform.ToString();
	}
}
