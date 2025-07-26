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
│   │   └── openai-cli.py
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
│   └── 08-foundry-multi-model/
│       ├── FoundryMultiModel.Sample.csproj
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
| 2. OpenAI Python (Notebook) | Jupyter notebook with OpenAI SDK | ⏳ Pending | Interactive exploration |
| 3. OpenAI Python (CLI) | Command line app with OpenAI SDK | ⏳ Pending | Loop until user quits |
| 4. OpenAI .NET (CLI) | Command line app with OpenAI .NET SDK | ⏳ Pending | Loop until user quits |
| 5. LangChain (Notebook) | Jupyter notebook with LangChain | ⏳ Pending | Python notebook format |
| 6. Semantic Kernel .NET | Command line app with Semantic Kernel | ⏳ Pending | Loop until user quits |
| 7. AI Foundry SDK .NET | Command line app with AI Foundry SDK | ⏳ Pending | Native Foundry integration |
| 8. Multi-Model Foundry | Compare OpenAI vs Phi-4 models | ⏳ Pending | Two model comparison |

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
- **File**: `samples/03-openai-python-cli/openai-cli.py`
- **Purpose**: Command line application using OpenAI Python SDK
- **Usage**: `python openai-cli.py`
- **Features**:
  - Continuous input loop
  - Clean output formatting
  - Graceful exit on quit

### 4. OpenAI SDK .NET (CLI)
- **Files**: `samples/04-openai-dotnet-cli/`
- **Purpose**: Command line application using OpenAI .NET SDK
- **Usage**: `dotnet run`
- **Features**:
  - Continuous input loop
  - Configuration from .env file
  - Graceful exit on quit

### 5. LangChain Python (Notebook)
- **File**: `samples/05-langchain-notebook/langchain-sample.ipynb`
- **Purpose**: Demonstrate LangChain integration with AI Foundry
- **Usage**: Run cells in Jupyter notebook environment
- **Features**:
  - LangChain setup with Azure OpenAI
  - Basic chain execution
  - Response handling

### 6. Semantic Kernel .NET
- **Files**: `samples/06-semantic-kernel-dotnet/`
- **Purpose**: Command line application using Semantic Kernel
- **Usage**: `dotnet run`
- **Features**:
  - Semantic Kernel setup
  - Continuous input loop
  - Plugin-free simple completion

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
