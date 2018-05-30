using System;
using System.Threading.Tasks;

namespace Matchmore.SDK.Monitors
{
    public interface IMatchMonitor
    {
        event EventHandler<MatchReceivedEventArgs> MatchReceived;

        Task Start();

        Task Stop();
    }
}