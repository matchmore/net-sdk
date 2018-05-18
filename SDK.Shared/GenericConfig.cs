using System.Net.Http;
using Matchmore.SDK.Persistence;

namespace Matchmore.SDK
{
	public class GenericConfig : IConfig
	{
		private class GenericDeviceInfoProvider : IDeviceInfoProvider
		{
			public string DeviceName => "na";

			public string Platform => "na";
		}

		public string ApiKey { get; set; }
		public string Environment { get; set; }
		public bool UseSecuredCommunication { get; set; }
		public int? ServicePort { get; set; }
		public IStateManager StateManager { get; set; }
		public IDeviceInfoProvider DeviceInfoProvider { get; set; }
		public HttpClient HttpClient { get; set; }

		public void SetupDefaults()
		{
			StateManager = StateManager ?? new SimpleJsonStateManager(null);
			DeviceInfoProvider = DeviceInfoProvider ?? new GenericDeviceInfoProvider();

			HttpClient = HttpClient ?? new HttpClient();
		}
	}

	public class ConfigBuilder{
		public static IConfig WithApiKey(string apiKey)
        {
			IConfig config = null;
#if __ANDROID__
			config = new AndroidConfig();
#else
#if __IOS__
			config = new iOSConfig();
#else
			config = new GenericConfig();
#endif
#endif

			config.ApiKey = apiKey;
			config.UseSecuredCommunication = true;

			return config;
        }
	}
}