using System;
using System.Text.Json;
using System.Collections.Generic;
using Azure.AI.OpenAI;
using Azure.Identity;
using OpenAI.Chat;
using DotNetEnv;

// Sample 10: Tool Calling - Manual Implementation
//
// Demonstrates how to implement function calling manually using the OpenAI SDK.
// Shows the "hard way" of handling tool calls by writing custom code to:
// - Define function schemas
// - Parse tool call responses
// - Execute local functions
// - Send function results back to the model

// Load environment variables
Env.Load("../../.env");

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!;
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")!;

Console.WriteLine("🛠️  Tool Calling - Manual Implementation");
Console.WriteLine("Ask questions about the current date, time, or weather!\n");

// Create Azure OpenAI client
var credential = new DefaultAzureCredential();
var client = new AzureOpenAIClient(new Uri(endpoint), credential);
var chatClient = client.GetChatClient(deploymentName);

// Define available functions
var availableFunctions = new Dictionary<string, Func<Dictionary<string, object>, string>>
{
    ["get_current_datetime"] = args => GetCurrentDateTime(args?.GetValueOrDefault("format")?.ToString() ?? "short"),
    ["get_weather"] = args => GetWeather(
        args?.GetValueOrDefault("location")?.ToString() ?? "Unknown",
        args?.GetValueOrDefault("unit")?.ToString()?.ToLower() ?? "fahrenheit")
};

// Define function schemas for the model
var functionSchemas = new List<ChatTool>
{
    ChatTool.CreateFunctionTool(
        functionName: "get_current_datetime",
        functionDescription: "Get the current date and time",
        functionParameters: BinaryData.FromString("""
        {
            "type": "object",
            "properties": {
                "format": {
                    "type": "string",
                    "description": "The format for the date/time (e.g., 'short', 'long', 'iso')",
                    "enum": ["short", "long", "iso"]
                }
            },
            "required": []
        }
        """)
    ),
    ChatTool.CreateFunctionTool(
        functionName: "get_weather",
        functionDescription: "Get the current weather for a specified location",
        functionParameters: BinaryData.FromString("""
        {
            "type": "object",
            "properties": {
                "location": {
                    "type": "string",
                    "description": "The city and state, e.g. San Francisco, CA"
                },
                "unit": {
                    "type": "string",
                    "description": "The temperature unit",
                    "enum": ["celsius", "fahrenheit"]
                }
            },
            "required": ["location"]
        }
        """)
    )
};

// Interactive loop
Console.WriteLine("💬 Examples:");
Console.WriteLine("- What's the current date and time?");
Console.WriteLine("- What's the weather like in Seattle?");
Console.WriteLine("- Tell me the time in ISO format");
Console.WriteLine("- What's the weather in Paris in celsius?\n");

// Initialize conversation history
var messages = new List<ChatMessage>
{
    new SystemChatMessage("You are a helpful assistant that can provide information about the current date/time and weather. Use the available functions when needed.")
};

while (true)
{
    Console.Write("You: ");
    var userInput = Console.ReadLine()?.Trim();
    
    if (string.IsNullOrEmpty(userInput) || 
        userInput.Equals("quit", StringComparison.OrdinalIgnoreCase) ||
        userInput.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
        userInput.Equals("q", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("👋 Goodbye!");
        break;
    }

    try
    {
        await ProcessUserMessage(chatClient, userInput, functionSchemas, availableFunctions, messages);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error: {ex.Message}");
    }
    
    Console.WriteLine();
}

static async Task ProcessUserMessage(
    ChatClient chatClient,
    string userMessage,
    List<ChatTool> functionSchemas,
    Dictionary<string, Func<Dictionary<string, object>, string>> availableFunctions,
    List<ChatMessage> messages)
{
    // Add the current user message to the conversation history
    messages.Add(new UserChatMessage(userMessage));

    // Initial request to the model with function definitions
    var chatOptions = new ChatCompletionOptions();
    foreach (var tool in functionSchemas)
    {
        chatOptions.Tools.Add(tool);
    }

    var response = await chatClient.CompleteChatAsync(messages, chatOptions);

    // Keep processing until the model finishes normally (not with tool calls)
    while (response.Value.FinishReason == ChatFinishReason.ToolCalls)
    {
        Console.WriteLine("🔧 AI is calling functions...");
        
        // Add the assistant's response (with tool calls) to the conversation
        messages.Add(new AssistantChatMessage(response.Value));

        // Process each tool call
        foreach (var toolCall in response.Value.ToolCalls)
        {
            if (toolCall is ChatToolCall functionToolCall)
            {
                var functionName = functionToolCall.FunctionName;
                var functionArgs = functionToolCall.FunctionArguments;

                Console.WriteLine($"  📞 Calling: {functionName}({functionArgs})");

                if (availableFunctions.TryGetValue(functionName, out var function))
                {
                    // Parse function arguments from JSON
                    var args = JsonSerializer.Deserialize<Dictionary<string, object>>(functionArgs) ?? new Dictionary<string, object>();
                    
                    // Execute the function with parsed arguments
                    var functionResult = function(args);
                    
                    Console.WriteLine($"  📋 Result: {functionResult}");

                    // Add the function result to the conversation
                    messages.Add(new ToolChatMessage(functionToolCall.Id, functionResult));
                }
                else
                {
                    // Function not found
                    var errorMessage = $"Function '{functionName}' is not available.";
                    messages.Add(new ToolChatMessage(functionToolCall.Id, errorMessage));
                }
            }
        }

        // Get the next response from the model (might include more function calls)
        response = await chatClient.CompleteChatAsync(messages, chatOptions);
    }

    // Handle different finish reasons
    switch (response.Value.FinishReason)
    {
        case ChatFinishReason.Stop:
            // Normal completion - display the response
            var finalResponseText = response.Value.Content.Count > 0 ? response.Value.Content[0].Text : "I'm not sure how to help with that.";
            Console.WriteLine($"🤖 Assistant: {finalResponseText}");
            break;
            
        case ChatFinishReason.Length:
            // Response was truncated due to length
            var truncatedText = response.Value.Content.Count > 0 ? response.Value.Content[0].Text : "";
            Console.WriteLine($"🤖 Assistant: {truncatedText}");
            Console.WriteLine("⚠️  (Response was truncated due to length limit)");
            break;
            
        case ChatFinishReason.ContentFilter:
            Console.WriteLine("🚫 Response was filtered due to content policy.");
            break;
            
        default:
            Console.WriteLine($"🤔 Unexpected finish reason: {response.Value.FinishReason}");
            var defaultText = response.Value.Content.Count > 0 ? response.Value.Content[0].Text : "No response available.";
            Console.WriteLine($"🤖 Assistant: {defaultText}");
            break;
    }
    
    // Add the assistant's final response to the conversation history
    messages.Add(new AssistantChatMessage(response.Value));
}

static string GetCurrentDateTime(string format = "short")
{
    try
    {
        return format.ToLower() switch
        {
            "short" => DateTime.Now.ToString("M/d/yyyy h:mm tt"),
            "long" => DateTime.Now.ToString("dddd, MMMM dd, yyyy 'at' h:mm:ss tt"),
            "iso" => DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            _ => DateTime.Now.ToString("M/d/yyyy h:mm tt")
        };
    }
    catch (Exception ex)
    {
        return $"Error getting date/time: {ex.Message}";
    }
}

static string GetWeather(string location, string unit = "fahrenheit")
{
    try
    {
        // Simulate weather data (in a real app, you'd call a weather API)
        var random = new Random();
        var conditions = new[] { "sunny", "cloudy", "partly cloudy", "rainy", "foggy" };
        var condition = conditions[random.Next(conditions.Length)];
        
        var temperature = unit == "celsius" 
            ? random.Next(-10, 35)  // Celsius range
            : random.Next(15, 95);  // Fahrenheit range
        
        var tempUnit = unit == "celsius" ? "°C" : "°F";

        return $"The weather in {location} is currently {condition} with a temperature of {temperature}{tempUnit}. " +
               $"(Note: This is simulated weather data for demonstration purposes)";
    }
    catch (Exception ex)
    {
        return $"Error getting weather: {ex.Message}";
    }
}
