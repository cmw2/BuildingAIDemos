# Sample 01: Direct API Calling

This sample demonstrates how to make direct HTTP requests to AI Foundry models using the VS Code REST Client extension.

## Prerequisites

1. **VS Code with REST Client Extension**: Install the "REST Client" extension by Huachao Mao
2. **Environment Configuration**: Copy `.env.example` to `.env` and fill in your AI Foundry details

## Setup

1. Copy the `.env.example` file in the root directory to `.env`
2. Update the values in `.env` with your actual AI Foundry endpoint and API key:
   ```
   AZURE_OPENAI_ENDPOINT=https://your-resource-name.openai.azure.com
   AZURE_OPENAI_API_KEY=your_api_key_here
   AZURE_OPENAI_DEPLOYMENT_NAME=gpt-4o
   AZURE_OPENAI_API_VERSION=2024-02-15-preview
   ```

## Usage

1. Open the `ai-foundry.http` file in VS Code
2. You'll see "Send Request" links above each HTTP request
3. Click "Send Request" to execute any of the example requests
4. View the response in the REST Client response panel

## Available Examples

The HTTP file includes several examples:

1. **Basic Chat Completion**: Simple question-answer interaction
2. **Conversation History**: Multi-turn conversation with context
3. **Streaming Response**: Real-time streaming of the AI response
4. **Temperature Variations**: 
   - Low temperature (0.1) for factual responses
   - High temperature (0.9) for creative responses

## Example Request Structure

```http
POST {{endpoint}}/openai/deployments/{{deploymentName}}/chat/completions?api-version={{apiVersion}}
Content-Type: application/json
api-key: {{apiKey}}

{
  "messages": [
    {
      "role": "system",
      "content": "You are a helpful AI assistant."
    },
    {
      "role": "user",
      "content": "Your question here"
    }
  ],
  "max_tokens": 150,
  "temperature": 0.7
}
```

## Notes

- The file uses environment variables from your `.env` file for security
- Each request is separated by `###` for clarity
- You can modify the messages, temperature, and other parameters as needed
- Streaming responses will show incremental content delivery
