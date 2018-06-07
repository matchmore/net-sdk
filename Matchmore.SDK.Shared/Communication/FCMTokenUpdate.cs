namespace Matchmore.SDK.Communication
{
    public class FCMTokenUpdate : IMatchChannelUpdate
    {
        readonly string _token;

        public FCMTokenUpdate(string token)
        {
            _token = token;
        }

        public DeviceUpdate AsDeviceUpdate()
        {
            return new DeviceUpdate
            {
                DeviceToken = "fcm://" + _token
            };
        }
    }
}
