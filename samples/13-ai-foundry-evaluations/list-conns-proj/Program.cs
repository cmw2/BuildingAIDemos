using Azure.AI.Projects;
using Azure.Identity;
using Azure.Core;
using DotNetEnv;

Env.Load("../../../.env");
var projectEndpoint = Environment.GetEnvironmentVariable("AI_FOUNDRY_PROJECT_CONNECTION_STRING")!;
var credential = new DefaultAzureCredential();
var tokenCredential = new TokenCredentialWrapper(credential);
var projectClient = new AIProjectClient(new Uri(projectEndpoint), tokenCredential);

Console.WriteLine("All connections:");
var connections = projectClient.Connections.GetConnections();
foreach (var conn in connections)
{
    Console.WriteLine($"  Name={conn.Name}, Type={conn.Type}, Target={conn.Target}");
}

try
{
    var conn = await projectClient.Connections.GetDefaultAsync(ConnectionType.AzureOpenAI, includeCredentials: true);
    Console.WriteLine($"\nDefault AzureOpenAI: Name={conn.Name}, Type={conn.Type}, Target={conn.Target}");
}
catch (Exception ex) { Console.WriteLine($"\nDefault AzureOpenAI: Error - {ex.Message}"); }

public class TokenCredentialWrapper : TokenCredential
{
    private readonly TokenCredential _inner;
    public TokenCredentialWrapper(TokenCredential inner) => _inner = inner;
    public override AccessToken GetToken(TokenRequestContext ctx, CancellationToken ct) =>
        _inner.GetToken(new TokenRequestContext(new[] { "https://ai.azure.com/.default" }), ct);
    public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext ctx, CancellationToken ct) =>
        _inner.GetTokenAsync(new TokenRequestContext(new[] { "https://ai.azure.com/.default" }), ct);
}
