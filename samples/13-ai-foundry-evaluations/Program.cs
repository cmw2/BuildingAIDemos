using Azure.AI.Projects;
using Azure.Identity;
using DotNetEnv;
using System.Text.Json;

// Load environment variables
Env.Load(".env");

// Configure Azure OpenAI model
var modelConfig = new
{
    AzureEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT"),
    ApiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY"),
    AzureDeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME"),
    ApiVersion = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_VERSION")
};

// Initialize the AI Projects client for evaluations
var credential = new DefaultAzureCredential();
var projectsClient = new AIProjectsClient(
    new Uri(modelConfig.AzureEndpoint!), 
    credential
);

// Define the conversation data
var conversation = new
{
    messages = new[]
    {
        new { content = "Which tent is the most waterproof?", role = "user" },
        new { 
            content = "The Alpine Explorer Tent is the most waterproof", 
            role = "assistant", 
            context = "From the our product list the alpine explorer tent is the most waterproof. The Adventure Dining Table has higher weight." 
        },
        new { content = "How much does it cost?", role = "user" },
        new { 
            content = "$120.", 
            role = "assistant", 
            context = "The Alpine Explorer Tent is $120." 
        }
    }
};

// Create evaluation request for groundedness
var evaluationRequest = new
{
    conversation = conversation,
    evaluationType = "groundedness"
};

try
{
    Console.WriteLine("🔍 Running Groundedness Evaluation...\n");
    
    // Note: This is a simplified representation as the exact Azure.AI.Projects API
    // for evaluations may differ. The actual implementation would depend on the
    // specific SDK methods available for groundedness evaluation.
    
    // Serialize and display the conversation that would be evaluated
    var jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    var conversationJson = JsonSerializer.Serialize(conversation, jsonOptions);
    Console.WriteLine("Conversation to evaluate:");
    Console.WriteLine(conversationJson);
    
    Console.WriteLine("\n✅ Groundedness evaluation would be performed here.");
    Console.WriteLine("💡 This demonstrates the C# equivalent structure of the Python evaluation code.");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error during evaluation: {ex.Message}");
}
