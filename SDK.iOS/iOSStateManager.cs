using System;
using System.Collections.Generic;
using Matchmore.SDK.Persistence;
using Xamarin.Forms;

[assembly: Dependency(typeof(iOSStateManager))]
namespace Matchmore.SDK.Persistence
{
	public class iOSStateManager : SimpleJsonStateManager, IStateManager
	{
		public override string PersistenceDirectory => Environment.GetFolderPath(Environment.SpecialFolder.Personal);
		public iOSStateManager(string env, string fileName) : base(env ?? "prod", fileName)
		{
		}

		public iOSStateManager() : this(null, null)
		{
		}
	}
}
