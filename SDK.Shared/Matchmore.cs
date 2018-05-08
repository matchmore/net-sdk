using System;
using System.Collections.Generic;

using Matchmore.SDK.Persistence;
using System.Threading.Tasks;
using System.Net.Http;

namespace Matchmore.SDK
{
    public class Matchmore
    {
        private static Matchmore _instance;
        public static readonly string API_VERSION = "v5";
        public static readonly string PRODUCTION = "api.matchmore.io";
        private ApiClient _client;
        private IStateManager _state;
        private string _environment;
        private string _apiKey;
        private bool _secured;
        private int? _servicePort;
        private Dictionary<string, IMatchMonitor> _monitors = new Dictionary<string, IMatchMonitor>();
        private List<EventHandler<MatchReceivedEventArgs>> _eventHandlers = new List<EventHandler<MatchReceivedEventArgs>>();
        private readonly Config _config;
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

        public Dictionary<string, IMatchMonitor> Monitors
        {
            get
            {
                return _monitors;
            }
        }

        public Device MainDevice
        {
            get
            {
                return _state.Device;
            }
            private set
            {
                _state.Device = value;
                _state.Save();
            }
        }

        /// <summary>
        /// Mock location for development purposes which will be used instead of the device location
        /// </summary>
        /// <value>The mock location.</value>
        public Location MockLocation { get; set; }

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

        public static void Configure(string apiKey)
        {
            Configure(Config.WithApiKey(apiKey));
        }

        public static void Configure(Config config)
        {
            if (_instance != null)
            {
                throw new InvalidOperationException("Matchmore static instance already configured");
            }
            _instance = new Matchmore(config, new HttpClient());
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

        public Matchmore(Config config, HttpClient httpClient)
        {
            _config = config;

            //MatchmoreLogger.Enabled = config.LoggingEnabled;

            if (string.IsNullOrEmpty(config.ApiKey))
            {
                throw new ArgumentException("Api key null or empty");
            }

            _apiKey = config.ApiKey;
            _servicePort = config.ServicePort;
            _environment = config.Environment ?? PRODUCTION;
            _secured = config.UseSecuredCommunication;
            _client = new ApiClient(httpClient, _apiKey)
            {
                BaseUrl = ApiUrl
            };

            //_state = new StateManager(_environment, config.PersistenceFile);
        }

        public async Task<Device> SetupMainDevice()
        {
            if (MainDevice != null)
                return MainDevice;

            return await CreateDevice(new MobileDevice(), makeMain: true);
        }

        /// <summary>
        /// Starts the location service with specified type
        /// Coroutined, uses a dedicated coroutine
        /// Threaded, creates an ongoing thread which polls the Unity location service and reports it to matchmore
        /// </summary>
        /// <param name="type">Type,</param>
        public void StartLocationService(LocationServiceType type)
        {
            if (_locationService != null)
            {
                _locationService.Stop();
            }

            _locationService.MockLocation = MockLocation;
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
        /// <param name="makeMain">If set to <c>true</c> make main.</param>
        public async Task<Device> CreateDevice(Device device, bool makeMain = false)
        {
            if (_state == null)
            {
                throw new InvalidOperationException("Persistence wasn't setup!!!");
            }

            Device createdDevice = null;

            if (!string.IsNullOrEmpty(device.Id))
            {
                //UnityEngine.MonoBehaviour.print("Device ID will be ignored!!!");
            }

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

                mobileDevice.Name = string.IsNullOrEmpty(mobileDevice.Name) ? "Main" : mobileDevice.Name;
                mobileDevice.Platform = string.IsNullOrEmpty(mobileDevice.Platform) ? "main" : mobileDevice.Platform;
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
            var deviceInBackend = await _client.CreateDeviceAsync(createdDevice);
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

            var createdDevice = await _client.CreateDeviceAsync(pinDevice);

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
            var createdDevice = await CreatePinDevice(pinDevice);
            var monitor = SubscribeMatches(channel, createdDevice);

            return Tuple.Create(createdDevice, monitor);
        }

        /// <summary>
        /// Creates the subscription
        /// </summary>
        /// <returns>The subscription.</returns>
        /// <param name="sub">Sub.</param>
        /// <param name="device">Device, if null will default to main device</param>
        public async Task<Subscription> CreateSubscription(Subscription sub, Device device = null)
        {
            var usedDevice = device != null ? device : _state.Device;
            return await CreateSubscription(sub, usedDevice.Id);
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

            var _sub = await _client.CreateSubscriptionAsync(deviceId, sub);
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
        public async Task<Publication> CreatePublication(Publication pub, Device device = null)
        {
            var usedDevice = device != null ? device : _state.Device;
            return await CreatePublication(pub, usedDevice.Id);
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

            var _pub = await _client.CreatePublicationAsync(deviceId, pub);
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
        public async Task UpdateLocation(Location location, Device device = null)
        {
            var usedDevice = device != null ? device : _state.Device;
            await UpdateLocation(location, usedDevice.Id);
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

            await _client.CreateLocationAsync(deviceId, location);
        }


        /// <summary>
        /// Gets the matches.
        /// </summary>
        /// <returns>The matches.</returns>
        /// <param name="device">Device, if null will default to main device</param>
        public async Task<List<Match>> GetMatches(Device device = null)
        {
            var usedDevice = device != null ? device : _state.Device;
            return await GetMatches(usedDevice.Id);
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

            return await _client.GetMatchesAsync(deviceId);
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
            var deviceToSubscribe = device == null ? _state.Device : device;
            IMatchMonitor monitor = null;
            //switch (channel)
            //{
            //    case MatchChannel.polling:
            //        monitor = CreatePollingMonitor(deviceToSubscribe);
            //        break;
            //    case MatchChannel.websocket:
            //        monitor = CreateWebsocketMonitor(deviceToSubscribe);
            //        break;
            //    case MatchChannel.threadedPolling:
            //        monitor = CreateThreadedPollingMonitor(deviceToSubscribe);
            //        break;
            //    default:
            //        break;
            //}

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
            if (_state.Device.Id == deviceId)
                return _state.Device;
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
    }

}