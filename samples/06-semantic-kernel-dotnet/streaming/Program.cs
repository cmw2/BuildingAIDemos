using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using DotNetEnv;

// Sample 06b: Semantic Kernel .NET Streaming CLI Application
//
// Simple command-line chat with AI Foundry using Semantic Kernel with streaming responses.
// Watch AI responses appear in real-time, type 'quit' to exit.

// Load environment variables
Env.Load("../../../.env");

// Get configuration from environment
var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!;
var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")!;
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")!;

// Create Semantic Kernel with Azure OpenAI
var builder = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(
        deploymentName: deploymentName,
        endpoint: endpoint,
        apiKey: apiKey);

var kernel = builder.Build();

// Get chat completion service
var chatService = kernel.GetRequiredService<IChatCompletionService>();

Console.WriteLine("🤖 AI Foundry Semantic Kernel Streaming Chat");
Console.WriteLine("Type 'quit' to exit\n");

// Initialize chat history with system message
var chatHistory = new ChatHistory("You are a helpful AI assistant.");

// Create execution settings once
var executionSettings = new OpenAIPromptExecutionSettings 
{ 
    Temperature = 0.7f, 
    MaxTokens = 500 
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
    chatHistory.AddUserMessage(userInput);

    // Get streaming AI response
    Console.Write("AI: ");
    
    var fullResponse = "";
    await foreach (var chunk in chatService.GetStreamingChatMessageContentsAsync(
        chatHistory, 
        executionSettings))
    {
        if (chunk.Content != null)
        {
            Console.Write(chunk.Content);
            fullResponse += chunk.Content;
        }
    }
    
    Console.WriteLine("\n");

    // Add AI response to chat history
    chatHistory.AddAssistantMessage(fullResponse);
}
