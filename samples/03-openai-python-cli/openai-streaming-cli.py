#!/usr/bin/env python3
"""
Sample 03b: OpenAI Python Streaming CLI Application

Simple command-line chat with AI Foundry using OpenAI SDK with streaming responses.
Watch AI responses appear in real-time, type 'quit' to exit.
"""

import os
from dotenv import load_dotenv
from openai import AzureOpenAI
from azure.identity import DefaultAzureCredential, get_bearer_token_provider

def main():
    # Load environment variables
    load_dotenv('../../.env')
    
    # Initialize client with DefaultAzureCredential
    token_provider = get_bearer_token_provider(
        DefaultAzureCredential(), "https://cognitiveservices.azure.com/.default"
    )
    client = AzureOpenAI(
        azure_endpoint=os.getenv('AZURE_OPENAI_ENDPOINT'),
        azure_ad_token_provider=token_provider,
        api_version=os.getenv('AZURE_OPENAI_API_VERSION', '2024-02-15-preview')
    )
    
    deployment_name = os.getenv('AZURE_OPENAI_DEPLOYMENT_NAME')
    
    print("🤖 AI Foundry Streaming CLI Chat")
    print("Type 'quit' to exit\n")
    
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
        
        # Get streaming AI response
        print("AI: ", end="", flush=True)
        
        stream = client.chat.completions.create(
            model=deployment_name,
            messages=messages,
            max_tokens=500,
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
