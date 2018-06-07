using System;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;

using System.Threading;
using System.Threading.Tasks;
using Matchmore.SDK.Events;

namespace Matchmore.SDK.Xamarin.Shared
{
	public class GeoPluginLocationService : ILocationService
    {
        IGeolocator _locator;
        
        CancellationTokenSource _cancelationTokenSource;

		public event EventHandler<LocationUpdatedEventArgs> LocationUpdated;

		public void Start()
        {
			_locator = CrossGeolocator.Current;
            _cancelationTokenSource = new CancellationTokenSource();
            RecurrentCancellableTask.StartNew(async () =>
            {
                var location = await _locator.GetPositionAsync(TimeSpan.FromSeconds(10), _cancelationTokenSource.Token).ConfigureAwait(false);
                

				LocationUpdated?.Invoke(this, new LocationUpdatedEventArgs(new Location
                    {
                        Longitude = location.Longitude,
                        Altitude = location.Altitude,
                        Latitude = location.Latitude
                    }));

            }, TimeSpan.FromSeconds(10), _cancelationTokenSource.Token, TaskCreationOptions.LongRunning);
        }

        public void Stop()
        {
            _cancelationTokenSource.Cancel();
        }
    }
}
