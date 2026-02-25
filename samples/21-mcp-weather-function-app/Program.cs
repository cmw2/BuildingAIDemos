using McpWeatherFunctionApp;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer(options =>
    {
        options.ServerInfo = new Implementation
        {
            Name = "MCP Weather Server (.NET)",
            Version = "1.0.0"
        };
    })
    .WithHttpTransport()
    .WithTools<WeatherTools>()
    .WithPrompts<WeatherPrompts>();

var app = builder.Build();

app.MapMcp("/mcp");

app.Run();
