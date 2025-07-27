using Azure;
using Azure.AI.OpenAI;
using DotNetEnv;
using OpenAI.Chat;

// Load environment variables
Env.Load("../../../.env");

// Get configuration
var endpoint = new Uri(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!);
var apiKey = new AzureKeyCredential(Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")!);
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")!;

// Create client
var client = new AzureOpenAIClient(endpoint, apiKey);
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

Console.WriteLine("🤖 Simple AI Chat (Streaming) - Type 'quit' to exit");
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
