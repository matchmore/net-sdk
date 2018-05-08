using System;
using System.Collections;
using System.Collections.Generic;

namespace Matchmore.SDK
{
    public interface IMatchMonitor
    {
        event EventHandler<MatchReceivedEventArgs> MatchReceived;

        string Id
        {
            get;
        }

        void Stop();
    }
}