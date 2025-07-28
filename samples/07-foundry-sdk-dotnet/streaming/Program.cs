using Azure.AI.Inference;
using Azure.Core;
using Azure.Identity;
using DotNetEnv;
using Sprache;

// Sample 07b: AI Foundry SDK .NET Streaming CLI Application
//
// Simple command-line chat with AI Foundry using the native AI Foundry SDK with streaming responses.
// Watch AI responses appear in real-time, type 'quit' to exit.

// Load environment variables
// Try current directory first (for Visual Studio), then relative path (for command line)
if (File.Exists(".env"))
{
    Env.Load(".env");
}
else
{
    Env.Load("../../../.env");
}

// Get configuration from environment
var endpoint = new Uri(Environment.GetEnvironmentVariable("AI_FOUNDRY_INFERENCE_ENDPOINT")!);
var modelDeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")!;

// Create ChatCompletionsClient with DefaultAzureCredential and custom token scope
var credential = new DefaultAzureCredential();
var tokenCredential = new TokenCredentialWrapper(credential);
var chatClient = new ChatCompletionsClient(endpoint, tokenCredential);

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
    await foreach (var streamingUpdate in streamingResponse)
    {
        if (streamingUpdate?.ContentUpdate != null)
        {
            Console.Write(streamingUpdate.ContentUpdate);
            fullResponse += streamingUpdate.ContentUpdate;
        }
    }
    
    Console.WriteLine("\n");

    // Add AI response to chat history
    messages.Add(new ChatRequestAssistantMessage(fullResponse));
}

// Custom wrapper to handle token scope
public class TokenCredentialWrapper : TokenCredential
{
    private readonly TokenCredential _innerCredential;

    public TokenCredentialWrapper(TokenCredential innerCredential)
    {
        _innerCredential = innerCredential;
    }

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        var context = new TokenRequestContext(new[] { "https://ai.azure.com/.default" });
        return _innerCredential.GetToken(context, cancellationToken);
    }

    public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        var context = new TokenRequestContext(new[] { "https://ai.azure.com/.default" });
        return _innerCredential.GetTokenAsync(context, cancellationToken);
    }
}
