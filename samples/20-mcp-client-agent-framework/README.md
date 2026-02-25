# Sample 20: MCP Client with Agent Framework

This sample demonstrates how to build an AI agent that discovers and uses tools from **multiple MCP (Model Context Protocol) servers**, using the **Microsoft Agent Framework RC1**.

Instead of defining tools locally (like Sample 11a), this sample connects to two MCP servers simultaneously:

1. **Weather Server** (Sample 19) — local Python server via stdio transport
2. **Microsoft Learn MCP Server** — remote cloud server via Streamable HTTP transport

Tools from all servers are merged and passed to a single Agent Framework agent, demonstrating how MCP enables a composable, multi-server architecture.

## Key Concepts

### MCP Multi-Server + Agent Framework Integration

```
                                         ┌──────────────────────────┐
                          stdio          │  Sample 19 (Python)      │
                     ┌──────────────────►│  MCP Weather Server      │
                     │   JSON-RPC        │                          │
                     │                   │  - get_current_weather   │
┌────────────────────┤                   │  - get_weather_forecast  │
│  Sample 20 (.NET)  │                   │  - get_current_datetime  │
│                    │                   │  + prompts               │
│  ┌──────────────┐  │                   └──────────────────────────┘
│  │ Agent        │  │
│  │ Framework    │  │                   ┌──────────────────────────┐
│  │ (AIAgent)    │  │  Streamable HTTP  │  Microsoft Learn         │
│  │              │  ├──────────────────►│  MCP Server (Remote)     │
│  │ ┌──────────┐ │  │                   │                          │
│  │ │ Merged   │ │  │                   │  - microsoft_docs_search │
│  │ │MCP Tools │ │  │                   │  - microsoft_docs_fetch  │
│  │ └──────────┘ │  │                   │  - microsoft_code_sample │
│  └──────────────┘  │                   │    _search               │
│                    │                   └──────────────────────────┘
│  Azure OpenAI ◄────┤
└────────────────────┘
```

1. **`McpClientFactory.CreateAsync()`** — Connects to each MCP server (stdio or HTTP)
2. **`client.ListToolsAsync()`** — Discovers tools from each server; returns `McpClientTool` objects
3. **Tools are merged** — `weatherTools.Concat(learnTools)` creates a unified tool list
4. **`ChatOptions.Tools`** — All MCP tools passed to the Agent Framework in one list
5. **`chatClient.AsAIAgent()`** — The Agent Framework handles tool calling across all servers

### Two Transport Types

| Transport | Server | Connection |
|-----------|--------|------------|
| **Stdio** | Weather Server (local) | Launches Python process, communicates via stdin/stdout JSON-RPC |
| **Streamable HTTP** | MS Learn Server (remote) | Connects to `https://learn.microsoft.com/api/mcp` over HTTP |

### MCP Prompts (Slash Commands)

MCP servers can provide **prompt templates** — reusable instructions that guide the LLM. Per the MCP spec, prompts are "user-controlled" and invoked via slash commands:

```
/time_in_location Tokyo
/daily_briefing Seattle, WA
/weather_comparison Seattle, WA | Miami, FL
```

The client discovers prompts from all servers and maps each to its owning server for invocation.

### How It Compares to Sample 11a

| Aspect | Sample 11a (Local Tools) | Sample 20 (MCP Tools) |
|--------|--------------------------|----------------------|
| Tool location | In-process static methods | External MCP servers |
| Servers | N/A | Multiple (local + remote) |
| Tool discovery | Manual `AIFunctionFactory.Create()` | Automatic via `ListToolsAsync()` |
| Tool definition | `[Description]` attributes on C# methods | JSON Schema in MCP servers |
| Agent Framework | `chatClient.AsAIAgent()` | `chatClient.AsAIAgent()` (same!) |
| Tool type | `AIFunction` | `McpClientTool` (both extend `AITool`) |

## Prerequisites

- .NET 8.0 SDK
- Python 3.x (for the local MCP Weather Server)
- Azure OpenAI deployment with a chat model (e.g., gpt-4o)
- Azure CLI logged in (`az login`) for DefaultAzureCredential
- Internet access (for the remote MS Learn MCP Server)

## Running the Sample

1. Make sure your `.env` file is configured (see `.env.example` in the repo root)

2. Run the sample:
   ```bash
   cd samples/20-mcp-client-agent-framework
   dotnet run
   ```

   This will:
   - Launch the Python MCP Weather Server (Sample 19) as a child process
   - Connect to the Microsoft Learn MCP Server over HTTP

3. Try asking questions:
   ```
   🌤️ Weather:
   You: What's the weather in Seattle?
   You: Give me a 5-day forecast for New York

   📚 Microsoft Learn:
   You: Search Microsoft Learn for Azure Functions best practices
   You: How do I use dependency injection in ASP.NET Core?

   📋 Slash commands (MCP prompts):
   You: /time_in_location Tokyo
   You: /daily_briefing Seattle, WA
   ```

4. Type `quit`, `exit`, or `q` to stop.

## How It Works

### 1. Connect to MCP Servers
The sample connects to two MCP servers using different transports:

```csharp
// Local server via stdio
await using var weatherMcpClient = await McpClientFactory.CreateAsync(
    new StdioClientTransport(new StdioClientTransportOptions
    {
        Command = "python",
        Arguments = ["../19-mcp-weather-server/server.py"],
        Name = "WeatherMcpServer"
    }));

// Remote server via Streamable HTTP
await using var learnMcpClient = await McpClientFactory.CreateAsync(
    new SseClientTransport(new SseClientTransportOptions
    {
        Endpoint = new Uri("https://learn.microsoft.com/api/mcp"),
        Name = "MicrosoftLearnMcpServer",
        TransportMode = HttpTransportMode.StreamableHttp
    }));
```

### 2. Discover and Merge Tools
Tools from all servers are discovered and merged into a single list:

```csharp
var weatherTools = await weatherMcpClient.ListToolsAsync();
var learnTools = await learnMcpClient.ListToolsAsync();
var allTools = weatherTools.Concat(learnTools).ToList();
```

### 3. Build Agent with Merged Tools
All MCP tools are passed to the Agent Framework in one list:

```csharp
var chatOptions = new ChatOptions
{
    Tools = [.. allTools],  // Tools from ALL servers
    // ...
};

AIAgent agent = chatClient.AsAIAgent(
    new ChatClientAgentOptions
    {
        Name = "McpMultiServerAgent",
        ChatOptions = chatOptions
    });
```

### 4. Chat Loop
The Agent Framework handles the complete tool-calling cycle across all servers:
1. User message → LLM
2. LLM requests tool call → Agent Framework invokes the correct MCP tool (routed to the right server)
3. MCP server returns result → Agent Framework sends back to LLM
4. LLM generates final response → displayed to user

## Available MCP Tools

### Weather Server (Sample 19, local)

| Tool | Description | Parameters |
|------|-------------|------------|
| `get_current_weather` | Get current weather for a location | `location` (required) |
| `get_weather_forecast` | Get multi-day forecast | `location` (required), `days` (1-7, optional) |
| `get_current_datetime` | Get current date/time | `timezone` (optional: "UTC" or "local") |

### Microsoft Learn Server (remote)

| Tool | Description | Parameters |
|------|-------------|------------|
| `microsoft_docs_search` | Search Microsoft technical documentation | `query` (required) |
| `microsoft_docs_fetch` | Fetch a docs page as markdown | `url` (required) |
| `microsoft_code_sample_search` | Search for official code samples | `query` (required), `language` (optional) |

## NuGet Packages

| Package | Purpose |
|---------|---------|
| `Microsoft.Agents.AI.OpenAI` (1.0.0-rc1) | Agent Framework with automatic tool calling |
| `ModelContextProtocol` (0.3.0-preview.1) | MCP client for connecting to MCP servers |
| `Azure.AI.OpenAI` | Azure OpenAI client |
| `Azure.Identity` | DefaultAzureCredential authentication |
| `DotNetEnv` | Load `.env` configuration |

## Educational Value

This sample demonstrates:

1. **Multi-Server MCP** — Connecting to multiple MCP servers with different transports
2. **Local + Remote** — Combining a local stdio server with a remote HTTP server
3. **Dynamic Tool Discovery** — Tools are discovered at runtime from each server
4. **Tool Merging** — Tools from multiple servers work together in a single agent
5. **MCP Prompts** — Server-provided prompt templates invoked via slash commands
6. **Seamless Abstraction** — MCP tools work with the Agent Framework without adaptation
7. **Cross-Language Interop** — .NET client consuming a Python server via MCP
8. **Separation of Concerns** — Tool logic lives in separate, reusable servers
