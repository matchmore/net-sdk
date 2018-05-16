using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Net.WebSockets;
using System.Text;
using System.Linq;

namespace Matchmore.SDK
{
	public interface IMatchMonitor
	{
		event EventHandler<MatchReceivedEventArgs> MatchReceived;

		Task Start();

		Task Stop();
	}

	public class PollingMatchMonitor : IMatchMonitor
	{
		private ApiClient _client;
		private Device _deviceToSubscribe;
		private CancellationTokenSource _cancelationTokenSource;

		public PollingMatchMonitor(ApiClient client, Device deviceToSubscribe)
		{
			_client = client;
			_deviceToSubscribe = deviceToSubscribe;

		}

		public Task Start()
		{
			_cancelationTokenSource = new CancellationTokenSource();
			RecurrentCancellableTask.StartNew(async () =>
		{
			var matches = await _client.GetMatchesAsync(_deviceToSubscribe.Id, _cancelationTokenSource.Token);
			MatchReceived?.Invoke(this, new MatchReceivedEventArgs(_deviceToSubscribe, MatchChannel.polling, matches));

		}, TimeSpan.FromSeconds(10), _cancelationTokenSource.Token, TaskCreationOptions.LongRunning);
			return Task.FromResult<object>(null);
		}

		public event EventHandler<MatchReceivedEventArgs> MatchReceived;

		public Task Stop()
		{
			_cancelationTokenSource.Cancel();
			return Task.FromResult<object>(null);
		}
	}

	public class WebsocketMatchMonitor : IMatchMonitor
	{
		private ApiClient _client;
		private Device _deviceToSubscribe;
		private readonly string _worldId;
		private ClientWebSocket _ws;
		private CancellationTokenSource _cancelationTokenSource;

		public WebsocketMatchMonitor(ApiClient client, Device deviceToSubscribe, string worldId)
		{
			_client = client;
			_deviceToSubscribe = deviceToSubscribe;
			_worldId = worldId;

		}

		public async Task Start()
		{
			_cancelationTokenSource = new CancellationTokenSource();
			_ws = new ClientWebSocket();
			_ws.Options.AddSubProtocol("api-key");
			_ws.Options.AddSubProtocol(_worldId);

			var uri = _client.BaseUrl
				  .Replace("http://", "ws://")
				  .Replace("https://", "wss://")
				  .Replace("/v5", "/pusher/v5/ws/" + _deviceToSubscribe.Id);

			await _ws.ConnectAsync(new Uri(uri), _cancelationTokenSource.Token);
			await Task.Factory.StartNew(async () =>
			{
				while (true)
				{
					await ReadMessage();
				}
			}, _cancelationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
		}

		private async Task ReadMessage()
		{
			WebSocketReceiveResult result;
			var message = new ArraySegment<byte>(new byte[4096]);
			do
			{
				result = await _ws.ReceiveAsync(message, _cancelationTokenSource.Token);
				if (result.MessageType != WebSocketMessageType.Text)
					break;
				var messageBytes = message.Skip(message.Offset).Take(result.Count).ToArray();
				var matchId = Encoding.UTF8.GetString(messageBytes);
				if (!IsMatchId(matchId))
					continue;
				try
				{
					var match = await _client.GetMatchAsync(_deviceToSubscribe.Id, matchId).ConfigureAwait(false);
					MatchReceived?.Invoke(this, new MatchReceivedEventArgs(_deviceToSubscribe, MatchChannel.websocket, new List<Match> { match }));
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
				}

			}
			while (!result.EndOfMessage);
		}

		private bool IsMatchId(string matchId)
		{
			Guid x;
			return Guid.TryParse(matchId, out x);
		}

		public event EventHandler<MatchReceivedEventArgs> MatchReceived;

		public async Task Stop()
		{
			_cancelationTokenSource.Cancel();
			await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).ConfigureAwait(false);
		}
	}
}