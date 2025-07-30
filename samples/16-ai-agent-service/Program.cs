using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.AI.Projects;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Azure.Core;
using DotNetEnv;
using System.Text.Json;

// Sample 16: Azure AI Agent Service with Tool Calling
//
// Demonstrates Azure AI Agents with:
// - Local and UTC time functions
// - Weather forecast function with date parsing
// - Code interpreter for calculations
// - Automatic tool calling with proper date handling

// Load environment variables
Env.Load("../../.env");

// Get configuration from environment
var projectEndpoint = Environment.GetEnvironmentVariable("AI_FOUNDRY_PROJECT_CONNECTION_STRING")!;
var modelDeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")!;

Console.WriteLine("🤖 Azure AI Agent Service with Tool Calling\n");

try
{
    // Set up authentication and clients
    var credential = new DefaultAzureCredential();
    var projectClient = new AIProjectClient(new Uri(projectEndpoint), credential);
    var agentsClient = new PersistentAgentsClient(projectEndpoint, credential);
    
    // Create tools and agent
    var toolFunctions = CreateToolFunctions();
    var localTimeTool = CreateLocalTimeToolDefinition();
    var utcTimeTool = CreateUTCTimeToolDefinition();
    var weatherTool = CreateWeatherToolDefinition();
    var codeInterpreterTool = new CodeInterpreterToolDefinition();
    
    var agent = CreateAgentWithTools(agentsClient, modelDeploymentName, localTimeTool, utcTimeTool, weatherTool, codeInterpreterTool);
    Console.WriteLine($"Agent created: {agent.Name}");
    
    // Cleanup on Ctrl+C
    Console.CancelKeyPress += async (sender, e) =>
    {
        e.Cancel = true;
        await agentsClient.Administration.DeleteAgentAsync(agent.Id);
        Environment.Exit(0);
    };
    
    // Create conversation thread
    var thread = agentsClient.Threads.CreateThread().Value;
    
    Console.WriteLine("Ask about time, weather, or data analysis. Type 'quit' to exit.\n");
    
    // Interactive loop
    while (true)
    {
        Console.Write("You: ");
        var input = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(input) || input.ToLower() is "quit" or "exit" or "q")
            break;

        try
        {
            agentsClient.Messages.CreateMessage(thread.Id, MessageRole.User, input);
            
            // Setup automatic function calling
            var toolDelegates = new Dictionary<string, Delegate>
            {
                ["GetCurrentLocalDateTime"] = toolFunctions["GetCurrentLocalDateTime"],
                ["GetUTCDateTime"] = toolFunctions["GetUTCDateTime"],
                ["GetWeather"] = toolFunctions["GetWeather"]
            };
            
            var runOptions = new CreateRunStreamingOptions
            {
                AutoFunctionCallOptions = new AutoFunctionCallOptions(toolDelegates, maxRetry: 10)
            };
            
            await foreach (var update in agentsClient.Runs.CreateRunStreamingAsync(thread.Id, agent.Id, options: runOptions))
            {
                if (update is MessageContentUpdate content)
                    Console.Write(content.Text);
            }
            Console.WriteLine("\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}\n");
        }
    }
    
    // Cleanup
    await agentsClient.Administration.DeleteAgentAsync(agent.Id);
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

/// <summary>
/// Creates the tool function delegates for automatic calling
/// </summary>
static Dictionary<string, Delegate> CreateToolFunctions()
{
    var functions = new Dictionary<string, Delegate>();
    
    // Local time function
    functions["GetCurrentLocalDateTime"] = new Func<string, string>((string format) =>
    {
        Console.WriteLine($"[TOOL] GetCurrentLocalDateTime(format: '{format}')");
        var result = format.ToLower() switch
        {
            "short" => DateTime.Now.ToString("M/d/yyyy h:mm tt"),
            "long" => DateTime.Now.ToString("dddd, MMMM dd, yyyy 'at' h:mm:ss tt"),
            "iso" => DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff"),
            _ => DateTime.Now.ToString("M/d/yyyy h:mm tt")
        };
        return result;
    });
    
    // UTC time function
    functions["GetUTCDateTime"] = new Func<string, string>((string format) =>
    {
        Console.WriteLine($"[TOOL] GetUTCDateTime(format: '{format}')");
        var result = format.ToLower() switch
        {
            "short" => DateTime.UtcNow.ToString("M/d/yyyy H:mm"),
            "long" => DateTime.UtcNow.ToString("dddd, MMMM dd, yyyy 'at' H:mm:ss"),
            "iso" => DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            _ => DateTime.UtcNow.ToString("M/d/yyyy H:mm")
        };
        return result;
    });
    
    // Weather function  
    functions["GetWeather"] = new Func<string, string, string, string>((string location, string unit, string date) =>
    {
        Console.WriteLine($"[TOOL] GetWeather(location: '{location}', unit: '{unit}', date: '{date}')");
        // Parse date and determine context
        DateTime targetDate = DateTime.TryParse(date, out var parsed) ? parsed : DateTime.Today;
        var today = DateTime.Today;
        
        string timeContext = targetDate.Date == today ? "current" 
                           : targetDate.Date < today ? "historical" 
                           : "forecast";
        
        // Generate consistent weather data
        var random = new Random(location.GetHashCode() + targetDate.DayOfYear);
        var conditions = new[] { "sunny", "cloudy", "partly cloudy", "rainy", "foggy", "snowy", "thunderstorms", "clear" };
        var condition = conditions[random.Next(conditions.Length)];
        
        var temperature = unit?.ToLower() == "celsius" 
            ? random.Next(-10, 35) : random.Next(15, 95);
        var tempUnit = unit?.ToLower() == "celsius" ? "°C" : "°F";
        var humidity = random.Next(30, 90);
        var windSpeed = random.Next(5, 25);
        
        // Seasonal adjustments
        var month = targetDate.Month;
        if (timeContext == "forecast" && (month == 12 || month == 1 || month == 2))
        {
            temperature = unit?.ToLower() == "celsius" 
                ? Math.Max(temperature - 15, -20) 
                : Math.Max(temperature - 25, 10);
            if (random.Next(3) == 0) condition = "snowy";
        }
        else if (timeContext == "forecast" && (month >= 6 && month <= 8))
        {
            temperature = unit?.ToLower() == "celsius" 
                ? Math.Min(temperature + 10, 40) 
                : Math.Min(temperature + 15, 100);
        }

        var dateDisplay = targetDate.ToString("dddd, MMMM dd, yyyy");
        return $"{timeContext.ToUpper()} weather for {location} on {dateDisplay}: " +
               $"{condition}, {temperature}{tempUnit}, {humidity}% humidity, wind {windSpeed} mph";
    });
    
    return functions;
}

static FunctionToolDefinition CreateLocalTimeToolDefinition()
{
    var parameters = BinaryData.FromObjectAsJson(new
    {
        type = "object",
        properties = new
        {
            format = new
            {
                type = "string",
                description = "The format for the local date/time (e.g., 'short', 'long', 'iso')",
                @enum = new[] { "short", "long", "iso" }
            }
        },
        required = new[] { "format" }
    });

    return new FunctionToolDefinition(
        name: "GetCurrentLocalDateTime",
        description: "Get the current local date and time in various formats. All times returned are in the system's local timezone.",
        parameters: parameters);
}

static FunctionToolDefinition CreateUTCTimeToolDefinition()
{
    var parameters = BinaryData.FromObjectAsJson(new
    {
        type = "object",
        properties = new
        {
            format = new
            {
                type = "string",
                description = "The format for the UTC date/time (e.g., 'short', 'long', 'iso')",
                @enum = new[] { "short", "long", "iso" }
            }
        },
        required = new[] { "format" }
    });

    return new FunctionToolDefinition(
        name: "GetUTCDateTime",
        description: "Get the current UTC date and time in various formats. All times returned are in UTC timezone.",
        parameters: parameters);
}

static FunctionToolDefinition CreateWeatherToolDefinition()
{
    var parameters = BinaryData.FromObjectAsJson(new
    {
        type = "object",
        properties = new
        {
            location = new
            {
                type = "string",
                description = "The city and state/country, e.g. 'Seattle, WA' or 'London, UK'"
            },
            unit = new
            {
                type = "string",
                description = "Temperature unit preference",
                @enum = new[] { "celsius", "fahrenheit" }
            },
            date = new
            {
                type = "string",
                description = "The date for weather information in YYYY-MM-DD format ONLY. Do NOT use relative terms like 'today', 'tomorrow'. Use code interpreter to calculate actual dates first."
            }
        },
        required = new[] { "location", "unit", "date" }
    });

    return new FunctionToolDefinition(
        name: "GetWeather",
        description: "Get weather information or forecast for a specified location and date. REQUIRES exact date in YYYY-MM-DD format. Use time functions and code interpreter to calculate dates first.",
        parameters: parameters);
}

static PersistentAgent CreateAgentWithTools(
    PersistentAgentsClient client,
    string modelName,
    params ToolDefinition[] tools)
{
    return client.Administration.CreateAgent(
        model: modelName,
        name: "Weather Analysis Assistant",
        instructions: @"You are a helpful weather assistant with access to:

1. Time functions: GetCurrentLocalDateTime (local time), GetUTCDateTime (UTC time)
2. Weather functions: GetWeather (requires YYYY-MM-DD format dates)
3. Code interpreter: For calculations and text visualizations

IMPORTANT WORKFLOW for date/time requests:
1. FIRST call appropriate time function to get current date/time
2. Use code interpreter to calculate relative dates (""tomorrow"" = current date + 1 day)
3. ONLY THEN call GetWeather with calculated YYYY-MM-DD format date
4. Never pass ""today"", ""tomorrow"", etc. directly to GetWeather
5. If user doesn't specify temperature unit, use the one most likely for that location

Be conversational and explain your tool usage. Use text-based visualizations for terminal environments.",
        tools: tools).Value;
}
