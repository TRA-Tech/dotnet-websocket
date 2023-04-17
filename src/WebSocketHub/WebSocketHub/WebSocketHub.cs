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
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        private static readonly Func<WebSocket, bool> _openSocketSelector = socket => socket.State == WebSocketState.Open;

        public WebSocketHub()
        {
            _webSocketDictionary = new();
            _jsonSerializerSettings = new JsonSerializerSettings();
        }

        public WebSocketHub(JsonSerializerSettings jsonSerializerSettings)
        {
            _webSocketDictionary = new();
            _jsonSerializerSettings = jsonSerializerSettings;
        }

        private byte[] EncodeMessage(Message message)
        {
            return Encoding.UTF8.GetBytes(
                JsonConvert.SerializeObject(message, _jsonSerializerSettings)
            );
        }

        private static async Task CloseWebSocket(WebSocket webSocket)
        {
            if (webSocket.State != WebSocketState.Closed && webSocket.State != WebSocketState.Aborted)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection End", CancellationToken.None);
            }
        }

        public ReadOnlyCollection<WebSocket> GetSocketList(TKey key)
        {
            if (_webSocketDictionary.Keys.Contains(key))
            {
                return _webSocketDictionary[key].AsReadOnly();
            }
            throw new KeyNotFoundException(key.ToString());
        }

        public void Add(TKey key, WebSocket webSocket)
        {
            if (webSocket == null) return;

            lock (_webSocketDictionary)
            {
                if (!_webSocketDictionary.Keys.Contains(key)) _webSocketDictionary[key] = new List<WebSocket>();
                _webSocketDictionary[key].Add(webSocket);
            }
        }

        public async Task Remove(TKey key, WebSocket webSocket)
        {
            lock (_webSocketDictionary)
            {
                if (_webSocketDictionary.ContainsKey(key)) _webSocketDictionary[key].Remove(webSocket);
                if (_webSocketDictionary[key].Count == 0) _webSocketDictionary.Remove(key);
            }

            await WebSocketHub<TKey>.CloseWebSocket(webSocket);
        }

        public async Task RemoveFirst(Func<TKey, bool> selector)
        {
            IEnumerable<WebSocket> sockets;

            lock (_webSocketDictionary)
            {
                var key = _webSocketDictionary.Keys.FirstOrDefault(selector);
                if (key == null) return;
                sockets = _webSocketDictionary[key];
                _webSocketDictionary.Remove(key);
            }

            foreach (var socket in sockets)
            {
                await WebSocketHub<TKey>.CloseWebSocket(socket);
            }
        }

        public async Task RemoveWhere(Func<TKey, bool> selector)
        {
            List<WebSocket> sockets = new List<WebSocket>();

            lock (_webSocketDictionary)
            {
                IEnumerable<TKey>? keys = _webSocketDictionary.Keys.Where(selector);
                if (keys == null || keys?.Count() == 0) return;

                foreach (var key in keys)
                {
                    sockets.AddRange(_webSocketDictionary[key]);
                    _webSocketDictionary.Remove(key);
                }
            }

            foreach (var socket in sockets)
            {
                await WebSocketHub<TKey>.CloseWebSocket(socket);
            }
        }

        public async Task RemoveAll()
        {
            List<WebSocket> sockets = new List<WebSocket>();
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
                await WebSocketHub<TKey>.CloseWebSocket(socket);
            }
        }

        private static async Task Send(byte[] message, WebSocket socket)
        {
            if (socket == null || socket.State != WebSocketState.Open) return;
            if (message == null) return;

            if (socket.State == WebSocketState.Open)
                await socket.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task Send(Message message, params WebSocket[] sockets)
        {
            if (sockets == null || sockets.Length == 0) return;
            if (message == null) return;

            byte[] byteMessage = EncodeMessage(message);

            foreach (var socket in sockets.Where(_openSocketSelector))
            {
                await WebSocketHub<TKey>.Send(byteMessage, socket);
            }
        }

        public async Task Send(Message message, TKey key)
        {
            if (key == null) return;
            if (message == null) return;
            if (!_webSocketDictionary.ContainsKey(key)) return;

            IEnumerable<WebSocket> openSocketList = _webSocketDictionary[key].Where(_openSocketSelector);

            byte[] byteMessage = EncodeMessage(message);

            foreach (var websocket in openSocketList)
            {
                await Send(byteMessage, websocket);
            }
        }

        public async Task SendWhere(Message message, Func<TKey, bool> selector)
        {
            if (message == null) return;

            IEnumerable<TKey>? keys = _webSocketDictionary.Keys.Where(selector);
            if (keys == null || keys?.Count() == 0) return;

            byte[] byteMessage = EncodeMessage(message);

            foreach (var key in keys)
            {
                foreach (var socket in _webSocketDictionary[key].Where(_openSocketSelector))
                {
                    await Send(byteMessage, socket);
                }
            }
        }

        public async Task SendAll(Message message)
        {
            if (message == null) return;

            byte[] byteMessage = EncodeMessage(message);

            foreach (KeyValuePair<TKey, List<WebSocket>> keyValue in _webSocketDictionary)
            {
                foreach (var socket in keyValue.Value.Where(_openSocketSelector))
                {
                    await Send(byteMessage, socket);
                }
            }
        }
    }
}
