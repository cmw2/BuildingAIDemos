# Sample 10: Tool Calling - Manual Implementation

This sample demonstrates manual tool calling implementation using Azure OpenAI SDK with custom function definitions and execution logic.

## What It Does

- **Manual Tool Calling**: Implements tool calling "the hard way" using Azure OpenAI SDK directly
- **Function Definitions**: Defines custom functions for date/time and weather information
- **Parameter Handling**: Uses strongly-typed parameters with JSON parsing
- **Interactive Chat**: Provides a conversational interface that can call local functions

## Key Features

### Available Functions

1. **Get Current Date/Time**
   - Function: `get_current_datetime`
   - Parameters: `format` (optional: "short", "long", "iso")
   - Returns: Current date and time in the specified format

2. **Get Weather Information**
   - Function: `get_weather` 
   - Parameters: `location` (required), `unit` (optional: "celsius", "fahrenheit")
   - Returns: Simulated weather data for the specified location

### Implementation Details

- **Function Schemas**: Uses `ChatTool.CreateFunctionTool()` to define function schemas
- **Tool Call Processing**: Manually processes `ChatToolCall` objects from API responses
- **Parameter Parsing**: Converts JSON arguments to strongly-typed `Dictionary<string, object>`
- **Result Integration**: Adds function results back to conversation using `ToolChatMessage`

## Usage Examples

```
You: what time is it?
🔧 AI is calling functions...
  📞 Calling: get_current_datetime({"format":"long"})
  📋 Result: Sunday, July 27, 2025 at 10:34:48 AM
🤖 Assistant: It is Sunday, July 27, 2025, at 10:34:48 AM.

You: what's the weather in paris in celsius?
🔧 AI is calling functions...
  📞 Calling: get_weather({"location":"Paris","unit":"celsius"})
  📋 Result: The weather in Paris is currently rainy with a temperature of 30°C.
🤖 Assistant: The current weather in Paris is rainy with a temperature of 30°C.
```

## Code Architecture

```csharp
// Function schema definition
var functionSchemas = new List<ChatTool>
{
    ChatTool.CreateFunctionTool("get_current_datetime", "Get current date and time", schema),
    ChatTool.CreateFunctionTool("get_weather", "Get weather information", schema)
};

// Function implementation mapping
var availableFunctions = new Dictionary<string, Func<Dictionary<string, object>, string>>
{
    ["get_current_datetime"] = GetCurrentDateTime,
    ["get_weather"] = GetWeather
};

// Tool call processing
foreach (var toolCall in response.Value.ToolCalls)
{
    if (toolCall is ChatToolCall functionToolCall)
    {
        var functionName = functionToolCall.FunctionName;
        var functionArgs = functionToolCall.FunctionArguments;
        var args = JsonSerializer.Deserialize<Dictionary<string, object>>(functionArgs);
        var functionResult = function(args);
        messages.Add(new ToolChatMessage(functionToolCall.Id, functionResult));
    }
}
```

## Authentication

Uses `DefaultAzureCredential` for Azure OpenAI authentication:

```csharp
var credential = new DefaultAzureCredential();
var chatClient = new AzureOpenAIClient(endpoint, credential).GetChatClient(deploymentName);
```

## Dependencies

- `Azure.AI.OpenAI` - Azure OpenAI SDK
- `Azure.Identity` - Authentication
- `System.Text.Json` - JSON parsing

## Running the Sample

```bash
cd samples/10-tool-calling-manual
dotnet run
```

## Learning Points

1. **Manual Implementation**: Shows the "hard way" of implementing tool calling with full control
2. **Function Schema Design**: How to define proper JSON schemas for function parameters
3. **API Integration**: Direct usage of Azure OpenAI SDK for tool calling
4. **Error Handling**: Robust handling of function calls and responses
5. **Type Safety**: Using strongly-typed parameters instead of string manipulation

## Next Steps

- See Sample 11 for Semantic Kernel auto tool calling implementation
- Compare manual vs. automated approaches for tool calling
- Explore more complex function schemas and parameter validation
