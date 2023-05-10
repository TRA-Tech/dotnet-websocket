using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace TraTech.WebSocketHub
{
    public class WebSocketHubBuilder
    {
        public WebSocketHubBuilder(IServiceCollection services) => Services = services;

        public virtual IServiceCollection Services { get; }

        public WebSocketHubBuilder AddRequestHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler>(string messageType)
            where THandler : class, IWebSocketRequestHandler
        {
            Services.Configure<WebSocketHubOptions>(o =>
            {
                o.WebSocketRequestHandler.TryAddHandler<THandler>(messageType);
            });

            Services.TryAddTransient<THandler>();

            return this;
        }

        public WebSocketHubBuilder AddJsonSerializerSettings(JsonSerializerSettings jsonSerializerSettings)
        {
            Services.Configure<WebSocketHubOptions>(o =>
            {
                o.UseJsonSerializerSettings(jsonSerializerSettings);
            });

            return this;
        }

        public WebSocketHubBuilder AddReceiveBufferSize(int receiveBufferSize)
        {
            Services.Configure<WebSocketHubOptions>(o =>
            {
                o.UseReceiveBufferSize(receiveBufferSize);
            });

            return this;
        }
    }
}
