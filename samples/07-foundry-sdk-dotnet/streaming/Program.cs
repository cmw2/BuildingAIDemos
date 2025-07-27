using Azure.AI.Inference;
using Azure.Identity;
using DotNetEnv;

// Sample 07b: AI Foundry SDK .NET Streaming CLI Application
//
// Simple command-line chat with AI Foundry using the native AI Foundry SDK with streaming responses.
// Watch AI responses appear in real-time, type 'quit' to exit.

// Load environment variables
Env.Load("../../../.env");

// Get configuration from environment
var foundryEndpoint = Environment.GetEnvironmentVariable("AI_FOUNDRY_ENDPOINT")!;
var modelDeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")!;

// Create ChatCompletionsClient with DefaultAzureCredential
var credential = new DefaultAzureCredential();
var chatClient = new ChatCompletionsClient(new Uri(foundryEndpoint), credential);

Console.WriteLine("🤖 AI Foundry SDK Streaming Chat");
Console.WriteLine("Type 'quit' to exit\n");

// Initialize chat history with system message
var messages = new List<ChatRequestMessage>
{
    new ChatRequestSystemMessage("You are a helpful AI assistant.")
};

// Chat loop
while (true)
{
    Console.Write("You: ");
    var userInput = Console.ReadLine()?.Trim();

    if (string.IsNullOrEmpty(userInput) || 
        userInput.Equals("quit", StringComparison.OrdinalIgnoreCase) ||
        userInput.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
        userInput.Equals("q", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Goodbye!");
        break;
    }

    // Add user message to chat history
    messages.Add(new ChatRequestUserMessage(userInput));

    // Create completion options with current messages
    var requestOptions = new ChatCompletionsOptions()
    {
        Model = modelDeploymentName,
        Temperature = 0.7f,
        MaxTokens = 500
    };

    // Add all messages to the options
    foreach (var message in messages)
    {
        requestOptions.Messages.Add(message);
    }

    // Get streaming AI response
    Console.Write("AI: ");
    
    var fullResponse = "";
    var streamingResponse = await chatClient.CompleteStreamingAsync(requestOptions);
    
    await foreach (var streamingChoice in streamingResponse.Value)
    {
        if (streamingChoice.Choices.Count > 0)
        {
            var delta = streamingChoice.Choices[0].Delta;
            if (delta.Content != null)
            {
                Console.Write(delta.Content);
                fullResponse += delta.Content;
            }
        }
    }
    
    Console.WriteLine("\n");

    // Add AI response to chat history
    messages.Add(new ChatRequestAssistantMessage(fullResponse));
}
