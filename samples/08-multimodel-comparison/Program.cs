using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.AI.Inference;
using Azure.Identity;
using Azure.Core;
using DotNetEnv;

// Sample 08: Multi-Model AI Foundry Comparison
//
// Compare responses from different models in AI Foundry for the same prompt.
// Shows how different models approach the same question.

// Load environment variables
Env.Load("../../.env");

// Get configuration from environment
var foundryEndpoint = Environment.GetEnvironmentVariable("AI_FOUNDRY_ENDPOINT")!;

// Create project endpoint from foundry endpoint
var projectEndpoint = foundryEndpoint.Replace("https://ai.azure.com", "https://").Replace("/", "") + ".inference.ai.azure.com";

// Set up authentication with DefaultAzureCredential and token policy
var credential = new DefaultAzureCredential();
var tokenRequestContext = new TokenRequestContext(new[] { "https://ai.azure.com/.default" });
var tokenPolicy = new BearerTokenAuthenticationPolicy(credential, tokenRequestContext);

// Create ChatCompletionsClient with project endpoint and authentication
var chatClient = new ChatCompletionsClient(
    new Uri($"https://{projectEndpoint}"),
    credential,
    new ChatCompletionsClientOptions
    {
        NetworkTimeout = TimeSpan.FromMinutes(5)
    });

Console.WriteLine("🔄 Multi-Model AI Foundry Comparison");
Console.WriteLine("Compare how different models respond to the same prompt\n");

// Define models to compare (you can modify these based on your deployments)
var models = new[]
{
    "gpt-4o", // Primary model
    "gpt-4o-mini", // Faster/cheaper model
    "gpt-35-turbo" // Alternative model
};

// Get user prompt
Console.Write("Enter your prompt: ");
var userPrompt = Console.ReadLine()?.Trim();

if (string.IsNullOrEmpty(userPrompt))
{
    Console.WriteLine("No prompt provided. Exiting.");
    return;
}

Console.WriteLine($"\nComparing responses from {models.Length} models...\n");

// Test each model
foreach (var model in models)
{
    Console.WriteLine($"🤖 {model.ToUpper()}");
    Console.WriteLine(new string('─', 50));

    try
    {
        // Create completion options for this model
        var requestOptions = new ChatCompletionsOptions()
        {
            Model = model,
            Temperature = 0.7f,
            MaxTokens = 300
        };

        // Add system and user messages
        requestOptions.Messages.Add(new ChatRequestSystemMessage("You are a helpful AI assistant. Provide concise, accurate responses."));
        requestOptions.Messages.Add(new ChatRequestUserMessage(userPrompt));

        // Get response from this model
        var response = await chatClient.CompleteAsync(requestOptions);
        var aiResponse = response.Value.Content;

        Console.WriteLine(aiResponse);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error with {model}: {ex.Message}");
    }

    Console.WriteLine($"\n{new string('═', 60)}\n");
}

Console.WriteLine("✅ Comparison complete!");
