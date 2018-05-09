using Matchmore.SDK.Persistence;

namespace Matchmore.SDK
{
    public class Config
    {
        public string ApiKey { get; set; }
        public string Environment { get; set; }
        public bool UseSecuredCommunication { get; set; }
        public bool StartWebsocketImmediately { get; set; }
        public string WorldId { get; set; }
        public int? PusherPort { get; set; }
        public int? ServicePort { get; set; }
        public string PersistenceFile { get; set; }
        public bool LoggingEnabled { get; set; }
        public IStateManager StateManager { get; set; }
        public IDeviceInfoProvider DeviceInfoProvider { get; set; }

        public Config()
        {
            StateManager = StateManager ?? Xamarin.Forms.DependencyService.Get<IStateManager>();
            DeviceInfoProvider = DeviceInfoProvider ?? Xamarin.Forms.DependencyService.Get<IDeviceInfoProvider>();
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
