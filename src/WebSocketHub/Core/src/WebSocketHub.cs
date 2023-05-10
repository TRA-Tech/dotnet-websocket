using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Net.WebSockets;
using System.Text;

namespace TraTech.WebSocketHub
{
    public class WebSocketHub<TKey>
        where TKey : notnull
    {
        private readonly Dictionary<TKey, List<WebSocket>> _webSocketDictionary;

        private static readonly Func<WebSocket, bool> _openSocketSelector = socket => socket.State == WebSocketState.Open;

        public WebSocketHubOptions Options { get; private set; }

        public WebSocketHub(IOptions<WebSocketHubOptions> options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            Options = options.Value;

            _webSocketDictionary = new();
        }

        private byte[] EncodeMessage(Message message)
        {
            return Encoding.UTF8.GetBytes(
                JsonConvert.SerializeObject(message, Options.JsonSerializerSettings)
            );
        }

        private static async Task SendAsync(byte[] message, WebSocket socket)
        {
            if (socket == null || socket.State != WebSocketState.Open) return;
            if (message == null) return;

            if (socket.State == WebSocketState.Open)
                await socket.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private static async Task CloseWebSocketAsync(WebSocket webSocket)
        {
            if (webSocket.State != WebSocketState.Closed && webSocket.State != WebSocketState.Aborted)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection End", CancellationToken.None);
            }
        }

        public Message? DeserializeMessage(string message)
        {
            return JsonConvert.DeserializeObject<Message>(message, Options.JsonSerializerSettings);
        }

        public ReadOnlyCollection<WebSocket> GetSocketList(TKey key)
        {
            lock (_webSocketDictionary)
            {
                if (!_webSocketDictionary.ContainsKey(key)) throw new KeyNotFoundException(nameof(key));

                return _webSocketDictionary[key].AsReadOnly();
            }
        }

        public ReadOnlyCollection<WebSocket> GetSocketList(Func<TKey, bool> selector)
        {
            lock (_webSocketDictionary)
            {
                TKey key = _webSocketDictionary.Keys.FirstOrDefault(selector) ?? throw new KeyNotFoundException(nameof(selector));

                return _webSocketDictionary[key].AsReadOnly();
            }
        }

        public void Add(TKey key, WebSocket webSocket)
        {
            if (webSocket == null) return;

            lock (_webSocketDictionary)
            {
                if (!_webSocketDictionary.ContainsKey(key)) _webSocketDictionary[key] = new List<WebSocket>();
                _webSocketDictionary[key].Add(webSocket);
            }
        }

        public async Task RemoveAsync(TKey key, WebSocket webSocket)
        {
            lock (_webSocketDictionary)
            {
                if (!_webSocketDictionary.ContainsKey(key)) throw new KeyNotFoundException(nameof(key));

                _webSocketDictionary[key].Remove(webSocket);

                if (_webSocketDictionary[key].Count == 0) _webSocketDictionary.Remove(key);
            }

            await WebSocketHub<TKey>.CloseWebSocketAsync(webSocket);
        }

        public async Task RemoveAsync(Func<TKey, bool> selector, WebSocket webSocket)
        {
            lock (_webSocketDictionary)
            {
                TKey key = _webSocketDictionary.Keys.FirstOrDefault(selector) ?? throw new KeyNotFoundException(nameof(key));

                if (_webSocketDictionary.ContainsKey(key)) _webSocketDictionary[key].Remove(webSocket);
                if (_webSocketDictionary[key].Count == 0) _webSocketDictionary.Remove(key);
            }

            await WebSocketHub<TKey>.CloseWebSocketAsync(webSocket);
        }

        public async Task RemoveFirstAsync(Func<TKey, bool> selector)
        {
            IEnumerable<WebSocket> sockets = Enumerable.Empty<WebSocket>();

            lock (_webSocketDictionary)
            {
                TKey key = _webSocketDictionary.Keys.FirstOrDefault(selector) ?? throw new KeyNotFoundException(nameof(key));

                sockets = _webSocketDictionary[key];
                _webSocketDictionary.Remove(key);
            }

            foreach (var socket in sockets)
            {
                await WebSocketHub<TKey>.CloseWebSocketAsync(socket);
            }
        }

        public async Task RemoveWhereAsync(Func<TKey, bool> selector)
        {
            List<WebSocket> sockets = new();

            lock (_webSocketDictionary)
            {
                IEnumerable<TKey>? keys = _webSocketDictionary.Keys.Where(selector);

                if (keys == null) throw new NullReferenceException(nameof(keys));

                if (!keys.Any()) return;

                foreach (var key in keys)
                {
                    sockets.AddRange(_webSocketDictionary[key]);
                    _webSocketDictionary.Remove(key);
                }
            }

            foreach (var socket in sockets)
            {
                await WebSocketHub<TKey>.CloseWebSocketAsync(socket);
            }
        }

        public async Task RemoveAllAsync()
        {
            List<WebSocket> sockets = new();

            lock (_webSocketDictionary)
            {
                foreach (var keyValue in _webSocketDictionary)
                {
                    sockets.AddRange(keyValue.Value);
                }

                _webSocketDictionary.Clear();
            }

            foreach (var socket in sockets)
            {
                await WebSocketHub<TKey>.CloseWebSocketAsync(socket);
            }
        }

        public async Task SendAsync(Message message, params WebSocket[] sockets)
        {
            if (sockets == null || sockets.Length == 0) return;
            if (message == null) return;

            byte[] byteMessage = EncodeMessage(message);

            foreach (var socket in sockets.Where(_openSocketSelector))
            {
                await WebSocketHub<TKey>.SendAsync(byteMessage, socket);
            }
        }

        public async Task SendAsync(Message message, TKey key)
        {
            if (key == null) return;
            if (message == null) return;

            IEnumerable<WebSocket> openSocketList = Enumerable.Empty<WebSocket>();

            lock (_webSocketDictionary)
            {
                if (!_webSocketDictionary.ContainsKey(key)) throw new KeyNotFoundException(nameof(key));

                openSocketList = _webSocketDictionary[key].Where(_openSocketSelector);
            }

            byte[] byteMessage = EncodeMessage(message);

            foreach (var websocket in openSocketList)
            {
                await SendAsync(byteMessage, websocket);
            }
        }

        public async Task SendAsync(Message message, Func<TKey, bool> selector)
        {
            if (message == null) return;

            IEnumerable<WebSocket> openSocketList = Enumerable.Empty<WebSocket>();

            lock (_webSocketDictionary)
            {
                TKey key = _webSocketDictionary.Keys.FirstOrDefault(selector) ?? throw new KeyNotFoundException(nameof(selector));
                openSocketList = _webSocketDictionary[key].Where(_openSocketSelector);
            }

            byte[] byteMessage = EncodeMessage(message);

            foreach (var websocket in openSocketList)
            {
                await SendAsync(byteMessage, websocket);
            }
        }

        public async Task SendWhereAsync(Message message, Func<TKey, bool> selector)
        {
            if (message == null) return;

            List<WebSocket> sockets = new();

            lock (_webSocketDictionary)
            {
                IEnumerable<TKey>? keys = _webSocketDictionary.Keys.Where(selector);

                if (keys == null) throw new NullReferenceException(nameof(keys));

                if (!keys.Any()) return;

                foreach (var key in keys)
                {
                    sockets.AddRange(_webSocketDictionary[key].Where(_openSocketSelector));
                }
            }

            byte[] byteMessage = EncodeMessage(message);

            foreach (var socket in sockets)
            {
                await SendAsync(byteMessage, socket);
            }
        }

        public async Task SendAllAsync(Message message)
        {
            if (message == null) return;

            List<WebSocket> sockets = new();

            lock (_webSocketDictionary)
            {
                foreach (var keyValuePair in _webSocketDictionary)
                {
                    sockets.AddRange(keyValuePair.Value.Where(_openSocketSelector));
                }
            }

            byte[] byteMessage = EncodeMessage(message);

            foreach (var socket in sockets)
            {
                await SendAsync(byteMessage, socket);
            }
        }
    }
}
