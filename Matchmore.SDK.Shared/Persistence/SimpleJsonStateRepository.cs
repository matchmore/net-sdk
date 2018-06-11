using System.Linq;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System;

namespace Matchmore.SDK.Persistence
{
    /// <summary>
    /// Simple json state repository.
	/// A reference implementation for a matchmore client side specific state repo using a simple file which hold json
    /// </summary>
    public class SimpleJsonStateRepository : IStateRepository
    {
        private State _state;
        private string _env;
        private string _persistenceFile;
        private string _persistenceDirectory;
        private bool _isLoaded;

        public virtual string PersistenceDirectory => _persistenceDirectory ?? "";

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
                return _state.MainDevice;
            }
        }

        public void SetMainDevice(Device device)
        {
            if (_state == null)
            {
                _state = new State();
            }

            _state.MainDevice = device;
            Save();
        }


        public IEnumerable<Subscription> ActiveSubscriptions => _state.Subscriptions.Where(pub => pub.IsAlive());

        public IEnumerable<Publication> ActivePublications => _state.Publications.Where(pub => pub.IsAlive());

        public void PruneDead()
        {
            _state.Publications = ActivePublications.ToList();
            _state.Subscriptions = ActiveSubscriptions.ToList();

            Save();
        }

        public IEnumerable<Device> Devices => _state.Devices.AsReadOnly();

        public bool IsLoaded => _isLoaded;

        public SimpleJsonStateRepository(string env = "prod", string persistenceFile = null, string persistenceDirectory = null)
        {
            _env = env;
            _persistenceFile = string.IsNullOrEmpty(persistenceFile) ? "state.data" : persistenceFile;
            _persistenceDirectory = persistenceDirectory;
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

        void Save()
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

        public void RemoveSubscription(Subscription sub)
        {
            var subToDelete = _state.Subscriptions.FirstOrDefault(p => sub.Id == p.Id);
            if (subToDelete == null)
                return;
            _state.Subscriptions.Remove(subToDelete);
            Save();
        }

        public void AddPublication(Publication pub)
        {
            _state.Publications.Add(pub);
            Save();
        }

        public void RemovePublication(Publication pub)
        {
            var pubToDelete = _state.Publications.FirstOrDefault(p => pub.Id == p.Id);
            if (pubToDelete == null)
                return;
            _state.Publications.Remove(pubToDelete);
            Save();
        }

        public void UpsertDevice(Device device)
        {
            var existing = _state.Devices.FirstOrDefault(d => device.Id == d.Id);
            if (existing != null)
                _state.Devices.Remove(existing);

            _state.Devices.Add(device);
            Save();
        }

        public void RemoveDevice(Device device)
        {
            var pinToDelete = _state.Devices.FirstOrDefault(p => device.Id == p.Id);
            if (pinToDelete == null)
                return;
            _state.Devices.Remove(pinToDelete);
            Save();
        }
    }
}