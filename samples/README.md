# AI Foundry Model Calling Samples

A collection of samples demonstrating different approaches to calling AI models in Azure AI Foundry.

## 🎯 Sample Overview

### Sample 01: Direct API Calling
**Language:** Python  
**Approach:** Raw HTTP requests to AI Foundry REST API  
**Good for:** Understanding the underlying API structure

### Sample 02: OpenAI SDK with Python (Jupyter Notebook)
**Language:** Python (Jupyter)  
**Approach:** OpenAI Python SDK with Azure configuration  
**Good for:** Interactive development and experimentation

### Sample 03: OpenAI SDK with Python (Command Line)
**Language:** Python (CLI)  
**Approach:** OpenAI Python SDK in a command-line application  
**Good for:** Standalone Python applications

### Sample 04: OpenAI SDK for .NET (Command Line)
**Language:** C# (.NET)  
**Approach:** OpenAI .NET SDK with Azure configuration  
**Good for:** .NET applications using familiar OpenAI patterns

### Sample 05: LangChain (Python Notebook)
**Language:** Python (Jupyter)  
**Approach:** LangChain framework with AI Foundry integration  
**Good for:** Complex workflows, chaining, and AI application patterns

### Sample 06: Semantic Kernel (.NET)
**Language:** C# (.NET)  
**Approach:** Microsoft Semantic Kernel framework  
**Good for:** Enterprise .NET applications with AI orchestration  
**Structure:** Organized into `streaming/` and `nonstreaming/` folders

### Sample 07: AI Foundry SDK (.NET)
**Language:** C# (.NET)  
**Approach:** Native Azure.AI.Inference SDK with proper AI Foundry authentication  
**Good for:** Native Azure integration with project endpoints  
**Structure:** Organized into `streaming/` and `nonstreaming/` folders

### Sample 08: Multi-Model Comparison
**Language:** C# (.NET)  
**Approach:** Compare multiple models with the same prompt using AI Foundry SDK  
**Good for:** Model evaluation and comparison

### Sample 09-14: Advanced Scenarios
Various samples covering multimodal capabilities, tool calling, prompt templating, evaluations, and intent resolution.

### Sample 15: Azure OpenAI - Use Your Own Data
**Language:** C# (.NET)  
**Approach:** Azure OpenAI SDK with Azure AI Search integration  
**Good for:** RAG (Retrieval-Augmented Generation) scenarios where you want to chat with your own documents

## 🚀 Quick Start

1. **Configure Environment**: Copy `.env.example` to `.env` and add your AI Foundry details
2. **Choose a Sample**: Navigate to any sample folder
3. **Run the Sample**: Follow the README in each sample folder

## 🔧 Environment Configuration

All samples use a shared `.env` file in the root directory:

```env
# AI Foundry Configuration
AI_FOUNDRY_PROJECT_CONNECTION_STRING=https://your-project.ai.azure.com
AI_FOUNDRY_MODEL_DEPLOYMENT_NAME=gpt-4o

# Azure OpenAI (for OpenAI SDK samples)
AZURE_OPENAI_ENDPOINT=https://your-resource.openai.azure.com/
AZURE_OPENAI_API_KEY=your-api-key
AZURE_OPENAI_DEPLOYMENT_NAME=gpt-4o
```

## 📁 Sample Structure

```
samples/
├── 01-direct-api/              # Raw HTTP API calls
├── 02-openai-sdk-python-nb/    # OpenAI SDK in Jupyter
├── 03-openai-sdk-python-cli/   # OpenAI SDK command line
├── 04-openai-sdk-dotnet/       # OpenAI SDK .NET
├── 05-langchain-python-nb/     # LangChain framework
├── 06-semantic-kernel-dotnet/  # Semantic Kernel (.NET)
│   ├── nonstreaming/
│   └── streaming/
├── 07-foundry-sdk-dotnet/      # Native AI Foundry SDK
│   ├── nonstreaming/
│   └── streaming/
├── 08-multimodel-comparison/   # Multi-model testing
├── 09-multimodal-storage/      # Multimodal scenarios
├── 10-tool-calling-manual/     # Manual tool calling
├── 11-tool-calling-semantic-kernel/ # SK tool calling
├── 12-prompt-templating-semantic-kernel/ # Prompt templating
├── 13-ai-foundry-evaluations/ # Model evaluations
├── 14-intent-resolution-notebook/ # Intent resolution
└── 15-openai-your-data/        # Use Your Own Data (RAG)
```

## 🎨 Design Philosophy

- **"Just the Meat"**: Minimal, focused code without unnecessary complexity
- **Educational**: Each sample teaches a specific approach
- **Practical**: Real-world patterns you can use immediately
- **Clean**: Well-commented, readable code structure

## 🔍 Which Sample Should I Use?

- **Learning AI Foundry API**: Start with Sample 01 (Direct API)
- **Python Development**: Use Sample 02 (Jupyter) or 03 (CLI)
- **Enterprise .NET**: Use Sample 06 (Semantic Kernel) or 07 (AI Foundry SDK)
- **Model Comparison**: Use Sample 08 (Multi-Model)
- **Framework Integration**: Sample 05 (LangChain) for Python, Sample 06 (Semantic Kernel) for .NET
- **RAG with Your Documents**: Use Sample 15 (Use Your Own Data) for Azure AI Search integration

Each sample is self-contained and demonstrates a complete working approach to AI model integration.
