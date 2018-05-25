using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Matchmore.SDK.Persistence
{
    public class State
    {
        private bool _isDirty;
        private string environment;
        private Device mainDevice;
        private List<Subscription> subscriptions;
        private List<Publication> publications;
        private List<Device> devices;

        [JsonProperty(PropertyName = "env")]
        public string Environment
		{
			get => environment;

			set
			{
				_isDirty = true;
				environment = value;
			}
		}

		[JsonProperty(PropertyName = "mainDevice")]
        public Device MainDevice
		{
			get => mainDevice;

			set
			{
				_isDirty = true;
				mainDevice = value;
			}
		}

		[JsonProperty(PropertyName = "subscriptions")]
        public List<Subscription> Subscriptions
		{
			get => subscriptions;

			set
			{
				_isDirty = true;
				subscriptions = value;
			}
		}

		[JsonProperty(PropertyName = "publications")]
        public List<Publication> Publications
		{
			get => publications;

			set
			{
				_isDirty = true;
				publications = value;
			}
		}

		[JsonProperty(PropertyName = "devices")]
        public List<Device> Devices
		{
			get => devices;

			set
			{
				_isDirty = true;
				devices = value;
			}
		}      
		public State()
        {
            Subscriptions = new List<Subscription>();
            Publications = new List<Publication>();
			Devices = new List<Device>();
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