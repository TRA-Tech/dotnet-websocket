using Newtonsoft.Json;
using TraTech.WebSocketHub.Abstractions;

namespace TraTech.WebSocketHub.Core
{
    /// <summary>
    /// Represents the options for configuring the WebSocketHub.
    /// </summary>
    /// <remarks>
    /// This class represents the options for configuring the WebSocketHub. It contains properties for configuring the WebSocket request handler provider, the JSON serializer settings, and the receive buffer size.
    /// </remarks>
    public class WebSocketHubOptions
    {
        /// <summary>
        /// Gets the WebSocket request handler provider.
        /// </summary>
        /// <remarks>
        /// This property gets the WebSocket request handler provider. It is used to provide WebSocket request handlers for handling WebSocket requests.
        /// </remarks>
        public IWebSocketRequestHandlerProvider WebSocketRequestHandler { get; private set; }

        /// <summary>
        /// Gets the JSON serializer settings.
        /// </summary>
        /// <remarks>
        /// This property gets the JSON serializer settings. It is used to configure the JSON serializer used to serialize WebSocket messages.
        /// </remarks>
        public JsonSerializerSettings? JsonSerializerSettings { get; private set; }

        /// <summary>
        /// Gets the receive buffer size.
        /// </summary>
        /// <remarks>
        /// This property gets the receive buffer size. It is used to configure the size of the buffer used to receive WebSocket messages.
        /// </remarks>
        public int ReceiveBufferSize { get; private set; } = 4 * 1024;

        /// <summary>
        /// Initializes a new instance of the WebSocketHubOptions class.
        /// </summary>
        /// <remarks>
        /// This constructor initializes a new instance of the WebSocketHubOptions class with a default WebSocket request handler provider.
        /// </remarks>
        public WebSocketHubOptions()
        {
            WebSocketRequestHandler = new WebSocketRequestHandlerProvider();
        }
        /// <summary>
        /// Sets the WebSocket request handler provider.
        /// </summary>
        /// <param name="webSocketRequestHandlerProvider">The WebSocket request handler provider to set.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="webSocketRequestHandlerProvider"/> is null.</exception>
        /// <remarks>
        /// This method sets the WebSocket request handler provider. It is used to provide WebSocket request handlers for handling WebSocket requests.
        /// </remarks>
        public void UseWebSocketRequestHandlerProvider(IWebSocketRequestHandlerProvider webSocketRequestHandlerProvider)
        {
            WebSocketRequestHandler = webSocketRequestHandlerProvider ?? throw new ArgumentNullException(nameof(webSocketRequestHandlerProvider));
        }

        /// <summary>
        /// Sets the JSON serializer settings.
        /// </summary>
        /// <param name="jsonSerializerSettings">The JSON serializer settings to set.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="jsonSerializerSettings"/> is null.</exception>
        /// <remarks>
        /// This method sets the JSON serializer settings. It is used to configure the JSON serializer used to serialize WebSocket messages.
        /// </remarks>
        public void UseJsonSerializerSettings(JsonSerializerSettings jsonSerializerSettings)
        {
            JsonSerializerSettings = jsonSerializerSettings ?? throw new ArgumentNullException(nameof(jsonSerializerSettings));
        }

        /// <summary>
        /// Sets the receive buffer size.
        /// </summary>
        /// <param name="receiveBufferSize">The receive buffer size to set.</param>
        /// <remarks>
        /// This method sets the receive buffer size. It is used to configure the size of the buffer used to receive WebSocket messages.
        /// </remarks>
        public void UseReceiveBufferSize(int receiveBufferSize)
        {
            ReceiveBufferSize = receiveBufferSize;
        }
    }
}
