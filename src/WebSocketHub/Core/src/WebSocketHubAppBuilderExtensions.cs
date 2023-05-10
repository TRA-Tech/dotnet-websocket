using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebSockets;

namespace TraTech.WebSocketHub
{
    public static class WebSocketHubAppBuilderExtensions
    {
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

        public static IApplicationBuilder UseWebSocketHub<TKey>(this IApplicationBuilder app, WebSocketOptions options, Func<HttpContext, bool> acceptIf, Func<HttpContext, TKey> keyGenerator)
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
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            if (keyGenerator == null)
            {
                throw new ArgumentNullException(nameof(keyGenerator));
            }

            VerifyServicesRegistered(app, options);

            return app.UseMiddleware<WebSocketHubMiddleware<TKey>>(acceptIf, keyGenerator, options.ReceiveBufferSize);
        }

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
