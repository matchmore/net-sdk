using System.Collections.Generic;

namespace Matchmore.SDK.Persistence
{
    public interface IStateManager
    {
        string PersistenceFile { get; }
        Device Device { get; set; }
        string PersistencePath { get; }
        List<Subscription> ActiveSubscriptions { get; }
        List<Publication> ActivePublications { get; }
        List<PinDevice> Pins { get; }

        void AddPinDevice(PinDevice pinDevice);
        void AddPublication(Publication pub);
        void AddSubscription(Subscription sub);
        void Load();
        void Save();
        void WipeData();
    }
}