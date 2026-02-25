using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using OpenAI.Chat;
using Microsoft.Extensions.Logging;
using DotNetEnv;

// Sample 20: MCP Client with Agent Framework
//
// Demonstrates how to connect to multiple MCP (Model Context Protocol) servers
// and use their tools with the Microsoft Agent Framework.
//
// This sample combines two key technologies:
// - MCP Client: Discovers and invokes tools from multiple MCP servers:
//     1. Sample 19's Weather Server (local, stdio transport)
//     2. Microsoft Learn MCP Server (remote, Streamable HTTP transport)
// - Agent Framework: Orchestrates the AI agent with automatic tool calling
//
// The MCP client discovers tools dynamically from each server — no local tool
// definitions needed. McpClientTool extends AITool, so MCP tools plug directly
// into the Agent Framework's ChatOptions.Tools with zero adaptation code.
//
// It also discovers MCP prompts — reusable prompt templates that servers
// provide to guide the LLM on how to best use their tools. For example, the
// weather server provides a "time_in_location" prompt that instructs the LLM to
// fetch UTC time and do timezone math, rather than embedding that logic
// in the client's system prompt.
//
// Compare this to Sample 11a which defines tools locally with AIFunctionFactory.
// Here, the tools live in separate MCP server processes and remote services.

// Load environment variables
Env.Load("../../.env");

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!;
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")!;

Console.WriteLine("🔌 MCP Client + Agent Framework (Multi-Server)");
Console.WriteLine("Connecting to MCP servers...\n");

// --- Step 1a: Connect to the local MCP Weather Server ---
// Launch the Python MCP server as a child process using stdio transport.
// The MCP client communicates via stdin/stdout JSON-RPC messages.
Console.WriteLine("  [1/2] Connecting to Weather MCP Server (local, stdio)...");
await using var weatherMcpClient = await McpClientFactory.CreateAsync(
    new StdioClientTransport(new StdioClientTransportOptions
    {
        Command = "python",
        Arguments = ["../19-mcp-weather-server/server.py"],
        Name = "WeatherMcpServer"
    }));

// --- Step 1b: Connect to the remote Microsoft Learn MCP Server ---
// Connect to the cloud-hosted MS Learn MCP server using Streamable HTTP transport.
// No API key or authentication required — it's a free, public MCP server.
Console.WriteLine("  [2/2] Connecting to Microsoft Learn MCP Server (remote, HTTP)...");
await using var learnMcpClient = await McpClientFactory.CreateAsync(
    new SseClientTransport(new SseClientTransportOptions
    {
        Endpoint = new Uri("https://learn.microsoft.com/api/mcp"),
        Name = "MicrosoftLearnMcpServer",
        TransportMode = HttpTransportMode.StreamableHttp
    }));

// --- Step 2: Discover tools from all MCP servers ---
// ListToolsAsync returns McpClientTool objects which extend AITool,
// making them directly compatible with the Agent Framework.
var weatherTools = await weatherMcpClient.ListToolsAsync();
var learnTools = await learnMcpClient.ListToolsAsync();
var allTools = weatherTools.Concat(learnTools).ToList();

Console.WriteLine($"\n✅ Connected! Discovered {allTools.Count} tools across {2} MCP servers:");
Console.WriteLine($"\n  🌤️ Weather Server ({weatherTools.Count} tools):");
foreach (var tool in weatherTools)
{
    Console.WriteLine($"     - {tool.Name}: {tool.Description}");
}
Console.WriteLine($"\n  📚 Microsoft Learn Server ({learnTools.Count} tools):");
foreach (var tool in learnTools)
{
    Console.WriteLine($"     - {tool.Name}: {tool.Description}");
}

// --- Step 2b: Discover prompts from MCP servers ---
// MCP prompts are reusable templates servers provide to guide the LLM.
// The server knows how its tools work best, so it can provide prompts that
// get reliable results — like getting UTC time and doing timezone conversion.
var weatherPrompts = await weatherMcpClient.ListPromptsAsync();
// The Learn server currently provides tools only, but we check for prompts
// in case they add them in the future (dynamic discovery is key to MCP).
IList<McpClientPrompt> learnPrompts;
try
{
    learnPrompts = await learnMcpClient.ListPromptsAsync();
}
catch
{
    learnPrompts = [];
}
var allPrompts = weatherPrompts.Concat(learnPrompts).ToList();

// Track which MCP client owns which prompt so we can call the right server.
var promptClientMap = new Dictionary<string, IMcpClient>(StringComparer.OrdinalIgnoreCase);
foreach (var p in weatherPrompts) promptClientMap[p.Name] = weatherMcpClient;
foreach (var p in learnPrompts) promptClientMap[p.Name] = learnMcpClient;

if (allPrompts.Count > 0)
{
    Console.WriteLine($"\n📋 Discovered {allPrompts.Count} prompts (use as slash commands):");
    foreach (var prompt in allPrompts)
    {
        var promptArgDisplay = string.Join(", ", prompt.ProtocolPrompt.Arguments?.Select(a => $"<{a.Name}>") ?? []);
        Console.WriteLine($"   /{prompt.Name} {promptArgDisplay}");
        Console.WriteLine($"     {prompt.Description}");
    }
}
Console.WriteLine();

// --- Step 3: Create Azure OpenAI client ---
var client = new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential());
var chatClient = client.GetChatClient(deploymentName);

// --- Step 4: Configure chat options with MCP tools ---
// Tools from all MCP servers are merged into a single list.
// The MCP tools are passed directly as AITool instances — no wrapping needed.
var chatOptions = new ChatOptions
{
    Instructions = """
        You are a helpful AI assistant with access to tools from multiple MCP servers:
        
        1. **Weather Server** — real-time weather and date/time tools:
           - Current weather conditions for any location
           - Weather forecasts for upcoming days
           - Current date and time
        
        2. **Microsoft Learn Server** — official Microsoft documentation tools:
           - Search Microsoft technical documentation
           - Fetch full documentation pages
           - Search for official code samples
        
        Use the available tools to provide accurate, up-to-date information.
        When presenting weather data, format it nicely for the user.
        Note that weather temperatures are in Fahrenheit by default.
        When citing Microsoft documentation, include the relevant URLs.
        """,
    Temperature = 0.7f,
    MaxOutputTokens = 800,
    Tools = [.. allTools]
};

// --- Step 5: Build the Agent Framework agent ---
// Same pattern as Sample 11a — the agent automatically handles the tool call loop,
// but now the tools come from multiple external MCP servers.
//
// Add a console logger so we can see tool invocations in the output.
// The clientFactory parameter lets us insert FunctionInvokingChatClient into
// the pipeline — it handles tool calling AND logs each invocation automatically
// (function name, arguments, duration, result) with zero custom wrapper code.
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Trace);
});

AIAgent agent = chatClient.AsAIAgent(
    new ChatClientAgentOptions
    {
        Name = "McpMultiServerAgent",
        ChatOptions = chatOptions
    },
    clientFactory: inner => new ChatClientBuilder(inner)
        .UseFunctionInvocation(loggerFactory)
        .Build(),
    loggerFactory: loggerFactory);

Console.WriteLine("💬 Examples:");
Console.WriteLine("  🌤️  What's the weather in Seattle?");
Console.WriteLine("  🌤️  Give me a 5-day forecast for New York");
Console.WriteLine("  🌤️  What time is it?");
Console.WriteLine("  📚 Search Microsoft Learn for Azure Functions best practices");
Console.WriteLine("  📚 How do I use dependency injection in ASP.NET Core?");
Console.WriteLine();
Console.WriteLine("📋 Or use slash commands for MCP prompts:");
Console.WriteLine("  /time_in_location Tokyo");
Console.WriteLine("  /daily_briefing Seattle, WA");
Console.WriteLine("  /weather_comparison Seattle, WA | Miami, FL\n");

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
        Console.WriteLine("👋 Goodbye!");
        break;
    }

    try
    {
        // Check if the user typed a slash command to invoke an MCP prompt.
        // Per the MCP spec, prompts are "user-controlled" — the user explicitly
        // selects them, typically via slash commands or a UI menu.
        var messageToSend = userInput;

        if (userInput.StartsWith('/'))
        {
            var promptMessage = await TryInvokeMcpPromptAsync(allPrompts, promptClientMap, userInput);
            if (promptMessage != null)
            {
                messageToSend = promptMessage;
                Console.WriteLine("📋 [DEBUG] Invoking MCP prompt from server");
            }
            else
            {
                Console.WriteLine("❓ Unknown slash command. Available prompts:");
                foreach (var p in allPrompts)
                {
                    var promptArgHelp = string.Join(", ", p.ProtocolPrompt.Arguments?.Select(a => $"<{a.Name}>") ?? []);
                    Console.WriteLine($"   /{p.Name} {promptArgHelp}");
                }
                Console.WriteLine();
                continue;
            }
        }

        // Run the agent — it automatically handles:
        // 1. Sending the message to the model
        // 2. Detecting tool call requests from the model
        // 3. Invoking MCP tools (via the MCP client → server round-trip)
        // 4. Sending tool results back to the model
        // 5. Returning the final response
        var response = await agent.RunAsync(messageToSend, session);

        Console.WriteLine($"🤖 Assistant: {response}\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error: {ex.Message}\n");
    }
}

// --- Slash Command Handler ---
// Parses slash commands and invokes the corresponding MCP prompt.
// Per the MCP spec, prompts are designed to be user-controlled — the user
// explicitly selects them (e.g., via slash commands), and the server provides
// structured prompt templates that guide the LLM on how to best use its tools.
//
// Syntax:
//   /prompt_name arg1 | arg2        (arguments separated by |)
//   /prompt_name arg1               (single argument)
//   /time_in_location Tokyo
//   /weather_comparison Seattle | Miami

static async Task<string?> TryInvokeMcpPromptAsync(
    IList<McpClientPrompt> prompts,
    Dictionary<string, IMcpClient> promptClientMap,
    string input)
{
    // Parse: /prompt_name arg1 | arg2 | ...
    var spaceIdx = input.IndexOf(' ');
    var promptName = (spaceIdx > 0 ? input[1..spaceIdx] : input[1..]).Trim();
    var argString = spaceIdx > 0 ? input[(spaceIdx + 1)..].Trim() : "";

    // Find the matching prompt
    var prompt = prompts.FirstOrDefault(p =>
        p.Name.Equals(promptName, StringComparison.OrdinalIgnoreCase));

    if (prompt == null)
        return null;

    // Look up which MCP client owns this prompt
    if (!promptClientMap.TryGetValue(prompt.Name, out var mcpClient))
        return null;

    // Build arguments dictionary by matching positional values to prompt argument definitions
    var argValues = argString.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    var promptArgs = prompt.ProtocolPrompt.Arguments?.ToList() ?? [];
    var argsDict = new Dictionary<string, object?>();

    for (int i = 0; i < promptArgs.Count; i++)
    {
        var value = i < argValues.Length ? argValues[i] : "";
        argsDict[promptArgs[i].Name] = value;
    }

    // Call the correct MCP server to get the prompt template
    var promptResult = await mcpClient.GetPromptAsync(promptName, argsDict);

    // Extract text from the first message
    var message = promptResult.Messages.FirstOrDefault();
    if (message?.Content is TextContentBlock textContent)
    {
        return textContent.Text;
    }

    return null;
}
