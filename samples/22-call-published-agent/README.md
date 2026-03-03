# Sample 22 – Call a Published Agent (Responses API)

This sample shows how to call a **published agent** in Azure AI Foundry using the standard OpenAI .NET SDK's `ResponsesClient` (Responses API).

## How It Works

1. Authenticates with Entra ID (`DefaultAzureCredential`) using scope `https://ai.azure.com/.default`
2. Creates an `OpenAI.Responses.ResponsesClient` pointing at the published agent's OpenAI-compatible endpoint
3. Sends user input via `CreateResponseAsync` and displays the agent's response
4. Supports multi-turn conversation by tracking `PreviousResponseId`

## Prerequisites

- A published agent application in Azure AI Foundry
- Your user account must have the **Azure AI User** role on the Foundry project
- .env file with:
  - `AI_FOUNDRY_PROJECT_CONNECTION_STRING` – e.g. `https://<account>.services.ai.azure.com/api/projects/<project>`
  - `AGENT_APP_NAME` – the published agent application name (e.g. `mysimpleagent`)
  - `AZURE_TENANT_ID` – your Entra ID tenant

## Run

```bash
dotnet run
```

## Endpoint Pattern

The published agent exposes an OpenAI-compatible endpoint at:

```
{connectionString}/applications/{appName}/protocols/openai
```

The SDK appends `/responses` automatically when calling the Responses API.
