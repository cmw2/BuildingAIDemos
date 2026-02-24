using Azure.Identity;
using DotNetEnv;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

// Load environment variables
Env.Load("../../.env");

// Get configuration
var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!;
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")!;

// -----------------------------------------------------------------
// Key difference: Using the /openai/v1 endpoint
// This is the new versioned endpoint that doesn't require api-version
// and is compatible with the standard OpenAI SDK.
// See: https://learn.microsoft.com/en-us/azure/ai-foundry/openai/api-version-lifecycle#api-evolution
// -----------------------------------------------------------------

// Get a token using DefaultAzureCredential (same auth as before)
var credential = new DefaultAzureCredential();
var tokenResponse = await credential.GetTokenAsync(
    new Azure.Core.TokenRequestContext(["https://cognitiveservices.azure.com/.default"]));

// Create a standard OpenAI client pointed at Azure's /openai/v1 endpoint
var client = new OpenAIClient(
    new ApiKeyCredential(tokenResponse.Token),
    new OpenAIClientOptions
    {
        Endpoint = new Uri($"{endpoint.TrimEnd('/')}/openai/v1")
    });
var chatClient = client.GetChatClient(deploymentName);

// Initialize conversation with system message
var messages = new List<ChatMessage>
{
    new SystemChatMessage("You are a helpful AI assistant.")
};

// Configure chat options
var options = new ChatCompletionOptions()
{
    MaxOutputTokenCount = 500,
    Temperature = 0.7f
};

Console.WriteLine("🤖 AI Chat (OpenAI v1 Endpoint) - Type 'quit' to exit");
Console.WriteLine("   Using /openai/v1 - no api-version needed!");
Console.WriteLine();

while (true)
{
    Console.Write("You: ");
    var userInput = Console.ReadLine();

    if (userInput?.ToLower() is "quit" or "exit" or "q")
        break;

    // Add user message to conversation
    messages.Add(new UserChatMessage(userInput!));

    // Streaming response
    Console.Write("AI: ");
    var streamingResponse = chatClient.CompleteChatStreamingAsync(messages, options);
    string fullResponse = "";

    await foreach (var update in streamingResponse)
    {
        if (update.ContentUpdate.Count > 0)
        {
            var content = update.ContentUpdate[0].Text;
            Console.Write(content);
            fullResponse += content;
        }
    }
    Console.WriteLine();

    // Add AI response to conversation for context
    messages.Add(new AssistantChatMessage(fullResponse));
    Console.WriteLine();
}
