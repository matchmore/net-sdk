using System;
using System.Threading.Tasks;
using System.Threading;

namespace Matchmore.SDK.Monitors
{
    public class PollingMatchMonitor : IMatchMonitor
    {
		readonly Matchmore _client;
		readonly Device _deviceToSubscribe;
        CancellationTokenSource _cancelationTokenSource;

        public PollingMatchMonitor(Matchmore client, Device deviceToSubscribe)
        {
            _client = client;
            _deviceToSubscribe = deviceToSubscribe;
        }

        public Task Start()
        {
            _cancelationTokenSource = new CancellationTokenSource();
            RecurrentCancellableTask.StartNew(async () =>
        {
            var matches = await _client.GetMatchesAsync(_deviceToSubscribe.Id);
            MatchReceived?.Invoke(this, new MatchReceivedEventArgs(_deviceToSubscribe, MatchChannel.Polling, matches));

        },
                                              pollInterval: TimeSpan.FromSeconds(10),
                                              token: _cancelationTokenSource.Token,
                                              taskCreationOptions: TaskCreationOptions.LongRunning);
            return Task.CompletedTask;
        }

        public event EventHandler<MatchReceivedEventArgs> MatchReceived;

        public Task Stop()
        {
            _cancelationTokenSource.Cancel();
            return Task.CompletedTask;
        }
    }
}