# Sample 11a: Tool Calling - Microsoft Agent Framework

This sample demonstrates automatic tool calling using the Microsoft Agent Framework, providing the same functionality as Sample 11 (Semantic Kernel) but using the newer Agent Framework approach.

## Key Features

- **AIFunctionFactory Tool Creation**: Tools are created from plain static methods using `AIFunctionFactory.Create()`
- **Automatic Tool Invocation**: The Agent Framework's built-in `FunctionInvokingChatClient` handles the entire tool call loop automatically
- **Simple Tool Definitions**: No plugin classes or `[KernelFunction]` attributes — just static methods with `[Description]` attributes
- **Same Tools**: Implements the same `GetCurrentDateTime` and `GetWeather` functions as Sample 11
- **Session Management**: Multi-turn conversations with built-in history tracking

## How It Works

1. **Tool Definition**: Functions are defined as static methods with `[Description]` attributes on methods and parameters
2. **Tool Registration**: `AIFunctionFactory.Create()` wraps each method as an `AITool`, inferring the schema from the method signature
3. **Agent Configuration**: Tools are passed via `ChatOptions.Tools` in `ChatClientAgentOptions`
4. **Automatic Execution**: The Agent Framework automatically decorates the chat client with a `FunctionInvokingChatClient` that handles tool call detection, invocation, and result forwarding

## Comparison with Sample 11 (Semantic Kernel)

| Aspect | Sample 11 (Semantic Kernel) | Sample 11a (Agent Framework) |
|--------|---------------------------|------------------------------|
| Tool Definition | `[KernelFunction]` + plugin classes | Static methods + `[Description]` |
| Tool Registration | `builder.Plugins.AddFromType<>()` | `AIFunctionFactory.Create()` |
| Auto Invocation | `ToolCallBehavior.AutoInvokeKernelFunctions` | Built-in (default behavior) |
| Options | `OpenAIPromptExecutionSettings` | `ChatOptions` (M.E.AI standard) |
| Session/History | `ChatHistory` (SK-specific) | `AgentSession` (built-in) |
| Framework | Semantic Kernel | Microsoft Agent Framework |
| Abstractions | SK-specific types | `Microsoft.Extensions.AI` standard |

## SDK Packages

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.Agents.AI.OpenAI` | 1.0.0-rc1 | Agent Framework OpenAI integration |
| `Azure.AI.OpenAI` | 2.8.0-beta.1 | Azure OpenAI client |
| `Azure.Identity` | 1.17.1 | `DefaultAzureCredential` for auth |
| `DotNetEnv` | 3.1.1 | Environment variable loading |

## Usage

```bash
cd samples/11a-tool-calling-agent-framework
dotnet run
```

## Example Interactions

```
You: What's the current date and time?
🕐 [DEBUG] TimeTools.GetCurrentDateTime called with format: 'short'
🕐 [DEBUG] TimeTools returning: '2/23/2026 2:30 PM'
🤖 Assistant: The current date and time is 2/23/2026 2:30 PM.

You: What's the weather like in Seattle?
🌤️  [DEBUG] WeatherTools.GetWeather called with location: 'Seattle', unit: 'fahrenheit'
🌤️  [DEBUG] WeatherTools returning: 'The weather in Seattle is currently sunny with a temperature of 72°F.'
🤖 Assistant: The weather in Seattle is currently sunny with a temperature of 72°F.

You: Tell me the time in ISO format
🕐 [DEBUG] TimeTools.GetCurrentDateTime called with format: 'iso'
🕐 [DEBUG] TimeTools returning: '2026-02-23T14:30:15.123Z'
🤖 Assistant: The current time in ISO format is 2026-02-23T14:30:15.123Z.
```

## Learning Objectives

- Understand how `AIFunctionFactory.Create()` wraps methods as AI tools
- See how the Agent Framework automatically handles the tool call loop
- Compare the Agent Framework approach to Semantic Kernel's plugin architecture
- Learn how `ChatOptions.Tools` integrates tools into agent configuration
- Observe that `Microsoft.Extensions.AI` provides a standard abstraction layer
