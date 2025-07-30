# Sample 16: Azure AI Agent Service with Tool Calling

This sample demonstrates the Azure AI Agent Service capabilities, including:

## Features

- **Azure AI Agent Service**: Full agent workflows with persistent conversations
- **Local Function Tools**: Time and weather tools with automatic calling
- **Code Interpreter Tool**: Built-in data analysis and visualization capabilities  
- **Terminal-Friendly**: Text-based outputs perfect for cloud shell environments
- **Automatic Tool Execution**: Agents automatically call tools when needed

## What This Demonstrates

1. **Agent Creation**: Creating agents with multiple tool types
2. **Tool Integration**: Combining local functions with built-in tools
3. **Streaming Conversations**: Real-time responses with tool calling visibility
4. **Auto Function Calling**: Automatic tool execution without manual intervention
5. **Data Analysis**: Code interpreter for calculations and text visualizations

## How It Works

### Agent Architecture
- Uses Azure AI Foundry SDK foundation (from Sample 7)
- Creates persistent agents with tool calling capabilities
- Implements function delegates for automatic tool resolution
- Supports streaming responses with tool execution feedback

### Tool Types
1. **Local Functions**: Custom C# functions (time, weather)
2. **Code Interpreter**: Built-in tool for data analysis and visualization
3. **Automatic Calling**: Agent decides when and how to use tools

### Sample Interactions

**Time Queries:**
```
You: What time is it?
🤖 Agent is thinking and calling tools...
⏰ [TOOL] Getting current time in 'short' format
⏰ [TOOL] Result: 1/15/2024 2:30 PM
The current time is 1/15/2024 2:30 PM.
```

**Weather Queries:**
```
You: What's the weather in Seattle?
🤖 Agent is thinking and calling tools...
🌤️ [TOOL] Getting weather for 'Seattle' in fahrenheit
🌤️ [TOOL] Result: Weather in Seattle: partly cloudy, 68°F, 65% humidity, wind 12 mph
The weather in Seattle is currently partly cloudy with a temperature of 68°F...
```

**Data Analysis:**
```
You: Analyze the trend in [1, 5, 3, 8, 2, 9, 4, 7]
🤖 Agent is thinking and calling tools...
[Agent uses code interpreter to analyze data and create text visualization]
```

## Configuration

Requires the same `.env` file as Sample 7:

```env
AI_FOUNDRY_PROJECT_CONNECTION_STRING=your_project_connection_string
AZURE_OPENAI_DEPLOYMENT_NAME=your_model_deployment_name
```

## Key Differences from Other Samples

| Sample | Approach | Tool Calling | Agent Features |
|--------|----------|--------------|----------------|
| Sample 7 | Direct AI Foundry SDK | None | Basic chat completion |
| Sample 11 | Semantic Kernel | Manual with attributes | Framework-based tools |
| **Sample 16** | **Azure AI Agent Service** | **Automatic with delegates** | **Full agent workflows** |

## Technical Architecture

### Package Dependencies
- `Azure.AI.Projects` (1.0.0-beta.10) - AI Foundry foundation
- `Azure.AI.Agents.Persistent` (1.0.0-beta.1) - Agent service
- `Azure.AI.Inference` (1.0.0-beta.5) - Model inference
- `Azure.Identity` (1.14.2) - Authentication

### Key Classes Used
- `PersistentAgentsClient` - Main agent service client
- `AutoFunctionCallOptions` - Automatic tool calling configuration
- `FunctionToolDefinition` - Local function tool definitions
- `CodeInterpreterToolDefinition` - Built-in analysis tool

## Running the Sample

```bash
cd samples/16-ai-agent-service
dotnet run
```

The agent will start an interactive conversation where you can:
- Ask about current time in different formats
- Request weather information for any location
- Perform data analysis with automatic visualizations
- See real-time tool calling in action

Perfect for demonstrations in cloud shell, terminal environments, and workshops where you need reliable text-based AI interactions with tool calling capabilities!
