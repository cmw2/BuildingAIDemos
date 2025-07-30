# Sample 19: MCP Weather Server

This sample demonstrates how to create a custom Model Context Protocol (MCP) server that provides weather and datetime tools. The server implements the same fake weather and datetime functions used in samples 10, 11, 16, and 17, but exposes them as an MCP server that can be consumed by any MCP-compatible client.

## What is MCP?

Model Context Protocol (MCP) is an open standard for connecting AI assistants to external tools and data sources. It enables:

- **Standardized Tool Integration**: AI agents can discover and use tools through a consistent interface
- **Interoperability**: One MCP server can work with multiple AI clients (Claude Desktop, VS Code Copilot, custom applications)
- **Separation of Concerns**: Tool logic is separated from AI agent implementation
- **Reusability**: Tools can be shared across different AI applications

## Available Tools

This MCP server provides three tools:

### 1. get_current_weather
Get current weather information for a specific location.

**Parameters:**
- `location` (string, required): The city and state, e.g., "Seattle, WA"

**Example Response:**
```json
{
  "location": "Seattle, WA",
  "temperature": 72,
  "condition": "Partly Cloudy",
  "humidity": 65,
  "unit": "Fahrenheit"
}
```

### 2. get_weather_forecast
Get weather forecast for a specific location.

**Parameters:**
- `location` (string, required): The city and state, e.g., "Seattle, WA"
- `days` (integer, optional): Number of days to forecast (1-7, default: 3)

**Example Response:**
```json
{
  "location": "Seattle, WA",
  "forecast": [
    {
      "date": "2025-07-30",
      "temperature_high": 72,
      "temperature_low": 62,
      "condition": "Sunny",
      "humidity": 50
    }
  ],
  "unit": "Fahrenheit"
}
```

### 3. get_current_datetime
Get current date and time information.

**Parameters:**
- `timezone` (string, optional): "UTC" or "local" (default: "local")

**Example Response:**
```json
{
  "datetime": "2025-07-30T14:30:00.000Z",
  "formatted": "2025-07-30 14:30:00",
  "timezone": "Local",
  "timestamp": 1722350200
}
```

## Implementation Versions

This sample provides two implementations:

### Python Version (`server.py`)
- **Usage**: `python server.py`
- **Features**: 
  - Pure Python implementation using standard library
  - Async/await support for handling requests
  - JSON-RPC 2.0 protocol implementation
  - Comprehensive error handling

### .NET Version (`Program.cs`)
- **Usage**: `dotnet run`
- **Features**:
  - Modern C# with records and pattern matching
  - System.Text.Json for serialization
  - Strongly-typed request/response models
  - Comprehensive error handling

## Running the Server

### Python Version
```bash
cd samples/19-mcp-weather-server
python server.py
```

Capabilities:
- Read JSON-RPC requests from stdin
- Write JSON-RPC responses to stdout
- Log informational messages to stderr
- Follow the MCP protocol specification

## MCP Protocol Details

The server implements the following MCP protocol methods:

### initialize
Handshake method to establish the connection and exchange capabilities.

**Request:**
```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "initialize",
  "params": {}
}
```

**Response:**
```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "result": {
    "protocolVersion": "2024-11-05",
    "capabilities": {
      "tools": {}
    },
    "serverInfo": {
      "name": "weather-server",
      "version": "1.0.0"
    }
  }
}
```

### tools/list
Discover available tools and their schemas.

**Request:**
```json
{
  "jsonrpc": "2.0",
  "id": 2,
  "method": "tools/list"
}
```

**Response:**
```json
{
  "jsonrpc": "2.0",
  "id": 2,
  "result": {
    "tools": [
      {
        "name": "get_current_weather",
        "description": "Get the current weather for a specific location",
        "inputSchema": {
          "type": "object",
          "properties": {
            "location": {
              "type": "string",
              "description": "The city and state, e.g. San Francisco, CA"
            }
          },
          "required": ["location"]
        }
      }
    ]
  }
}
```

### tools/call
Execute a specific tool with provided arguments.

**Request:**
```json
{
  "jsonrpc": "2.0",
  "id": 3,
  "method": "tools/call",
  "params": {
    "name": "get_current_weather",
    "arguments": {
      "location": "Seattle, WA"
    }
  }
}
```

**Response:**
```json
{
  "jsonrpc": "2.0",
  "id": 3,
  "result": {
    "content": [
      {
        "type": "text",
        "text": "{\"location\": \"Seattle, WA\", \"temperature\": 72, ...}"
      }
    ]
  }
}
```

## Testing the Server

### Using MCP Inspector (Recommended)

MCP Inspector is the official testing tool for MCP servers. It provides a web-based interface to interact with your server.

1. **Install MCP Inspector** (if not already installed):
   ```bash
   npm install -g @modelcontextprotocol/inspector
   ```

2. **Test the Python server**:
   ```bash
   cd samples/19-mcp-weather-server
   mcp-inspector python server.py
   ```

3. **Test the .NET server**:
   ```bash
   cd samples/19-mcp-weather-server
   mcp-inspector dotnet run
   ```

4. **Open your browser** to the URL shown (typically `http://localhost:5173`)

5. **Try the tools** in the web interface:
   - Click "Connect" to establish the MCP connection
   - Browse available tools in the sidebar
   - Test each tool with different parameters
   - View request/response details

### Manual Testing

You can also test the server manually by sending JSON-RPC requests via stdin:

1. Start the server: `python server.py` or `dotnet run`
2. Send test requests:

```json
{"jsonrpc": "2.0", "id": 1, "method": "initialize", "params": {}}
{"jsonrpc": "2.0", "id": 2, "method": "tools/list"}
{"jsonrpc": "2.0", "id": 3, "method": "tools/call", "params": {"name": "get_current_weather", "arguments": {"location": "Seattle"}}}
```

## Integration with AI Clients

This MCP server can be integrated with various AI clients:

### Claude Desktop
Add to your MCP configuration file to use with Claude Desktop.

### VS Code Copilot
Configure as an MCP server in your VS Code settings.

### Custom Applications
Use any MCP client library to connect and consume the tools.

## Fake Data

The server uses generates random weather data.

## Educational Value

This sample demonstrates:

1. **MCP Protocol Implementation**: How to build a compliant MCP server
2. **Tool Definition**: Proper schema definition for AI tool discovery
3. **JSON-RPC Communication**: Standard protocol for client-server communication  
4. **Error Handling**: Robust error handling and response formatting
5. **Code Reuse**: How to extract tool logic from AI agent implementations
6. **Interoperability**: Creating tools that work across different AI platforms

## Next Steps

After running this sample, you can:

1. **Extend Tools**: Add more weather-related tools (alerts, historical data, etc.)
2. **Connect Real APIs**: Replace fake data with real weather service APIs
3. **Add Authentication**: Implement API key validation for tool access
4. **Create Client**: Build a custom MCP client to consume these tools
5. **Deploy**: Host the server for remote access by multiple clients

