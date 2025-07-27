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

Console.WriteLine("🤖 Simple AI Chat - Type 'quit' to exit");
Console.WriteLine();

while (true)
{
    Console.Write("You: ");
    var userInput = Console.ReadLine();
    
    if (userInput?.ToLower() is "quit" or "exit" or "q")
        break;

    // Add user message to conversation
    messages.Add(new UserChatMessage(userInput!));

    var response = await chatClient.CompleteChatAsync(messages, options);
    
    var aiResponse = response.Value.Content[0].Text;
    Console.WriteLine($"AI: {aiResponse}");
    
    // Add AI response to conversation for context
    messages.Add(new AssistantChatMessage(aiResponse));
    Console.WriteLine();
}
