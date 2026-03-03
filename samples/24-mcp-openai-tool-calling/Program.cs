using System.Text.Json;
using Azure.AI.OpenAI;
using Azure.Identity;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using OpenAI.Chat;
using DotNetEnv;

// Sample 24: MCP + OpenAI SDK Tool Calling (C#)
//
// Demonstrates how to use a remote MCP server as the tool provider for
// OpenAI SDK manual tool calling. The key piece is converting MCP tool
// definitions into OpenAI-compatible ChatTool function schemas — and
// routing the model's tool call requests back to the MCP server.
//
// This bridges two worlds:
//   - MCP (Model Context Protocol): Dynamically discovers tools from a remote server
//   - OpenAI SDK: Manual tool calling loop with ChatTool function definitions
//
// The interesting conversion:
//   MCP Tool.InputSchema (JSON Schema) → ChatTool.CreateFunctionTool(functionParameters)
//   They're both JSON Schema — so the mapping is direct, no transformation needed!
//
// Compare to:
//   - Sample 10: Manual tool calling with locally-defined functions
//   - Sample 20: MCP client with Agent Framework (automatic tool loop)
//   - Sample 21: The remote MCP weather server we're calling here

// Load environment variables
Env.Load("../../.env");

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!;
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")!;

// The MCP endpoint from Sample 21's deployed weather server.
// Override with MCP_WEATHER_ENDPOINT env var, or run sample 21 locally
// (e.g., "http://localhost:5000/mcp").
var mcpEndpoint = Environment.GetEnvironmentVariable("MCP_WEATHER_ENDPOINT")
    ?? "https://app-mcp-weather-demo-cmw.azurewebsites.net/mcp";

Console.WriteLine("🔌 MCP + OpenAI SDK Tool Calling");
Console.WriteLine($"   Connecting to MCP server at {mcpEndpoint}...\n");

// ──────────────────────────────────────────────────────────────
// Step 1: Connect to the remote MCP Weather Server
// ──────────────────────────────────────────────────────────────
// Uses Streamable HTTP transport to connect to the server deployed in Sample 21.
await using var mcpClient = await McpClient.CreateAsync(
    new HttpClientTransport(new HttpClientTransportOptions
    {
        Endpoint = new Uri(mcpEndpoint),
        Name = "WeatherMcpServer"
    }));

// ──────────────────────────────────────────────────────────────
// Step 2: Discover tools from the MCP server
// ──────────────────────────────────────────────────────────────
var mcpTools = await mcpClient.ListToolsAsync();

Console.WriteLine($"✅ Connected! Discovered {mcpTools.Count} tools:\n");

// ──────────────────────────────────────────────────────────────
// Step 3: Convert MCP tools → OpenAI ChatTool format
// ──────────────────────────────────────────────────────────────
// This is the KEY BRIDGE between MCP and the OpenAI SDK:
//
//   MCP Tool definition:
//     - Name (string)
//     - Description (string)
//     - InputSchema (JsonElement — a JSON Schema object)
//
//   OpenAI ChatTool.CreateFunctionTool:
//     - functionName (string)
//     - functionDescription (string)
//     - functionParameters (BinaryData — a JSON Schema object)
//
// Both use JSON Schema for parameters, so the conversion is trivial!

var chatTools = new List<ChatTool>();

foreach (var mcpTool in mcpTools)
{
    // Access the underlying MCP protocol Tool object
    var protocolTool = mcpTool.ProtocolTool;

    // Convert MCP InputSchema (JsonElement) → BinaryData for OpenAI SDK.
    // MCP's InputSchema is already a valid JSON Schema — no transformation needed.
    var chatTool = ChatTool.CreateFunctionTool(
        functionName: protocolTool.Name,
        functionDescription: protocolTool.Description ?? "",
        functionParameters: BinaryData.FromString(
            protocolTool.InputSchema.GetRawText()));

    chatTools.Add(chatTool);

    Console.WriteLine($"  📋 {protocolTool.Name}: {protocolTool.Description}");
    Console.WriteLine($"     Schema: {protocolTool.InputSchema.GetRawText()}\n");
}

// ──────────────────────────────────────────────────────────────
// Step 4: Create Azure OpenAI client
// ──────────────────────────────────────────────────────────────
var credential = new DefaultAzureCredential();
var client = new AzureOpenAIClient(new Uri(endpoint), credential);
var chatClient = client.GetChatClient(deploymentName);

Console.WriteLine("💬 Ask about weather, forecasts, or the current time!");
Console.WriteLine("   Examples:");
Console.WriteLine("   - What's the weather in Seattle?");
Console.WriteLine("   - Give me a 5-day forecast for New York");
Console.WriteLine("   - What time is it?");
Console.WriteLine("   Type 'quit' to exit.\n");

// ──────────────────────────────────────────────────────────────
// Step 5: Chat loop with manual tool calling
// ──────────────────────────────────────────────────────────────
// Same pattern as Sample 10, but instead of calling local functions,
// we forward tool calls to the remote MCP server.
var messages = new List<ChatMessage>
{
    new SystemChatMessage(
        "You are a helpful assistant with access to weather and time tools. " +
        "Use the available functions when the user asks about weather, forecasts, or the current time. " +
        "When presenting weather data, format it nicely. " +
        "Temperatures are in Fahrenheit by default.")
};

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

    messages.Add(new UserChatMessage(userInput));

    try
    {
        await ProcessToolCallLoop(chatClient, chatTools, mcpClient, messages);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error: {ex.Message}");
    }

    Console.WriteLine();
}


// ──────────────────────────────────────────────────────────────
// The Manual Tool Call Loop
// ──────────────────────────────────────────────────────────────
// Same structure as Sample 10's loop, but when the model requests
// a function call, we forward it to the MCP server instead of
// calling a local C# method.

static async Task ProcessToolCallLoop(
    ChatClient chatClient,
    List<ChatTool> chatTools,
    McpClient mcpClient,
    List<ChatMessage> messages)
{
    // Build chat options with our MCP-converted tools
    var chatOptions = new ChatCompletionOptions();
    foreach (var tool in chatTools)
    {
        chatOptions.Tools.Add(tool);
    }

    var response = await chatClient.CompleteChatAsync(messages, chatOptions);

    // Keep processing until the model stops requesting tools
    while (response.Value.FinishReason == ChatFinishReason.ToolCalls)
    {
        Console.WriteLine("🔧 AI is calling MCP tools...");

        // Add the assistant's tool-call message to conversation history
        messages.Add(new AssistantChatMessage(response.Value));

        // Process each tool call — forward to the MCP server
        foreach (var toolCall in response.Value.ToolCalls)
        {
            var functionName = toolCall.FunctionName;
            var functionArgs = toolCall.FunctionArguments;

            Console.WriteLine($"  📞 Calling MCP tool: {functionName}({functionArgs})");

            // Parse the OpenAI function arguments (JSON string) into a
            // dictionary that the MCP client expects.
            var argsDict = new Dictionary<string, object?>();
            if (functionArgs != null)
            {
                using var doc = JsonDocument.Parse(functionArgs);
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    argsDict[prop.Name] = prop.Value.ValueKind switch
                    {
                        JsonValueKind.String => prop.Value.GetString(),
                        JsonValueKind.Number => prop.Value.TryGetInt32(out var i) ? i : prop.Value.GetDouble(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        _ => prop.Value.GetRawText()
                    };
                }
            }

            // ★ Forward the tool call to the remote MCP server ★
            // This is where MCP shines — the tool execution happens
            // on the server, we just pass the name and arguments.
            var mcpResult = await mcpClient.CallToolAsync(functionName, argsDict);

            // Extract text content from the MCP response.
            // MCP tool results contain a list of content blocks;
            // weather tools return text content with JSON payloads.
            var resultText = string.Join("\n",
                mcpResult.Content
                    .OfType<TextContentBlock>()
                    .Select(c => c.Text ?? ""));

            if (string.IsNullOrEmpty(resultText))
            {
                // Fallback: serialize the full content list
                resultText = JsonSerializer.Serialize(mcpResult.Content);
            }

            Console.WriteLine($"  📋 MCP Result: {resultText}");

            // Feed the result back to the model as a tool response
            messages.Add(new ToolChatMessage(toolCall.Id, resultText));
        }

        // Get the next response (may trigger more tool calls or a final answer)
        response = await chatClient.CompleteChatAsync(messages, chatOptions);
    }

    // Display the final response
    switch (response.Value.FinishReason)
    {
        case ChatFinishReason.Stop:
            var text = response.Value.Content.Count > 0
                ? response.Value.Content[0].Text
                : "I'm not sure how to help with that.";
            Console.WriteLine($"🤖 Assistant: {text}");
            break;

        case ChatFinishReason.Length:
            var truncated = response.Value.Content.Count > 0
                ? response.Value.Content[0].Text
                : "";
            Console.WriteLine($"🤖 Assistant: {truncated}");
            Console.WriteLine("⚠️  (Response was truncated due to length limit)");
            break;

        case ChatFinishReason.ContentFilter:
            Console.WriteLine("🚫 Response was filtered due to content policy.");
            break;

        default:
            Console.WriteLine($"🤔 Unexpected finish reason: {response.Value.FinishReason}");
            break;
    }

    // Add the final assistant response to conversation history
    messages.Add(new AssistantChatMessage(response.Value));
}
