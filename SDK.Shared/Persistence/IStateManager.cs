using System.Collections.Generic;

namespace Matchmore.SDK.Persistence
{
    public interface IStateManager
    {
		/// <summary>
        /// Device which is considered as main device
        /// </summary>
        /// <value>The device.</value>
        Device MainDevice { get; }

		/// <summary>
		/// Sets the main device.
		/// </summary>
		/// <param name="device">Device.</param>
		void SetMainDevice(Device device);

        /// <summary>
        /// Gets the active subscriptions.
        /// </summary>
        /// <value>The active subscriptions.</value>
        List<Subscription> ActiveSubscriptions { get; }
        /// <summary>
        /// Gets the active publications.
        /// </summary>
        /// <value>The active publications.</value>
        List<Publication> ActivePublications { get; }
        /// <summary>
        /// Gets the pins.
        /// </summary>
        /// <value>The pins.</value>
        List<PinDevice> Pins { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:Matchmore.SDK.Persistence.IStateManager"/> is loaded.
        /// </summary>
        /// <value><c>true</c> if is loaded; otherwise, <c>false</c>.</value>
        bool IsLoaded { get; }

        /// <summary>
        /// Adds the pin device.
        /// </summary>
        /// <param name="pinDevice">Pin device.</param>
        void AddPinDevice(PinDevice pinDevice);
        /// <summary>
        /// Adds a publication.
        /// </summary>
        /// <param name="pub">Pub.</param>
        void AddPublication(Publication pub);
        /// <summary>
        /// Adds a subscription.
        /// </summary>
        /// <param name="sub">Sub.</param>
        void AddSubscription(Subscription sub);
        /// <summary>
        /// Loads current state from the file
        /// </summary>
        void Load();
        /// <summary>
        /// Saves current state to the file
        /// </summary>
        void Save();
        /// <summary>
        /// Wipes the data.
        /// </summary>
        void WipeData();
    }
}