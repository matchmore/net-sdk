using System;
using System.Collections.Generic;

using Matchmore.SDK.Persistence;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;

namespace Matchmore.SDK
{
	public class Matchmore
	{
		private static Matchmore _instance;
		public static readonly string API_VERSION = "v5";
		public static readonly string PRODUCTION = "api.matchmore.io";
		private ApiClient _client;
		private readonly IStateManager _state;
		private readonly IDeviceInfoProvider _deviceInfoProvider;
		private string _environment;
		private string _apiKey;
		private bool _secured;
		private int? _servicePort;
		private Dictionary<string, IMatchMonitor> _monitors = new Dictionary<string, IMatchMonitor>();
		private List<EventHandler<MatchReceivedEventArgs>> _eventHandlers = new List<EventHandler<MatchReceivedEventArgs>>();
		private readonly Config _config;

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
				return _state.MainDevice;
			}
			private set
			{
				_state.SetMainDevice(value);
				_state.Save();
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
					var protocol = _secured ? "https" : "http";
					var port = _servicePort == null ? "" : ":" + _servicePort;
					return String.Format("{2}://{0}{3}/{1}", _environment, API_VERSION, protocol, port);
				}
				else
				{
					var protocol = _secured ? "https" : "http";
					return String.Format("{0}://{1}/{2}", protocol, PRODUCTION, API_VERSION);
				}
			}
		}

		public async static Task ConfigureAsync(string apiKey)
		{
			await ConfigureAsync(Config.WithApiKey(apiKey)).ConfigureAwait(false);
		}

		public static async Task ConfigureAsync(Config config)
		{
			if (_instance != null)
			{
				throw new InvalidOperationException("Matchmore static instance already configured");
			}

			config.Defaults();

			_instance = new Matchmore(config);
			await _instance.SetupMainDeviceAsync().ConfigureAwait(false);
		}

		public static void Reset()
		{
			if (_instance != null)
			{
				_instance.CleanUp();
				_instance = null;
			}
		}

		//
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

		public Matchmore(Config config)
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
			_secured = config.UseSecuredCommunication;
			_client = new ApiClient(config.HttpClient, _apiKey)
			{
				BaseUrl = ApiUrl
			};
		}

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
			if (_locationService != null)
			{
				_locationService.Stop();
			}
			_locationService = new SimpleLocationService(_client, MainDevice);

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
			return deviceInBackend;
		}

		/// <summary>
		/// Creates the pin device.
		/// </summary>
		/// <returns>The pin device.</returns>
		/// <param name="pinDevice">Pin device.</param>
		public async Task<PinDevice> CreatePinDevice(PinDevice pinDevice, bool ignorePersistence = false)
		{
			if (pinDevice.Location == null)
			{
				throw new ArgumentException("Location required for Pin Device");
			}

			var createdDevice = await _client.CreateDeviceAsync(pinDevice).ConfigureAwait(false);

			//The generated swagger api returns a generic device partially losing the information about the pin.
			//We rewrite the data to fit the pin device contract.
			var createdPin = new PinDevice
			{
				Id = createdDevice.Id,
				CreatedAt = createdDevice.CreatedAt,
				Location = pinDevice.Location,
				Group = createdDevice.Group,
				Name = createdDevice.Name,
				UpdatedAt = createdDevice.UpdatedAt
			};
			if (!ignorePersistence)
				_state.AddPinDevice(createdPin);

			return createdPin;
		}

		/// <summary>
		/// Creates the pin device and start listening. This is useful when the device also manages pins
		/// </summary>
		/// <returns>The pin device and start listening.</returns>
		/// <param name="pinDevice">Pin device.</param>
		/// <param name="channel">Channel.</param>
		public async Task<Tuple<PinDevice, IMatchMonitor>> CreatePinDeviceAndStartListening(PinDevice pinDevice, MatchChannel channel)
		{
			var createdDevice = await CreatePinDevice(pinDevice).ConfigureAwait(false);
			var monitor = SubscribeMatches(channel, createdDevice);

			return Tuple.Create(createdDevice, monitor);
		}

		/// <summary>
		/// Creates the subscription
		/// </summary>
		/// <returns>The subscription.</returns>
		/// <param name="sub">Sub.</param>
		/// <param name="device">Device, if null will default to main device</param>
		public async Task<Subscription> CreateSubscriptionAsync(Subscription sub, Device device = null)
		{
			var usedDevice = device != null ? device : _state.MainDevice;
			return await CreateSubscription(sub, usedDevice.Id).ConfigureAwait(false);
		}

		/// <summary>
		/// Creates the subscription.
		/// </summary
		/// <returns>The subscription.</returns>
		/// <param name="sub">Sub.</param>
		/// <param name="deviceId">Device identifier.</param>
		/// <param name="ignorePersistence">If set to <c>true</c> ignore persistence.</param>
		public async Task<Subscription> CreateSubscription(Subscription sub, string deviceId, bool ignorePersistence = false)
		{
			if (string.IsNullOrEmpty(deviceId))
			{
				throw new ArgumentException("Device Id null or empty");
			}

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
			var usedDevice = device != null ? device : _state.MainDevice;
			return await CreatePublication(pub, usedDevice.Id).ConfigureAwait(false);
		}

		/// <summary>
		/// Creates the publication.
		/// </summary>
		/// <returns>The publication.</returns>
		/// <param name="pub">Pub.</param>
		/// <param name="deviceId">Device identifier.</param>
		/// /// <param name="ignorePersistence">If set to <c>true</c> ignore persistence.</param>
		public async Task<Publication> CreatePublication(Publication pub, string deviceId, bool ignorePersistence = false)
		{
			if (string.IsNullOrEmpty(deviceId))
			{
				throw new ArgumentException("Device Id null or empty");
			}

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
			var usedDevice = device != null ? device : _state.MainDevice;
			await UpdateLocation(location, usedDevice.Id).ConfigureAwait(false);
		}

		/// <summary>
		/// Updates the location.
		/// </summary>
		/// <returns>The location.</returns>
		/// <param name="location">Location.</param>
		/// <param name="deviceId">Device identifier.</param>
		public async Task UpdateLocation(Location location, string deviceId)
		{
			if (string.IsNullOrEmpty(deviceId))
			{
				throw new ArgumentException("Device Id null or empty");
			}

			await _client.CreateLocationAsync(deviceId, location).ConfigureAwait(false);
		}


		/// <summary>
		/// Gets the matches.
		/// </summary>
		/// <returns>The matches.</returns>
		/// <param name="device">Device, if null will default to main device</param>
		public async Task<List<Match>> GetMatchesAsync(Device device = null)
		{
			var usedDevice = device != null ? device : _state.MainDevice;
			return await GetMatches(usedDevice.Id).ConfigureAwait(false);
		}

		/// <summary>
		/// Gets the matches.
		/// </summary>
		/// <returns>The matches.</returns>
		/// <param name="deviceId">Device identifier.</param>
		public async Task<List<Match>> GetMatches(string deviceId)
		{
			if (string.IsNullOrEmpty(deviceId))
			{
				throw new ArgumentException("Device Id null or empty");
			}

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
			{
				throw new ArgumentException("Device Id null or empty");
			}

			return SubscribeMatches(channel, FindDevice(deviceId));
		}

		/// <summary>
		/// Subscribes the matches.
		/// </summary>
		/// <returns>The matches.</returns>
		/// <param name="channel">Channel.</param>
		/// <param name="device">Device, if null will default to main device</param>
		public IMatchMonitor SubscribeMatches(MatchChannel channel, Device device = null)
		{
			var deviceToSubscribe = device == null ? _state.MainDevice : device;
			IMatchMonitor monitor = null;
			switch (channel)
			{
				case MatchChannel.polling:
					monitor = new PollingMatchMonitor(_client, deviceToSubscribe);
					break;
				case MatchChannel.websocket:
					monitor = new WebsocketMatchMonitor(_client, deviceToSubscribe, _worldId);
					break;
				//    case MatchChannel.threadedPolling:
				//        monitor = CreateThreadedPollingMonitor(deviceToSubscribe);
				//        break;
				default:
					break;
			}

			if (monitor == null)
			{
				throw new ArgumentException(String.Format("{0} is an unrecognized channel", channel));
			}

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


		private Device FindDevice(string deviceId)
		{
			if (_state.MainDevice.Id == deviceId)
				return _state.MainDevice;
			else
				return _state.Pins.Find(pin => pin.Id == deviceId);
		}

		/// <summary>
		/// Gets the active subscriptions.
		/// </summary>
		/// <value>The active subscriptions.</value>
		public IEnumerable<Subscription> ActiveSubscriptions
		{
			get
			{
				return _state.ActiveSubscriptions.AsReadOnly();
			}
		}

		/// <summary>
		/// Gets the active publications.
		/// </summary>
		/// <value>The active publications.</value>
		public IEnumerable<Publication> ActivePublications
		{
			get
			{
				return _state.ActivePublications.AsReadOnly();
			}
		}

		/// <summary>
		/// Cleans up. Stops all monitors, destroys the game object
		/// </summary>
		public void CleanUp()
		{
			StopEverything();
		}

		public void StopEverything()
		{
			var keys = new List<string>(_monitors.Keys);
			foreach (var key in keys)
			{
				IMatchMonitor monitor = null;
				if (_monitors.TryGetValue(key, out monitor))
					monitor.Stop();
			}

			_monitors.Clear();
			if (_locationService != null)
				_locationService.Stop();

		}

		private class ApiKeyObject
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