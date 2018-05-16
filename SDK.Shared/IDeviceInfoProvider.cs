namespace Matchmore.SDK
{
    public interface IDeviceInfoProvider
    {
		string DeviceName { get; }
		string Platform { get; }
    }
}