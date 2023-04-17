using TraTech.WebSocketHub;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<WebSocketHub<int>>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseWebSocketHub<int>(
    acceptIf: (httpContext) => httpContext.Request.Path == "/ws" && !string.IsNullOrEmpty(httpContext.Request.Query["id"]),
    keyGenerator: (httpContext) => int.Parse(httpContext.Request.Query["id"])
);

app.MapControllers();

app.Run();
