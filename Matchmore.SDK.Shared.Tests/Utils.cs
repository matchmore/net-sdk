using System;
using System.Threading.Tasks;

namespace Matchmore.Tests
{
    public static class Utils
    {
		public static T RunSync<T>(Func<Task<T>> task)
        {
            var r = Task.Run(async () => await task());
            Task.WaitAll(r);
            return r.Result;
        }

		public static void RunSync(Func<Task> task)
        {
            var r = Task.Run(async () => await task());
            Task.WaitAll(r);
        }
    }
}
