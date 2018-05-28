using System;
using System.Collections.Generic;

using Matchmore.SDK.Persistence;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using System.Linq;
using Matchmore.SDK.Communication;

namespace Matchmore.SDK
{
	public class Matchmore
	{
		private static Matchmore _instance;
		public static readonly string API_VERSION = "v5";
		public static readonly string PRODUCTION = "api.matchmore.io";
		private ApiClient _client;
		private readonly IStateRepository _state;
		private readonly IDeviceInfoProvider _deviceInfoProvider;
		private readonly string _environment;
		private string _apiKey;
		private bool _useSSL;
		private int? _servicePort;
		private Dictionary<string, IMatchMonitor> _monitors = new Dictionary<string, IMatchMonitor>();
		private List<EventHandler<MatchReceivedEventArgs>> _eventHandlers = new List<EventHandler<MatchReceivedEventArgs>>();
		private readonly IConfig _config;

		public string _worldId { get; }

		private ILocationService _locationService;

		public event EventHandler<MatchReceivedEventArgs> MatchReceived
		{
			add
			{
				foreach (var monitor in _monitors)
				{
					_eventHandlers.Add(value);
					monitor.Value.MatchReceived += value;
				}
			}
			remove
			{
				foreach (var monitor in _monitors)
				{
					_eventHandlers.Remove(value);
					monitor.Value.MatchReceived -= value;
				}
			}
		}

		public Dictionary<string, IMatchMonitor> Monitors => _monitors;

		public Device MainDevice
		{
			get
			{
				if (!_state.IsLoaded)
					_state.Load();
				return _state.MainDevice;
			}
			private set
			{
				_state.SetMainDevice(value);
			}
		}

		/// <summary>
		/// Gets the API URL.
		/// </summary>
		/// <value>The API URL.</value>
		public string ApiUrl
		{
			get
			{
				if (_environment != null)
				{
					var protocol = _useSSL ? "https" : "http";
					var port = _servicePort == null ? "" : ":" + _servicePort;
					return String.Format("{2}://{0}{3}/{1}", _environment, API_VERSION, protocol, port);
				}
				else
				{
					var protocol = _useSSL ? "https" : "http";
					return String.Format("{0}://{1}/{2}", protocol, PRODUCTION, API_VERSION);
				}
			}
		}

		public async static Task ConfigureAsync(string apiKey)
		{
			await ConfigureAsync(ConfigBuilder.WithApiKey(apiKey)).ConfigureAwait(false);
		}

		public static Task ConfigureAsync(IConfig config)
		{
			if (_instance != null)
			{
				throw new InvalidOperationException("Matchmore static instance already configured");
			}

			config.SetupDefaults();

			_instance = new Matchmore(config);
			return Task.CompletedTask;
		}

		public static void Reset()
		{
			if (_instance != null)
			{
				_instance.CleanUp();
				_instance = null;
			}
		}

		public static Matchmore Instance
		{
			get
			{
				if (_instance == null)
				{
					throw new InvalidOperationException("Matchmore not initialized!!!");
				}
				return _instance;
			}
		}

		public Matchmore(IConfig config)
		{
			_state = config.StateManager;
			_deviceInfoProvider = config.DeviceInfoProvider;
			_config = config;
			_worldId = ExtractWorldId(config.ApiKey);

			if (string.IsNullOrEmpty(config.ApiKey))
			{
				throw new ArgumentException("Api key null or empty");
			}

			_apiKey = config.ApiKey;
			_servicePort = config.ServicePort;
			_environment = config.Environment ?? PRODUCTION;
			_useSSL = config.UseSSL;
			_client = new ApiClient(config.HttpClient, _apiKey)
			{
				BaseUrl = ApiUrl
			};
		}

        /// <summary>
        /// Setups the main device async. This device will be used for all calls as default unless providing other device id or instance
        /// </summary>
        /// <returns>The main device async.</returns>
		public async Task<Device> SetupMainDeviceAsync()
		{
			if (MainDevice != null)
				return MainDevice;

			return await CreateDeviceAsync(new MobileDevice(), makeMain: true).ConfigureAwait(false);
		}

		/// <summary>
		/// Starts the location service
		/// </summary>
		public void StartLocationService()
		{
			EventHandler<Events.LocationUpdatedEventArgs> onLocationUpdated = async (object sender, Events.LocationUpdatedEventArgs e) =>
			{
				var location = e.Location;
				await _client.CreateLocationAsync(MainDevice.Id, new Location
				{
					Longitude = location.Longitude,
					Altitude = location.Altitude,
					Latitude = location.Latitude
				});
			};

			if (_locationService != null)
			{
				_locationService.LocationUpdated -= onLocationUpdated;
				_locationService.Stop();
			}

			_locationService = _config.LocationService;



			_locationService.LocationUpdated += onLocationUpdated;
			_locationService.Start();
		}

		public void WipeData()
		{
			_state.WipeData();
		}

		/// <summary>
		/// Creates the device.
		/// </summary>
		/// <returns>The device.</returns>
		/// <param name="device">Device.</param>
		/// <param name="makeMain">If set to <c>true</c> makes the device main. Not recommended</param>
		public async Task<Device> CreateDeviceAsync(Device device, bool makeMain = false)
		{
			if (_state == null)
			{
				throw new InvalidOperationException("Persistence wasn't setup!!!");
			}

			Device createdDevice = null;

			if (device is PinDevice)
			{
				var pinDevice = device as PinDevice;
				if (pinDevice.Location == null)
				{
					throw new ArgumentException("Location required for Pin Device");
				}

				createdDevice = pinDevice;
			}

			if (device is MobileDevice)
			{
				var mobileDevice = device as MobileDevice;

				mobileDevice.Name = string.IsNullOrEmpty(mobileDevice.Name) ? _deviceInfoProvider.DeviceName : mobileDevice.Name;
				mobileDevice.Platform = string.IsNullOrEmpty(mobileDevice.Platform) ? _deviceInfoProvider.Platform : mobileDevice.Platform;
				mobileDevice.DeviceToken = string.IsNullOrEmpty(mobileDevice.DeviceToken) ? "" : mobileDevice.DeviceToken;

				createdDevice = mobileDevice;
			}

			if (device is IBeaconDevice)
			{
				var ibeaconDevice = device as IBeaconDevice;

				if (string.IsNullOrEmpty(ibeaconDevice.Name))
				{
					throw new ArgumentException("Name required for Ibeacon Device");
				}

				createdDevice = ibeaconDevice;
			}

			var deviceInBackend = await _client.CreateDeviceAsync(createdDevice).ConfigureAwait(false);
			//only mobile can be considered as a main device
			if (makeMain && createdDevice is MobileDevice)
			{
				MainDevice = deviceInBackend;
			}
			else
			{
				_state.UpsertDevice(deviceInBackend);
			}
			return deviceInBackend;
		}

		/// <summary>
		/// Updates the device token for APNS or FCM communication
		/// </summary>
		/// <returns>The device communication async.</returns>
		/// <param name="comUpdate">com update.</param>
		/// <param name="deviceId">Device identifier.</param>
		public async Task<Device> UpdateDeviceCommunicationAsync(IMatchChannelUpdate comUpdate, string deviceId = null)
		=> await UpdateDeviceAsync(comUpdate.AsDeviceUpdate(), deviceId).ConfigureAwait(false);

		/// <summary>
		/// Updates the device.
		/// </summary>
		/// <returns>The device.</returns>
		/// <param name="deviceUpdate">Device update.</param>
		/// <param name="deviceId">Device identifier.</param>
		public async Task<Device> UpdateDeviceAsync(DeviceUpdate deviceUpdate, string deviceId = null)
		{
			var _deviceId = deviceId ?? MainDevice.Id;
			var updatedDevice = await _client.UpdateDeviceAsync(deviceId, deviceUpdate);
			var (device, isMain) = FindDevice(updatedDevice.Id);
			if (isMain)
			{
				MainDevice = updatedDevice;
			}
			else
			{
				_state.UpsertDevice(updatedDevice);
			}
			return updatedDevice;
		}

        /// <summary>
        /// Deletes the device async.
        /// </summary>
        /// <returns>The device async.</returns>
        /// <param name="deviceId">Device identifier.</param>
		public async Task DeleteDeviceAsync(string deviceId)
		{
			var (device, isMain) = FindDevice(deviceId);
			if (isMain)
				throw new MatchmoreException("You cannot delete your main device");
			await _client.DeleteDeviceAsync(deviceId);

			if (device != null)
				_state.RemoveDevice(device);
		}

		/// <summary>
		/// Creates the device and start listening. This is useful when the device also manages pins
		/// </summary>
		/// <returns>The pin device and start listening.</returns>
		/// <param name="device">Device.</param>
		/// <param name="channel">Channel.</param>
		public async Task<(Device, IMatchMonitor)> CreateDeviceAndStartListening(Device device, MatchChannel channel)
		{
			var createdDevice = await CreateDeviceAsync(device).ConfigureAwait(false);
			var monitor = SubscribeMatches(channel, createdDevice);

			return (createdDevice, monitor);
		}

		/// <summary>
		/// Creates the subscription
		/// </summary>
		/// <returns>The subscription.</returns>
		/// <param name="sub">Sub.</param>
		/// <param name="device">Device, if null will default to main device</param>
		public async Task<Subscription> CreateSubscriptionAsync(Subscription sub, Device device = null)
		{
			var usedDevice = device ?? _state.MainDevice;
			return await CreateSubscriptionAsync(sub, usedDevice.Id).ConfigureAwait(false);
		}

		/// <summary>
		/// Creates the subscription.
		/// </summary
		/// <returns>The subscription.</returns>
		/// <param name="sub">Sub.</param>
		/// <param name="deviceId">Device identifier.</param>
		/// <param name="ignorePersistence">If set to <c>true</c> ignore persistence.</param>
		public async Task<Subscription> CreateSubscriptionAsync(Subscription sub, string deviceId, bool ignorePersistence = false)
		{
			if (string.IsNullOrEmpty(deviceId))
				throw new ArgumentException("Device Id null or empty");

			sub.WorldId = sub.WorldId ?? _worldId;
			sub.DeviceId = sub.DeviceId ?? deviceId;

			var _sub = await _client.CreateSubscriptionAsync(deviceId, sub).ConfigureAwait(false);
			if (!ignorePersistence)
				_state.AddSubscription(_sub);
			return _sub;
		}

		/// <summary>
		/// Creates the publication.
		/// </summary>
		/// <returns>The publication.</returns>
		/// <param name="pub">Pub.</param>
		/// <param name="device">Device, if null will default to main device</param>
		public async Task<Publication> CreatePublicationAsync(Publication pub, Device device = null)
		{
			var usedDevice = device ?? _state.MainDevice;
			return await CreatePublicationAsync(pub, usedDevice.Id).ConfigureAwait(false);
		}

		/// <summary>
		/// Creates the publication.
		/// </summary>
		/// <returns>The publication.</returns>
		/// <param name="pub">Pub.</param>
		/// <param name="deviceId">Device identifier.</param>
		/// /// <param name="ignorePersistence">If set to <c>true</c> ignore persistence.</param>
		public async Task<Publication> CreatePublicationAsync(Publication pub, string deviceId, bool ignorePersistence = false)
		{
			if (string.IsNullOrEmpty(deviceId))
				throw new ArgumentException("Device Id null or empty");

			pub.WorldId = pub.WorldId ?? _worldId;
			pub.DeviceId = pub.DeviceId ?? deviceId;

			var _pub = await _client.CreatePublicationAsync(deviceId, pub).ConfigureAwait(false);
			if (!ignorePersistence)
				_state.AddPublication(_pub);
			return _pub;
		}

		/// <summary>
		/// Updates the location.
		/// </summary>
		/// <returns>The location.</returns>
		/// <param name="location">Location.</param>
		/// <param name="device">Device, if null will default to main device</param>
		public async Task UpdateLocationAsync(Location location, Device device = null)
		{
			var usedDevice = device ?? _state.MainDevice;
			await UpdateLocationAsync(location, usedDevice.Id).ConfigureAwait(false);
		}

		/// <summary>
		/// Updates the location.
		/// </summary>
		/// <returns>The location.</returns>
		/// <param name="location">Location.</param>
		/// <param name="deviceId">Device identifier.</param>
		public async Task UpdateLocationAsync(Location location, string deviceId)
		{
			if (string.IsNullOrEmpty(deviceId))
				throw new ArgumentException("Device Id null or empty");

			await _client.CreateLocationAsync(deviceId, location).ConfigureAwait(false);
		}

        /// <summary>
        /// Gets the match async.
        /// </summary>
        /// <returns>The match async.</returns>
        /// <param name="matchId">Match identifier.</param>
        /// <param name="device">Device.</param>
		public async Task<Match> GetMatchAsync(MatchId matchId, Device device = null){
			var usedDevice = device ?? _state.MainDevice;
			return await _client.GetMatchAsync(usedDevice.Id, matchId.ToString()).ConfigureAwait(false);
		}

		/// <summary>
		/// Gets the matches.
		/// </summary>
		/// <returns>The matches.</returns>
		/// <param name="device">Device, if null will default to main device</param>
		public async Task<List<Match>> GetMatchesAsync(Device device = null)
		{
			var usedDevice = device ?? _state.MainDevice;
			return await GetMatchesAsync(usedDevice.Id).ConfigureAwait(false);
		}

		/// <summary>
		/// Gets the matches.
		/// </summary>
		/// <returns>The matches.</returns>
		/// <param name="deviceId">Device identifier.</param>
		public async Task<List<Match>> GetMatchesAsync(string deviceId)
		{
			if (string.IsNullOrEmpty(deviceId))
				throw new ArgumentException("Device Id null or empty");

			return await _client.GetMatchesAsync(deviceId).ConfigureAwait(false);
		}

		/// <summary>
		/// Subscribes the matches.
		/// </summary>
		/// <returns>The matches.</returns>
		/// <param name="channel">Channel.</param>
		/// <param name="deviceId">Device identifier.</param>
		public IMatchMonitor SubscribeMatches(MatchChannel channel, string deviceId)
		{
			if (string.IsNullOrEmpty(deviceId))
				throw new ArgumentException("Device Id null or empty");

			return SubscribeMatches(channel, FindDevice(deviceId).device);
		}

		/// <summary>
		/// Subscribes the matches.
		/// </summary>
		/// <returns>The matches.</returns>
		/// <param name="channel">Channel.</param>
		/// <param name="device">Device, if null will default to main device</param>
		public IMatchMonitor SubscribeMatches(MatchChannel channel, Device device = null)
		{
			var deviceToSubscribe = device ?? _state.MainDevice;

			var monitors = new List<IMatchMonitor>();

			if (channel.HasFlag(MatchChannel.Polling))
				monitors.Add(new PollingMatchMonitor(this, deviceToSubscribe));

			if (channel.HasFlag(MatchChannel.Websocket))
				monitors.Add(new WebsocketMatchMonitor(this, deviceToSubscribe, _worldId));

			if (channel.HasFlag(MatchChannel.APNS))
			{
				//todo
			}

			if (channel.HasFlag(MatchChannel.FCM))
			{
				//todo
			}

			if (!monitors.Any())
				throw new MatchmoreException("Invalid match monitors");

			IMatchMonitor monitor = null;
			if (monitors.Count == 1)
				monitor = monitors.Single();
			else
				monitor = new MultiChannelMatchMonitor(monitors.ToArray());

			if (_monitors.ContainsKey(deviceToSubscribe.Id))
			{
				_monitors[deviceToSubscribe.Id].Stop();
				_monitors.Remove(deviceToSubscribe.Id);
			}

			foreach (var handler in _eventHandlers)
			{
				monitor.MatchReceived += handler;
			}

			_monitors.Add(deviceToSubscribe.Id, monitor);

			return monitor;
		}


		public async Task DeletePublicationAsync(string pubId, string deviceId = null)
		{
			var _deviceId = deviceId ?? MainDevice.Id;

			await _client.DeletePublicationAsync(_deviceId, pubId);
			var ac = _state.ActivePublications.FirstOrDefault(p => p.Id == pubId);
			if (ac != null)
				_state.RemovePublication(ac);
		}

		public async Task DeleteSubscriptionAsync(string subId, string deviceId = null)
		{
			var _deviceId = deviceId ?? MainDevice.Id;

			await _client.DeleteSubscriptionAsync(_deviceId, subId);
			var ac = _state.ActiveSubscriptions.FirstOrDefault(p => p.Id == subId);
			if (ac != null)
				_state.RemoveSubscription(ac);
		}


		(Device device, bool isMain) FindDevice(string deviceId)
		{
			if (_state.MainDevice.Id == deviceId)
				return (_state.MainDevice, true);

			return (_state.Devices.FirstOrDefault(pin => pin.Id == deviceId), false);
		}

        /// <summary>
        /// Gets the devices. All device types included
        /// </summary>
        /// <value>The devices.</value>
        public IEnumerable<Device> Devices => _state.Devices;

		/// <summary>
		/// Gets the active subscriptions.
		/// </summary>
		/// <value>The active subscriptions.</value>
		public IEnumerable<Subscription> ActiveSubscriptions => _state.ActiveSubscriptions;

		/// <summary>
		/// Gets the active publications.
		/// </summary>
		/// <value>The active publications.</value>
		public IEnumerable<Publication> ActivePublications => _state.ActivePublications;

		/// <summary>
		/// Cleans up. Stops all monitors, destroys the game object
		/// </summary>
		public void CleanUp() => StopEverything();

		public void StopEverything()
		{
			var keys = new List<string>(_monitors.Keys);
			foreach (var key in keys)
			{
				if (_monitors.TryGetValue(key, out IMatchMonitor monitor))
					monitor.Stop();
			}

			_monitors.Clear();
			if (_locationService != null)
				_locationService.Stop();

		}

		class ApiKeyObject
		{
			public string Sub { get; set; }
		}

		public static string ExtractWorldId(string apiKey)
		{
			try
			{
				var subjectData = Convert.FromBase64String(apiKey.Split('.')[1]);
				var subject = Encoding.UTF8.GetString(subjectData);
				var deserializedApiKey = JsonConvert.DeserializeObject<ApiKeyObject>(subject);

				return deserializedApiKey.Sub;
			}
			catch (Exception e)
			{
				throw new ArgumentException("Api key was invalid", e);
			}
		}
	}

}