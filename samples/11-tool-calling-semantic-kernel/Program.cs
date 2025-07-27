using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using DotNetEnv;

// Sample 11: Tool Calling - Semantic Kernel Auto Implementation
//
// Demonstrates how to implement tool calling automatically using Semantic Kernel.
// Shows the "easy way" of handling tool calls by letting Semantic Kernel:
// - Automatically discover and register functions via attributes
// - Handle tool call parsing and execution
// - Manage conversation flow with functions
// - Organize functions into separate plugin classes for better separation of concerns
//
// Compare this to Sample 10 which shows the manual implementation approach.

// Load environment variables
Env.Load("../../.env");

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!;
var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")!;
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")!;

Console.WriteLine("🛠️  Tool Calling - Semantic Kernel Auto Implementation");
Console.WriteLine("Ask questions about the current date, time, or weather!\n");

// Create Semantic Kernel with Azure OpenAI
var builder = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(
        deploymentName: deploymentName,
        endpoint: endpoint,
        apiKey: apiKey);

// IMPORTANT: Import plugins BEFORE building the kernel!
// Once kernel.Build() is called, the kernel becomes immutable.
// Plugins added after Build() won't be available to the kernel.
builder.Plugins.AddFromType<TimePlugin>("Time");
builder.Plugins.AddFromType<WeatherPlugin>("Weather");

var kernel = builder.Build();

// Get chat completion service
var chatService = kernel.GetRequiredService<IChatCompletionService>();

// Configure execution settings to enable automatic tool calling
var executionSettings = new OpenAIPromptExecutionSettings 
{ 
    Temperature = 0.7f, 
    MaxTokens = 800,
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions // This enables auto tool calling!
};

// Interactive loop
Console.WriteLine("💬 Examples:");
Console.WriteLine("- What's the current date and time?");
Console.WriteLine("- What's the weather like in Seattle?");
Console.WriteLine("- Tell me the time in ISO format");
Console.WriteLine("- What's the weather in Paris in celsius?\n");

// Initialize conversation history
var chatHistory = new ChatHistory("You are a helpful assistant that can provide information about the current date/time and weather. Use the available functions when needed.  Provide answers that are simple and helpful to the user.  If the user asks about time for a particular location get it in ISO format and convert it to the appropriate time zone, as needed.  Always convert back to a simple time format to display to user unless they specifically ask for iso or indicate they want the more complicated form.");

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
        // Add user message to chat history
        chatHistory.AddUserMessage(userInput);

        // Get AI response with automatic tool calling
        // Semantic Kernel will automatically:
        // 1. Detect if functions need to be called
        // 2. Execute the functions (watch for [DEBUG] output!)
        // 3. Send results back to the model
        // 4. Return the final response
        var response = await chatService.GetChatMessageContentAsync(
            chatHistory, 
            executionSettings,
            kernel);

        Console.WriteLine($"🤖 Assistant: {response.Content}");

        // Add AI response to chat history
        chatHistory.AddAssistantMessage(response.Content!);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error: {ex.Message}");
    }
    
    Console.WriteLine();
}

/// <summary>
/// Plugin containing time and date functions.
/// Semantic Kernel will automatically discover and register these functions
/// based on the KernelFunction attributes.
/// </summary>
public class TimePlugin
{
    [KernelFunction("get_current_datetime")]
    [Description("Get the current date and time")]
    public string GetCurrentDateTime(
        [Description("The format for the date/time (e.g., 'short', 'long', 'iso')")]
        string format = "short")
    {
        Console.WriteLine($"🕐 [DEBUG] TimePlugin.GetCurrentDateTime called with format: '{format}'");
        
        try
        {
            var now = DateTime.Now;
            
            var result = format.ToLower() switch
            {
                "short" => now.ToString("M/d/yyyy h:mm tt"),
                "long" => now.ToString("dddd, MMMM dd, yyyy 'at' h:mm:ss tt"),
                "iso" => now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                _ => now.ToString("M/d/yyyy h:mm tt")
            };
            
            Console.WriteLine($"🕐 [DEBUG] TimePlugin returning: '{result}'");
            return result;
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error getting date/time: {ex.Message}";
            Console.WriteLine($"🕐 [DEBUG] TimePlugin error: {errorMsg}");
            return errorMsg;
        }
    }
}

/// <summary>
/// Plugin containing weather-related functions.
/// Semantic Kernel will automatically discover and register these functions
/// based on the KernelFunction attributes.
/// </summary>
public class WeatherPlugin
{
    [KernelFunction("get_weather")]
    [Description("Get the current weather for a specified location")]
    public string GetWeather(
        [Description("The city and state, e.g. San Francisco, CA")]
        string location,
        [Description("The temperature unit (celsius or fahrenheit)")]
        string unit = "fahrenheit")
    {
        Console.WriteLine($"🌤️  [DEBUG] WeatherPlugin.GetWeather called with location: '{location}', unit: '{unit}'");
        
        try
        {
            // Simulate weather data (in a real app, you'd call a weather API)
            var random = new Random();
            var conditions = new[] { "sunny", "cloudy", "partly cloudy", "rainy", "foggy" };
            var condition = conditions[random.Next(conditions.Length)];
            
            var temperature = unit?.ToLower() == "celsius" 
                ? random.Next(-10, 35)  // Celsius range
                : random.Next(15, 95);  // Fahrenheit range
            
            var tempUnit = unit?.ToLower() == "celsius" ? "°C" : "°F";

            var result = $"The weather in {location} is currently {condition} with a temperature of {temperature}{tempUnit}. " +
                   $"(Note: This is simulated weather data for demonstration purposes)";
            
            Console.WriteLine($"🌤️  [DEBUG] WeatherPlugin returning: '{result}'");
            return result;
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error getting weather: {ex.Message}";
            Console.WriteLine($"🌤️  [DEBUG] WeatherPlugin error: {errorMsg}");
            return errorMsg;
        }
    }
}
