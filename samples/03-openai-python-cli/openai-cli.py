#!/usr/bin/env python3
"""
Sample 03: OpenAI Python CLI Application

Simple command-line chat with AI Foundry using OpenAI SDK.
Type messages, get AI responses, type 'quit' to exit.
"""

import os
from dotenv import load_dotenv
from openai import AzureOpenAI

def main():
    # Load environment variables
    load_dotenv('../../.env')
    
    # Initialize client
    client = AzureOpenAI(
        azure_endpoint=os.getenv('AZURE_OPENAI_ENDPOINT'),
        api_key=os.getenv('AZURE_OPENAI_API_KEY'),
        api_version=os.getenv('AZURE_OPENAI_API_VERSION', '2024-02-15-preview')
    )
    
    deployment_name = os.getenv('AZURE_OPENAI_DEPLOYMENT_NAME')
    
    print("🤖 AI Foundry CLI Chat")
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
        
        # Get AI response
        response = client.chat.completions.create(
            model=deployment_name,
            messages=messages,
            max_tokens=500,
            temperature=0.7
        )
        
        ai_response = response.choices[0].message.content
        
        # Add AI response to conversation history
        messages.append({"role": "assistant", "content": ai_response})
        
        print(f"AI: {ai_response}\n")

if __name__ == "__main__":
    main()
