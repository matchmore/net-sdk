using System;
namespace Matchmore.SDK.Events
{
    public class LocationUpdatedEventArgs
    {
		public Location Location
		{
			get;
			private set;
		}

		public LocationUpdatedEventArgs(Location location)
		{
			this.Location = location;
		}
	}
}
