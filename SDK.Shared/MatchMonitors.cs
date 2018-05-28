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
			foreach (var monitor in _matchMonitors)
			{
				await monitor.Start();
			}

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

	public class PollingMatchMonitor : IMatchMonitor
	{
		private Matchmore _client;
		private Device _deviceToSubscribe;
		private CancellationTokenSource _cancelationTokenSource;

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

	public class WebsocketMatchMonitor : IMatchMonitor
	{
		private Matchmore _client;
		private Device _deviceToSubscribe;
		private readonly string _worldId;
		private ClientWebSocket _ws;
		private CancellationTokenSource _cancelationTokenSource;

		public WebsocketMatchMonitor(Matchmore client, Device deviceToSubscribe, string worldId)
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

			var uri = _client.ApiUrl
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

		async Task ReadMessage()
		{
			WebSocketReceiveResult result;
			var message = new ArraySegment<byte>(new byte[4096]);
			do
			{
				result = await _ws.ReceiveAsync(message, _cancelationTokenSource.Token);
				if (result.MessageType != WebSocketMessageType.Text)
					break;
				var messageBytes = message.Skip(message.Offset).Take(result.Count).ToArray();
				var messageStr = Encoding.UTF8.GetString(messageBytes);
				if (messageStr == "ping")
				{
					await _ws.SendAsync(StringToByte("pong"), WebSocketMessageType.Text, true, _cancelationTokenSource.Token).ConfigureAwait(false);
					continue;
				}
				if (messageStr == "pong")
				{
					await _ws.SendAsync(StringToByte("ping"), WebSocketMessageType.Text, true, _cancelationTokenSource.Token).ConfigureAwait(false);
					continue;
				}
				if (!MatchId.TryParse(messageStr, out MatchId matchId))
					continue;
				try
				{
					var match = await _client.GetMatchAsync(matchId, _deviceToSubscribe).ConfigureAwait(false);
					MatchReceived?.Invoke(this, new MatchReceivedEventArgs(_deviceToSubscribe, MatchChannel.Websocket, new List<Match> { match }));
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
				}

			}
			while (!result.EndOfMessage);
		}

		ArraySegment<byte> StringToByte(string str)
		{
			var byteMessage = Encoding.UTF8.GetBytes(str);
			return new ArraySegment<byte>(byteMessage);
		}

		bool IsMatchId(string matchId) => Guid.TryParse(matchId, out Guid x);

		public event EventHandler<MatchReceivedEventArgs> MatchReceived;

		public async Task Stop()
		{
			_cancelationTokenSource.Cancel();
			await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).ConfigureAwait(false);
		}
	}
}