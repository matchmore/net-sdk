using System;
using System.Collections.Generic;
using Matchmore.SDK.Persistence;
using Xamarin.Forms;

[assembly: Dependency(typeof(AndroidStateManager))]
namespace Matchmore.SDK.Persistence
{
	public class AndroidStateManager : SimpleJsonStateManager, IStateManager
	{
		public AndroidStateManager() : base("dev")
		{
		}
	}
}
