#!/usr/bin/env python3
"""
Sample 03: OpenAI Python CLI Application

A simple command-line interface that allows users to interact with AI Foundry models
using the OpenAI Python SDK. The application runs in a continuous loop, accepting
user input and providing AI responses until the user types 'quit'.

Features:
- Continuous conversation loop
- Environment-based configuration
- Error handling with graceful degradation
- Clean output formatting
- Hardcoded temperature and max_tokens for simplicity

Usage:
    python openai-cli.py

Requirements:
    - Python 3.8+
    - OpenAI SDK installed (pip install openai)
    - python-dotenv installed (pip install python-dotenv)
    - .env file configured with AI Foundry credentials
"""

import os
import sys
from dotenv import load_dotenv
from openai import AzureOpenAI

# Configuration constants
TEMPERATURE = 0.7        # Balance between creativity and consistency
MAX_TOKENS = 500         # Reasonable response length for CLI interaction
SYSTEM_MESSAGE = "You are a helpful AI assistant. Provide clear, concise responses."

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

def get_ai_response(client, deployment_name, user_message):
    """
    Get AI response for the given user message.
    
    Args:
        client (AzureOpenAI): The OpenAI client instance
        deployment_name (str): Name of the model deployment
        user_message (str): User's input message
        
    Returns:
        str: AI response or error message
    """
    try:
        response = client.chat.completions.create(
            model=deployment_name,
            messages=[
                {"role": "system", "content": SYSTEM_MESSAGE},
                {"role": "user", "content": user_message}
            ],
            max_tokens=MAX_TOKENS,
            temperature=TEMPERATURE
        )
        
        return response.choices[0].message.content
        
    except Exception as e:
        return f"❌ Error getting AI response: {str(e)}"

def print_welcome_message(config):
    """Print welcome message with configuration info."""
    print("🤖 AI Foundry CLI Chat")
    print("=" * 50)
    print(f"Connected to: {config['azure_endpoint']}")
    print(f"Using model: {config['deployment_name']}")
    print(f"Settings: Temperature={TEMPERATURE}, Max Tokens={MAX_TOKENS}")
    print("=" * 50)
    print("Type your message and press Enter to chat.")
    print("Type 'quit', 'exit', or 'q' to stop.")
    print()

def main():
    """
    Main application entry point.
    Handles the conversation loop and user interaction.
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
                
                # Get and display AI response
                print("🤖 AI: ", end="", flush=True)
                ai_response = get_ai_response(client, config['deployment_name'], user_input)
                print(ai_response)
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
