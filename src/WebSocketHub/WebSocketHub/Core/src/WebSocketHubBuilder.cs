using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
    }
}
