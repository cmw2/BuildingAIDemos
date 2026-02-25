using System.ComponentModel;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;

namespace McpWeatherFunctionApp;

/// <summary>
/// MCP Weather Prompts — reusable prompt templates that guide the LLM on how
/// to use the weather tools. These match the same 3 prompts from Sample 19.
///
/// Each [McpServerPrompt] method returns messages that the client can inject
/// into the chat to structure how tools are invoked and how results are presented.
/// </summary>
[McpServerPromptType]
public class WeatherPrompts
{
    [McpServerPrompt(Name = "time_in_location"), Description("Get the current time in a specific city or timezone")]
    public static IEnumerable<ChatMessage> TimeInLocation(
        [Description("The city or region, e.g. 'Tokyo', 'London', 'New York'")] string location)
    {
        return
        [
            new ChatMessage(ChatRole.User,
                $"What time is it right now in {location}?\n\n" +
                "IMPORTANT INSTRUCTIONS FOR ANSWERING:\n" +
                "1. First, call the get_current_datetime tool with timezone set to 'UTC' " +
                "to get the current time in ISO 8601 / UTC format.\n" +
                "2. Then convert from UTC to the correct local timezone for the requested location. " +
                "Apply the appropriate UTC offset (e.g., Tokyo is UTC+9, London is UTC+0/+1, " +
                "New York is UTC-5/-4, etc.). Account for daylight saving time if applicable.\n" +
                "3. Present ONLY the final local time in a friendly, human-readable format like " +
                "'It's 10:28 PM on Monday, February 24th in New York (Eastern Standard Time).' " +
                "Do NOT show the raw UTC time, ISO 8601 strings, or the conversion math. " +
                "Keep it short and conversational — just the answer.")
        ];
    }

    [McpServerPrompt(Name = "daily_briefing"), Description("Get a morning briefing with current time, weather, and forecast for a location")]
    public static IEnumerable<ChatMessage> DailyBriefing(
        [Description("The city to get the briefing for, e.g. 'Seattle, WA'")] string location)
    {
        return
        [
            new ChatMessage(ChatRole.User,
                $"Give me a morning briefing for {location}. " +
                "Please get the current date/time, current weather, " +
                "and a 3-day forecast, then present it as a concise morning briefing.")
        ];
    }

    [McpServerPrompt(Name = "weather_comparison"), Description("Compare current weather between two cities")]
    public static IEnumerable<ChatMessage> WeatherComparison(
        [Description("First city, e.g. 'Seattle, WA'")] string location1,
        [Description("Second city, e.g. 'Miami, FL'")] string location2)
    {
        return
        [
            new ChatMessage(ChatRole.User,
                $"Compare the current weather in {location1} and {location2}. " +
                "Get the weather for both locations and present a side-by-side comparison " +
                "including temperature, conditions, humidity, and wind speed.")
        ];
    }
}
