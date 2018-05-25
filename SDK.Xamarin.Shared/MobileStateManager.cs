using System;
using Matchmore.SDK.Persistence;

namespace Matchmore.SDK.Xamarin.Shared
{
	public class MobileStateManager : SimpleJsonStateRepository, IStateRepository
    {
        public override string PersistenceDirectory => Environment.GetFolderPath(Environment.SpecialFolder.Personal);

		public MobileStateManager(string env, string fileName) : base(env ?? "prod", fileName)
        {
        }

		public MobileStateManager() : this(null, null)
        {
        }
    }
}
