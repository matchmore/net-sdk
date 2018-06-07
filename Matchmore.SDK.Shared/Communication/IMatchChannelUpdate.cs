using System;
namespace Matchmore.SDK.Communication
{
	public interface IMatchChannelUpdate
	{
		DeviceUpdate AsDeviceUpdate();
	}
}
