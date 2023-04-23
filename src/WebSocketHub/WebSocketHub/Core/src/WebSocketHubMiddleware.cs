using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace TraTech.WebSocketHub
{
    public class WebSocketHubMiddleware<TKey>
            where TKey : notnull
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly RequestDelegate _next;
        private readonly IOptions<WebSocketHubOptions> _options;
        private readonly Func<HttpContext, bool> _acceptIf;
        private readonly WebSocketHub<TKey> _webSocketHub;
        private readonly Func<HttpContext, TKey> _keyGenerator;
        private readonly int _receiveBufferSize;
        private readonly byte[] _receiveBuffer;

        public WebSocketHubMiddleware(IServiceProvider serviceProvider, RequestDelegate next, IOptions<WebSocketHubOptions> options, WebSocketHub<TKey> webSocketHub, Func<HttpContext, bool> acceptIf, Func<HttpContext, TKey> keyGenerator, int receiveBufferSize = 4 * 1024)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = options ?? throw new ArgumentNullException(nameof(options));
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
                        try
                        {
                            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(_receiveBuffer), CancellationToken.None);
                            var request = Encoding.UTF8.GetString(_receiveBuffer, 0, result.Count);

                            if (result.MessageType == WebSocketMessageType.Close)
                            {
                                break;
                            }
                            // TODO: refactor 
                            Message? serializedRequest = _webSocketHub.DeserializeMessage(request);
                            if (serializedRequest == null) { throw new NullReferenceException(nameof(serializedRequest)); }

                            Type? handlerType = await _options.Value.WebSocketRequestHandler.GetHandlerAsync(serializedRequest.Type);
                            if (handlerType == null) { throw new InvalidOperationException(nameof(handlerType)); }

                            var service = _serviceProvider.GetService(handlerType) as IWebSocketRequestHandler;
                            if (service == null) { throw new InvalidOperationException(nameof(service)); }
                            await service.HandleRequestAsync(key.ToString(), serializedRequest.Payload.ToString());

                        }
                        catch (Exception exp)
                        {
                            Console.WriteLine(exp.ToString());
                            continue;
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
