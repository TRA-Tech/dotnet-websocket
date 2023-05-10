using Newtonsoft.Json;

namespace TraTech.WebSocketHub
{
    public class WebSocketHubOptions
    {
        public IWebSocketRequestHandlerProvider WebSocketRequestHandler { get; private set; }

        public JsonSerializerSettings? JsonSerializerSettings { get; private set; }

        public int ReceiveBufferSize { get; private set; } = 4 * 1024;

        public WebSocketHubOptions()
        {
            WebSocketRequestHandler = new WebSocketRequestHandlerProvider();
        }

        public void UseWebSocketRequestHandlerProvider(IWebSocketRequestHandlerProvider webSocketRequestHandlerProvider)
        {
            WebSocketRequestHandler = webSocketRequestHandlerProvider ?? throw new ArgumentNullException(nameof(webSocketRequestHandlerProvider));
        }

        public void UseJsonSerializerSettings(JsonSerializerSettings jsonSerializerSettings)
        {
            JsonSerializerSettings = jsonSerializerSettings ?? throw new ArgumentNullException(nameof(jsonSerializerSettings));
        }

        public void UseReceiveBufferSize(int receiveBufferSize)
        {
            ReceiveBufferSize = receiveBufferSize;
        }
    }
}
