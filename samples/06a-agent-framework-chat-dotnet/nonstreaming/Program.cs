using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using DotNetEnv;

// Sample 06a: Microsoft Agent Framework .NET CLI Application
//
// Simple command-line chat with Azure OpenAI using the Microsoft Agent Framework.
// This is the non-streaming version. Type messages, get AI responses, type 'quit' to exit.
//
// The Agent Framework is the successor to Semantic Kernel and AutoGen, combining
// simple abstractions with enterprise-grade features like session management,
// middleware, and tool support.

// Load environment variables
Env.Load("../../../.env");

// Get configuration from environment
var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!;
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")!;

// Create an Azure OpenAI client and build an AI agent from it
var client = new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential());
var chatClient = client.GetChatClient(deploymentName);

// Configure chat options (instructions, temperature, max tokens, etc.)
var chatOptions = new ChatOptions
{
    Instructions = "You are a helpful AI assistant.",
    Temperature = 0.7f,
    MaxOutputTokens = 500
};

AIAgent agent = chatClient.AsAIAgent(
    new ChatClientAgentOptions
    {
        Name = "ChatAgent",
        ChatOptions = chatOptions
    });

Console.WriteLine("🤖 AI Foundry Agent Framework Chat");
Console.WriteLine("Type 'quit' to exit\n");

// Create a session for multi-turn conversation (maintains chat history)
var session = await agent.CreateSessionAsync();

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

    // Run the agent with the user's message; the session keeps conversation history
    var response = await agent.RunAsync(userInput, session);

    Console.WriteLine($"AI: {response}\n");
}
