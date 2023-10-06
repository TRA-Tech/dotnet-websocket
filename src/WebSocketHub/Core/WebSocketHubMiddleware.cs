using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using TraTech.WebSocketHub.Abstractions;

namespace TraTech.WebSocketHub.Core
{
    public class WebSocketHubMiddleware<TKey>
            where TKey : notnull
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly RequestDelegate _next;
        private readonly Func<HttpContext, bool> _acceptIf;
        private readonly WebSocketHub<TKey> _webSocketHub;
        private readonly Func<HttpContext, TKey> _keyGenerator;
        private readonly byte[] _receiveBuffer;

        public WebSocketHubMiddleware(IServiceProvider serviceProvider, RequestDelegate next, WebSocketHub<TKey> webSocketHub, Func<HttpContext, bool> acceptIf, Func<HttpContext, TKey> keyGenerator)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _acceptIf = acceptIf ?? throw new ArgumentNullException(nameof(acceptIf));
            _webSocketHub = webSocketHub ?? throw new ArgumentNullException(nameof(webSocketHub));
            _keyGenerator = keyGenerator ?? throw new ArgumentNullException(nameof(keyGenerator));
            _receiveBuffer = new byte[_webSocketHub.Options.ReceiveBufferSize];
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
                            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(_receiveBuffer), CancellationToken.None);
                            string request = Encoding.UTF8.GetString(_receiveBuffer, 0, result.Count);

                            if (result.MessageType == WebSocketMessageType.Close)
                            {
                                break;
                            }

                            Message? serializedRequest = _webSocketHub.DeserializeMessage(request);
                            if (serializedRequest == null) { throw new NullReferenceException(nameof(serializedRequest)); }

                            Type? handlerType = await _webSocketHub.Options.WebSocketRequestHandler.GetHandlerAsync(serializedRequest.Type);
                            if (handlerType == null) { throw new NullReferenceException(nameof(handlerType)); }

                            if (_serviceProvider.GetService(handlerType) is not IWebSocketRequestHandler service) { throw new NullReferenceException(nameof(service)); }

                            await service.HandleRequestAsync(
                                JsonConvert.SerializeObject(key, _webSocketHub.Options.JsonSerializerSettings),
                                JsonConvert.SerializeObject(serializedRequest.Payload, _webSocketHub.Options.JsonSerializerSettings)
                            );
                        }
                        catch (Exception exp)
                        {
                            Console.WriteLine(exp.ToString());
                            continue;
                        }
                    }

                    await _webSocketHub.RemoveAsync(key, webSocket);
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
