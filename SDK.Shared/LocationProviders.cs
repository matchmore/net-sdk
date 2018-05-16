using System;

using System.Threading;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using System.Threading.Tasks;

namespace Matchmore.SDK
{
	public interface ILocationService
	{
		void Start();
		void Stop();
	}

	public class SimpleLocationService : ILocationService
	{
		private IGeolocator _locator;
		private ApiClient _client;
		private Device _mainDevice { get; }

		private CancellationTokenSource _cancelationTokenSource;

		public SimpleLocationService(ApiClient client, Device mainDevice)
		{
			_locator = CrossGeolocator.Current;
			_client = client;
			_mainDevice = mainDevice;
		}

		public void Start()
		{
			_cancelationTokenSource = new CancellationTokenSource();
			RecurrentCancellableTask.StartNew(async () =>
			{
				var location = await _locator.GetPositionAsync(TimeSpan.FromSeconds(10), _cancelationTokenSource.Token).ConfigureAwait(false);
				try
				{
					await _client.CreateLocationAsync(_mainDevice.Id, new Location
					{
						Longitude = location.Longitude,
						Altitude = location.Altitude,
						Latitude = location.Latitude
					}, _cancelationTokenSource.Token);
				}
				catch (SwaggerException e) when (e.StatusCode == 201)
				{
                    //ignore if you get swagger exception with 201, means it was created
				}

			}, TimeSpan.FromSeconds(10), _cancelationTokenSource.Token, TaskCreationOptions.LongRunning);
		}

		public void Stop()
		{
			_cancelationTokenSource.Cancel();
		}
	}
}