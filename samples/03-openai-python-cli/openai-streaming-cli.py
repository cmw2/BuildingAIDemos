#!/usr/bin/env python3
"""
Sample 03b: OpenAI Python Streaming CLI Application

A command-line interface that allows users to interact with AI Foundry models
using the OpenAI Python SDK with streaming responses. The application runs in a 
continuous loop, accepting user input and streaming AI responses in real-time
until the user types 'quit'.

Features:
- Continuous conversation loop
- Real-time streaming responses for better UX
- Environment-based configuration
- Error handling with graceful degradation
- Clean output formatting
- Hardcoded temperature and max_tokens for simplicity

Usage:
    python openai-streaming-cli.py

Requirements:
    - Python 3.8+
    - OpenAI SDK installed (pip install openai)
    - python-dotenv installed (pip install python-dotenv)
    - .env file configured with AI Foundry credentials
"""

import os
import sys
import time
from dotenv import load_dotenv
from openai import AzureOpenAI

# Configuration constants
TEMPERATURE = 0.7        # Balance between creativity and consistency
MAX_TOKENS = 800         # Slightly higher for streaming to show effect better
SYSTEM_MESSAGE = "You are a helpful AI assistant. Provide clear, detailed responses."

def load_configuration():
    """
    Load and validate configuration from environment variables.
    
    Returns:
        dict: Configuration dictionary with all required values
        
    Raises:
        SystemExit: If required configuration is missing
    """
    # Load environment variables from .env file (two levels up from this script)
    env_path = os.path.join(os.path.dirname(__file__), '..', '..', '.env')
    load_dotenv(env_path)
    
    config = {
        'azure_endpoint': os.getenv('AZURE_OPENAI_ENDPOINT'),
        'api_key': os.getenv('AZURE_OPENAI_API_KEY'),
        'deployment_name': os.getenv('AZURE_OPENAI_DEPLOYMENT_NAME'),
        'api_version': os.getenv('AZURE_OPENAI_API_VERSION')
    }
    
    # Validate all required configuration is present
    missing_config = [key for key, value in config.items() if not value]
    if missing_config:
        print("❌ Error: Missing required configuration:")
        for key in missing_config:
            print(f"   - {key.upper()}")
        print("\nPlease check your .env file and ensure all required values are set.")
        sys.exit(1)
    
    return config

def initialize_client(config):
    """
    Initialize the Azure OpenAI client with the provided configuration.
    
    Args:
        config (dict): Configuration dictionary
        
    Returns:
        AzureOpenAI: Initialized client instance
        
    Raises:
        SystemExit: If client initialization fails
    """
    try:
        client = AzureOpenAI(
            azure_endpoint=config['azure_endpoint'],
            api_key=config['api_key'],
            api_version=config['api_version']
        )
        return client
    except Exception as e:
        print(f"❌ Error initializing Azure OpenAI client: {str(e)}")
        sys.exit(1)

def get_streaming_ai_response(client, deployment_name, user_message):
    """
    Get streaming AI response for the given user message.
    
    Args:
        client (AzureOpenAI): The OpenAI client instance
        deployment_name (str): Name of the model deployment
        user_message (str): User's input message
        
    Returns:
        str: Complete AI response or error message
    """
    try:
        # Create streaming completion
        stream = client.chat.completions.create(
            model=deployment_name,
            messages=[
                {"role": "system", "content": SYSTEM_MESSAGE},
                {"role": "user", "content": user_message}
            ],
            max_tokens=MAX_TOKENS,
            temperature=TEMPERATURE,
            stream=True
        )
        
        full_response = ""
        print("🤖 AI: ", end="", flush=True)
        
        # Process each chunk as it arrives
        for chunk in stream:
            # Check if chunk has choices and the first choice has content
            if (chunk.choices and 
                len(chunk.choices) > 0 and 
                chunk.choices[0].delta and 
                chunk.choices[0].delta.content is not None):
                content = chunk.choices[0].delta.content
                print(content, end="", flush=True)
                full_response += content
                # Small delay to make streaming more visible (optional)
                time.sleep(0.01)
        
        print()  # New line after streaming is complete
        return full_response
        
    except Exception as e:
        error_msg = f"❌ Error getting AI response: {str(e)}"
        print(error_msg)
        return error_msg

def print_welcome_message(config):
    """Print welcome message with configuration info."""
    print("🤖 AI Foundry Streaming CLI Chat")
    print("=" * 55)
    print(f"Connected to: {config['azure_endpoint']}")
    print(f"Using model: {config['deployment_name']}")
    print(f"Settings: Temperature={TEMPERATURE}, Max Tokens={MAX_TOKENS}")
    print("Mode: Streaming responses enabled ⚡")
    print("=" * 55)
    print("Type your message and press Enter to chat.")
    print("Type 'quit', 'exit', or 'q' to stop.")
    print("Watch the AI response stream in real-time!")
    print()

def main():
    """
    Main application entry point.
    Handles the conversation loop and user interaction with streaming.
    """
    try:
        # Load configuration and initialize client
        config = load_configuration()
        client = initialize_client(config)
        
        # Print welcome message
        print_welcome_message(config)
        
        # Main conversation loop
        while True:
            try:
                # Get user input
                user_input = input("👤 You: ").strip()
                
                # Check for quit commands
                if user_input.lower() in ['quit', 'exit', 'q', '']:
                    print("\n👋 Goodbye!")
                    break
                
                # Get and display streaming AI response
                ai_response = get_streaming_ai_response(client, config['deployment_name'], user_input)
                print()  # Add blank line for readability
                
            except KeyboardInterrupt:
                print("\n\n👋 Goodbye!")
                break
            except EOFError:
                print("\n\n👋 Goodbye!")
                break
                
    except Exception as e:
        print(f"❌ Unexpected error: {str(e)}")
        sys.exit(1)

if __name__ == "__main__":
    main()
