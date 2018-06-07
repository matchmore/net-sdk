using System.Collections.Generic;

namespace Matchmore.SDK.Persistence
{
    public interface IStateRepository
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
		IEnumerable<Subscription> ActiveSubscriptions { get; }
        /// <summary>
        /// Gets the active publications.
        /// </summary>
        /// <value>The active publications.</value>
		IEnumerable<Publication> ActivePublications { get; }
        /// <summary>
        /// Gets the devices.
        /// </summary>
        /// <value>The devices.</value>
		IEnumerable<Device> Devices { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:Matchmore.SDK.Persistence.IStateManager"/> is loaded.
        /// </summary>
        /// <value><c>true</c> if is loaded; otherwise, <c>false</c>.</value>
        bool IsLoaded { get; }

        /// <summary>
        /// Adds the device.
        /// </summary>
        /// <param name="device">Device.</param>
		void UpsertDevice(Device device);
        /// <summary>
        /// Removes the device.
        /// </summary>
        /// <param name="device">Device.</param>
		void RemoveDevice(Device device);
        /// <summary>
        /// Adds a publication.
        /// </summary>
        /// <param name="pub">Pub.</param>
        void AddPublication(Publication pub);
        /// <summary>
        /// Removes the publication.
        /// </summary>
        /// <param name="pub">Pub.</param>
		void RemovePublication(Publication pub);
        /// <summary>
        /// Adds a subscription.
        /// </summary>
        /// <param name="sub">Sub.</param>
        void AddSubscription(Subscription sub);
        /// <summary>
        /// Removes the subscription.
        /// </summary>
        /// <param name="sub">Sub.</param>
		void RemoveSubscription(Subscription sub);
        /// <summary>
        /// Loads current state from the file
        /// </summary>
        void Load();
        /// <summary>
        /// Wipes the data.
        /// </summary>
        void WipeData();
    }
}