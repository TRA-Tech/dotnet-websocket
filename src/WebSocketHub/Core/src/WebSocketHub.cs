using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Net.WebSockets;
using System.Text;

namespace TraTech.WebSocketHub
{
    /// <summary>
    /// Represents a hub for managing WebSocket connections associated with keys of type TKey.
    /// </summary>
    /// <typeparam name="TKey">The type of key associated with WebSocket connections.</typeparam>
    /// <remarks>
    /// This class provides methods for managing WebSocket connections associated with keys of type TKey, such as adding, removing, and retrieving WebSocket connections.
    /// </remarks>
    public class WebSocketHub<TKey>
        where TKey : notnull
    {
        /// <summary>
        /// A dictionary that maps keys to a list of WebSocket connections.
        /// </summary>
        /// <remarks>
        /// This dictionary is used to maintain a mapping between keys and the WebSocket connections associated with them.
        /// </remarks>
        private readonly Dictionary<TKey, List<WebSocket>> _webSocketDictionary;

        /// <summary>
        /// A function that selects WebSocket connections that are open.
        /// </summary>
        /// <remarks>
        /// This function returns true if the specified WebSocket connection is open, and false otherwise.
        /// </remarks>
        private static readonly Func<WebSocket, bool> _openSocketSelector = socket => socket.State == WebSocketState.Open;


        public WebSocketHubOptions Options { get; private set; }

        /// <summary>
        /// Initializes a new instance of the WebSocketHub class with the specified options.
        /// </summary>
        /// <param name="options">The options to configure the WebSocketHub.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
        /// <remarks>
        /// This constructor initializes a new instance of the WebSocketHub class with the specified options. It also initializes an empty WebSocket dictionary.
        /// </remarks>
        public WebSocketHub(IOptions<WebSocketHubOptions> options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            Options = options.Value;

            _webSocketDictionary = new();
        }

        /// <summary>
        /// Encodes the specified Message object into a UTF-8 encoded byte array.
        /// </summary>
        /// <param name="message">The Message object to encode.</param>
        /// <returns>A UTF-8 encoded byte array representing the specified Message object.</returns>
        /// <remarks>
        /// This method encodes the specified Message object into a UTF-8 encoded byte array using the JSON serializer settings specified in the WebSocketHubOptions.
        /// </remarks>
        private byte[] EncodeMessage(Message message)
        {
            return Encoding.UTF8.GetBytes(
                JsonConvert.SerializeObject(message, Options.JsonSerializerSettings)
            );
        }

        /// <summary>
        /// Sends the specified message to the specified WebSocket connection if it is open.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="socket">The WebSocket connection to send the message to.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <remarks>
        /// This method sends the specified message to the specified WebSocket connection if it is open. If the WebSocket connection is not open or the message is null, the method returns without sending the message.
        /// </remarks>
        private static async Task SendAsync(byte[] message, WebSocket socket)
        {
            if (socket == null || socket.State != WebSocketState.Open) return;
            if (message == null) return;

            if (socket.State == WebSocketState.Open)
                await socket.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        /// <summary>
        /// Closes the specified WebSocket connection if it is not already closed or aborted.
        /// </summary>
        /// <param name="webSocket">The WebSocket connection to close.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <remarks>
        /// This method closes the specified WebSocket connection with the WebSocket close status of "NormalClosure" and the reason string of "Connection End" if the WebSocket connection is not already closed or aborted.
        /// </remarks>
        private static async Task CloseWebSocketAsync(WebSocket webSocket)
        {
            if (webSocket.State != WebSocketState.Closed && webSocket.State != WebSocketState.Aborted)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection End", CancellationToken.None);
            }
        }

        /// <summary>
        /// Deserializes the specified JSON message into a Message object.
        /// </summary>
        /// <param name="message">The JSON message to deserialize.</param>
        /// <returns>A Message object representing the deserialized JSON message.</returns>
        /// <remarks>
        /// This method deserializes the specified JSON message into a Message object using the JSON serializer settings specified in the WebSocketHubOptions.
        /// </remarks>
        public Message? DeserializeMessage(string message)
        {
            return JsonConvert.DeserializeObject<Message>(message, Options.JsonSerializerSettings);
        }

        /// <summary>
        /// Gets the list of WebSocket instances associated with the specified key.
        /// </summary>
        /// <param name="key">The key to look up the WebSocket list.</param>
        /// <returns>A read-only collection of WebSocket instances.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the key is not found in the WebSocket dictionary.</exception>
        public ReadOnlyCollection<WebSocket> GetSocketList(TKey key)
        {
            lock (_webSocketDictionary)
            {
                if (!_webSocketDictionary.ContainsKey(key)) throw new KeyNotFoundException(nameof(key));

                return _webSocketDictionary[key].AsReadOnly();
            }
        }

        /// <summary>
        /// Returns a read-only collection of WebSocket connections associated with the key selected by the specified function from the WebSocket dictionary.
        /// </summary>
        /// <param name="selector">The function used to select the key associated with the WebSocket connections to retrieve.</param>
        /// <exception cref="KeyNotFoundException">Thrown when the selected key cannot be found in the WebSocket dictionary.</exception>
        /// <returns>A read-only collection of WebSocket connections associated with the selected key.</returns>
        /// <remarks>
        /// This method returns a read-only collection of WebSocket connections associated with the key that fulfills the criteria specified by the <paramref name="selector"/> function from the WebSocket dictionary.
        /// </remarks>
        public ReadOnlyCollection<WebSocket> GetSocketList(Func<TKey, bool> selector)
        {
            lock (_webSocketDictionary)
            {
                TKey key = _webSocketDictionary.Keys.FirstOrDefault(selector) ?? throw new KeyNotFoundException(nameof(selector));

                return _webSocketDictionary[key].AsReadOnly();
            }
        }

        /// <summary>
        /// Adds the specified WebSocket connection to the WebSocket dictionary associated with the specified key.
        /// </summary>
        /// <param name="key">The key associated with the WebSocket connection.</param>
        /// <param name="webSocket">The WebSocket connection to add.</param>
        public void Add(TKey key, WebSocket webSocket)
        {
            if (webSocket == null) return;

            lock (_webSocketDictionary)
            {
                if (!_webSocketDictionary.ContainsKey(key)) _webSocketDictionary[key] = new List<WebSocket>();
                _webSocketDictionary[key].Add(webSocket);
            }
        }

        /// <summary>
        /// Removes the specified WebSocket connection associated with the specified key from the WebSocket dictionary and closes it.
        /// </summary>
        /// <param name="key">The key associated with the WebSocket connection to remove.</param>
        /// <param name="webSocket">The WebSocket connection to remove.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="webSocket"/> is null.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the specified key cannot be found in the WebSocket dictionary.</exception>
        public async Task RemoveAsync(TKey key, WebSocket webSocket)
        {
            if (webSocket == null) throw new ArgumentNullException(nameof(webSocket));

            lock (_webSocketDictionary)
            {
                if (!_webSocketDictionary.ContainsKey(key)) throw new KeyNotFoundException(nameof(key));

                _webSocketDictionary[key].Remove(webSocket);

                if (_webSocketDictionary[key].Count == 0) _webSocketDictionary.Remove(key);
            }

            await WebSocketHub<TKey>.CloseWebSocketAsync(webSocket);
        }

        /// <summary>
        /// Removes the specified WebSocket connection associated with the key selected by the specified function from the WebSocket dictionary and closes it.
        /// </summary>
        /// <param name="selector">The function used to select the key associated with the WebSocket connection to remove.</param>
        /// <param name="webSocket">The WebSocket connection to remove.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="webSocket"/> is null.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the selected key cannot be found in the WebSocket dictionary.</exception>
        /// <remarks>
        /// This method removes the specified WebSocket connection associated with the first key that fulfills the criteria specified by the <paramref name="selector"/> function from the WebSocket dictionary and closes it.
        /// </remarks>
        public async Task RemoveAsync(Func<TKey, bool> selector, WebSocket webSocket)
        {
            if (webSocket == null) throw new ArgumentNullException(nameof(webSocket));

            lock (_webSocketDictionary)
            {
                TKey key = _webSocketDictionary.Keys.FirstOrDefault(selector) ?? throw new KeyNotFoundException(nameof(key));

                if (_webSocketDictionary.ContainsKey(key)) _webSocketDictionary[key].Remove(webSocket);
                if (_webSocketDictionary[key].Count == 0) _webSocketDictionary.Remove(key);
            }

            await WebSocketHub<TKey>.CloseWebSocketAsync(webSocket);
        }

        /// <summary>
        /// Removes the WebSocket connection associated with the first key selected by the specified function from the WebSocket dictionary and closes it.
        /// </summary>
        /// <param name="selector">The function used to select the key associated with the WebSocket connection to remove.</param>
        /// <exception cref="KeyNotFoundException">Thrown when the selected key cannot be found in the WebSocket dictionary.</exception>
        /// <remarks>
        /// This method removes the WebSocket connection associated with the first key that fulfills the criteria specified by the <paramref name="selector"/> function from the WebSocket dictionary and closes it.
        /// </remarks>
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

        /// <summary>
        /// Removes the WebSocket connections associated with the keys selected by the specified function from the WebSocket dictionary and closes them.
        /// </summary>
        /// <param name="selector">The function used to select the keys associated with the WebSocket connections to remove.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is null.</exception>
        /// <exception cref="NullReferenceException">Thrown when the selected keys are null.</exception>
        /// <remarks>
        /// This method removes the WebSocket connections associated with the keys that fulfill the criteria specified by the <paramref name="selector"/> function from the WebSocket dictionary and closes them.
        /// </remarks>
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

        /// <summary>
        /// Removes all WebSocket connections from the WebSocket dictionary and closes them.
        /// </summary>
        /// <returns>A task representing the asynchronous remove operation.</returns>
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

        /// <summary>
        /// Sends the specified message to the specified WebSocket connections.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <param name="sockets">The WebSocket connections to send the message to.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sockets"/> is null or empty, or when <paramref name="message"/> is null.</exception>
        /// <remarks>
        /// This method sends the specified <paramref name="message"/> to the specified <paramref name="sockets"/> that are currently open.
        /// </remarks>
        public async Task SendAsync(Message message, params WebSocket[] sockets)
        {
            if (sockets == null || sockets.Length == 0 || message == null)
                return;

            byte[] byteMessage = EncodeMessage(message);

            foreach (var socket in sockets.Where(_openSocketSelector))
            {
                await WebSocketHub<TKey>.SendAsync(byteMessage, socket);
            }
        }

        /// <summary>
        /// Sends the specified message to the WebSocket connections associated with the specified key.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <param name="key">The key associated with the WebSocket connections to send the message to.</param>
        /// <typeparam name="TKey">The type of the key associated with the WebSocket connections.</typeparam>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> or <paramref name="message"/> is null.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the specified key cannot be found in the WebSocket dictionary.</exception>
        /// <remarks>
        /// This method sends the specified <paramref name="message"/> to all WebSocket connections associated with the specified <paramref name="key"/> that are currently open.
        /// </remarks>
        public async Task SendAsync(Message message, TKey key)
        {
            if (key == null || message == null)
                return;

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

        /// <summary>
        /// Sends the specified message to the WebSocket connections associated with the selected key.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <param name="selector">The function used to select the key associated with the WebSocket connections to send the message to.</param>
        /// <typeparam name="TKey">The type of the key associated with the WebSocket connections.</typeparam>
        /// <param name="_openSocketSelector">The function used to determine if a WebSocket connection is open.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> or <paramref name="selector"/> is null.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the selected key cannot be found in the WebSocket dictionary.</exception>
        /// <remarks>
        /// This method sends the specified <paramref name="message"/> to all WebSocket connections associated with the first key that fulfills the criteria specified by the <paramref name="selector"/> function.
        /// </remarks>
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

        /// <summary>
        /// Sends the specified message to the WebSocket connections selected by the specified function.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <param name="selector">The function used to select the keys associated with the WebSocket connections to send the message to.</param>
        /// <typeparam name="TKey">The type of the key associated with the WebSocket connections.</typeparam>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> or <paramref name="selector"/> is null.</exception>
        /// <exception cref="NullReferenceException">Thrown when the selected keys are null.</exception>
        /// <remarks>
        /// This method sends the specified <paramref name="message"/> to all WebSocket connections associated with the keys that fulfill the criteria specified by the <paramref name="selector"/> function.
        /// </remarks>
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

        /// <summary>
        /// Sends the specified message to all WebSocket connections in the WebSocket dictionary.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
        /// <remarks>
        /// This method sends the specified <paramref name="message"/> to all WebSocket connections in the WebSocket dictionary that are currently open.
        /// </remarks>
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
