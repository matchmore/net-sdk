using System;
using System.Collections.Generic;
using Matchmore.SDK;
using Plugin.DeviceInfo;
using Xamarin.Forms;


[assembly: Dependency(typeof(AndroidDeviceInfo))]
namespace Matchmore.SDK
{
	public class AndroidDeviceInfo : IDeviceInfoProvider
	{
		public string DeviceName => CrossDeviceInfo.Current.DeviceName;

		public string Platform => CrossDeviceInfo.Current.Platform.ToString();
	}
}
