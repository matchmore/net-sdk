using Matchmore.SDK.Xamarin.Shared;

namespace Matchmore.SDK
{
    public partial class ConfigBuilder
    {
        //for test purposes
        public static void SetMobileConfig(IConfig mobileConfig) => _mobileConfig = mobileConfig;
        static IConfig MobileConfig
        {
            get
            {
                if (_mobileConfig == null)
                {
                    _mobileConfig = new MobileConfig();
                }
                return _mobileConfig;

            }
        }
        static IConfig _mobileConfig;
    }
}
