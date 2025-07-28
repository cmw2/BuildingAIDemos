# Sample 13: AI Foundry Evaluations

This sample demonstrates how to evaluate AI model responses using AI Foundry's evaluation capabilities. It shows how to measure response quality and compare different prompt strategies.

## Overview

This sample focuses on the fundamentals of AI evaluation:
- **Built-in Evaluators**: Uses standard evaluation metrics (Groundedness, Relevance, Coherence, Fluency)
- **Batch Processing**: Evaluates multiple test cases automatically
- **Prompt Comparison**: Tests different system prompts against the same questions
- **Results Analysis**: Displays detailed scores and summary statistics

## Files Structure

```
13-ai-foundry-evaluations/
├── 13-ai-foundry-evaluations.csproj    # Project file with AI Foundry SDK
├── Program.cs                          # Main evaluation logic
├── test-dataset.json                   # Sample questions and expected responses
├── evaluation-prompts.txt              # Different prompt variations
└── README.md                           # This file
```

## Features Demonstrated

### 1. **Evaluation Metrics**
- **Groundedness**: How factually accurate is the response?
- **Relevance**: Does the response answer the actual question?
- **Coherence**: Is the response well-structured and logical?
- **Fluency**: Is the response well-written and grammatically correct?

### 2. **Test Dataset**
The sample includes 6 diverse test cases covering:
- **Factual**: Simple fact-based questions
- **Explanatory**: Questions requiring detailed explanations
- **Technical**: Programming and technical questions
- **Creative**: Open-ended creative tasks
- **Comparative**: Questions requiring analysis and comparison

### 3. **Prompt Variations**
Tests three different system prompt styles:
1. **Basic Assistant**: Simple, direct responses
2. **Friendly Teacher**: Educational, example-rich responses
3. **Professional Expert**: Detailed, precise responses

### 4. **Batch Evaluation**
- Processes all test cases automatically
- Generates responses for each prompt variation
- Calculates evaluation scores for each response
- Provides summary statistics and comparisons

## Usage

### Prerequisites
- .NET 8.0+
- AI Foundry endpoint and API key configured in `.env` file

### Environment Variables Required
```
AI_FOUNDRY_ENDPOINT=your_foundry_endpoint_here
AI_FOUNDRY_API_KEY=your_foundry_api_key_here
AI_FOUNDRY_PROJECT_NAME=your_project_name_here
OPENAI_MODEL_DEPLOYMENT=gpt-4o
```

### Running the Sample
```bash
cd samples/13-ai-foundry-evaluations
dotnet run
```

## Sample Output

```
🧪 AI Foundry Evaluations Demo
==================================================
✅ Configuration loaded
   Endpoint: your-endpoint.cognitiveservices.azure.com
   Project: your-project-name
   Model: gpt-4o

📊 Loaded 6 test cases

🎯 Testing Prompt 1:
   "You are a helpful assistant. Answer questions clearly and accurately."
================================================================================

📝 Question 1: What is the capital of France?
🤖 Response: The capital of France is Paris.
📊 Evaluation Results:
   ✅ Groundedness: 9.2/10 ⭐
   ✅ Relevance: 9.8/10 🌟
   ✅ Coherence: 9.5/10 🌟
   ✅ Fluency: 9.7/10 🌟

[... continues for all test cases and prompts ...]

📈 Summary for Prompt 1:
   Average Groundedness: 8.9/10
   Average Relevance: 9.1/10
   Average Coherence: 8.7/10
   Average Fluency: 9.3/10
   Overall Average: 9.0/10

🎉 All evaluations completed!
```

## Key Learning Points

### 1. **Objective Quality Measurement**
- Learn how to quantify AI response quality
- Understand different evaluation dimensions
- See how scores translate to actionable insights

### 2. **Prompt Engineering Impact**
- Compare how different prompts affect response quality
- Understand the relationship between prompt style and evaluation scores
- Learn to optimize prompts based on evaluation results

### 3. **Evaluation-Driven Development**
- Use evaluations to guide AI application improvements
- Implement quality gates in AI workflows
- Build confidence in AI system outputs

### 4. **Batch Processing Benefits**
- Automate quality assessments at scale
- Generate comprehensive reports
- Enable systematic A/B testing of AI approaches

## Educational Value

This sample demonstrates:
- **Responsible AI**: Building quality controls into AI applications
- **Iterative Improvement**: Using data to enhance AI performance
- **Systematic Testing**: Applying software testing principles to AI
- **Objective Assessment**: Moving beyond subjective quality judgments

## Next Steps

After understanding this basic evaluation approach, consider exploring:
- Custom evaluators for domain-specific criteria
- Human-in-the-loop evaluation workflows
- Integration with CI/CD pipelines for automated quality gates
- Advanced evaluation techniques like adversarial testing

## Notes

- This sample simulates evaluation scores for demonstration purposes
- In production, you would integrate with actual AI Foundry evaluation APIs
- Evaluation criteria should be tailored to your specific use case
- Regular evaluation helps maintain and improve AI system quality over time
