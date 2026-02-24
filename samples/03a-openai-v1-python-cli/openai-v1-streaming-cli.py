#!/usr/bin/env python3
"""
Sample 03a: OpenAI v1 Endpoint - Python Streaming CLI

Uses the /openai/v1 endpoint which doesn't require an api-version parameter.
This uses the standard OpenAI SDK (not AzureOpenAI) pointed at the Azure v1 endpoint.

See: https://learn.microsoft.com/en-us/azure/ai-foundry/openai/api-version-lifecycle#api-evolution
"""

import os
from dotenv import load_dotenv
from openai import OpenAI
from azure.identity import DefaultAzureCredential

def main():
    # Load environment variables
    load_dotenv('../../.env')

    endpoint = os.getenv('AZURE_OPENAI_ENDPOINT', '').rstrip('/')
    deployment_name = os.getenv('AZURE_OPENAI_DEPLOYMENT_NAME')

    # -----------------------------------------------------------------
    # Key difference: Using the /openai/v1 endpoint
    # This is the new versioned endpoint that doesn't require api-version
    # and is compatible with the standard OpenAI SDK.
    # -----------------------------------------------------------------

    # Get a token using DefaultAzureCredential
    credential = DefaultAzureCredential()
    token = credential.get_token("https://cognitiveservices.azure.com/.default")

    # Create a standard OpenAI client pointed at Azure's /openai/v1 endpoint
    client = OpenAI(
        api_key=token.token,
        base_url=f"{endpoint}/openai/v1"
    )

    print("🤖 AI Chat (OpenAI v1 Endpoint) - Type 'quit' to exit")
    print("   Using /openai/v1 - no api-version needed!\n")

    # Initialize conversation with system message
    messages = [
        {"role": "system", "content": "You are a helpful AI assistant."}
    ]

    # Chat loop
    while True:
        user_input = input("You: ").strip()

        if user_input.lower() in ['quit', 'exit', 'q']:
            print("Goodbye!")
            break

        if not user_input:
            continue

        # Add user message to conversation history
        messages.append({"role": "user", "content": user_input})

        # Streaming response
        print("AI: ", end="", flush=True)

        stream = client.chat.completions.create(
            model=deployment_name,
            messages=messages,
            max_completion_tokens=500,
            temperature=0.7,
            stream=True
        )

        full_response = ""
        for chunk in stream:
            if (chunk.choices and
                chunk.choices[0].delta and
                chunk.choices[0].delta.content):
                content = chunk.choices[0].delta.content
                print(content, end="", flush=True)
                full_response += content

        print("\n")  # New line after streaming

        # Add AI response to conversation history
        messages.append({"role": "assistant", "content": full_response})

if __name__ == "__main__":
    main()
