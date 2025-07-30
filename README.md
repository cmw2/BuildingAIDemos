# AI Foundry Model Calling Samples

This repository contains a collection of simple sample code demonstrating various methods for calling models hosted in AI Foundry. Each sample focuses on straightforward inferencing without complex features like agents or RAG.

## Sample Code

This repository contains sample code intended for demonstration purposes. The
code is provided as-is and may require modifications to fit specific use cases or
production environments.

## Project Structure

```
IntroDemos/
├── README.md                 # This file - project overview and progress tracker
├── .env                      # Environment variables (endpoints, keys)
├── requirements.txt          # Python dependencies for all samples
├── samples/
│   ├── 01-direct-api/
│   │   └── ai-foundry.http   # REST client file for direct API calls
│   ├── 02-openai-python-notebook/
│   │   └── openai-sample.ipynb
│   ├── 03-openai-python-cli/
│   │   ├── openai-cli.py
│   │   └── openai-streaming-cli.py
│   ├── 04-openai-dotnet-cli/
│   │   ├── OpenAI.Sample.csproj
│   │   └── Program.cs
│   ├── 05-langchain-notebook/
│   │   └── langchain-sample.ipynb
│   ├── 06-semantic-kernel-dotnet/
│   │   ├── SemanticKernel.Sample.csproj
│   │   └── Program.cs
│   ├── 07-foundry-sdk-dotnet/
│   │   ├── FoundrySDK.Sample.csproj
│   │   └── Program.cs
│   ├── 08-foundry-multi-model/
│   │   ├── FoundryMultiModel.Sample.csproj
│   │   └── Program.cs
│   ├── 09-multimodal-storage/
│   │   ├── MultimodalStorage.Sample.csproj
│   │   └── Program.cs
│   ├── 10-tool-calling-manual/
│   │   ├── 10-tool-calling-manual.csproj
│   │   └── Program.cs
│   ├── 11-tool-calling-semantic-kernel/
│   │   ├── 11-tool-calling-semantic-kernel.csproj
│   │   └── Program.cs
│   ├── 12-prompt-templating-semantic-kernel/
│   │   ├── 12-prompt-templating-semantic-kernel.csproj
│   │   ├── Program.cs
│   │   └── system-prompt.txt
│   └── 13-ai-foundry-evaluations/
│       ├── 13-ai-foundry-evaluations.csproj
│       ├── Program.cs
│       ├── README.md
│       ├── test-dataset.json
│       └── evaluation-prompts.txt
│   ├── 14-intent-resolution-notebook/
│   │   ├── cloud-evaluations.ipynb
│   │   ├── health_fitness_eval_data.jsonl
│   │   ├── intent-resolution-evaluation.ipynb
│   │   └── README.md
│   ├── 15-openai-your-data/
│   │   ├── 15-openai-your-data.csproj
│   │   ├── Program.cs
│   │   └── README.md
│   ├── 16-ai-agent-service/
│   │   ├── 16-ai-agent-service.csproj
│   │   ├── Program.cs
│   │   └── README.md
│   └── 17-semantic-kernel-agents/
│       ├── 17-semantic-kernel-agents.csproj
│       ├── Program.cs
│       └── README.md
```

## Environment Setup

### Python Environment
- Single Python virtual environment at the root level
- All Python samples will use the same environment
- Dependencies managed via `requirements.txt`

### .NET Environment
- Each .NET sample will have its own project file
- Shared configuration through .env file (using dotenv packages)

### Environment Variables (.env)
```
# AI Foundry Configuration
AZURE_OPENAI_ENDPOINT=your_endpoint_here
AZURE_OPENAI_API_KEY=your_api_key_here
AZURE_OPENAI_DEPLOYMENT_NAME=your_deployment_name_here
AZURE_OPENAI_API_VERSION=2024-02-15-preview

# AI Foundry SDK Configuration
AI_FOUNDRY_PROJECT_CONNECTION_STRING=your_foundry_endpoint_here
AI_FOUNDRY_API_KEY=your_foundry_api_key_here
AI_FOUNDRY_PROJECT_NAME=your_project_name_here

# Azure AI Search Configuration (for sample 15 - Use Your Own Data)
AZURE_SEARCH_ENDPOINT=your_search_service_endpoint_here
AZURE_SEARCH_API_KEY=your_search_api_key_here
AZURE_SEARCH_INDEX_NAME=your_search_index_name_here

# Model Deployment Names
OPENAI_MODEL_DEPLOYMENT=gpt-4o
PHI_MODEL_DEPLOYMENT=phi-4
```

## Sample Progress Tracker

| Sample | Description | Status | Notes |
|--------|-------------|--------|-------|
| 1. Direct API | HTTP file for REST client extension | ✅ Complete | Uses .http file format |
| 2. OpenAI Python (Notebook) | Jupyter notebook with OpenAI SDK | ✅ Complete | Interactive exploration |
| 3. OpenAI Python (CLI) | Command line app with OpenAI SDK | ✅ Complete | Loop until user quits |
| 4. OpenAI .NET (CLI) | Command line app with OpenAI .NET SDK | ✅ Complete | Loop until user quits |
| 5. LangChain (Notebook) | Jupyter notebook with LangChain | ✅ Complete | Python notebook format |
| 6. Semantic Kernel .NET | Command line app with Semantic Kernel | ✅ Complete | Loop until user quits |
| 7. AI Foundry SDK .NET | Command line app with AI Foundry SDK | ✅ Complete | Native Foundry integration |
| 8. Multi-Model Foundry | Compare OpenAI vs Phi-4 models | ✅ Complete | Two model comparison |
| 9. Multimodal Storage | AI Foundry with blob storage integration | ✅ Complete | Image analysis with connected resources |
| 10. Tool Calling Manual | Manual function calling with Azure OpenAI SDK | ✅ Complete | Date/time and weather functions |
| 11. Tool Calling Semantic Kernel | Auto function calling with Semantic Kernel | ✅ Complete | Same functions, automatic execution |
| 12. Prompt Templating Semantic Kernel | Prompt templates and observability with Semantic Kernel | ✅ Complete | System prompts and telemetry tracking |
| 13. AI Foundry Evaluations | Response quality evaluation with AI Foundry | ❌ Blocked | Known issues with current implementation |
| 14. Intent Resolution Notebook | Intent resolution evaluation with notebooks | ❌ Blocked | Known issues with current implementation |
| 15. OpenAI Use Your Own Data | RAG with Azure AI Search integration | ✅ Complete | Demonstrates Use Your Own Data feature |
| 16. Azure AI Agent Service | AI agent with automatic tool calling using Azure AI Agent Service | ✅ Complete | Weather forecasting and date functions |
| 17. Semantic Kernel Agents | AI agent with automatic tool calling using Semantic Kernel Agents | ✅ Complete | Same functionality as Sample 16 for comparison |

### Status Legend
- ⏳ Pending - Not started
- 🔄 In Progress - Currently being developed
- ✅ Complete - Finished and tested
- ❌ Blocked - Waiting for dependencies or decisions

## Sample Details

### 1. Direct API Calling (HTTP File)
- **File**: `samples/01-direct-api/ai-foundry.http`
- **Purpose**: Demonstrate raw HTTP API calls to AI Foundry
- **Usage**: Use VS Code REST Client extension to execute requests
- **Features**: 
  - Authentication with API key
  - Basic chat completion request
  - Response handling

### 2. OpenAI SDK Python (Jupyter Notebook)
- **File**: `samples/02-openai-python-notebook/openai-sample.ipynb`
- **Purpose**: Interactive exploration of OpenAI SDK with Python
- **Usage**: Run cells in Jupyter notebook environment
- **Features**:
  - Environment setup and configuration
  - Basic chat completion
  - Response streaming example

### 3. OpenAI SDK Python (CLI)
- **Files**: 
  - `samples/03-openai-python-cli/openai-cli.py` - Standard responses
  - `samples/03-openai-python-cli/openai-streaming-cli.py` - Streaming responses
- **Purpose**: Command line application using OpenAI Python SDK
- **Usage**: 
  - Standard: `python samples/03-openai-python-cli/openai-cli.py`
  - Streaming: `python samples/03-openai-python-cli/openai-streaming-cli.py`
- **Features**:
  - Continuous input loop until user types 'quit'
  - Environment-based configuration from .env file
  - Error handling with graceful degradation
  - Clean output formatting with emojis
  - Hardcoded temperature (0.7) and max_tokens (500/800) for simplicity
  - Streaming version shows real-time response generation

### 4. OpenAI SDK .NET (CLI)
- **Files**: 
  - `samples/04-openai-dotnet-cli/OpenAI.Sample.csproj` - Project file
  - `samples/04-openai-dotnet-cli/Program.cs` - Main application
- **Purpose**: Command line application using Azure OpenAI .NET SDK
- **Usage**: `cd samples/04-openai-dotnet-cli && dotnet run`
- **Features**:
  - Continuous input loop until user types 'quit'
  - Environment-based configuration from .env file using DotNetEnv
  - Error handling with graceful degradation
  - Console color formatting for better UX
  - Hardcoded temperature (0.7) and max_tokens (500) for simplicity
  - Async/await pattern for non-blocking operations

### 5. LangChain Python (Notebook)
- **File**: `samples/05-langchain-notebook/langchain-sample.ipynb`
- **Purpose**: Interactive exploration of LangChain integration with AI Foundry
- **Usage**: Run cells in Jupyter notebook environment
- **Features**:
  - LangChain ChatOpenAI setup with Azure OpenAI
  - Prompt templates with variable substitution
  - Conversation memory and chain operations
  - Temperature comparison examples
  - Interactive playground function
  - Clean, educational code focused on core LangChain concepts

### 6. Semantic Kernel .NET
- **Files**: 
  - `samples/06-semantic-kernel-dotnet/nonstreaming/` - Standard responses
  - `samples/06-semantic-kernel-dotnet/streaming/` - Streaming responses
- **Purpose**: Command line application using Semantic Kernel with Azure OpenAI
- **Usage**: 
  - Standard: `cd samples/06-semantic-kernel-dotnet/nonstreaming && dotnet run`
  - Streaming: `cd samples/06-semantic-kernel-dotnet/streaming && dotnet run`
- **Features**:
  - Continuous input loop until user types 'quit'
  - Environment-based configuration from .env file using DotNetEnv
  - Chat history maintained throughout conversation
  - Clean, simple code focused on Semantic Kernel essentials
  - Hardcoded temperature (0.7) and max_tokens (500) for simplicity
  - Uses Semantic Kernel's ChatHistory for conversation management
  - Streaming version shows real-time response generation

### 7. AI Foundry SDK .NET
- **Files**: `samples/07-foundry-sdk-dotnet/`
- **Purpose**: Command line application using AI Foundry SDK
- **Usage**: `dotnet run`
- **Features**:
  - Native AI Foundry SDK integration
  - Direct model inference calls
  - Continuous input loop

### 8. Multi-Model AI Foundry Comparison
- **Files**: `samples/08-foundry-multi-model/`
- **Purpose**: Compare responses from different model types
- **Usage**: `dotnet run`
- **Features**:
  - Side-by-side comparison of GPT-4o and Phi-4
  - Same prompt sent to both models
  - Response comparison display

### 9. Multimodal Storage Integration
- **Files**: `samples/09-multimodal-storage/`
- **Purpose**: Demonstrate AI Foundry with connected Azure Storage resources
- **Usage**: `cd samples/09-multimodal-storage && dotnet run`
- **Features**:
  - Multi-modal image analysis using AI Foundry connected resources
  - Azure Blob Storage integration with SAS token generation
  - DefaultAzureCredential authentication with fallback strategies
  - GPT-4o Vision model for image content analysis
  - Demonstrates connected resource workflows in AI Foundry

### 10. Tool Calling - Manual Implementation
- **Files**: `samples/10-tool-calling-manual/`
- **Purpose**: Manual tool calling implementation using Azure OpenAI SDK
- **Usage**: `cd samples/10-tool-calling-manual && dotnet run`
- **Features**:
  - Manual tool calling "the hard way" with full control over function execution
  - Custom function definitions for date/time and weather information
  - JSON schema definitions for function parameters
  - Strongly-typed parameter handling with Dictionary<string, object>
  - Interactive chat loop with function calling capabilities
  - Demonstrates ChatTool.CreateFunctionTool and ChatToolCall processing

### 11. Tool Calling - Semantic Kernel Auto Implementation
- **Files**: `samples/11-tool-calling-semantic-kernel/`
- **Purpose**: Automatic tool calling implementation using Semantic Kernel
- **Usage**: `cd samples/11-tool-calling-semantic-kernel && dotnet run`
- **Features**:
  - Automatic function calling "the easy way" using Semantic Kernel
  - Attribute-based function definitions with [KernelFunction] and [Description]
  - Auto-discovery and registration of functions via reflection
  - Built-in tool call handling with ToolCallBehavior.AutoInvokeKernelFunctions
  - Same date/time and weather functions as Sample 10 but with much cleaner code
  - Demonstrates the power of Semantic Kernel's plugin architecture

### 12. Prompt Templating and Observability - Semantic Kernel
- **Files**: `samples/12-prompt-templating-semantic-kernel/`
- **Purpose**: Demonstrate prompt templates and observability features with Semantic Kernel
- **Usage**: `cd samples/12-prompt-templating-semantic-kernel && dotnet run`
- **Features**:
  - System prompt loading from external files (`system-prompt.txt`)
  - Template variable substitution and management
  - Built-in observability and telemetry tracking
  - Semantic Kernel prompt template functionality
  - Clean separation of prompts from code
  - Demonstrates monitoring and logging capabilities for AI applications

### 13. AI Foundry Evaluations
- **Files**: `samples/13-ai-foundry-evaluations/`
- **Purpose**: Demonstrate response quality evaluation using AI Foundry's evaluation capabilities
- **Usage**: `cd samples/13-ai-foundry-evaluations && dotnet run`
- **Status**: ❌ Known issues with current implementation
- **Features**:
  - Built-in evaluation metrics (Groundedness, Relevance, Coherence, Fluency)
  - Batch processing of test cases from JSON dataset
  - Prompt variation testing and comparison
  - Automated scoring and summary statistics
  - Quality measurement for AI application improvement
  - Demonstrates evaluation-driven development practices

### 14. Intent Resolution Evaluation (Notebook)
- **Files**: `samples/14-intent-resolution-notebook/`
- **Purpose**: Demonstrate intent resolution evaluation using notebook environments
- **Usage**: Open notebooks in Jupyter or VS Code
- **Status**: ❌ Known issues with current implementation
- **Features**:
  - Intent classification evaluation
  - Health and fitness domain examples
  - Cloud-based evaluation workflows

### 15. OpenAI Use Your Own Data (RAG)
- **Files**: `samples/15-openai-your-data/`
- **Purpose**: Demonstrate Azure OpenAI's "Use Your Own Data" feature with Azure AI Search
- **Usage**: `cd samples/15-openai-your-data && dotnet run`
- **Features**:
  - Azure OpenAI integration with Azure AI Search data sources
  - RAG (Retrieval-Augmented Generation) pattern implementation
  - Built-in "Use Your Own Data" feature using AzureSearchChatDataSource
  - Configurable search parameters (TopNDocuments, Strictness, QueryType)
  - Field mapping configuration for custom index schemas
  - Interactive chat interface that searches your indexed documents
  - Comprehensive error handling and troubleshooting guidance
  - Environment-based configuration for search endpoint, API keys, and index names

### 16. Azure AI Agent Service
- **Files**: `samples/16-ai-agent-service/`
- **Purpose**: Demonstrate AI agent with automatic tool calling using Azure AI Agent Service
- **Usage**: `cd samples/16-ai-agent-service && dotnet run`
- **Features**:
  - Azure AI Agent Service with persistent agent sessions
  - Automatic tool calling with weather forecasting capabilities
  - Date and time functions (local and UTC)
  - DefaultAzureCredential authentication with AI Foundry
  - Streaming responses with real-time tool call logging
  - Clean session management and proper resource cleanup
  - Educational code focused on agent fundamentals

### 17. Semantic Kernel Agents
- **Files**: `samples/17-semantic-kernel-agents/`
- **Purpose**: Demonstrate AI agent with automatic tool calling using Semantic Kernel Agents
- **Usage**: `cd samples/17-semantic-kernel-agents && dotnet run`
- **Features**:
  - Semantic Kernel AzureAIAgent integration with Azure AI Foundry
  - Identical functionality to Sample 16 for direct framework comparison
  - Automatic tool calling with the same weather and date/time functions
  - KernelFunction attribute-based tool definitions
  - Streaming responses with comprehensive tool call visibility
  - Proper thread management and agent lifecycle handling
  - Demonstrates Semantic Kernel's agent architecture patterns

## Getting Started

### Prerequisites
- Python 3.8+
- .NET 8.0+
- VS Code with REST Client extension
- AI Foundry endpoint and API key

### Setup Steps
1. Clone/navigate to this repository
2. Copy `.env.example` to `.env` and fill in your AI Foundry details
3. Create Python virtual environment: `python -m venv venv`
4. Activate virtual environment: `venv\Scripts\Activate.ps1` (Windows) or `source venv/bin/activate` (Linux/Mac)
5. Install Python dependencies: `pip install -r requirements.txt`
6. Choose a sample and follow its specific instructions

## Usage Patterns

### Command Line Samples
All CLI samples follow this pattern:
```
> Sample is ready. Type your message (or 'quit' to exit):
User: Hello, how are you?
AI: I'm doing well, thank you! How can I help you today?

User: quit
> Goodbye!
```

### Notebook Samples
- Open in VS Code or Jupyter
- Run cells sequentially
- Modify prompts and re-run as needed

### HTTP File Sample
- Open .http file in VS Code
- Click "Send Request" above each request
- View response in VS Code REST Client panel

## Notes
- All samples use API key authentication for simplicity
- Error handling is minimal to keep code clear and educational
- Production applications should include proper error handling, logging, and security measures
- Each sample is self-contained and can be run independently

## Disclaimer

This Sample Code is provided for the purpose of illustration only and is not
intended to be used in a production environment. THIS SAMPLE CODE AND ANY RELATED
INFORMATION ARE PROVIDED 'AS IS' WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED
OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF
MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.