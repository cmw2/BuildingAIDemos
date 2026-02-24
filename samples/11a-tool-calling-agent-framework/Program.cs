using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using DotNetEnv;
using System.ComponentModel;

// Sample 11a: Tool Calling - Microsoft Agent Framework
//
// Demonstrates automatic tool calling using the Microsoft Agent Framework.
// Compare this to Sample 11 which uses Semantic Kernel for the same functionality.
//
// The Agent Framework uses Microsoft.Extensions.AI's AIFunction and AIFunctionFactory
// to define tools, and automatically handles:
// - Tool discovery and registration via AIFunctionFactory.Create()
// - Automatic invocation of tools when the model requests them
// - Passing tool results back to the model for final response generation
//
// Tools are defined as simple static methods with [Description] attributes,
// then wrapped with AIFunctionFactory.Create() — no plugin classes or [KernelFunction]
// attributes needed.

// Load environment variables
Env.Load("../../.env");

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!;
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")!;

Console.WriteLine("🛠️  Tool Calling - Agent Framework Implementation");
Console.WriteLine("Ask questions about the current date, time, or weather!\n");

// Create an Azure OpenAI client
var client = new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential());
var chatClient = client.GetChatClient(deploymentName);

// Create tools from static methods using AIFunctionFactory
// This is the Agent Framework equivalent of Semantic Kernel's [KernelFunction] plugins
var tools = new List<AITool>
{
    AIFunctionFactory.Create(TimeTools.GetCurrentDateTime),
    AIFunctionFactory.Create(WeatherTools.GetWeather)
};

// Configure chat options with instructions, model parameters, and tools
var chatOptions = new ChatOptions
{
    Instructions = """
        You are a helpful AI assistant with access to real-time tools for date/time and weather information.
        
        Use the available tools to provide accurate, up-to-date information when users ask questions about:
        - Current date and time (in various formats)
        - Weather conditions for any location
        
        Always be helpful, accurate, and conversational in your responses.
        If the user is asking about time in a particular location, 
        get the current time in iso format and do any needed timezone adjustments 
        showing them the time local to their specified location.
        """,
    Temperature = 0.7f,
    MaxOutputTokens = 800,
    Tools = tools
};

// Build the agent — the Agent Framework automatically wraps the chat client
// with a FunctionInvokingChatClient that handles the tool call loop for you
AIAgent agent = chatClient.AsAIAgent(
    new ChatClientAgentOptions
    {
        Name = "ToolCallingAgent",
        ChatOptions = chatOptions
    });

Console.WriteLine("💬 Examples:");
Console.WriteLine("- What's the current date and time?");
Console.WriteLine("- What's the weather like in Seattle?");
Console.WriteLine("- Tell me the time in ISO format");
Console.WriteLine("- What's the weather in Paris in celsius?\n");

// Create a session for multi-turn conversation (maintains chat history)
var session = await agent.CreateSessionAsync();

// Chat loop
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
        // Run the agent — it automatically handles:
        // 1. Sending the message to the model
        // 2. Detecting tool call requests from the model
        // 3. Invoking the tool functions locally
        // 4. Sending tool results back to the model
        // 5. Returning the final response
        var response = await agent.RunAsync(userInput, session);

        Console.WriteLine($"🤖 Assistant: {response}\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error: {ex.Message}\n");
    }
}

// --- Tool Definitions ---
// Tools are plain static methods with [Description] attributes.
// AIFunctionFactory.Create() uses reflection to discover parameter names,
// types, and descriptions — similar to how Semantic Kernel discovers
// [KernelFunction] methods, but without requiring a plugin class hierarchy.

/// <summary>
/// Time and date tool functions.
/// </summary>
public static class TimeTools
{
    [Description("Get the current date and time")]
    public static string GetCurrentDateTime(
        [Description("The format for the date/time (e.g., 'short', 'long', 'iso')")]
        string format = "short")
    {
        Console.WriteLine($"🕐 [DEBUG] TimeTools.GetCurrentDateTime called with format: '{format}'");

        try
        {
            var result = format.ToLower() switch
            {
                "short" => DateTime.Now.ToString("M/d/yyyy h:mm tt"),
                "long" => DateTime.Now.ToString("dddd, MMMM dd, yyyy 'at' h:mm:ss tt"),
                "iso" => DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                _ => DateTime.Now.ToString("M/d/yyyy h:mm tt")
            };

            Console.WriteLine($"🕐 [DEBUG] TimeTools returning: '{result}'");
            return result;
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error getting date/time: {ex.Message}";
            Console.WriteLine($"🕐 [DEBUG] TimeTools error: {errorMsg}");
            return errorMsg;
        }
    }
}

/// <summary>
/// Weather-related tool functions.
/// </summary>
public static class WeatherTools
{
    [Description("Get the current weather for a specified location")]
    public static string GetWeather(
        [Description("The city and state, e.g. San Francisco, CA")]
        string location,
        [Description("The temperature unit (celsius or fahrenheit)")]
        string unit = "fahrenheit")
    {
        Console.WriteLine($"🌤️  [DEBUG] WeatherTools.GetWeather called with location: '{location}', unit: '{unit}'");

        try
        {
            // Simulate weather data (in a real app, you'd call a weather API)
            var random = new Random();
            var conditions = new[] { "sunny", "cloudy", "partly cloudy", "rainy", "foggy" };
            var condition = conditions[random.Next(conditions.Length)];

            var temperature = unit?.ToLower() == "celsius"
                ? random.Next(-10, 35)   // Celsius range
                : random.Next(15, 95);   // Fahrenheit range

            var tempUnit = unit?.ToLower() == "celsius" ? "°C" : "°F";

            var result = $"The weather in {location} is currently {condition} with a temperature of {temperature}{tempUnit}. " +
                   $"(Note: This is simulated weather data for demonstration purposes)";

            Console.WriteLine($"🌤️  [DEBUG] WeatherTools returning: '{result}'");
            return result;
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error getting weather: {ex.Message}";
            Console.WriteLine($"🌤️  [DEBUG] WeatherTools error: {errorMsg}");
            return errorMsg;
        }
    }
}
