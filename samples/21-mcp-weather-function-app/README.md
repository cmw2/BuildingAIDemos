# Sample 21 – MCP Weather Server (.NET / App Service)

This sample ports the MCP Weather Server from [Sample 19](../19-mcp-weather-server/) (Python/stdio) to a **.NET 8 ASP.NET Core** application that exposes the same weather tools and prompts over **Streamable HTTP**. It can be deployed to **Azure App Service** and consumed by:

- **Azure AI Foundry portal** agents (as a remote MCP server)
- **VS Code Copilot** (via `mcp.json`)
- **Sample 20** or any other MCP client

## Architecture

- Uses the official [ModelContextProtocol.AspNetCore](https://www.nuget.org/packages/ModelContextProtocol.AspNetCore) SDK (v1.0.0)
- Standard ASP.NET Core app — no Functions host, no storage dependency
- Exposes the MCP endpoint at `/mcp`

## Tools

| Tool | Description |
|------|-------------|
| `get_current_weather` | Get current weather for a location (deterministic fake data) |
| `get_weather_forecast` | Get multi-day forecast (1–7 days) with seasonal adjustments |
| `get_current_datetime` | Get current date/time in UTC or local timezone |

## Prompts

| Prompt | Description |
|--------|-------------|
| `time_in_location` | Get the current time in a specific city (instructs LLM to convert from UTC) |
| `daily_briefing` | Morning briefing with time, weather, and 3-day forecast |
| `weather_comparison` | Side-by-side weather comparison between two cities |

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Run Locally

```bash
cd samples/21-mcp-weather-function-app
dotnet run
```

The MCP endpoint will be available at `http://localhost:5000/mcp`.

### Test with VS Code

Add to `.vscode/mcp.json`:
```json
{
  "servers": {
    "weather-local": {
      "type": "http",
      "url": "http://localhost:5000/mcp"
    }
  }
}
```

## Deploy to Azure App Service

A deployment script is included:

```bash
./deploy.ps1
```

This creates a resource group, App Service Plan (B1 Linux), and a .NET 8 Web App, then publishes via zip deploy. Edit the variables at the top of the script to customize names.

The remote MCP endpoint will be at:
```
https://<app-name>.azurewebsites.net/mcp
```

## Connect from Azure AI Foundry

1. In the Foundry portal, open your agent
2. Under **Tools** → **Add** → **+ Add a new tool**
3. Select **Custom** → **Model Context Protocol (MCP)** → **Create**
4. Set the endpoint to `https://<app-name>.azurewebsites.net/mcp`
5. Configure authentication if needed (Entra ID or API key)

## Key Files

| File | Purpose |
|------|---------|
| `Program.cs` | ASP.NET Core host with MCP server wiring |
| `WeatherTools.cs` | MCP tool definitions (weather + datetime) |
| `WeatherPrompts.cs` | MCP prompt templates (time, briefing, comparison) |
| `deploy.ps1` | App Service deployment script |
