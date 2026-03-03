using Azure.Identity;
using DotNetEnv;
using OpenAI;
using OpenAI.Responses;
using System.ClientModel;

// Load environment variables
Env.Load("../../.env");

var connectionString = Environment.GetEnvironmentVariable("AI_FOUNDRY_PROJECT_CONNECTION_STRING")
    ?? throw new InvalidOperationException("AI_FOUNDRY_PROJECT_CONNECTION_STRING required. Check .env file.");
var agentAppName = Environment.GetEnvironmentVariable("AGENT_APP_NAME")
    ?? throw new InvalidOperationException("AGENT_APP_NAME required. Check .env file.");
var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");

// Build the published agent's OpenAI-compatible endpoint
var endpoint = $"{connectionString.TrimEnd('/')}/applications/{agentAppName}/protocols/openai";

Console.WriteLine($"Calling published agent: {agentAppName}");
Console.WriteLine($"Endpoint: {endpoint}");
Console.WriteLine();

// Authenticate with Entra ID (scope: https://ai.azure.com/.default)
var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions { TenantId = tenantId });
var token = await credential.GetTokenAsync(
    new Azure.Core.TokenRequestContext(["https://ai.azure.com/.default"]));

// Create the OpenAI Responses client pointing at the published agent endpoint
var client = new ResponsesClient(
    model: "gpt-4.1",  // model is configured on the agent; this value may be ignored
    credential: new ApiKeyCredential(token.Token),
    options: new OpenAIClientOptions { Endpoint = new Uri(endpoint) });

Console.WriteLine("Type your message (or 'quit' to exit):");
Console.WriteLine();

string? previousResponseId = null;

while (true)
{
    Console.Write("You: ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input) || input.Equals("quit", StringComparison.OrdinalIgnoreCase))
        break;

    try
    {
        var result = await client.CreateResponseAsync(input, previousResponseId);
        var response = result.Value;

        // Track the response ID for multi-turn conversation
        previousResponseId = response.Id;

        // Extract text from output items
        var outputText = string.Join("", response.OutputItems
            .OfType<MessageResponseItem>()
            .SelectMany(m => m.Content)
            .Where(c => c.Kind == ResponseContentPartKind.OutputText)
            .Select(c => c.Text));

        Console.WriteLine($"Agent: {outputText}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
    Console.WriteLine();
}
