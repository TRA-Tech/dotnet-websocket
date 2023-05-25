# dotnet-websocket
WebSocketHub is a middleware for ASP.NET Core that simplifies the process of building WebSocket applications. It provides a flexible and easy-to-use API for handling WebSocket connections and messages, and allows you to easily integrate WebSocket functionality into your existing ASP.NET Core application. With WebSocketHub, you can quickly build real-time applications that can handle large volumes of data and provide a seamless user experience. Whether you're building a chat application, a real-time dashboard, or any other type of WebSocket-based application, WebSocketHub makes it easy to get started and build robust, scalable solutions.

# Getting Started #
1. Add the following code to register the WebSocketHub service with the desired key type.
Replace TKey with the type of key you want to use for WebSocket connections.

  ```
  services.AddWebSocketHub<TKey>();
  ```
  
2. (Optional) Configure the WebSocketHub by chaining method calls to the AddWebSocketHub method:

  ```
  services.AddWebSocketHub<TKey>()
    .AddJsonSerializerSettings(jsonSerializerSettings)
    .AddReceiveBufferSize(receiveBufferSize)
    .AddRequestHandler<THandler>(messageType);
  ```
  
3. Add the following code to use the WebSocketHub middleware:

  ```
   app.UseWebSocketHub(
       acceptIf: (httpContext) => /* Your accept condition */,
       keyGenerator: (httpContext) => /* Your key generator */
   );
  ```
    
4. Use the WebSocketHub instance with dependency injection wherever you need it.

  ```
   private readonly WebSocketHub<TKey> _webSocketHub;

   public MyController(WebSocketHub<TKey> webSocketHub)
   {
       _webSocketHub = webSocketHub;
   }
  ```
  
  ```
   [HttpGet("GetSocketList")]
   public IActionResult GetSocketList(TKey id)
   {
       var socketListOfUser = _webSocketHub.GetSocketList(id);
       return Ok(socketListOfUser);
   }
  ```
5. Handle requests with classes that implement the ```IWebSocketRequestHandler``` interface.
Don't forget to register the implementation of ```IWebSocketRequestHandler``` with the ```WebSocketHub```
   
  ```
   public class MyWebSocketRequestHandler : IWebSocketRequestHandler
   {
       public Task HandleRequestAsync(string key, string data)
       {
           // Handle the WebSocket request
           return Task.CompletedTask;
       }
   }
  ```
  
  ```
   services.AddWebSocketHub<TKey>()
       .AddRequestHandler<MyWebSocketRequestHandler>("messageType");
  ```
  
