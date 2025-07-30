# Sample 17: Semantic Kernel Agents with Tool Calling

This sample demonstrates how to create and use Semantic Kernel Agents with automatic tool calling capabilities.

## Features

- **Semantic Kernel Agents**: Uses Microsoft.SemanticKernel.Agents.OpenAI for agent functionality
- **Native Function Plugins**: Time and weather tools implemented as Semantic Kernel plugins
- **Code Interpreter**: Built-in code interpreter for calculations and data analysis
- **Automatic Tool Calling**: Agent automatically calls appropriate tools based on user requests
- **Date Handling**: Proper workflow for converting relative dates to absolute dates

## Key Differences from Sample 16 (Azure AI Agent Service)

| Aspect | Sample 16 (Azure AI Agents) | Sample 17 (Semantic Kernel Agents) |
|--------|----------------------------|-----------------------------------|
| **Agent Framework** | Azure AI Agent Service | Semantic Kernel Agents |
| **Tool Definition** | FunctionToolDefinition + Delegates | KernelFunction attributes |
| **Function Organization** | Dictionary<string, Delegate> | Plugin classes with attributes |
| **Agent Creation** | PersistentAgentsClient | OpenAIAssistantAgent |
| **Tool Registration** | AutoFunctionCallOptions | kernelBuilder.Plugins.AddFromType |

## Tools Available

1. **GetCurrentLocalDateTime**: Returns local system time in various formats
2. **GetUTCDateTime**: Returns UTC time in various formats  
3. **GetWeather**: Returns weather information for a location and date (requires YYYY-MM-DD format)

## Example Interactions

```
You: What time is it locally?
[TOOL] GetCurrentLocalDateTime(format: 'short')
The current local time is 7/29/2025 2:30 PM.

You: What's the weather in Seattle tomorrow?
[TOOL] GetCurrentLocalDateTime(format: 'iso')
[TOOL] GetWeather(location: 'Seattle, WA', unit: 'fahrenheit', date: '2025-07-30')
FORECAST weather for Seattle, WA on Tuesday, July 30, 2025: partly cloudy, 78°F, 65% humidity, wind 12 mph
```

## Usage

```bash
cd samples/17-semantic-kernel-agents
dotnet run
```

## Configuration

Requires the same environment variables as other samples:
- `AZURE_OPENAI_ENDPOINT`
- `AZURE_OPENAI_API_KEY` (optional if using managed identity)
- `AZURE_OPENAI_DEPLOYMENT_NAME`
