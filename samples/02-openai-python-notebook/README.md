# Sample 02: OpenAI SDK Python (Jupyter Notebook)

This notebook demonstrates how to use the OpenAI Python SDK to interact with AI Foundry models in an interactive Jupyter environment.

## Prerequisites

1. **Python Environment**: Python 3.8+ with required packages installed
2. **Environment Configuration**: Copy `.env.example` to `.env` and fill in your AI Foundry details
3. **Jupyter**: Jupyter notebook environment (included in requirements.txt)

## Setup

1. **Create Python Virtual Environment** (if not already created):
   ```powershell
   # From the root directory (IntroDemos/)
   python -m venv venv
   ```

2. **Activate Python Environment**:
   ```powershell
   # From the root directory
   .\venv\Scripts\Activate.ps1
   ```

3. **Install Dependencies** (if not already done):
   ```powershell
   pip install -r requirements.txt
   ```

4. **Configure Environment**: 
   - Copy `.env.example` to `.env`
   - Fill in your AI Foundry credentials

5. **Launch Jupyter**:
   ```powershell
   jupyter notebook
   ```
   Or open the notebook directly in VS Code.

## What You'll Learn

The notebook covers:

1. **Environment Setup**: Package verification and configuration loading
2. **Basic Chat Completion**: Simple request-response interactions
3. **Conversation History**: Maintaining context across multiple turns
4. **Streaming Responses**: Real-time response generation
5. **Temperature Variations**: Controlling response creativity vs consistency
6. **Interactive Playground**: Hands-on experimentation area

## Usage

1. Open `openai-sample.ipynb` in Jupyter or VS Code
2. Run cells sequentially (Shift+Enter)
3. Modify the interactive playground section to test your own prompts
4. Experiment with different parameters and settings

## Key Features

- ✅ **Environment Variable Management**: Secure credential handling
- ✅ **Error Handling**: Proper exception handling patterns
- ✅ **Interactive Examples**: Ready-to-run code cells
- ✅ **Educational Content**: Detailed explanations and best practices
- ✅ **Streaming Support**: Real-time response visualization
- ✅ **Parameter Exploration**: Temperature and token limit demonstrations

## Common Issues

- **Missing Environment Variables**: Ensure `.env` file is properly configured
- **Import Errors**: Run the environment setup cell first
- **Connection Issues**: Verify your AI Foundry endpoint and API key

## Next Steps

After completing this notebook, try:
- Sample 03: Command line version of OpenAI SDK
- Sample 05: LangChain integration
- Building your own notebook-based AI applications
