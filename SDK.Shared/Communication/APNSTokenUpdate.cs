namespace Matchmore.SDK.Communication
{
    public class APNSTokenUpdate : IMatchChannelUpdate
    {
        readonly string _token;

		public APNSTokenUpdate(string token)
        {
            _token = token;
        }

        public DeviceUpdate AsDeviceUpdate()
        {
            return new DeviceUpdate
            {
                DeviceToken = "apns://" + _token
            };
        }
    }
}
