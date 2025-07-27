using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using DotNetEnv;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

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

// Configure comprehensive OpenTelemetry to see all activity
var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService("prompt-templating-semantic-kernel-sample");

// Enable model diagnostics with sensitive data.
AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive", true);

using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .SetResourceBuilder(resourceBuilder)
    .AddSource("Microsoft.SemanticKernel*")
    .AddConsoleExporter()
    .Build();

// using var meterProvider = Sdk.CreateMeterProviderBuilder()
//     .SetResourceBuilder(resourceBuilder)
//     .AddMeter("Microsoft.SemanticKernel*")
//     .AddConsoleExporter()
//     .Build();

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddOpenTelemetry(options =>
    {
        options.SetResourceBuilder(resourceBuilder);
        options.AddConsoleExporter();
        options.IncludeFormattedMessage = true;
        options.IncludeScopes = true;
    });
    builder.SetMinimumLevel(LogLevel.Information);
});

Console.WriteLine("🔍 Full OpenTelemetry enabled - you'll see all telemetry data\n");

// Create Semantic Kernel with Azure OpenAI
var builder = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(
        deploymentName: deploymentName,
        endpoint: endpoint,
        apiKey: apiKey);

// Add logging to see function calls
builder.Services.AddSingleton(loggerFactory);

// IMPORTANT: Import plugins BEFORE building the kernel!
// Once kernel.Build() is called, the kernel becomes immutable.
// Plugins added after Build() won't be available to the kernel.
builder.Plugins.AddFromType<TimePlugin>("Time");
builder.Plugins.AddFromType<WeatherPlugin>("Weather");

var kernel = builder.Build();

// Add prompt-based functions directly to the kernel
// These are callable functions that use prompt templates for text processing
kernel.Plugins.Add(KernelPluginFactory.CreateFromFunctions("TextProcessing", new[]
{
    kernel.CreateFunctionFromPrompt(
        new PromptTemplateConfig
        {
            Name = "analyze_sentiment",
            Description = "Analyze the emotional sentiment and tone of text",
            Template = """
                Analyze the sentiment and emotional tone of the following text.
                
                Text to analyze: {{$text}}
                
                Provide your analysis in this format:
                - Overall Sentiment: [Positive/Negative/Neutral]
                - Confidence: [High/Medium/Low]
                - Key Emotions: [list main emotions detected]
                - Tone: [describe the overall tone]
                - Brief Explanation: [1-2 sentences explaining your analysis]
                """,
            InputVariables = new List<InputVariable>
            {
                new InputVariable { Name = "text", Description = "The text to analyze for sentiment", IsRequired = true }
            }
        }),

    kernel.CreateFunctionFromPrompt(
        new PromptTemplateConfig
        {
            Name = "format_as_email",
            Description = "Convert informal text into a professional email format",
            Template = """
                Convert the following informal text into a professional email format.
                
                Informal text: {{$informalText}}
                Subject: {{$subject}}
                
                Create a well-structured professional email with:
                - Appropriate greeting
                - Clear subject line (use provided subject or create one if empty)
                - Professional tone and language
                - Proper closing
                - Maintain the core message while improving professionalism
                
                Format as a complete email ready to send.
                """,
            InputVariables = new List<InputVariable>
            {
                new InputVariable { Name = "informalText", Description = "The informal text to convert into a professional email", IsRequired = true },
                new InputVariable { Name = "subject", Description = "The subject line for the email (optional)", IsRequired = false }
            }
        }),

    kernel.CreateFunctionFromPrompt(
        new PromptTemplateConfig
        {
            Name = "create_summary",
            Description = "Create a concise summary of long text with bullet points",
            Template = """
                Create a concise summary of the following text using bullet points.
                
                Text to summarize: {{$text}}
                Maximum bullet points: {{$maxPoints}}
                
                Requirements:
                - Use clear, concise bullet points
                - Capture the most important information
                - Keep each point to 1-2 sentences
                - Prioritize key facts, decisions, or insights
                - Use action-oriented language where appropriate
                
                Format as a clean bullet list.
                """,
            InputVariables = new List<InputVariable>
            {
                new InputVariable { Name = "text", Description = "The text to summarize", IsRequired = true },
                new InputVariable { Name = "maxPoints", Description = "The maximum number of bullet points (default: 5)", IsRequired = false }
            }
        }),

    kernel.CreateFunctionFromPrompt(
        new PromptTemplateConfig
        {
            Name = "translate_tone",
            Description = "Rewrite text to match a specific tone while preserving the message",
            Template = """
                Rewrite the following text to match the specified tone while preserving the core message and meaning.
                
                Original text: {{$originalText}}
                Target tone: {{$targetTone}}
                
                Guidelines:
                - Maintain the original meaning and key information
                - Adjust vocabulary, sentence structure, and style to match the target tone
                - Ensure the rewritten text feels natural and authentic
                - Keep the same approximate length unless the tone requires significant changes
                
                Provide only the rewritten text.
                """,
            InputVariables = new List<InputVariable>
            {
                new InputVariable { Name = "originalText", Description = "The original text to rewrite", IsRequired = true },
                new InputVariable { Name = "targetTone", Description = "The target tone (e.g., formal, casual, friendly, urgent, apologetic)", IsRequired = true }
            }
        })
}));

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
Console.WriteLine("- Analyze the sentiment of this text: 'I love sunny days!'");
Console.WriteLine("- Format this as a professional email: 'hey, can we meet tomorrow?'");
Console.WriteLine("- Summarize this text: [paste any long text]");
Console.WriteLine("- Tell me the time in ISO format\n");

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
