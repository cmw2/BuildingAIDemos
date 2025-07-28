# Sample 14: Intent Resolution Evaluator

This sample demonstrates how to use the Intent Resolution Evaluator from the Azure AI Evaluation SDK in a Python notebook. The Intent Resolution evaluator measures how well an agent has identified and resolved the user intent.

## Overview

This sample focuses on:
- **Intent Resolution Evaluation**: Using the Azure AI Evaluation SDK to measure intent resolution quality
- **String-based Evaluation**: Evaluating simple query-response pairs
- **Message-based Evaluation**: Evaluating complex conversation structures with tool calls
- **Batch Processing**: Evaluating multiple conversations from data files
- **Scoring System**: Understanding the 1-5 scale scoring for intent resolution

## Files Structure

```
14-intent-resolution-notebook/
├── README.md                           # This file
└── intent-resolution-evaluation.ipynb  # Jupyter notebook with evaluation examples
```

## Scoring Scale

The Intent Resolution evaluator uses a 1-5 integer scale:
- **Score 1**: Response completely unrelated to user intent
- **Score 2**: Response minimally relates to user intent  
- **Score 3**: Response partially addresses the user intent but lacks complete details
- **Score 4**: Response addresses the user intent with moderate accuracy but has minor inaccuracies or omissions
- **Score 5**: Response directly addresses the user intent and fully resolves it

## Prerequisites

Before running this sample, make sure you have:

1. **Required Python packages** installed:
   ```bash
   pip install azure-ai-evaluation azure-identity azure-ai-projects
   ```

2. **Environment variables** set in your `.env` file:
   - `AZURE_OPENAI_ENDPOINT` - Your Azure OpenAI endpoint
   - `AZURE_OPENAI_API_KEY` - Your Azure OpenAI API key  
   - `AZURE_OPENAI_API_VERSION` - API version (e.g., "2024-02-01")
   - `MODEL_DEPLOYMENT_NAME` - Name of your deployed model

## Usage

1. Open the Jupyter notebook `intent-resolution-evaluation.ipynb`
2. Run through the cells to see different evaluation scenarios
3. Modify the examples to test your own queries and responses
4. Experiment with tool definitions to see how they affect evaluation

## Key Features Demonstrated

- **Basic Intent Resolution**: Simple query-response evaluation
- **Complex Conversations**: Multi-turn conversations with tool calls
- **Tool Definitions**: How tool availability affects intent resolution scoring
- **Batch Evaluation**: Processing multiple conversations efficiently

This sample provides a comprehensive introduction to intent resolution evaluation using Azure AI services.
