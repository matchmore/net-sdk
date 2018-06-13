using System;
using Matchmore.SDK.Events;

namespace Matchmore.SDK
{
	public interface ILocationService
	{
		event EventHandler<LocationUpdatedEventArgs> LocationUpdated;
		void Start();
		void Stop();
	}
}