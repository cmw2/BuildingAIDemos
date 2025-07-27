# Sample 08: Multi-Model AI Foundry Comparison

Compare responses from different AI models in AI Foundry using the same prompt.

## Features
- Tests multiple models with the same prompt
- Shows response differences between models
- Uses proper AI Foundry SDK authentication
- Clean comparison output format

## Usage

```bash
dotnet run
```

Enter a prompt and see how different models respond to the same question.

## Models Tested
- gpt-4o (Primary model)
- gpt-4o-mini (Faster/cheaper)
- gpt-35-turbo (Alternative)

Modify the `models` array in Program.cs to test different model deployments.
