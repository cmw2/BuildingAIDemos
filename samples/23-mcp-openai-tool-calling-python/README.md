# Sample 23: MCP + OpenAI SDK Tool Calling (Python)

This sample demonstrates how to use a **remote MCP server** as the tool provider for the standard **OpenAI SDK manual tool calling** loop — in Python. The key technique is converting MCP tool definitions into OpenAI-compatible function schemas and routing tool call requests back through the MCP client.

## What It Does

- **Connects to a remote MCP server** (Sample 21's weather server deployed to Azure)
- **Discovers tools dynamically** via `session.list_tools()` — no hardcoded schemas
- **Converts MCP → OpenAI format**: Maps `Tool.inputSchema` (JSON Schema dict) to OpenAI function tool `parameters` — both are JSON Schema, so the mapping is direct
- **Runs the standard OpenAI tool call loop** (same pattern you'd use with local functions)
- **Forwards tool calls to the MCP server** via `session.call_tool()` instead of calling local functions

## The Key Bridge: MCP → OpenAI Conversion

```
MCP Tool Definition              OpenAI Function Tool
──────────────────               ────────────────────
Tool.name              →         function.name
Tool.description       →         function.description
Tool.inputSchema       →         function.parameters
     ↑ JSON Schema dict               ↑ JSON Schema dict
     └──── identical format ───────────┘
```

The conversion is trivial because both MCP and OpenAI use JSON Schema for parameter definitions:

```python
openai_tool = {
    "type": "function",
    "function": {
        "name": tool.name,
        "description": tool.description or "",
        "parameters": tool.inputSchema,  # Already JSON Schema!
    },
}
```

## Architecture

```
┌─────────────┐     ┌──────────────┐     ┌─────────────────┐
│  User Chat  │────▶│  Azure OpenAI │────▶│  Tool Call      │
│  (Python)   │◀────│  (GPT-4.1)   │◀────│  Request        │
└─────────────┘     └──────────────┘     └────────┬────────┘
                                                   │
                              ┌─────────────────────┘
                              ▼
                    ┌──────────────────┐
                    │  MCP Client      │
                    │  (this sample)   │
                    │                  │
                    │  1. Parse args   │
                    │  2. call_tool()  │
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

| Sample | Language | Tools Defined | Tool Execution | Orchestration |
|--------|----------|--------------|----------------|---------------|
| **10** (Manual) | C# | Locally in code | Local C# methods | Manual loop |
| **03** (CLI) | Python | None | N/A | Simple chat |
| **20** (Agent Framework + MCP) | C# | Discovered from MCP | Remote MCP server | Agent Framework auto-loop |
| **23** (This sample) | **Python** | **Discovered from MCP** | **Remote MCP server** | **Manual loop** |
| **24** (C# version) | C# | Discovered from MCP | Remote MCP server | Manual loop |

## Prerequisites

1. **Azure OpenAI** — configured in `../../.env`
2. **Sample 21 MCP server** — deployed to Azure (or running locally)
3. Python packages: `openai`, `azure-identity`, `python-dotenv`, `mcp`

## Setup

```bash
# From the repo root (uses the root venv)
pip install -r requirements.txt
```

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
cd samples/23-mcp-openai-tool-calling-python
python mcp_openai_tool_calling.py
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
Instead of hardcoding function schemas, tools are discovered at runtime from the MCP server. If the server adds new tools, this client automatically picks them up.

### Schema Compatibility
MCP and OpenAI both use JSON Schema for parameter definitions. This means:
- No manual schema translation
- No risk of schema drift between client and server
- The server is the single source of truth for tool definitions

### Tool Execution Delegation
The client doesn't implement any tool logic. It simply forwards the model's tool call requests to the MCP server and relays the results back. The server handles all execution.
