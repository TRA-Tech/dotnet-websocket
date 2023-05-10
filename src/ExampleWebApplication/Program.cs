using ExampleWebApplication.WebSocketRequestHandlers;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using TraTech.WebSocketHub;
using ExampleWebApplication.WebSocketHubKeys;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddWebSocketHub<int>()
    .AddRequestHandler<WebSocketRequestHandler1>("messageType1");

builder.Services.AddWebSocketHub<SocketUser>()
    .AddJsonSerializerSettings(new JsonSerializerSettings
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    })
    .AddReceiveBufferSize(4 * 1024)
    .AddRequestHandler<WebSocketRequestHandler2>("messageType2");


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseWebSocketHub(
    acceptIf: (httpContext) => httpContext.Request.Path == "/ws1" && !string.IsNullOrEmpty(httpContext.Request.Query["id"]),
    keyGenerator: (httpContext) => int.Parse(httpContext.Request.Query["id"])
);

app.UseWebSocketHub(
    acceptIf: (httpContext) => httpContext.Request.Path == "/ws2" && !string.IsNullOrEmpty(httpContext.Request.Query["id"]),
    keyGenerator: (httpContext) =>
    {
        int id = int.Parse(httpContext.Request.Query["id"]);
        return new SocketUser(id);
    }
);

app.MapControllers();

app.Run();
