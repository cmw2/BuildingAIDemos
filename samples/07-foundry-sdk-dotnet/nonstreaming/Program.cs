using Azure;
using Azure.Identity;
using Azure.AI.Inference;
using Azure.Core;
using DotNetEnv;

// Sample 07: AI Foundry SDK .NET CLI Application
//
// Simple command-line chat with AI Foundry using the native AI Foundry SDK.
// Type messages, get AI responses, type 'quit' to exit.

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
var apiVersion = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_VERSION")!;

// Create ChatCompletionsClient directly with DefaultAzureCredential and custom token scope
var credential = new DefaultAzureCredential();
var tokenCredential = new TokenCredentialWrapper(credential);
ChatCompletionsClient chatClient = new ChatCompletionsClient(endpoint, tokenCredential);

//var modelInfo = await chatClient.GetModelInfoAsync();

Console.WriteLine($"🤖 AI Foundry SDK Chat");
//Console.WriteLine($"Model Name: {modelInfo.Value.ModelName}");
//Console.WriteLine($"Model Info: {modelInfo.Value.ToString()}");
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

    // Get AI response
    var response = chatClient.Complete(requestOptions);
    var aiResponse = response.Value.Content;

    Console.WriteLine($"AI: {aiResponse}\n");

    // Add AI response to chat history
    messages.Add(new ChatRequestAssistantMessage(aiResponse));
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
