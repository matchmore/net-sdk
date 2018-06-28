using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Matchmore.SDK.Monitors
{
    public class MultiChannelMatchMonitor : IMatchMonitor
    {
        private readonly IMatchMonitor[] _matchMonitors;
        private readonly List<EventHandler<MatchReceivedEventArgs>> _eventHandlers = new List<EventHandler<MatchReceivedEventArgs>>();

        public MultiChannelMatchMonitor(params IMatchMonitor[] matchMonitors)
        {
            this._matchMonitors = matchMonitors;
        }

        public event EventHandler<MatchReceivedEventArgs> MatchReceived
        {
            add
            {

                if (_matchMonitors == null)
                    return;
                foreach (var monitor in _matchMonitors)
                {
                    _eventHandlers.Add(value);
                    monitor.MatchReceived += value;
                }
            }
            remove
            {

                if (_matchMonitors == null)
                    return;
                foreach (var monitor in _matchMonitors)
                {
                    _eventHandlers.Remove(value);
                    monitor.MatchReceived -= value;
                }
            }
        }

        public async Task Start()
        {
            if (_matchMonitors == null)
                return;

            var multiException = new MatchmoreMultiException();

            foreach (var monitor in _matchMonitors)
            {
                try
                {
                    await monitor.Start();
                }
                catch (Exception e)
                {
                    multiException.Exceptions.Add(e);
                }
            }

            if (multiException.Exceptions.Count > 0)
                throw multiException;

            return;
        }

        public async Task Stop()
        {
            if (_matchMonitors == null)
                return;
            foreach (var monitor in _matchMonitors)
            {
                await monitor.Stop();
            }
        }
    }
}