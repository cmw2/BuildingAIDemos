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
            
            elif method == "initialize":
                return {
                    "jsonrpc": "2.0",
                    "id": request_id,
                    "result": {
                        "protocolVersion": "2024-11-05",
                        "capabilities": {
                            "tools": {}
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
