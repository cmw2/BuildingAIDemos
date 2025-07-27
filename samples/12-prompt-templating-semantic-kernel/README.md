# Sample 12: Prompt Templating - Semantic Kernel

This sample demonstrates advanced prompt templating capabilities with Semantic Kernel, showing how to create dynamic, data-driven system prompts that adapt to real-time information.

## Key Features

- **Dynamic Template Loading**: Load prompt templates from external files
- **Variable Substitution**: Use template variables like `{{$currentTime}}` and `{{$city}}`
- **Real-time Data Injection**: Inject current time and location data into prompts
- **Template Factory Pattern**: Use `KernelPromptTemplateFactory` for professional template management
- **Combined with Tool Calling**: Demonstrates how templating works alongside automatic function calling

## How It Works

1. **Template Loading**: System prompt is loaded from `system-prompt.txt` file
2. **Variable Definition**: Template uses placeholder variables in `{{$variableName}}` format
3. **Data Gathering**: Real-time data is collected (current time, default location)
4. **Template Rendering**: `promptTemplate.RenderAsync()` substitutes variables with actual values
5. **Dynamic System Prompt**: The rendered prompt becomes the system message for the AI

## Template Variables Used

- `{{$currentTime}}`: Injected with current date/time in long format
- `{{$city}}`: Set to "Lancaster, PA" as the default location

## Prompt Template Example

```
You are a helpful AI assistant with access to real-time information.

Current local time: {{$currentTime}}
Local city: {{$city}}

Use the available tools to provide accurate, up-to-date information when users ask questions about time, dates, or weather. Always be helpful and conversational.

When providing weather information, if no location is specified, assume they mean {{$city}}.
```

## Benefits of Prompt Templating

1. **Maintainability**: Prompts are separated from code and can be modified without recompilation
2. **Dynamic Content**: Real-time data keeps the AI context current and relevant
3. **Reusability**: Templates can be reused across different scenarios with different data
4. **Localization**: Easy to create location or time-specific variations
5. **A/B Testing**: Simple to test different prompt variations

## Usage

```bash
cd samples/12-prompt-templating-semantic-kernel
dotnet run
```

## Example Interactions

The AI will have access to real-time data through the template:

```
📄 Rendered system prompt with real-time data
🕐 Current time captured: Sunday, July 27, 2025 at 2:30:15 PM
🏙️  Local city set to: Lancaster, PA

You: What time is it?
🤖 Assistant: The current time is Sunday, July 27, 2025 at 2:30:15 PM.

You: What's the weather like?
🤖 Assistant: The weather in Lancaster, PA is currently sunny with a temperature of 78°F.
```

## Learning Objectives

- Understand how to load and manage external prompt templates
- Learn template variable syntax and substitution
- See how to inject real-time data into AI system prompts
- Observe the combination of templating with tool calling
- Appreciate the separation of prompts from application logic
