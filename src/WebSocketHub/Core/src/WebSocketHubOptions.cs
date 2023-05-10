namespace TraTech.WebSocketHub
{
    public class WebSocketHubOptions
    {
        public IWebSocketRequestHandlerProvider WebSocketRequestHandler { get; private set; }

        public WebSocketHubOptions()
        {
            WebSocketRequestHandler = new WebSocketRequestHandlerProvider();
        }

        public void UseWebSocketRequestHandlerProvider(IWebSocketRequestHandlerProvider webSocketRequestHandlerProvider)
        {
            WebSocketRequestHandler = webSocketRequestHandlerProvider ?? throw new ArgumentNullException(nameof(webSocketRequestHandlerProvider));
        }
    }
}
