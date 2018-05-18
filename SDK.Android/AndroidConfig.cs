using System;
using System.Net.Http;
using Matchmore.SDK.Persistence;

namespace Matchmore.SDK
{
	public class AndroidConfig: IConfig
    {
		public string ApiKey { get; set; }
        public string Environment { get; set; }
        public bool UseSecuredCommunication { get; set; }
        public int? ServicePort { get; set; }
        public IStateManager StateManager { get; set; }
        public IDeviceInfoProvider DeviceInfoProvider { get; set; }
        public HttpClient HttpClient { get; set; }

		public void SetupDefaults()
        {
            StateManager = StateManager ?? new AndroidStateManager();
            DeviceInfoProvider = DeviceInfoProvider ?? new AndroidDeviceInfo();

            HttpClient = HttpClient ?? new HttpClient();
        }
    }
}
