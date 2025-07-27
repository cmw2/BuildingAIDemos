# Sample 11: Tool Calling - Semantic Kernel Auto Imp3. **Type Safety**: Function parameters are strongly typed with compile-time checking
4. **Attribute-Based**: Clean, declarative function definitions using C# attributes
5. **Plugin Architecture**: Logical separation of concerns with dedicated plugin classes
6. **Built-in Error Handling**: Framework handles many edge cases automatically
7. **Maintainability**: Easier to add new functions - just add methods with attributes to appropriate plugin classesntation

This sample demonstrates automatic function calling using Semantic Kernel, providing a much simpler alternative to the manual implementation shown in Sample 10.

## Key Features

- **Automatic Function Discovery**: Functions are automatically discovered and registered using attributes
- **Auto Function Calling**: Semantic Kernel handles all the complexity of tool calling automatically
- **Clean Code**: Much simpler implementation compared to manual tool calling
- **Organized Plugin Architecture**: Functions are separated into logical plugin classes (TimePlugin and WeatherPlugin)
- **Same Functions**: Implements the same `get_current_datetime` and `get_weather` functions as Sample 10

## How It Works

1. **Plugin Definition**: Functions are defined as methods in separate plugin classes (TimePlugin, WeatherPlugin) with `[KernelFunction]` attributes
2. **Auto Registration**: `kernel.ImportPluginFromType<>()` automatically registers all functions from each plugin class
3. **Auto Execution**: `ToolCallBehavior.AutoInvokeKernelFunctions` enables automatic function calling
4. **Seamless Experience**: The user gets the same functionality with much less code and better organization

## Comparison with Sample 10

| Aspect | Sample 10 (Manual) | Sample 11 (Auto) |
|--------|-------------------|------------------|
| Function Registration | Manual schema definition | Automatic via attributes |
| Tool Call Handling | Manual parsing and execution | Automatic by Semantic Kernel |
| Code Complexity | ~260 lines | ~140 lines |
| Error Handling | Manual implementation | Built-in by framework |
| Function Discovery | Static dictionary lookup | Reflection-based discovery |

## Usage

```bash
cd samples/11-tool-calling-semantic-kernel
dotnet run
```

## Example Interactions

```
You: What's the current date and time?
🤖 Assistant: The current date and time is 7/27/2025 2:30 PM.

You: What's the weather like in Seattle?
🤖 Assistant: The weather in Seattle is currently sunny with a temperature of 72°F. (Note: This is simulated weather data for demonstration purposes)

You: Tell me the time in ISO format
🤖 Assistant: The current time in ISO format is 2025-07-27T14:30:15.123Z.
```

## Key Benefits of Semantic Kernel Approach

1. **Less Boilerplate**: No need to manually define JSON schemas or parse tool calls
2. **Type Safety**: Function parameters are strongly typed with compile-time checking
3. **Attribute-Based**: Clean, declarative function definitions using C# attributes
4. **Built-in Error Handling**: Framework handles many edge cases automatically
5. **Maintainability**: Easier to add new functions - just add methods with attributes

## Learning Objectives

- Understand how Semantic Kernel simplifies function calling
- Compare manual vs automatic tool calling approaches
- Learn about plugin architecture in Semantic Kernel with multiple plugin classes
- See the power of attribute-based programming for AI functions
- Understand separation of concerns through dedicated plugin classes
