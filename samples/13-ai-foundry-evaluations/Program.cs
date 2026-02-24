using Azure.AI.Projects;
using Azure.AI.Projects.OpenAI;
using Azure.AI.Inference;
using Azure.Identity;
using OpenAI.Evals;
using System.ClientModel;
using System.Text;
using System.Text.Json;
using DotNetEnv;

// Sample 13: AI Foundry Evaluations
//
// Demonstrates cloud-based evaluation of AI model responses using the
// OpenAI Evals API through Azure AI Foundry. The flow:
//   1. Generate AI responses for test questions
//   2. Upload evaluation data as a dataset to AI Foundry
//   3. Define evaluation with built-in quality evaluators
//   4. Run evaluation against the dataset
//   5. Poll for results and display scores
//
// Uses the two-step pattern: Create Evaluation → Create Evaluation Run
// Requires: Azure.AI.Projects 1.2.0+, Azure.AI.Projects.OpenAI

// Load environment variables
Env.Load("../../.env");

var projectEndpoint = Environment.GetEnvironmentVariable("AI_FOUNDRY_PROJECT_CONNECTION_STRING")!;
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")!;
var deploymentEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_ENDPOINT")!;

var credential = new DefaultAzureCredential();

Console.WriteLine("🧪 AI Foundry Evaluations Demo");
Console.WriteLine(new string('=', 50));

try
{
    // ─── Step 1: Connect to AI Foundry project ───────────────────────
    var projectClient = new AIProjectClient(new Uri(projectEndpoint), credential);
    EvaluationClient evaluationClient = projectClient.OpenAI.GetEvaluationClient();
    Console.WriteLine("✅ Connected to AI Foundry project");
    Console.WriteLine("✅ Evaluation client ready (OpenAI Evals API)");

    // ─── Step 2: Generate AI responses for test cases ────────────────
    Console.WriteLine("\n📝 Generating AI responses for test cases...");

    // ChatCompletionsClient needs explicit scope for Azure OpenAI endpoint
    var chatCredential = new ScopedTokenCredential(credential, "https://cognitiveservices.azure.com/.default");
    var chatClient = new ChatCompletionsClient(new Uri(deploymentEndpoint), chatCredential);

    // Load test cases
    var testDataPath = Path.Combine(AppContext.BaseDirectory, "test-dataset.json");
    if (!File.Exists(testDataPath))
        testDataPath = "test-dataset.json";
    var testData = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(testDataPath));
    var testCases = testData.GetProperty("testCases").EnumerateArray().ToList();

    // Generate responses and build evaluation JSONL
    var evalRows = new List<Dictionary<string, string>>();
    foreach (var testCase in testCases)
    {
        var question = testCase.GetProperty("question").GetString()!;
        var expectedResponse = testCase.GetProperty("expectedResponse").GetString()!;

        Console.WriteLine($"   Q: {question}");

        var requestOptions = new ChatCompletionsOptions()
        {
            Model = deploymentName,
            Temperature = 0.3f,
            MaxTokens = 300
        };
        requestOptions.Messages.Add(new ChatRequestSystemMessage(
            "You are a helpful assistant. Provide concise, accurate responses."));
        requestOptions.Messages.Add(new ChatRequestUserMessage(question));

        var response = await chatClient.CompleteAsync(requestOptions);
        var aiResponse = response.Value.Content;
        Console.WriteLine($"   A: {aiResponse}\n");

        evalRows.Add(new Dictionary<string, string>
        {
            ["query"] = question,
            ["response"] = aiResponse,
            ["context"] = expectedResponse,
            ["ground_truth"] = expectedResponse
        });
    }

    // Write JSONL file locally (for reference / debugging; inline data is sent directly)
    var jsonlPath = Path.Combine(Path.GetTempPath(), $"eval-data-{DateTime.Now:yyyyMMdd-HHmmss}.jsonl");
    await using (var writer = new StreamWriter(jsonlPath))
    {
        foreach (var row in evalRows)
        {
            await writer.WriteLineAsync(JsonSerializer.Serialize(row));
        }
    }
    Console.WriteLine($"✅ Generated evaluation data: {evalRows.Count} rows → {jsonlPath}");

    // ─── Step 3: Define evaluation with testing criteria ─────────────
    Console.WriteLine("\n📋 Defining evaluation criteria...");

    // Configure built-in evaluators with data mappings
    // {{item.field}} references fields from the JSONL dataset rows
    // AI-assisted evaluators need deployment_name for the judge model
    object[] testingCriteria = [
        new {
            type = "azure_ai_evaluator",
            name = "coherence",
            evaluator_name = "builtin.coherence",
            data_mapping = new { query = "{{item.query}}", response = "{{item.response}}" },
            initialization_parameters = new { deployment_name = deploymentName },
        },
        new {
            type = "azure_ai_evaluator",
            name = "fluency",
            evaluator_name = "builtin.fluency",
            data_mapping = new { query = "{{item.query}}", response = "{{item.response}}" },
            initialization_parameters = new { deployment_name = deploymentName },
        },
        new {
            type = "azure_ai_evaluator",
            name = "relevance",
            evaluator_name = "builtin.relevance",
            data_mapping = new { query = "{{item.query}}", response = "{{item.response}}" },
            initialization_parameters = new { deployment_name = deploymentName },
        },
        new {
            type = "azure_ai_evaluator",
            name = "f1_score",
            evaluator_name = "builtin.f1_score",
        },
    ];

    // Define the schema of our dataset items
    object dataSourceConfig = new
    {
        type = "custom",
        item_schema = new
        {
            type = "object",
            properties = new
            {
                query = new { type = "string" },
                response = new { type = "string" },
                context = new { type = "string" },
                ground_truth = new { type = "string" },
            },
            required = new { }
        },
        include_sample_schema = true
    };

    // Create the evaluation definition (step 1 of 2)
    BinaryData evaluationData = BinaryData.FromObjectAsJson(new
    {
        name = $"Sample 13 - {DateTime.Now:yyyy-MM-dd HH:mm}",
        data_source_config = dataSourceConfig,
        testing_criteria = testingCriteria
    });

    Console.WriteLine("   Evaluators: coherence, fluency, relevance, f1_score");

    using BinaryContent evaluationContent = BinaryContent.Create(evaluationData);
    ClientResult evalResult = await evaluationClient.CreateEvaluationAsync(evaluationContent);
    var evalFields = ParseClientResult(evalResult, ["name", "id"]);
    string evaluationId = evalFields["id"];
    Console.WriteLine($"✅ Evaluation definition created (ID: {evaluationId})");

    // ─── Step 4: Create evaluation run with inline data ─────────────
    // Pass evaluation data inline to avoid storage access requirements.
    // For larger datasets, use Datasets.UploadFileAsync with file_id source instead.
    Console.WriteLine("\n🚀 Starting evaluation run with inline data...");

    // Build inline content array from our generated evaluation rows
    var inlineContent = evalRows.Select(row => new
    {
        item = new
        {
            query = row["query"],
            response = row["response"],
            context = row["context"],
            ground_truth = row["ground_truth"]
        }
    }).ToArray();

    object dataSource = new
    {
        type = "jsonl",
        source = new
        {
            type = "file_content",
            content = inlineContent
        },
    };

    BinaryData runData = BinaryData.FromObjectAsJson(new
    {
        eval_id = evaluationId,
        name = $"Run - {DateTime.Now:yyyy-MM-dd HH:mm}",
        data_source = dataSource
    });

    using BinaryContent runContent = BinaryContent.Create(runData);
    ClientResult run = await evaluationClient.CreateEvaluationRunAsync(
        evaluationId: evaluationId, content: runContent);
    var runFields = ParseClientResult(run, ["id", "status"]);
    string runId = runFields["id"];
    string runStatus = runFields["status"];
    Console.WriteLine($"✅ Evaluation run created (ID: {runId}, Status: {runStatus})");

    // ─── Step 5: Poll for completion ─────────────────────────────────
    Console.WriteLine("\n⏳ Waiting for evaluation to complete...");

    while (runStatus != "failed" && runStatus != "completed")
    {
        await Task.Delay(TimeSpan.FromSeconds(5));
        run = await evaluationClient.GetEvaluationRunAsync(
            evaluationId: evaluationId,
            evaluationRunId: runId,
            options: new());
        runStatus = ParseClientResult(run, ["status"])["status"];
        Console.Write($"\r   Status: {runStatus}...     ");
    }

    Console.WriteLine();

    if (runStatus == "failed")
    {
        string error = GetErrorMessage(run);
        Console.WriteLine($"❌ Evaluation run failed: {error}");

        // Dump raw response for debugging
        Console.WriteLine("\n📋 Raw response:");
        var rawJson = run.GetRawResponse().Content.ToString();
        try
        {
            var formatted = JsonSerializer.Serialize(
                JsonSerializer.Deserialize<JsonElement>(rawJson),
                new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(formatted);
        }
        catch { Console.WriteLine(rawJson); }
    }
    else
    {
        // ─── Step 6: Display results ─────────────────────────────────
        Console.WriteLine("✅ Evaluation completed!\n");
        Console.WriteLine("📊 Evaluation Results");
        Console.WriteLine(new string('─', 60));

        // Show result counts
        string resultCounts = GetResultCounts(run);
        Console.WriteLine($"   Result counts: {resultCounts}");

        // Retrieve per-item results
        Console.WriteLine("\n📋 Per-item results:");
        Console.WriteLine(new string('─', 60));

        List<string> outputItems = await GetResultsAsync(
            evaluationClient, evaluationId, runId);

        Console.WriteLine($"   Total items: {outputItems.Count}\n");
        foreach (string item in outputItems)
        {
            // Pretty-print each result
            try
            {
                var formatted = JsonSerializer.Serialize(
                    JsonSerializer.Deserialize<JsonElement>(item),
                    new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(formatted);
            }
            catch { Console.WriteLine(item); }
        }
        Console.WriteLine(new string('─', 60));
    }

    // Clean up temp file
    File.Delete(jsonlPath);
}
catch (Exception ex)
{
    Console.WriteLine($"\n❌ Error: {ex.Message}");
    if (ex.InnerException != null)
        Console.WriteLine($"   Inner: {ex.InnerException.Message}");
    Console.WriteLine($"\n   Stack: {ex.StackTrace}");
}

Console.WriteLine("\n👋 Evaluation demo completed!");

// ─── Helper methods ──────────────────────────────────────────────────

// Parse string properties from a ClientResult's JSON response
static Dictionary<string, string> ParseClientResult(ClientResult result, string[] expectedProperties)
{
    Dictionary<string, string> results = [];
    JsonDocument document = JsonDocument.Parse(result.GetRawResponse().Content);
    foreach (JsonProperty prop in document.RootElement.EnumerateObject())
    {
        foreach (string key in expectedProperties)
        {
            if (prop.NameEquals(Encoding.UTF8.GetBytes(key)) && prop.Value.ValueKind == JsonValueKind.String)
            {
                results[key] = prop.Value.GetString()!;
            }
        }
    }
    List<string> notFound = expectedProperties.Where(key => !results.ContainsKey(key)).ToList();
    if (notFound.Count > 0)
    {
        // Non-fatal: log missing properties but continue
        Console.WriteLine($"   ⚠️  Missing JSON properties: {string.Join(", ", notFound)}");
        Console.WriteLine($"   Raw: {document.RootElement}");
    }
    return results;
}

// Extract error message from a failed evaluation run
static string GetErrorMessage(ClientResult result)
{
    JsonDocument document = JsonDocument.Parse(result.GetRawResponse().Content);
    string? code = null, message = null;
    foreach (JsonProperty prop in document.RootElement.EnumerateObject())
    {
        if (prop.NameEquals("error"u8) && prop.Value.ValueKind != JsonValueKind.Null)
        {
            foreach (JsonProperty errorProp in prop.Value.EnumerateObject())
            {
                if (errorProp.Value.ValueKind == JsonValueKind.String)
                {
                    if (errorProp.NameEquals("code"u8)) code = errorProp.Value.GetString();
                    else if (errorProp.NameEquals("message"u8)) message = errorProp.Value.GetString();
                }
            }
        }
    }
    return !string.IsNullOrEmpty(message)
        ? $"Message: {message}, Code: {code ?? "<none>"}"
        : "(no error details available)";
}

// Format result_counts from evaluation run response
static string GetResultCounts(ClientResult result)
{
    JsonDocument document = JsonDocument.Parse(result.GetRawResponse().Content);
    StringBuilder sb = new();
    foreach (JsonProperty prop in document.RootElement.EnumerateObject())
    {
        if (prop.NameEquals("result_counts"u8) && prop.Value.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty count in prop.Value.EnumerateObject())
            {
                if (count.Value.ValueKind == JsonValueKind.Number)
                {
                    if (sb.Length > 0) sb.Append(", ");
                    sb.Append($"{count.Name}: {count.Value.GetInt32()}");
                }
            }
        }
    }
    return sb.Length > 0 ? sb.ToString() : "(not available)";
}

// Retrieve all evaluation run output items (paged)
static async Task<List<string>> GetResultsAsync(
    EvaluationClient client, string evaluationId, string evaluationRunId)
{
    List<string> resultJsons = [];
    bool hasMore = false;
    do
    {
        ClientResult resultList = await client.GetEvaluationRunOutputItemsAsync(
            evaluationId: evaluationId,
            evaluationRunId: evaluationRunId,
            limit: null, order: "asc", after: default,
            outputItemStatus: default, options: new());
        JsonDocument document = JsonDocument.Parse(resultList.GetRawResponse().Content);

        foreach (JsonProperty topProp in document.RootElement.EnumerateObject())
        {
            if (topProp.NameEquals("has_more"u8))
            {
                hasMore = topProp.Value.GetBoolean();
            }
            else if (topProp.NameEquals("data"u8) && topProp.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement item in topProp.Value.EnumerateArray())
                {
                    resultJsons.Add(item.ToString());
                }
            }
        }
    } while (hasMore);

    return resultJsons;
}

// Helper: wraps a TokenCredential to always request a specific scope
public class ScopedTokenCredential : Azure.Core.TokenCredential
{
    private readonly Azure.Core.TokenCredential _inner;
    private readonly string _scope;

    public ScopedTokenCredential(Azure.Core.TokenCredential inner, string scope)
    {
        _inner = inner;
        _scope = scope;
    }

    public override Azure.Core.AccessToken GetToken(Azure.Core.TokenRequestContext requestContext, CancellationToken cancellationToken)
        => _inner.GetToken(new Azure.Core.TokenRequestContext(new[] { _scope }), cancellationToken);

    public override ValueTask<Azure.Core.AccessToken> GetTokenAsync(Azure.Core.TokenRequestContext requestContext, CancellationToken cancellationToken)
        => _inner.GetTokenAsync(new Azure.Core.TokenRequestContext(new[] { _scope }), cancellationToken);
}
