using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using DotNetEnv;

// Sample 06a: Microsoft Agent Framework .NET Streaming CLI Application
//
// Simple command-line chat with Azure OpenAI using the Microsoft Agent Framework
// with streaming responses. Watch AI responses appear in real-time.
// Type messages, type 'quit' to exit.
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
        Name = "StreamingChatAgent",
        ChatOptions = chatOptions
    });

Console.WriteLine("🤖 AI Foundry Agent Framework Streaming Chat");
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

    // Stream the agent's response token by token
    Console.Write("AI: ");

    await foreach (var chunk in agent.RunStreamingAsync(userInput, session))
    {
        Console.Write(chunk);
    }

    Console.WriteLine("\n");
}
