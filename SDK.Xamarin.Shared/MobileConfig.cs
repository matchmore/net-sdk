using System;
using System.Net.Http;
using Matchmore.SDK.Persistence;

namespace Matchmore.SDK.Xamarin.Shared
{
	public class MobileConfig : IConfig
    {
        public string ApiKey { get; set; }
        public string Environment { get; set; }
        public bool UseSecuredCommunication { get; set; }
        public int? ServicePort { get; set; }
        public IStateManager StateManager { get; set; }
        public IDeviceInfoProvider DeviceInfoProvider { get; set; }
        public HttpClient HttpClient { get; set; }
        public ILocationService LocationService { get; set; }

        public void SetupDefaults()
        {
			StateManager = StateManager ?? new MobileStateManager();
			DeviceInfoProvider = DeviceInfoProvider ?? new SimpleDeviceInfo();
			LocationService = LocationService ?? new SimpleLocationService();

            HttpClient = HttpClient ?? new HttpClient();
        }
    }
}
