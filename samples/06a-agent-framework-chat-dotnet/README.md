# Sample 06a: Microsoft Agent Framework Chat (.NET)

This sample demonstrates basic chat completions using the **Microsoft Agent Framework** — the successor to Semantic Kernel and AutoGen. It mirrors sample 06 (Semantic Kernel) but uses the new Agent Framework APIs instead.

## Overview

The Agent Framework combines AutoGen's simple abstractions with Semantic Kernel's enterprise features (session management, middleware, telemetry). For basic chat, the API is remarkably simple:

```csharp
AIAgent agent = new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential())
    .GetChatClient(deploymentName)
    .AsAIAgent(instructions: "You are a helpful AI assistant.");

Console.WriteLine(await agent.RunAsync("Hello!"));
```

## Variants

| Folder | Description |
|--------|-------------|
| `nonstreaming/` | Full response returned at once via `agent.RunAsync()` |
| `streaming/` | Token-by-token output via `agent.RunStreamingAsync()` |

Both variants maintain multi-turn conversation history using `AgentSession`.

## Files

```
06a-agent-framework-chat-dotnet/
├── nonstreaming/
│   ├── AgentFramework.NonStreaming.Sample.csproj
│   └── Program.cs
├── streaming/
│   ├── AgentFramework.Streaming.Sample.csproj
│   └── Program.cs
└── README.md
```

## Prerequisites

- .NET 8.0+
- Azure OpenAI deployment
- `DefaultAzureCredential` access (e.g. `az login`)

### Environment Variables (in root `.env`)

```
AZURE_OPENAI_ENDPOINT=https://<your-resource>.openai.azure.com
AZURE_OPENAI_DEPLOYMENT_NAME=gpt-4.1
```

## Running

```bash
# Non-streaming
cd samples/06a-agent-framework-chat-dotnet/nonstreaming
dotnet run

# Streaming
cd samples/06a-agent-framework-chat-dotnet/streaming
dotnet run
```

## Key Differences from Sample 06 (Semantic Kernel)

| Aspect | Sample 06 (Semantic Kernel) | Sample 06a (Agent Framework) |
|--------|----------------------------|------------------------------|
| Agent creation | `Kernel.CreateBuilder()` + `IChatCompletionService` | `chatClient.AsAIAgent()` |
| Chat history | Manual `ChatHistory` object | Automatic via `AgentSession` |
| Non-streaming | `GetChatMessageContentAsync()` | `RunAsync()` |
| Streaming | `GetStreamingChatMessageContentsAsync()` | `RunStreamingAsync()` |
| Packages | `Microsoft.SemanticKernel` | `Microsoft.Agents.AI.OpenAI` + `Azure.AI.OpenAI` |

## SDK Packages

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.Agents.AI.OpenAI` | 1.0.0-rc1 | Agent Framework with OpenAI support |
| `Azure.AI.OpenAI` | 2.8.0-beta.1 | Azure OpenAI client |
| `Azure.Identity` | 1.17.1 | `DefaultAzureCredential` |
