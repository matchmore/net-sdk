#if __ANDROID__ || __IOS__
using static Matchmore.Tests.Utils;
using NUnit.Framework;
using System;
using Matchmore.SDK;
using Matchmore.SDK.Xamarin.Shared;

namespace Matchmore.Tests
{
	[TestFixture]
	public class LocationServiceTest
	{
		[Test]
		public void LocationServiceEmitEvent()
		{
			var service = new GeoPluginLocationService();
			Location location = null;
			service.LocationUpdated += (sender, e) => location = e.Location;
			service.Start();

            //push now the location via the emulator
			while (location == null) { }

			Assert.NotNull(location);         
		}
	}
}
#endif