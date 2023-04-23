namespace TraTech.WebSocketHub
{
    public interface IWebSocketRequestHandler
    {
        public Task HandleRequestAsync(string key, string data);
    }
}
