using System;
using Matchmore.SDK.Persistence;
namespace Matchmore.SDK
{
    public partial class ConfigBuilder
    {
        public static IConfig WithApiKey(
            string apiKey,
            string persistenceDirectory,
            string persistenceFile = null,
            ILocationService locationService = null) => WithApiKey(
                apiKey,
                new SimpleJsonStateRepository(
                    persistenceFile: persistenceFile,
                    persistenceDirectory: persistenceDirectory),
                locationService);
        
        public static IConfig WithApiKey(string apiKey, IStateRepository stateRepository = null, ILocationService locationService = null)
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
            config.UseSSL = true;

            config.StateManager = stateRepository;
            config.LocationService = locationService;


            return config;
        }
    }
}
