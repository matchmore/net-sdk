using System.Net.Http;
using Matchmore.SDK.Persistence;

namespace Matchmore.SDK
{
	public class GenericConfig : IConfig
	{
		class GenericDeviceInfoProvider : IDeviceInfoProvider
		{
			public string DeviceName => "na";

			public string Platform => "na";
		}

		public string ApiKey { get; set; }
		public string Environment { get; set; }
		public bool UseSecuredCommunication { get; set; }
		public int? ServicePort { get; set; }
		public IStateRepository StateManager { get; set; }
		public IDeviceInfoProvider DeviceInfoProvider { get; set; }
		public HttpClient HttpClient { get; set; }
		public ILocationService LocationService { get; set; }

		public void SetupDefaults()
		{
			StateManager = StateManager ?? new SimpleJsonStateRepository(null);
			DeviceInfoProvider = DeviceInfoProvider ?? new GenericDeviceInfoProvider();

			HttpClient = HttpClient ?? new HttpClient();
		}
	}

	public partial class ConfigBuilder
	{
		public static IConfig WithApiKey(string apiKey)
		{
			IConfig config = null;
#if __ANDROID__ || __IOS__
			if (ConfigBuilder.MobileConfig == null)
			{
				throw new MatchmoreException("Mobile config wasn't bootstrapped");
			}
			config = ConfigBuilder.MobileConfig;
#else
			config = new GenericConfig();
#endif

			config.ApiKey = apiKey;
			config.UseSecuredCommunication = true;

			return config;
		}
	}
}