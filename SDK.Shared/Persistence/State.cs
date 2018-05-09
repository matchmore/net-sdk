using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Matchmore.SDK.Persistence
{
    public class State
    {
        private bool _isDirty;
        private string environment;
        private Device device;
        private List<Subscription> subscriptions;
        private List<Publication> publications;
        private List<PinDevice> pins;

        [JsonProperty(PropertyName = "env")]
        public string Environment
        {
            get
            {
                return environment;
            }

            set
            {
                _isDirty = true;
                environment = value;
            }
        }

        [JsonProperty(PropertyName = "device")]
        public Device Device
        {
            get
            {
                return device;
            }

            set
            {
                _isDirty = true;
                device = value;
            }
        }

        [JsonProperty(PropertyName = "subscriptions")]
        public List<Subscription> Subscriptions
        {
            get
            {
                return subscriptions;
            }

            set
            {
                _isDirty = true;
                subscriptions = value;
            }
        }

        [JsonProperty(PropertyName = "publications")]
        public List<Publication> Publications
        {
            get
            {
                return publications;
            }

            set
            {
                _isDirty = true;
                publications = value;
            }
        }

        [JsonProperty(PropertyName = "pins")]
        public List<PinDevice> Pins
        {
            get
            {
                return pins;
            }

            set
            {
                _isDirty = true;
                pins = value;
            }
        }

        public State()
        {
            Subscriptions = new List<Subscription>();
            Publications = new List<Publication>();
            Pins = new List<PinDevice>();
        }

        public bool IsDirty()
        {
            return _isDirty;
        }

        public void MarkAsClean()
        {
            _isDirty = false;
        }
    }
}