using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebSockets;

namespace TraTech.WebSocketHub
{
    public static class WebSocketHubAppBuilderExtensions
    {
        /// <summary>
        /// Adds the WebSocketHubMiddleware to the pipeline with the specified acceptIf and keyGenerator functions.
        /// </summary>
        /// <typeparam name="TKey">The type of key associated with WebSocket connections.</typeparam>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="acceptIf">The function used to determine if a WebSocket request should be accepted.</param>
        /// <param name="keyGenerator">The function used to generate a key for the WebSocket connection.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="app"/>, <paramref name="acceptIf"/>, or <paramref name="keyGenerator"/> is null.</exception>
        /// <remarks>
        /// This extension method adds the WebSocketHubMiddleware to the pipeline with the specified acceptIf and keyGenerator functions for handling WebSocket requests associated with keys of type TKey.
        /// </remarks>
        public static IApplicationBuilder UseWebSocketHub<TKey>(this IApplicationBuilder app, Func<HttpContext, bool> acceptIf, Func<HttpContext, TKey> keyGenerator)
            where TKey : notnull
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (acceptIf == null)
            {
                throw new ArgumentNullException(nameof(acceptIf));
            }
            if (keyGenerator == null)
            {
                throw new ArgumentNullException(nameof(keyGenerator));
            }

            VerifyServicesRegistered(app);

            return app.UseMiddleware<WebSocketHubMiddleware<TKey>>(acceptIf, keyGenerator);
        }

        /// <summary>
        /// Verifies that the WebSocketMiddleware is registered in the application services.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="options">The WebSocket options to configure the middleware.</param>
        /// <remarks>
        /// This method verifies that the WebSocketMiddleware is registered in the application services. If it is not registered, this method adds it to the pipeline with the specified WebSocket options, or with default options if no options are specified.
        /// </remarks>
        private static void VerifyServicesRegistered(IApplicationBuilder app, WebSocketOptions? options = null)
        {
            if (app.ApplicationServices.GetService(typeof(WebSocketMiddleware)) == null)
            {
                if (options is not null)
                {
                    app.UseWebSockets(options);
                }
                else
                {
                    app.UseWebSockets();
                }
            }
        }
    }
}
