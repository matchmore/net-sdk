using System;
using System.Collections.Generic;
using Matchmore.SDK.Persistence;
using Xamarin.Forms;

[assembly: Dependency(typeof(AndroidStateManager))]
namespace Matchmore.SDK.Persistence
{
	public class AndroidStateManager : SimpleJsonStateManager, IStateManager
	{
		public override string PersistenceDirectory => Environment.GetFolderPath(Environment.SpecialFolder.Personal);
		public AndroidStateManager(string env, string fileName) : base(env ?? "prod", fileName)
		{
		}

		public AndroidStateManager() : this(null, null)
		{
		}
	}
}
