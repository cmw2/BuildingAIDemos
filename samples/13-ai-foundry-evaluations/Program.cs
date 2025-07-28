using Azure.AI.Projects;
using Azure.AI.Inference;
using Azure.Identity;
using Azure;
using Azure.Core;
using DotNetEnv;
using System.Text.Json;

namespace AiFoundryEvaluations;

public class Program
{
    private static string? _endpoint;
    private static string? _apiKey;
    private static string? _modelDeployment;
    
    public static async Task Main(string[] args)
    {
        Console.WriteLine("🧪 AI Foundry Evaluations Demo");
        Console.WriteLine(new string('=', 50));
        
        try
        {
            // Load environment variables
            LoadEnvironmentVariables();
            
            // Initialize AI Foundry chat client
            var chatClient = CreateChatCompletionsClient();
            
            // Load test dataset
            var testCases = await LoadTestDataset();
            
            // Run evaluations
            await RunEvaluationDemo(chatClient, testCases);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.ResetColor();
        }
        
        Console.WriteLine("\n👋 Evaluation demo completed!");
    }
    
    private static void LoadEnvironmentVariables()
    {
        // Load environment variables
        // Try current directory first (for Visual Studio), then relative path (for command line)
        if (File.Exists(".env"))
        {
            Env.Load(".env");
        }
        else
        {
            Env.Load("../../.env");
        }
        
        _endpoint = Environment.GetEnvironmentVariable("AI_FOUNDRY_INFERENCE_ENDPOINT") 
            ?? throw new InvalidOperationException("AI_FOUNDRY_INFERENCE_ENDPOINT environment variable is required");
        _apiKey = Environment.GetEnvironmentVariable("AI_FOUNDRY_API_KEY") 
            ?? throw new InvalidOperationException("AI_FOUNDRY_API_KEY environment variable is required");
        _modelDeployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o";
        
        Console.WriteLine($"✅ Configuration loaded");
        Console.WriteLine($"   Endpoint: {_endpoint}");
        Console.WriteLine($"   Model: {_modelDeployment}");
        Console.WriteLine();
    }
    
    private static ChatCompletionsClient CreateChatCompletionsClient()
    {
        // Create ChatCompletionsClient directly with DefaultAzureCredential and custom token scope
        var endpoint = new Uri(_endpoint!);
        var credential = new DefaultAzureCredential();
        var tokenCredential = new TokenCredentialWrapper(credential);
        return new ChatCompletionsClient(endpoint, tokenCredential);
    }
    
    private static async Task<List<TestCase>> LoadTestDataset()
    {
        var datasetPath = "test-dataset.json";
        
        Console.WriteLine($"🔍 Looking for dataset at: {Path.GetFullPath(datasetPath)}");
        Console.WriteLine($"🔍 Current directory: {Directory.GetCurrentDirectory()}");
        
        if (!File.Exists(datasetPath))
        {
            throw new FileNotFoundException($"Test dataset not found: {datasetPath}");
        }
        
        var jsonContent = await File.ReadAllTextAsync(datasetPath);
        
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var dataset = JsonSerializer.Deserialize<TestDataset>(jsonContent, options);
        
        Console.WriteLine($"📊 Loaded {dataset?.TestCases?.Count ?? 0} test cases");
        return dataset?.TestCases ?? new List<TestCase>();
    }
    
    private static async Task RunEvaluationDemo(ChatCompletionsClient chatClient, List<TestCase> testCases)
    {
        var prompts = new[]
        {
            "You are a helpful assistant. Answer questions clearly and accurately.",
            "You are a friendly teacher explaining concepts to students. Use simple language and provide helpful examples when appropriate.",
            "You are a professional expert providing precise, well-structured responses. Focus on accuracy and completeness while maintaining clarity."
        };
        
        for (int promptIndex = 0; promptIndex < prompts.Length; promptIndex++)
        {
            Console.WriteLine($"\n🎯 Testing Prompt {promptIndex + 1}:");
            Console.WriteLine($"   \"{prompts[promptIndex]}\"");
            Console.WriteLine(new string('=', 80));
            
            var promptResults = new List<EvaluationResult>();
            
            foreach (var testCase in testCases)
            {
                Console.WriteLine($"\n📝 Question {testCase.Id}: {testCase.Question}");
                
                try
                {
                    // Generate response using current prompt
                    var response = await GenerateResponse(chatClient, prompts[promptIndex], testCase.Question);
                    Console.WriteLine($"🤖 Response: {response}");
                    
                    // Simulate evaluation (in real implementation, this would call AI Foundry evaluators)
                    var evaluation = SimulateEvaluation(testCase, response);
                    promptResults.Add(evaluation);
                    
                    // Display evaluation results
                    DisplayEvaluationResults(evaluation);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"❌ Error processing question {testCase.Id}: {ex.Message}");
                    Console.ResetColor();
                }
                
                // Add small delay to avoid rate limiting
                await Task.Delay(1000);
            }
            
            // Display summary for this prompt
            DisplayPromptSummary(promptIndex + 1, promptResults);
        }
        
        Console.WriteLine("\n🎉 All evaluations completed!");
    }
    
    private static async Task<string> GenerateResponse(ChatCompletionsClient chatClient, string systemPrompt, string question)
    {
        try
        {
            var messages = new List<ChatRequestMessage>
            {
                new ChatRequestSystemMessage(systemPrompt),
                new ChatRequestUserMessage(question)
            };

            var requestOptions = new ChatCompletionsOptions()
            {
                Model = _modelDeployment,
                Temperature = 0.7f,
                MaxTokens = 500
            };

            // Add all messages to the options
            foreach (var message in messages)
            {
                requestOptions.Messages.Add(message);
            }

            var response = await chatClient.CompleteAsync(requestOptions);
            
            return response.Value.Content ?? "No response generated.";
        }
        catch (Exception ex)
        {
            // Fall back to simulated responses if the API call fails
            Console.WriteLine($"⚠️ API call failed, using simulated response: {ex.Message}");
            return GetSimulatedResponse(systemPrompt, question);
        }
    }
    
    private static string GetSimulatedResponse(string systemPrompt, string question)
    {
        // Fallback simulated responses for demonstration
        var responses = new Dictionary<string, string[]>
        {
            ["What is the capital of France?"] = new[]
            {
                "The capital of France is Paris.",
                "Paris is the capital city of France, known for its culture and history.",
                "France's capital city is Paris, which is also its largest city."
            },
            ["Explain photosynthesis in simple terms."] = new[]
            {
                "Photosynthesis is how plants make food using sunlight, water, and carbon dioxide.",
                "Plants use photosynthesis to convert sunlight into energy. They take in carbon dioxide and water, and with the help of chlorophyll, they create glucose and release oxygen.",
                "Photosynthesis is the biological process where plants capture solar energy to synthesize glucose from carbon dioxide and water, producing oxygen as a byproduct."
            },
            ["How do you debug a null reference exception in C#?"] = new[]
            {
                "Check the stack trace, find which variable is null, add null checks.",
                "To debug null reference exceptions: examine the stack trace to locate the error, identify uninitialized objects, add proper null validation, and use debugging tools to step through your code.",
                "Null reference exceptions require systematic debugging: analyze stack traces for precise error locations, implement defensive null checking patterns, utilize IDE debugging capabilities, and ensure proper object initialization throughout the application lifecycle."
            }
        };
        
        // Find matching response or generate a generic one
        var matchingResponses = responses.FirstOrDefault(kvp => 
            question.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase));
        
        if (matchingResponses.Key != null)
        {
            // Return different responses based on prompt style
            var promptStyle = systemPrompt.Contains("teacher") ? 1 : 
                             systemPrompt.Contains("professional") ? 2 : 0;
            return matchingResponses.Value[Math.Min(promptStyle, matchingResponses.Value.Length - 1)];
        }
        
        // Generic responses for other questions
        return systemPrompt.Contains("teacher") 
            ? "Let me explain this in a simple way..."
            : systemPrompt.Contains("professional")
                ? "Based on established principles..."
                : "Here's the answer...";
    }
    
    private static EvaluationResult SimulateEvaluation(TestCase testCase, string response)
    {
        // In a real implementation, this would call AI Foundry's evaluation APIs
        // For demonstration, we'll simulate realistic evaluation scores
        
        var random = new Random(testCase.Id + response.Length); // Deterministic for demo
        
        return new EvaluationResult
        {
            TestCaseId = testCase.Id,
            Question = testCase.Question,
            Response = response,
            Groundedness = GenerateScore(random, testCase.Category == "factual" ? 8.5 : 7.0),
            Relevance = GenerateScore(random, 8.0),
            Coherence = GenerateScore(random, response.Length > 50 ? 8.5 : 7.0),
            Fluency = GenerateScore(random, 8.5)
        };
    }
    
    private static double GenerateScore(Random random, double baseScore)
    {
        // Generate scores around the base score with some variation
        var variation = (random.NextDouble() - 0.5) * 2.0; // -1.0 to +1.0
        var score = baseScore + variation;
        return Math.Max(1.0, Math.Min(10.0, Math.Round(score, 1)));
    }
    
    private static void DisplayEvaluationResults(EvaluationResult result)
    {
        Console.WriteLine("📊 Evaluation Results:");
        Console.WriteLine($"   ✅ Groundedness: {result.Groundedness:F1}/10 {GetScoreEmoji(result.Groundedness)}");
        Console.WriteLine($"   ✅ Relevance: {result.Relevance:F1}/10 {GetScoreEmoji(result.Relevance)}");
        Console.WriteLine($"   ✅ Coherence: {result.Coherence:F1}/10 {GetScoreEmoji(result.Coherence)}");
        Console.WriteLine($"   ✅ Fluency: {result.Fluency:F1}/10 {GetScoreEmoji(result.Fluency)}");
    }
    
    private static void DisplayPromptSummary(int promptNumber, List<EvaluationResult> results)
    {
        if (!results.Any()) return;
        
        Console.WriteLine($"\n📈 Summary for Prompt {promptNumber}:");
        Console.WriteLine($"   Average Groundedness: {results.Average(r => r.Groundedness):F1}/10");
        Console.WriteLine($"   Average Relevance: {results.Average(r => r.Relevance):F1}/10");
        Console.WriteLine($"   Average Coherence: {results.Average(r => r.Coherence):F1}/10");
        Console.WriteLine($"   Average Fluency: {results.Average(r => r.Fluency):F1}/10");
        Console.WriteLine($"   Overall Average: {results.Average(r => (r.Groundedness + r.Relevance + r.Coherence + r.Fluency) / 4):F1}/10");
    }
    
    private static string GetScoreEmoji(double score)
    {
        return score switch
        {
            >= 9.0 => "🌟",
            >= 8.0 => "⭐",
            >= 7.0 => "👍",
            >= 6.0 => "👌",
            _ => "⚠️"
        };
    }
}

// Data models
public class TestDataset
{
    public List<TestCase> TestCases { get; set; } = new();
}

public class TestCase
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string ExpectedResponse { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

public class EvaluationResult
{
    public int TestCaseId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public double Groundedness { get; set; }
    public double Relevance { get; set; }
    public double Coherence { get; set; }
    public double Fluency { get; set; }
}

// Custom wrapper to handle token scope
public class TokenCredentialWrapper : Azure.Core.TokenCredential
{
    private readonly Azure.Core.TokenCredential _innerCredential;

    public TokenCredentialWrapper(Azure.Core.TokenCredential innerCredential)
    {
        _innerCredential = innerCredential;
    }

    public override Azure.Core.AccessToken GetToken(Azure.Core.TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        var context = new Azure.Core.TokenRequestContext(new[] { "https://ai.azure.com/.default" });
        return _innerCredential.GetToken(context, cancellationToken);
    }

    public override ValueTask<Azure.Core.AccessToken> GetTokenAsync(Azure.Core.TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        var context = new Azure.Core.TokenRequestContext(new[] { "https://ai.azure.com/.default" });
        return _innerCredential.GetTokenAsync(context, cancellationToken);
    }
}
