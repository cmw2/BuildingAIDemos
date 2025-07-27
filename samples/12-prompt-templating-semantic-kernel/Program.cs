using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using DotNetEnv;

// Sample 12: Prompt Templating - Semantic Kernel
//
// Demonstrates how to use prompt templating with Semantic Kernel.
// Shows how to:
// - Load template files and render them with dynamic data
// - Use template variables and substitution
// - Combine tool calling with dynamic prompt generation
// - Manage real-time data injection into system prompts
//
// This builds on Sample 11's tool calling foundation by adding sophisticated
// prompt templating capabilities for more dynamic AI interactions.

// Load environment variables
Env.Load("../../.env");

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!;
var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")!;
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")!;

Console.WriteLine("📄 Prompt Templating - Semantic Kernel");
Console.WriteLine("Demonstrating dynamic prompt generation with real-time data!\n");

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

// Load and render system prompt using template factory with real-time data
var systemPromptPath = Path.Combine(Directory.GetCurrentDirectory(), "system-prompt.txt");
var systemPromptTemplate = await File.ReadAllTextAsync(systemPromptPath);

var promptTemplateFactory = new KernelPromptTemplateFactory();
var promptTemplate = promptTemplateFactory.Create(new PromptTemplateConfig(systemPromptTemplate));

// Get current time using our TimePlugin for consistency
var timePlugin = new TimePlugin();
var currentTime = timePlugin.GetCurrentDateTime("long");

// Render the template with context variables
var promptArgs = new KernelArguments
{
    ["city"] = "Lancaster, PA",
    ["currentTime"] = currentTime
};

var systemPrompt = await promptTemplate.RenderAsync(kernel, promptArgs);
Console.WriteLine($"📄 Rendered system prompt with real-time data");
Console.WriteLine($"🕐 Current time captured: {currentTime}");
Console.WriteLine($"🏙️  Local city set to: Lancaster, PA");
Console.WriteLine($"📝 System prompt template variables: {{$currentTime}}, {{$city}}\n");

// Initialize conversation history with rendered system prompt
var chatHistory = new ChatHistory(systemPrompt);

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
            var result = format.ToLower() switch
            {
                "short" => DateTime.Now.ToString("M/d/yyyy h:mm tt"),
                "long" => DateTime.Now.ToString("dddd, MMMM dd, yyyy 'at' h:mm:ss tt"),
                "iso" => DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                _ => DateTime.Now.ToString("M/d/yyyy h:mm tt")
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
