using TraTech.WebSocketHub;

namespace ExampleWebApplication.WebSocketRequestHandlers
{
    public class WebSocketRequestHandler1 : IWebSocketRequestHandler
    {
        public Task HandleRequestAsync(string key, string data)
        {
            Console.WriteLine("------- HandleRequestAsync started -------");
            Console.WriteLine("key");
            Console.WriteLine(key);
            Console.WriteLine("data");
            Console.WriteLine(data);
            Console.WriteLine("------- HandleRequestAsync finished -------");
            return Task.CompletedTask;
        }
    }
}
