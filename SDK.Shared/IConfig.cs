using System.Net.Http;
using Matchmore.SDK.Persistence;

namespace Matchmore.SDK
{
    public interface IConfig
    {
		/// <summary>
        /// Gets or sets the API key.
        /// </summary>
        /// <value>The API key.</value>
        string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the environment.
        /// </summary>
        /// <value>The environment.</value>
        string Environment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:Matchmore.SDK.IConfig"/> use secured communication.
        /// </summary>
        /// <value><c>true</c> if use secured communication; otherwise, <c>false</c>.</value>
        bool UseSecuredCommunication { get; set; }

        /// <summary>
        /// Gets or sets the service port.
        /// </summary>
        /// <value>The service port.</value>
        int? ServicePort { get; set; }

        /// <summary>
        /// Gets or sets the state manager.
        /// </summary>
        /// <value>The state manager.</value>
        IStateRepository StateManager { get; set; }

        /// <summary>
        /// Gets or sets the device info provider.
        /// </summary>
        /// <value>The device info provider.</value>
        IDeviceInfoProvider DeviceInfoProvider { get; set; }

        /// <summary>
        /// Gets or sets the location service.
        /// </summary>
        /// <value>The location service.</value>
		ILocationService LocationService { get; set; }
        /// <summary>
        /// Gets or sets the http client which will be used to issue requests
        /// </summary>
        /// <value>The http client.</value>
        HttpClient HttpClient { get; set; }

        /// <summary>
        /// Setups the defaults dependecies for the platform
        /// </summary>
		void SetupDefaults();
    }
}