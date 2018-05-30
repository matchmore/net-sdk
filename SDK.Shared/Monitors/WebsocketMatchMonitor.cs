using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Net.WebSockets;
using System.Text;
using System.Linq;

namespace Matchmore.SDK.Monitors
{

    public class WebsocketMatchMonitor : IMatchMonitor
    {
		readonly Matchmore _client;
		readonly Device _deviceToSubscribe;
        readonly string _worldId;
        ClientWebSocket _ws;
        CancellationTokenSource _cancelationTokenSource;

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