using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpWeatherFunctionApp;

/// <summary>
/// MCP Weather Tools — the same fake weather and datetime functions used across
/// samples 10, 11, 16, 17, 19, and 20, now exposed as an MCP server over HTTP.
///
/// Each [McpServerTool] method is automatically discovered and registered as an
/// MCP tool. The MCP SDK handles JSON schema generation from the method signatures
/// and [Description] attributes — no manual schema definitions needed.
/// </summary>
public class WeatherTools
{
    private static readonly string[] Conditions =
        ["sunny", "cloudy", "partly cloudy", "rainy", "foggy", "snowy", "thunderstorms", "clear"];

    /// <summary>
    /// Get the current weather for a specific location.
    /// Returns fake but deterministic weather data (same location + date = same result).
    /// </summary>
    [McpServerTool(Name = "get_current_weather"), Description("Get the current weather for a specific location")]
    public static object GetCurrentWeather(
        [Description("The city and state, e.g. San Francisco, CA")] string location)
    {
        var today = DateTime.Now.Date;
        var seed = location.GetHashCode() + today.DayOfYear;
        var rng = new Random(seed);

        var condition = Conditions[rng.Next(Conditions.Length)];
        var temperature = rng.Next(15, 96);
        var humidity = rng.Next(30, 91);
        var windSpeed = rng.Next(5, 26);

        return new
        {
            location,
            temperature,
            condition,
            humidity,
            wind_speed = windSpeed,
            unit = "Fahrenheit"
        };
    }

    /// <summary>
    /// Get the weather forecast for a specific location.
    /// Returns fake but deterministic forecast data for the requested number of days.
    /// </summary>
    [McpServerTool(Name = "get_weather_forecast"), Description("Get the weather forecast for a specific location")]
    public static object GetWeatherForecast(
        [Description("The city and state, e.g. San Francisco, CA")] string location,
        [Description("Number of days to forecast (1-7)")] int days = 3)
    {
        days = Math.Clamp(days, 1, 7);
        var today = DateTime.Now.Date;
        var forecast = new List<object>();

        for (int i = 0; i < days; i++)
        {
            var targetDate = today.AddDays(i);
            var seed = location.GetHashCode() + targetDate.DayOfYear;
            var rng = new Random(seed);

            var condition = Conditions[rng.Next(Conditions.Length)];
            var temperature = rng.Next(15, 96);
            var humidity = rng.Next(30, 91);
            var windSpeed = rng.Next(5, 26);

            // Seasonal adjustments
            var month = targetDate.Month;
            if (month is 12 or 1 or 2) // Winter
            {
                temperature = Math.Max(temperature - 25, 10);
                if (rng.Next(3) == 0) condition = "snowy";
            }
            else if (month is 6 or 7 or 8) // Summer
            {
                temperature = Math.Min(temperature + 15, 100);
            }

            var tempVariation = rng.Next(5, 16);
            forecast.Add(new
            {
                date = targetDate.ToString("yyyy-MM-dd"),
                temperature_high = temperature + tempVariation / 2,
                temperature_low = temperature - tempVariation / 2,
                condition,
                humidity,
                wind_speed = windSpeed
            });
        }

        return new { location, forecast, unit = "Fahrenheit" };
    }

    /// <summary>
    /// Get the current date and time.
    /// </summary>
    [McpServerTool(Name = "get_current_datetime"), Description("Get the current date and time")]
    public static object GetCurrentDatetime(
        [Description("Timezone (e.g., 'UTC', 'local')")] string timezone = "local")
    {
        var dt = timezone.Equals("utc", StringComparison.OrdinalIgnoreCase)
            ? DateTime.UtcNow
            : DateTime.Now;
        var tzName = timezone.Equals("utc", StringComparison.OrdinalIgnoreCase) ? "UTC" : "Local";

        return new
        {
            datetime = dt.ToString("o"),
            formatted = dt.ToString("yyyy-MM-dd HH:mm:ss"),
            timezone = tzName,
            timestamp = new DateTimeOffset(dt).ToUnixTimeSeconds()
        };
    }
}
