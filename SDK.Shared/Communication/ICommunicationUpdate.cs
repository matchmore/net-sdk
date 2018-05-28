using System;
namespace Matchmore.SDK.Communication
{
	public interface ICommunicationUpdate
	{
		DeviceUpdate AsDeviceUpdate();
	}

	public class FCMTokenUpdate : ICommunicationUpdate
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
