using System.Collections;

using System;

using System.Threading;

namespace Matchmore.SDK
{
    public enum LocationServiceType
    {
        threaded, coroutine
    }

    public interface ILocationService
    {
        Location MockLocation { get; set; }

        Action<Location> OnUpdateLocation { get; }

        void Start();
        void Stop();
    }

   
}