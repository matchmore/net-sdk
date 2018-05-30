using System;
using System.Threading.Tasks;
namespace Matchmore.SDK.Monitors
{
	public interface IMatchProvider
	{
		Task ProvideMatchIdAsync(MatchId matchId);
	}

	public interface IMatchProviderMonitor: IMatchMonitor, IMatchProvider{}

	public class AuxiliaryMatchMonitor : IMatchProviderMonitor
	{
		readonly Matchmore _client;
		readonly Device _deviceToSubscribe;
		public event EventHandler<MatchReceivedEventArgs> MatchReceived;

		public AuxiliaryMatchMonitor(Matchmore client, Device deviceToSubscribe)
		{
			_client = client;
			_deviceToSubscribe = deviceToSubscribe;
		}

		public async Task ProvideMatchIdAsync(MatchId matchId)
		{
			var matches = await _client.GetMatchesAsync(_deviceToSubscribe.Id);
			MatchReceived?.Invoke(this, new MatchReceivedEventArgs(_deviceToSubscribe, MatchChannel.ThirdParty, matches));
		}

		public Task Start()
		{
			return Task.CompletedTask;
		}

		public Task Stop()
		{
			return Task.CompletedTask;
		}
	}
}
