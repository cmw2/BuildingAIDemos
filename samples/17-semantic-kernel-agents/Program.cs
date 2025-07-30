using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel.ChatCompletion;
using Azure.Identity;
using Azure.Core;
using Azure.AI.Agents.Persistent;
using DotNetEnv;

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates

// Sample 17: Semantic Kernel AzureAIAgent with Tool Calling
//
// Demonstrates Semantic Kernel AzureAIAgent with:
// - Local and UTC time functions
// - Weather forecast function with date parsing
// - Native function plugins
// - Automatic tool calling with proper date handling

// Load environment variables
Env.Load("../../.env");

// Get configuration from environment
var endpoint = Environment.GetEnvironmentVariable("AI_FOUNDRY_PROJECT_CONNECTION_STRING")!;
var modelDeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")!;

Console.WriteLine("🤖 Semantic Kernel AzureAIAgent with Tool Calling\n");

try
{
    // Create Azure AI Agents client
    var credential = new DefaultAzureCredential();
        
    var agentsClient = AzureAIAgent.CreateAgentsClient(endpoint, credential);

    // Create agent definition on Azure AI Agent Service
    var definition = await agentsClient.Administration.CreateAgentAsync(
        model: modelDeploymentName,
        name: "Weather Analysis Assistant",
        description: "A helpful weather assistant with time and weather analysis capabilities",
        instructions: @"You are a helpful weather assistant with access to:

1. Time functions: GetCurrentLocalDateTime (local time), GetUTCDateTime (UTC time)
2. Weather functions: GetWeather (requires YYYY-MM-DD format dates)
3. Code interpreter: Available for calculations and text visualizations

IMPORTANT WORKFLOW for date/time requests:
1. FIRST call appropriate time function to get current date/time
2. Use calculations to determine relative dates (""tomorrow"" = current date + 1 day)
3. ONLY THEN call GetWeather with calculated YYYY-MM-DD format date
4. Never pass ""today"", ""tomorrow"", etc. directly to GetWeather
5. If user doesn't specify temperature unit, use the one most likely for that location

Be conversational and explain your tool usage. Use text-based visualizations for terminal environments.",
        tools: [new CodeInterpreterToolDefinition()]);

    // Create Semantic Kernel agent
    var agent = new AzureAIAgent(definition, agentsClient);

    // Create plugins for tool functions and add to agent's kernel
    var timePlugin = KernelPluginFactory.CreateFromType<TimeTools>("TimeTools");
    var weatherPlugin = KernelPluginFactory.CreateFromType<WeatherTools>("WeatherTools");
    
    agent.Kernel.Plugins.Add(timePlugin);
    agent.Kernel.Plugins.Add(weatherPlugin);

    Console.WriteLine($"Agent created: {agent.Name}");

    // Cleanup on Ctrl+C
    Console.CancelKeyPress += async (sender, e) =>
    {
        e.Cancel = true;
        await agentsClient.Administration.DeleteAgentAsync(agent.Id);
        Environment.Exit(0);
    };

    // Create conversation thread
    var agentThread = new AzureAIAgentThread(agentsClient);
    
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
            var message = new ChatMessageContent(AuthorRole.User, input);
            await foreach (StreamingChatMessageContent response in agent.InvokeStreamingAsync(message, agentThread))
            {
                Console.Write(response.Content);
            }
            Console.WriteLine("\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}\n");
        }
    }
    
    // Cleanup
    await agentThread.DeleteAsync();
    await agentsClient.Administration.DeleteAgentAsync(agent.Id);
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

/// <summary>
/// Time-related tools for Semantic Kernel
/// </summary>
public class TimeTools
{
    [KernelFunction, Description("Get the current local date and time in various formats. All times returned are in the system's local timezone.")]
    public string GetCurrentLocalDateTime(
        [Description("The format for the local date/time (e.g., 'short', 'long', 'iso')")]
        string format)
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
    }

    [KernelFunction, Description("Get the current UTC date and time in various formats. All times returned are in UTC timezone.")]
    public string GetUTCDateTime(
        [Description("The format for the UTC date/time (e.g., 'short', 'long', 'iso')")]
        string format)
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
    }
}

/// <summary>
/// Weather-related tools for Semantic Kernel
/// </summary>
public class WeatherTools
{
    [KernelFunction, Description("Get weather information or forecast for a specified location and date. REQUIRES exact date in YYYY-MM-DD format. Use time functions to calculate dates first.")]
    public string GetWeather(
        [Description("The city and state/country, e.g. 'Seattle, WA' or 'London, UK'")]
        string location,
        [Description("Temperature unit preference: 'celsius' or 'fahrenheit'")]
        string unit,
        [Description("The date for weather information in YYYY-MM-DD format ONLY. Do NOT use relative terms like 'today', 'tomorrow'.")]
        string date)
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
    }
}
