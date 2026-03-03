#!/usr/bin/env python3
"""
Sample 23: MCP + OpenAI SDK Tool Calling (Python)

Demonstrates how to use a remote MCP server as the tool provider for the
standard OpenAI SDK manual tool calling loop. The key technique is converting
MCP tool definitions into OpenAI-compatible function schemas — and routing
tool call requests back through the MCP client.

This bridges two worlds:
  - MCP (Model Context Protocol): Dynamically discovers tools from a remote server
  - OpenAI SDK: Manual tool calling loop with function definitions

The interesting conversion:
  MCP Tool.inputSchema (JSON Schema dict) → OpenAI function tool parameters
  They're both JSON Schema — so the mapping is direct, no transformation needed!

Compare to:
  - Sample 03: Basic OpenAI Python CLI (no tools)
  - Sample 10: Manual tool calling with locally-defined functions (C#)
  - Sample 20: MCP client with Agent Framework (automatic tool loop, C#)
  - Sample 21: The remote MCP weather server we're calling here
  - Sample 24: Same concept in C#
"""

import os
import sys
import json
import asyncio
from dotenv import load_dotenv
from openai import AzureOpenAI
from azure.identity import DefaultAzureCredential, get_bearer_token_provider
from mcp.client.streamable_http import streamablehttp_client
from mcp.client.session import ClientSession


def mcp_tools_to_openai(mcp_tools) -> list[dict]:
    """
    Convert MCP tool definitions → OpenAI function tool format.

    This is the KEY BRIDGE between MCP and the OpenAI SDK:

      MCP Tool:
        - name (str)
        - description (str)
        - inputSchema (dict — a JSON Schema object)

      OpenAI function tool:
        - type: "function"
        - function.name (str)
        - function.description (str)
        - function.parameters (dict — a JSON Schema object)

    Both use JSON Schema for parameters, so the conversion is trivial!
    """
    openai_tools = []
    for tool in mcp_tools:
        openai_tool = {
            "type": "function",
            "function": {
                "name": tool.name,
                "description": tool.description or "",
                "parameters": tool.inputSchema if tool.inputSchema else {"type": "object", "properties": {}},
            },
        }
        openai_tools.append(openai_tool)
    return openai_tools


async def process_tool_call_loop(
    client: AzureOpenAI,
    deployment_name: str,
    openai_tools: list[dict],
    mcp_session: ClientSession,
    messages: list[dict],
):
    """
    The standard OpenAI tool call loop — but forwarding calls to an MCP server.

    Same structure as any OpenAI function calling example:
    1. Send messages + tool definitions to the model
    2. If the model responds with tool_calls, execute them
    3. Send results back and repeat until the model gives a final text answer

    The twist: instead of calling local functions, we forward to the MCP server.
    """
    # Step 1: Initial request with tool definitions
    response = client.chat.completions.create(
        model=deployment_name,
        messages=messages,
        tools=openai_tools,
        tool_choice="auto",
    )

    choice = response.choices[0]

    # Step 2: Loop while the model keeps requesting tool calls
    while choice.finish_reason == "tool_calls":
        print("🔧 AI is calling MCP tools...")

        # Add the assistant's tool-call message to conversation history
        messages.append(choice.message.model_dump())

        # Process each tool call — forward to the MCP server
        for tool_call in choice.message.tool_calls:
            function_name = tool_call.function.name
            function_args = json.loads(tool_call.function.arguments)

            print(f"  📞 Calling MCP tool: {function_name}({json.dumps(function_args)})")

            # ★ Forward the tool call to the remote MCP server ★
            # This is where MCP shines — the tool execution happens
            # on the server, we just pass the name and arguments.
            mcp_result = await mcp_session.call_tool(function_name, function_args)

            # Extract text content from the MCP response.
            # MCP tool results contain a list of content blocks.
            result_text = "\n".join(
                c.text for c in mcp_result.content if hasattr(c, "text") and c.text
            )
            if not result_text:
                result_text = json.dumps([c.model_dump() for c in mcp_result.content])

            print(f"  📋 MCP Result: {result_text}")

            # Feed the result back as a tool response message
            messages.append(
                {
                    "role": "tool",
                    "tool_call_id": tool_call.id,
                    "content": result_text,
                }
            )

        # Step 3: Get the next response (may trigger more tool calls or a final answer)
        response = client.chat.completions.create(
            model=deployment_name,
            messages=messages,
            tools=openai_tools,
            tool_choice="auto",
        )
        choice = response.choices[0]

    # Display the final response
    if choice.finish_reason == "stop":
        assistant_text = choice.message.content or "I'm not sure how to help with that."
        print(f"🤖 Assistant: {assistant_text}")
        messages.append({"role": "assistant", "content": assistant_text})
    elif choice.finish_reason == "length":
        assistant_text = choice.message.content or ""
        print(f"🤖 Assistant: {assistant_text}")
        print("⚠️  (Response was truncated due to length limit)")
        messages.append({"role": "assistant", "content": assistant_text})
    elif choice.finish_reason == "content_filter":
        print("🚫 Response was filtered due to content policy.")
    else:
        print(f"🤔 Unexpected finish reason: {choice.finish_reason}")


async def main():
    # Load environment variables
    load_dotenv("../../.env")

    endpoint = os.getenv("AZURE_OPENAI_ENDPOINT")
    deployment_name = os.getenv("AZURE_OPENAI_DEPLOYMENT_NAME")
    api_version = os.getenv("AZURE_OPENAI_API_VERSION", "2024-02-15-preview")

    # MCP endpoint from Sample 21's deployed weather server
    mcp_endpoint = os.getenv(
        "MCP_WEATHER_ENDPOINT",
        "https://app-mcp-weather-demo-cmw.azurewebsites.net/mcp",
    )

    print("🔌 MCP + OpenAI SDK Tool Calling (Python)")
    print(f"   Connecting to MCP server at {mcp_endpoint}...\n")

    # ─── Step 1: Connect to the remote MCP Weather Server ───
    async with streamablehttp_client(mcp_endpoint) as (read_stream, write_stream, _):
        async with ClientSession(read_stream, write_stream) as session:
            await session.initialize()

            # ─── Step 2: Discover tools from the MCP server ───
            tools_result = await session.list_tools()
            mcp_tools = tools_result.tools

            print(f"✅ Connected! Discovered {len(mcp_tools)} tools:\n")

            # ─── Step 3: Convert MCP tools → OpenAI format ───
            openai_tools = mcp_tools_to_openai(mcp_tools)

            for tool in mcp_tools:
                schema_str = json.dumps(tool.inputSchema) if tool.inputSchema else "{}"
                print(f"  📋 {tool.name}: {tool.description}")
                print(f"     Schema: {schema_str}\n")

            # ─── Step 4: Create Azure OpenAI client ───
            token_provider = get_bearer_token_provider(
                DefaultAzureCredential(),
                "https://cognitiveservices.azure.com/.default",
            )
            client = AzureOpenAI(
                azure_endpoint=endpoint,
                azure_ad_token_provider=token_provider,
                api_version=api_version,
            )

            print("💬 Ask about weather, forecasts, or the current time!")
            print("   Examples:")
            print("   - What's the weather in Seattle?")
            print("   - Give me a 5-day forecast for New York")
            print("   - What time is it?")
            print("   Type 'quit' to exit.\n")

            # ─── Step 5: Chat loop with manual tool calling ───
            messages = [
                {
                    "role": "system",
                    "content": (
                        "You are a helpful assistant with access to weather and time tools. "
                        "Use the available functions when the user asks about weather, forecasts, "
                        "or the current time. When presenting weather data, format it nicely. "
                        "Temperatures are in Fahrenheit by default."
                    ),
                }
            ]

            while True:
                try:
                    user_input = input("You: ").strip()
                except (EOFError, KeyboardInterrupt):
                    print("\n👋 Goodbye!")
                    break

                if not user_input or user_input.lower() in ("quit", "exit", "q"):
                    print("👋 Goodbye!")
                    break

                messages.append({"role": "user", "content": user_input})

                try:
                    await process_tool_call_loop(
                        client, deployment_name, openai_tools, session, messages
                    )
                except Exception as ex:
                    print(f"❌ Error: {ex}")

                print()


if __name__ == "__main__":
    asyncio.run(main())
