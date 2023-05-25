using Microsoft.Extensions.DependencyInjection;

namespace TraTech.WebSocketHub
{
    public static class WebSocketHubServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the WebSocketHub to the service collection with the specified key type.
        /// </summary>
        /// <typeparam name="TKey">The type of key associated with WebSocket connections.</typeparam>
        /// <param name="services">The IServiceCollection instance.</param>
        /// <returns>A WebSocketHubBuilder instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
        /// <remarks>
        /// This extension method adds the WebSocketHub to the service collection with the specified key type. It also registers the WebSocketHub as a singleton service and adds options to the service collection.
        /// </remarks>
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
