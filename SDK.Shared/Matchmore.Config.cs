using System.Net.Http;
using Matchmore.SDK.Persistence;

namespace Matchmore.SDK
{
	public class Config
	{
		public string ApiKey { get; set; }
		public string Environment { get; set; }
		public bool UseSecuredCommunication { get; set; }
		public int? ServicePort { get; set; }
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
			if (DeviceInfoProvider == null || StateManager == null){
				throw new MatchmoreException("Missing state manager and/or device info. Make sure Xamarin.Form.Init was called");
			}

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
