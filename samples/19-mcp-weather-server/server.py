#!/usr/bin/env python3
"""
MCP Weather Server - A Model Context Protocol server providing weather and datetime tools.

This server implements the same fake weather and datetime functions used in samples 10, 11, 16, and 17,
but exposed as an MCP server that can be consumed by any MCP-compatible client.
"""

import json
import sys
from datetime import datetime, timezone, timedelta
from typing import Any, Dict, List, Optional
import asyncio
import logging
import random

# MCP Protocol implementation
class MCPServer:
    def __init__(self):
        self.prompts = {
            "time_in_location": {
                "name": "time_in_location",
                "description": "Get the current time in a specific city or timezone. Use this when a user asks what time it is in a particular location.",
                "arguments": [
                    {
                        "name": "location",
                        "description": "The city or region the user is asking about, e.g. 'Tokyo', 'London', 'New York'",
                        "required": True
                    }
                ]
            },
            "daily_briefing": {
                "name": "daily_briefing",
                "description": "Get a morning briefing with current time, weather, and forecast for a location.",
                "arguments": [
                    {
                        "name": "location",
                        "description": "The city to get the briefing for, e.g. 'Seattle, WA'",
                        "required": True
                    }
                ]
            },
            "weather_comparison": {
                "name": "weather_comparison",
                "description": "Compare current weather between two cities.",
                "arguments": [
                    {
                        "name": "location1",
                        "description": "First city, e.g. 'Seattle, WA'",
                        "required": True
                    },
                    {
                        "name": "location2",
                        "description": "Second city, e.g. 'Miami, FL'",
                        "required": True
                    }
                ]
            }
        }

        self.tools = {
            "get_current_weather": {
                "name": "get_current_weather",
                "description": "Get the current weather for a specific location",
                "inputSchema": {
                    "type": "object",
                    "properties": {
                        "location": {
                            "type": "string",
                            "description": "The city and state, e.g. San Francisco, CA"
                        }
                    },
                    "required": ["location"]
                }
            },
            "get_weather_forecast": {
                "name": "get_weather_forecast",
                "description": "Get the weather forecast for a specific location",
                "inputSchema": {
                    "type": "object",
                    "properties": {
                        "location": {
                            "type": "string",
                            "description": "The city and state, e.g. San Francisco, CA"
                        },
                        "days": {
                            "type": "integer",
                            "description": "Number of days to forecast (1-7)",
                            "minimum": 1,
                            "maximum": 7
                        }
                    },
                    "required": ["location"]
                }
            },
            "get_current_datetime": {
                "name": "get_current_datetime",
                "description": "Get the current date and time",
                "inputSchema": {
                    "type": "object",
                    "properties": {
                        "timezone": {
                            "type": "string",
                            "description": "Timezone (e.g., 'UTC', 'local')"
                        }
                    }
                }
            }
        }

    def get_current_weather(self, location: str) -> Dict[str, Any]:
        """Get fake current weather data for a location using consistent random generation."""
        # Generate consistent weather data based on location and current date
        today = datetime.now().date()
        seed = hash(location) + today.timetuple().tm_yday
        rng = random.Random(seed)
        
        conditions = ["sunny", "cloudy", "partly cloudy", "rainy", "foggy", "snowy", "thunderstorms", "clear"]
        condition = conditions[rng.randint(0, len(conditions) - 1)]
        
        # Generate temperature in Fahrenheit (15-95 range like sample 17)
        temperature = rng.randint(15, 95)
        humidity = rng.randint(30, 90)
        wind_speed = rng.randint(5, 25)
        
        return {
            "location": location,
            "temperature": temperature,
            "condition": condition,
            "humidity": humidity,
            "wind_speed": wind_speed,
            "unit": "Fahrenheit"
        }

    def get_weather_forecast(self, location: str, days: int = 3) -> Dict[str, Any]:
        """Get fake weather forecast data for a location using consistent random generation."""
        # Ensure days is within valid range
        days = max(1, min(7, days))
        
        forecast = []
        today = datetime.now().date()
        
        for i in range(days):
            target_date = today + timedelta(days=i)
            
            # Generate consistent weather data based on location and specific date
            seed = hash(location) + target_date.timetuple().tm_yday
            rng = random.Random(seed)
            
            conditions = ["sunny", "cloudy", "partly cloudy", "rainy", "foggy", "snowy", "thunderstorms", "clear"]
            condition = conditions[rng.randint(0, len(conditions) - 1)]
            
            # Generate base temperature in Fahrenheit
            temperature = rng.randint(15, 95)
            humidity = rng.randint(30, 90)
            wind_speed = rng.randint(5, 25)
            
            # Seasonal adjustments (like sample 17)
            month = target_date.month
            if month in [12, 1, 2]:  # Winter
                temperature = max(temperature - 25, 10)
                if rng.randint(0, 2) == 0:
                    condition = "snowy"
            elif month in [6, 7, 8]:  # Summer
                temperature = min(temperature + 15, 100)
            
            # Create high/low temps
            temp_variation = rng.randint(5, 15)
            forecast.append({
                "date": target_date.strftime("%Y-%m-%d"),
                "temperature_high": temperature + temp_variation // 2,
                "temperature_low": temperature - temp_variation // 2,
                "condition": condition,
                "humidity": humidity,
                "wind_speed": wind_speed
            })
        
        return {
            "location": location,
            "forecast": forecast,
            "unit": "Fahrenheit"
        }

    def get_current_datetime(self, timezone_str: str = "local") -> Dict[str, Any]:
        """Get current date and time."""
        if timezone_str.lower() == "utc":
            dt = datetime.now(timezone.utc)
            tz_name = "UTC"
        else:
            dt = datetime.now()
            tz_name = "Local"
        
        return {
            "datetime": dt.isoformat(),
            "formatted": dt.strftime("%Y-%m-%d %H:%M:%S"),
            "timezone": tz_name,
            "timestamp": dt.timestamp()
        }

    async def handle_request(self, request: Dict[str, Any]) -> Dict[str, Any]:
        """Handle incoming MCP requests."""
        method = request.get("method")
        params = request.get("params", {})
        request_id = request.get("id")

        try:
            if method == "tools/list":
                return {
                    "jsonrpc": "2.0",
                    "id": request_id,
                    "result": {
                        "tools": list(self.tools.values())
                    }
                }
            
            elif method == "tools/call":
                tool_name = params.get("name")
                arguments = params.get("arguments", {})
                
                if tool_name == "get_current_weather":
                    result = self.get_current_weather(arguments.get("location", ""))
                elif tool_name == "get_weather_forecast":
                    result = self.get_weather_forecast(
                        arguments.get("location", ""),
                        arguments.get("days", 3)
                    )
                elif tool_name == "get_current_datetime":
                    result = self.get_current_datetime(arguments.get("timezone", "local"))
                else:
                    raise ValueError(f"Unknown tool: {tool_name}")
                
                return {
                    "jsonrpc": "2.0",
                    "id": request_id,
                    "result": {
                        "content": [
                            {
                                "type": "text",
                                "text": json.dumps(result, indent=2)
                            }
                        ]
                    }
                }
            
            elif method == "prompts/list":
                return {
                    "jsonrpc": "2.0",
                    "id": request_id,
                    "result": {
                        "prompts": list(self.prompts.values())
                    }
                }

            elif method == "prompts/get":
                prompt_name = params.get("name")
                arguments = params.get("arguments", {})

                if prompt_name not in self.prompts:
                    raise ValueError(f"Unknown prompt: {prompt_name}")

                messages = self._get_prompt_messages(prompt_name, arguments)
                return {
                    "jsonrpc": "2.0",
                    "id": request_id,
                    "result": {
                        "description": self.prompts[prompt_name]["description"],
                        "messages": messages
                    }
                }

            elif method == "initialize":
                return {
                    "jsonrpc": "2.0",
                    "id": request_id,
                    "result": {
                        "protocolVersion": "2024-11-05",
                        "capabilities": {
                            "tools": {},
                            "prompts": {}
                        },
                        "serverInfo": {
                            "name": "weather-server",
                            "version": "1.0.0"
                        }
                    }
                }
            
            elif method == "notifications/initialized":
                # This is a notification, no response needed
                return None
            
            else:
                raise ValueError(f"Unknown method: {method}")
                
        except Exception as e:
            return {
                "jsonrpc": "2.0",
                "id": request_id,
                "error": {
                    "code": -1,
                    "message": str(e)
                }
            }

    def _get_prompt_messages(self, prompt_name: str, arguments: Dict[str, str]) -> List[Dict[str, Any]]:
        """Generate prompt messages for a given prompt name and arguments."""
        if prompt_name == "time_in_location":
            location = arguments.get("location", "the requested location")
            return [
                {
                    "role": "user",
                    "content": {
                        "type": "text",
                        "text": (
                            f"What time is it right now in {location}?\n\n"
                            "IMPORTANT INSTRUCTIONS FOR ANSWERING:\n"
                            "1. First, call the get_current_datetime tool with timezone set to 'UTC' "
                            "to get the current time in ISO 8601 / UTC format.\n"
                            "2. Then convert from UTC to the correct local timezone for the requested location. "
                            "Apply the appropriate UTC offset (e.g., Tokyo is UTC+9, London is UTC+0/+1, "
                            "New York is UTC-5/-4, etc.). Account for daylight saving time if applicable.\n"
                            "3. Present ONLY the final local time in a friendly, human-readable format like "
                            "'It's 10:28 PM on Monday, February 24th in New York (Eastern Standard Time).' "
                            "Do NOT show the raw UTC time, ISO 8601 strings, or the conversion math. "
                            "Keep it short and conversational — just the answer."
                        )
                    }
                }
            ]

        elif prompt_name == "daily_briefing":
            location = arguments.get("location", "the requested location")
            return [
                {
                    "role": "user",
                    "content": {
                        "type": "text",
                        "text": (
                            f"Give me a morning briefing for {location}. "
                            "Please get the current date/time, current weather, "
                            "and a 3-day forecast, then present it as a concise morning briefing."
                        )
                    }
                }
            ]

        elif prompt_name == "weather_comparison":
            loc1 = arguments.get("location1", "city 1")
            loc2 = arguments.get("location2", "city 2")
            return [
                {
                    "role": "user",
                    "content": {
                        "type": "text",
                        "text": (
                            f"Compare the current weather in {loc1} and {loc2}. "
                            "Get the weather for both locations and present a side-by-side comparison "
                            "including temperature, conditions, humidity, and wind speed."
                        )
                    }
                }
            ]

        else:
            raise ValueError(f"No message template for prompt: {prompt_name}")

async def main():
    """Main server loop."""
    server = MCPServer()
    
    logging.basicConfig(level=logging.INFO)
    logger = logging.getLogger("mcp-weather-server")
    
    logger.info("MCP Weather Server starting...")
    logger.info("Available tools: get_current_weather, get_weather_forecast, get_current_datetime")
    
    # Read from stdin and write to stdout (MCP protocol)
    while True:
        try:
            # Read line from stdin
            line = sys.stdin.readline()
            if not line:
                break
                
            line = line.strip()
            if not line:
                continue
                
            # Parse JSON-RPC request
            request = json.loads(line)
            
            # Handle request
            response = await server.handle_request(request)
            
            # Only send response if one is expected (not for notifications)
            if response is not None:
                print(json.dumps(response))
                sys.stdout.flush()
            
        except json.JSONDecodeError as e:
            logger.error(f"Invalid JSON: {e}")
        except Exception as e:
            logger.error(f"Error handling request: {e}")
            # Send error response if we can determine the request ID
            try:
                request = json.loads(line)
                request_id = request.get("id")
                error_response = {
                    "jsonrpc": "2.0",
                    "id": request_id,
                    "error": {
                        "code": -32603,
                        "message": f"Internal error: {str(e)}"
                    }
                }
                print(json.dumps(error_response))
                sys.stdout.flush()
            except:
                pass

if __name__ == "__main__":
    asyncio.run(main())
