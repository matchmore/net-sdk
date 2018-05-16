using System.Net.Http;
using Matchmore.SDK.Persistence;

namespace Matchmore.SDK
{
	public class Config
	{
		public string ApiKey { get; set; }
		public string Environment { get; set; }
		public bool UseSecuredCommunication { get; set; }
		public IStateManager StateManager { get; set; }
		public IDeviceInfoProvider DeviceInfoProvider { get; set; }
		public HttpClient HttpClient { get; set; }

		public Config()
		{
		}

		internal void Defaults()
		{
			StateManager = StateManager ?? Xamarin.Forms.DependencyService.Get<IStateManager>();
			DeviceInfoProvider = DeviceInfoProvider ?? Xamarin.Forms.DependencyService.Get<IDeviceInfoProvider>();
			HttpClient = HttpClient ?? new HttpClient();
		}

		public static Config WithApiKey(string apiKey)
		{
			return new Config
			{
				ApiKey = apiKey,
				UseSecuredCommunication = true
			};
		}
	}
}
