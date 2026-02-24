# Sample 04a: OpenAI v1 Endpoint - .NET CLI

This sample demonstrates using Azure OpenAI's new `/openai/v1/` endpoint from .NET. This is the new versioned endpoint that **does not require an `api-version` query parameter**.

## Key Difference from Sample 04

| | Sample 04 (Classic) | Sample 04a (v1 Endpoint) |
|---|---|---|
| **Client** | `AzureOpenAIClient` (from `Azure.AI.OpenAI`) | `OpenAIClient` (from `OpenAI`) |
| **Endpoint path** | `/openai/deployments/{name}/chat/completions?api-version=...` | `/openai/v1/chat/completions` |
| **API version** | Required in query string | Not needed |
| **Auth** | `DefaultAzureCredential` passed directly | Token from `DefaultAzureCredential` passed as API key |

The `/openai/v1` endpoint is OpenAI-API-compatible, so you use the standard `OpenAI` NuGet package instead of `Azure.AI.OpenAI`. The model/deployment name is specified via `GetChatClient()` just like with OpenAI directly.

See: [API Evolution](https://learn.microsoft.com/en-us/azure/ai-foundry/openai/api-version-lifecycle#api-evolution)

## Prerequisites

- .NET 8.0 SDK
- Azure OpenAI resource with a deployed model
- `az login` completed (for DefaultAzureCredential)
- `.env` file configured at the repo root

## Run

```bash
dotnet run
```
