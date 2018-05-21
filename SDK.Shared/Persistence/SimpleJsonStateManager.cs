using System.Linq;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System;

namespace Matchmore.SDK.Persistence
{
    public class SimpleJsonStateManager : IStateManager
    {
        private State _state;
        private string _env;
        private string _persistenceFile;
		private bool _isLoaded;

		public virtual string PersistenceDirectory => "";

		public string PersistenceFileName => _persistenceFile;

		public string PersistencePath => Path.Combine(PersistenceDirectory, PersistenceFileName);

        public Device MainDevice
        {
            get
            {
                if (_state == null)
                {
                    return null;
                }
                else
                {
                    return _state.Device;
                }
            }
        }

		public void SetMainDevice(Device device){
			if (_state == null)
            {
                _state = new State();
            }

            _state.Device = device;
		}


		public List<Subscription> ActiveSubscriptions => _state.Subscriptions;

		public List<Publication> ActivePublications => _state.Publications;

		public List<PinDevice> Pins => _state.Pins;

		public bool IsLoaded => _isLoaded;

		public SimpleJsonStateManager(string env, string persistenceFile = null)
        {
            _env = env;
            _persistenceFile = string.IsNullOrEmpty(persistenceFile) ? "state.data" : persistenceFile;
        }

        public void WipeData()
        {
            File.Delete(PersistencePath);
        }

        public void Load()
        {
            if (!File.Exists(PersistencePath))
            {
                _state = new State
                {
                    Environment = _env
                };
                return;
            }

            if (new FileInfo(PersistencePath).Length == 0)
            {
                _state = new State()
                {
                    Environment = _env  
                };
                return;
            }

            using (StreamReader file = File.OpenText(PersistencePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                var state = (State)serializer.Deserialize(file, typeof(State));
                if (IsCorrectEnv(state))
                {
                    _state = state;
					_isLoaded = true;
                }
                else
                {
                    throw new InvalidOperationException("State belongs to a wrong environment, please wipe the data using the WipeData() method");
                }
            }
        }

		private bool IsCorrectEnv(State state) => state.Environment == _env;

		public void Save()
        {
            if (_state.IsDirty())
            {
                using (StreamWriter file = File.CreateText(PersistencePath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    var json = JsonConvert.SerializeObject(_state, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });
                    serializer.Serialize(file, _state);
                    _state.MarkAsClean();
                }
            }
        }

        public void AddSubscription(Subscription sub)
        {
            _state.Subscriptions.Add(sub);
            Save();
        }

        public void AddPublication(Publication pub)
        {
            _state.Publications.Add(pub);
            Save();
        }

        public void AddPinDevice(PinDevice pinDevice)
        {
            _state.Pins.Add(pinDevice);
            Save();
        }

        public void PruneDead()
        {
            _state.Publications = _state.Publications.Where(pub => pub.IsAlive()).ToList();

            _state.Subscriptions = _state.Subscriptions.Where(sub => sub.IsAlive()).ToList();
        }
    }
}