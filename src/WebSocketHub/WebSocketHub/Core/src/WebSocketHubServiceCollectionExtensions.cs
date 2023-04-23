using Microsoft.Extensions.DependencyInjection;

namespace TraTech.WebSocketHub
{
    public static class WebSocketHubServiceCollectionExtensions
    {
        public static WebSocketHubBuilder AddWebSocketHub<TKey>(this IServiceCollection services)
            where TKey : notnull
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddOptions();

            services.AddSingleton<WebSocketHub<TKey>>();

            return new WebSocketHubBuilder(services);
        }
    }
}
