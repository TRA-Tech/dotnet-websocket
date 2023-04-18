using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace TraTech.WebSocketHub
{
    public class WebSocketHubMiddleware<TKey>
            where TKey : notnull
    {
        private readonly RequestDelegate _next;
        Func<HttpContext, bool> _acceptIf;
        private readonly WebSocketHub<TKey> _webSocketHub;
        private readonly Func<HttpContext, TKey> _keyGenerator;
        private readonly int _receiveBufferSize;
        private readonly byte[] _receiveBuffer;

        public WebSocketHubMiddleware(RequestDelegate next, WebSocketHub<TKey> webSocketHub, Func<HttpContext, bool> acceptIf, Func<HttpContext, TKey> keyGenerator, int receiveBufferSize = 4 * 1024)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _acceptIf = acceptIf ?? throw new ArgumentNullException(nameof(acceptIf));
            _webSocketHub = webSocketHub ?? throw new ArgumentNullException(nameof(webSocketHub));
            _keyGenerator = keyGenerator ?? throw new ArgumentNullException(nameof(keyGenerator));
            _receiveBufferSize = receiveBufferSize;
            _receiveBuffer = new byte[_receiveBufferSize];
        }

        public async Task Invoke(HttpContext httpContext)
        {

            if (httpContext.WebSockets.IsWebSocketRequest && _acceptIf(httpContext))
            {
                try
                {
                    WebSocket webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();

                    var key = _keyGenerator(httpContext);
                    _webSocketHub.Add(key, webSocket);

                    while (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseSent)
                    {
                        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(_receiveBuffer), CancellationToken.None);
                        var request = Encoding.UTF8.GetString(_receiveBuffer, 0, result.Count);

                        //TODO: Add request handle mechanism

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            break;
                        }
                    }

                    await _webSocketHub.Remove(key, webSocket);
                }
                catch (Exception exp)
                {
                    Console.WriteLine(exp.ToString());
                }
            }
            else
            {
                await _next(httpContext);
            }
        }
    }
}
