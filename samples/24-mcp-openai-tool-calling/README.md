# Sample 24: MCP + OpenAI SDK Tool Calling (C#)

This sample demonstrates how to use a **remote MCP server** as the tool provider for the standard **OpenAI SDK manual tool calling** loop. The key technique is converting MCP tool definitions into OpenAI-compatible function schemas — and routing tool call requests back through the MCP client.

## What It Does

- **Connects to a remote MCP server** (Sample 21's weather server deployed to Azure)
- **Discovers tools dynamically** via `ListToolsAsync()` — no hardcoded schemas
- **Converts MCP → OpenAI format**: Maps `Tool.InputSchema` (JSON Schema) to `ChatTool.CreateFunctionTool(functionParameters)` — both are JSON Schema, so the mapping is direct
- **Runs the standard OpenAI tool call loop** (same pattern as Sample 10)
- **Forwards tool calls to the MCP server** via `CallToolAsync()` instead of calling local functions

## The Key Bridge: MCP → OpenAI Conversion

```
MCP Tool Definition              OpenAI ChatTool
──────────────────               ───────────────
Tool.Name              →         functionName
Tool.Description       →         functionDescription
Tool.InputSchema       →         functionParameters (BinaryData)
     ↑ JSON Schema                    ↑ JSON Schema
     └──── identical format ──────────┘
```

The conversion is trivial because both MCP and OpenAI use JSON Schema for parameter definitions:

```csharp
ChatTool.CreateFunctionTool(
    functionName: protocolTool.Name,
    functionDescription: protocolTool.Description ?? "",
    functionParameters: BinaryData.FromString(
        protocolTool.InputSchema.GetRawText()));
```

## Architecture

```
┌─────────────┐     ┌──────────────┐     ┌─────────────────┐
│  User Chat  │────▶│  Azure OpenAI │────▶│  Tool Call      │
│             │◀────│  (GPT-4.1)   │◀────│  Request        │
└─────────────┘     └──────────────┘     └────────┬────────┘
                                                   │
                              ┌─────────────────────┘
                              ▼
                    ┌──────────────────┐
                    │  MCP Client      │
                    │  (this sample)   │
                    │                  │
                    │  1. Parse args   │
                    │  2. CallToolAsync│
                    │  3. Return text  │
                    └────────┬─────────┘
                             │ Streamable HTTP
                             ▼
                    ┌──────────────────┐
                    │  MCP Weather     │
                    │  Server          │
                    │  (Sample 21)     │
                    └──────────────────┘
```

## Compare To Other Samples

| Sample | Tools Defined | Tool Execution | Orchestration |
|--------|--------------|----------------|---------------|
| **10** (Manual) | Locally in code | Local C# methods | Manual loop |
| **11** (SK) | Locally with `[KernelFunction]` | Local C# methods | Semantic Kernel |
| **20** (Agent Framework + MCP) | Discovered from MCP | Remote MCP server | Agent Framework auto-loop |
| **23** (Python) | **Discovered from MCP** | **Remote MCP server** | **Manual loop** |
| **24** (This sample, C#) | **Discovered from MCP** | **Remote MCP server** | **Manual loop** |

## Prerequisites

1. **Azure OpenAI** — configured in `../../.env`
2. **Sample 21 MCP server** — deployed to Azure (or running locally)
3. Set `MCP_WEATHER_ENDPOINT` in `.env` (or use the default)

## Configuration

Add to your `../../.env` file:

```env
# MCP Weather Server endpoint (from Sample 21)
MCP_WEATHER_ENDPOINT=https://your-app-name.azurewebsites.net/mcp
```

Or run Sample 21 locally and use:
```env
MCP_WEATHER_ENDPOINT=http://localhost:5000/mcp
```

## Running

```bash
cd samples/24-mcp-openai-tool-calling
dotnet run
```

## Available MCP Tools

These are discovered dynamically from the server at runtime:

| Tool | Description | Parameters |
|------|-------------|------------|
| `get_current_weather` | Get current weather for a location | `location` (string) |
| `get_weather_forecast` | Get weather forecast | `location` (string), `days` (int, 1-7) |
| `get_current_datetime` | Get current date and time | `timezone` (string: "UTC" or "local") |

## Key Concepts

### Dynamic Tool Discovery
Instead of hardcoding function schemas (like Sample 10), tools are discovered at runtime from the MCP server. If the server adds new tools, this client automatically picks them up.

### Schema Compatibility
MCP and OpenAI both use JSON Schema for parameter definitions. This means:
- No manual schema translation
- No risk of schema drift between client and server
- The server is the single source of truth for tool definitions

### Tool Execution Delegation
The client doesn't implement any tool logic. It simply forwards the model's tool call requests to the MCP server and relays the results back. The server handles all execution.
