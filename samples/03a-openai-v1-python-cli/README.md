# Sample 03a: OpenAI v1 Endpoint - Python CLI

This sample demonstrates using Azure OpenAI's new `/openai/v1/` endpoint from Python. This is the new versioned endpoint that **does not require an `api-version` query parameter**.

## Key Difference from Sample 03

| | Sample 03 (Classic) | Sample 03a (v1 Endpoint) |
|---|---|---|
| **Client** | `AzureOpenAI` | `OpenAI` (standard) |
| **Endpoint path** | `/openai/deployments/{name}/chat/completions?api-version=...` | `/openai/v1/chat/completions` |
| **API version** | Required | Not needed |
| **Auth** | `get_bearer_token_provider()` | Token from `DefaultAzureCredential` passed as `api_key` |

The `/openai/v1` endpoint is OpenAI-API-compatible, so you use the standard `OpenAI` client instead of `AzureOpenAI`.

See: [API Evolution](https://learn.microsoft.com/en-us/azure/ai-foundry/openai/api-version-lifecycle#api-evolution)

## Prerequisites

- Python 3.10+
- `az login` completed (for DefaultAzureCredential)
- `.env` file configured at the repo root
- Dependencies installed: `pip install -r ../../requirements.txt`

## Run

```bash
python openai-v1-streaming-cli.py
```
