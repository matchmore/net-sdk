using System.Net.Http;
using Matchmore.SDK.Persistence;

namespace Matchmore.SDK
{
	public class GenericConfig : IConfig
	{
		class GenericDeviceInfoProvider : IDeviceInfoProvider
		{
			public string DeviceName => "default";

			public string Platform => "default";
		}

		public string ApiKey { get; set; }
		public string Environment { get; set; }
		public bool UseSSL { get; set; }
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
}