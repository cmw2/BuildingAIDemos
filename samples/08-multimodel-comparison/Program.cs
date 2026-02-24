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
var endpoint = new Uri(Environment.GetEnvironmentVariable("AI_FOUNDRY_INFERENCE_ENDPOINT")!);

// Set up authentication with DefaultAzureCredential and custom token scope
var credential = new DefaultAzureCredential();
var tokenCredential = new TokenCredentialWrapper(credential);

// Create ChatCompletionsClient with inference endpoint and authentication
var chatClient = new ChatCompletionsClient(endpoint, tokenCredential);

Console.WriteLine("🔄 Multi-Model AI Foundry Comparison");
Console.WriteLine("Compare how different models respond to the same prompt\n");

// Define models to compare (you can modify these based on your deployments)
var models = new[]
{
    "gpt-5.1", // Primary model
    "gpt-5-mini", // Faster/cheaper model
    "gpt-5.2-chat" // Alternative model
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
            Model = model
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
