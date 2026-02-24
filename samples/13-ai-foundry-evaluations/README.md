# Sample 13: AI Foundry Evaluations

> **⚠️ NOT CURRENTLY WORKING** — This sample builds and partially runs, but the
> evaluation run fails with a **storage RBAC permission error**
> (`ProjectMIUnauthorized` / `AuthorizationFailure`). The AI Foundry project's
> managed identity does not have **Storage Blob Data Contributor** access on the
> project's storage account. To fix this, grant the project managed identity the
> `Storage Blob Data Contributor` role on the associated storage account. See
> [Foundry Permissions](https://aka.ms/FoundryPermissions) for details.

This sample demonstrates cloud-based evaluation of AI model responses using the
**OpenAI Evals API** through Azure AI Foundry. It uses the new two-step pattern
available in `Azure.AI.Projects` 1.2.0+.

## Overview

The sample follows this flow:
1. **Generate AI responses** for 6 test questions using `ChatCompletionsClient`
2. **Define an evaluation** with built-in quality evaluators (coherence, fluency, relevance, F1 score)
3. **Create an evaluation run** with inline data
4. **Poll for completion** and display per-item scores

### Evaluation Metrics
| Evaluator | Type | Measures |
|-----------|------|----------|
| **Coherence** | AI-assisted | Is the response well-structured and logical? |
| **Fluency** | AI-assisted | Is the response well-written and grammatically correct? |
| **Relevance** | AI-assisted | Does the response answer the actual question? |
| **F1 Score** | Non-AI | Token overlap between response and ground truth |

## Files

```
13-ai-foundry-evaluations/
├── 13-ai-foundry-evaluations.csproj    # Project file (Azure.AI.Projects 1.2.0-beta.5)
├── Program.cs                          # Main evaluation logic (OpenAI Evals API)
├── test-dataset.json                   # 6 test cases with expected responses
├── evaluation-prompts.txt              # Prompt variations (not currently used)
└── README.md                           # This file
```

## Prerequisites

- .NET 8.0+
- Azure AI Foundry project with an OpenAI deployment
- `DefaultAzureCredential` access (e.g. `az login`)

### Environment Variables (in root `.env`)

```
AI_FOUNDRY_PROJECT_CONNECTION_STRING=https://<hub>.services.ai.azure.com/api/projects/<project>
AZURE_OPENAI_DEPLOYMENT_NAME=gpt-4.1
AZURE_OPENAI_DEPLOYMENT_ENDPOINT=https://<hub>.cognitiveservices.azure.com/openai/deployments/<model>
```

### Required RBAC Permissions

The AI Foundry **project managed identity** needs:
- **Storage Blob Data Contributor** on the project's associated storage account

Without this, the evaluation service cannot read/write data and runs will fail
with `AuthorizationFailure`.

## Running

```bash
cd samples/13-ai-foundry-evaluations
dotnet run
```

## Key Concepts

### Two-Step Evaluation Pattern
The new OpenAI Evals API uses two steps:
1. **Create Evaluation** — defines the evaluators, data schema, and testing criteria
2. **Create Evaluation Run** — submits data and starts the evaluation

```csharp
// Step 1: Define evaluation
EvaluationClient evalClient = projectClient.OpenAI.GetEvaluationClient();
await evalClient.CreateEvaluationAsync(evaluationContent);

// Step 2: Run evaluation with data
await evalClient.CreateEvaluationRunAsync(evaluationId, runContent);
```

### Inline Data vs. File Upload
This sample sends data inline (`file_content` source type) to keep things simple.
For larger datasets, use `Datasets.UploadFileAsync` with a `file_id` source instead.

### SDK Packages
| Package | Version | Purpose |
|---------|---------|---------|
| `Azure.AI.Projects` | 1.2.0-beta.5 | AI Foundry project client, datasets |
| `Azure.AI.Projects.OpenAI` | 1.0.0-beta.5 | `EvaluationClient` via OpenAI Evals |
| `Azure.AI.Inference` | 1.0.0-beta.5 | `ChatCompletionsClient` for response generation |
| `Azure.Identity` | 1.17.1 | `DefaultAzureCredential` |

## Next Steps

- Fix storage RBAC permissions to get evaluations running end-to-end
- Explore additional built-in evaluators (`builtin.groundedness`, `builtin.similarity`, etc.)
- Try file-based dataset upload for larger evaluation sets
- Integrate evaluations into CI/CD pipelines for automated quality gates
