# AI Foundry Model Calling Samples

This repository contains a collection of simple sample code demonstrating various methods for calling models hosted in AI Foundry. Each sample focuses on straightforward inferencing without complex features like agents or RAG.

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
│   └── 11-tool-calling-semantic-kernel/
│       ├── 11-tool-calling-semantic-kernel.csproj
│       └── Program.cs
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
AI_FOUNDRY_ENDPOINT=your_foundry_endpoint_here
AI_FOUNDRY_API_KEY=your_foundry_api_key_here
AI_FOUNDRY_PROJECT_NAME=your_project_name_here

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
