using Azure;
using Azure.AI.OpenAI;
using DotNetEnv;
using Azure.AI.OpenAI.Chat;
using OpenAI.Chat;

// Load environment variables
Env.Load("../../.env");

Console.WriteLine("🔍 Azure OpenAI - Use Your Own Data Demo");
Console.WriteLine("This demo shows how to use Azure OpenAI with Azure AI Search integration");
Console.WriteLine("using the built-in 'Use Your Own Data' feature");
Console.WriteLine();

// Get Azure OpenAI configuration
var endpoint = new Uri(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!);
var apiKey = new AzureKeyCredential(Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")!);
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")!;

// Get Azure AI Search configuration for "Use Your Own Data"
var searchEndpoint = Environment.GetEnvironmentVariable("AZURE_SEARCH_ENDPOINT");
var searchApiKey = Environment.GetEnvironmentVariable("AZURE_SEARCH_API_KEY");
var searchIndexName = Environment.GetEnvironmentVariable("AZURE_SEARCH_INDEX_NAME");

// Validate required environment variables
if (string.IsNullOrEmpty(searchEndpoint) || string.IsNullOrEmpty(searchApiKey) || string.IsNullOrEmpty(searchIndexName))
{
    Console.WriteLine("❌ Missing required Azure AI Search configuration. Please set the following environment variables:");
    Console.WriteLine("  - AZURE_SEARCH_ENDPOINT");
    Console.WriteLine("  - AZURE_SEARCH_API_KEY");
    Console.WriteLine("  - AZURE_SEARCH_INDEX_NAME");
    Console.WriteLine();
    Console.WriteLine("Example .env configuration:");
    Console.WriteLine("AZURE_SEARCH_ENDPOINT=https://your-search-service.search.windows.net");
    Console.WriteLine("AZURE_SEARCH_API_KEY=your-search-api-key");
    Console.WriteLine("AZURE_SEARCH_INDEX_NAME=your-index-name");
    return;
}

Console.WriteLine($"✅ Azure OpenAI Endpoint: {endpoint}");
Console.WriteLine($"✅ Deployment: {deploymentName}");
Console.WriteLine($"✅ Azure AI Search Endpoint: {searchEndpoint}");
Console.WriteLine($"✅ Search Index: {searchIndexName}");
Console.WriteLine();

try
{
    // Create Azure OpenAI client
    var client = new AzureOpenAIClient(endpoint, apiKey);
    var chatClient = client.GetChatClient(deploymentName);
    // Extension methods to use data sources with options are subject to SDK surface changes. Suppress the
    // warning to acknowledge and this and use the subject-to-change AddDataSource method.
    #pragma warning disable AOAI001

    // Initialize conversation with system message
    var messages = new List<ChatMessage>
    {
        new SystemChatMessage("You are a helpful AI assistant that can answer questions using the provided search index data. When you reference information from the search results, mention that it comes from the knowledge base. If you don't find relevant information in the search results, clearly state that the information is not available in the knowledge base.")
    };

    // Configure chat options with Azure AI Search data source integration
    var options = new ChatCompletionOptions()
    {
        MaxOutputTokenCount = 1000,
        Temperature = 0.3f // Lower temperature for more focused, factual responses
    };

    // Configure the Azure AI Search data source for "Use Your Own Data"
    options.AddDataSource(new AzureSearchChatDataSource()
    {
        Endpoint = new Uri(searchEndpoint),
        IndexName = searchIndexName,
        Authentication = DataSourceAuthentication.FromApiKey(searchApiKey),
        
        // Configure search parameters
        QueryType = DataSourceQueryType.Simple, // Use simple search (semantic search requires specific setup)
        FieldMappings = new DataSourceFieldMappings()
        {
            ContentFieldNames = { "content" }, // Adjust field names based on your index schema
            FilePathFieldName = "filepath",
            TitleFieldName = "title",
            VectorFieldNames = {"contentVector"},
            UrlFieldName = "url"
        },
        
        // Configure result filtering and ranking
        TopNDocuments = 5,              // Number of documents to retrieve from search
        InScope = true,                 // Restrict responses to only the search results
        Strictness = 3
    });

    Console.WriteLine("💬 Chat with your data - Type 'quit' to exit");
    Console.WriteLine("Ask questions about the information in your search index...");
    Console.WriteLine();

    while (true)
    {
        Console.Write("You: ");
        var userInput = Console.ReadLine();
        
        if (userInput?.ToLower() is "quit" or "exit" or "q")
            break;

        if (string.IsNullOrWhiteSpace(userInput))
            continue;

        try
        {
            // Add user message to conversation
            messages.Add(new UserChatMessage(userInput));

            Console.WriteLine("🔍 Searching your data and generating response...");
            
            // Get response from Azure OpenAI with integrated search
            var response = await chatClient.CompleteChatAsync(messages, options);
            
            var aiResponse = response.Value.Content[0].Text;
            Console.WriteLine($"🤖 AI: {aiResponse}");
            
            // Display citations if available (note: citation format may vary based on API version)
            if (response.Value.Usage != null)
            {
                Console.WriteLine($"\n📊 Token usage: {response.Value.Usage.InputTokenCount} input, {response.Value.Usage.OutputTokenCount} output");
            }
            
            // Add AI response to conversation for context
            messages.Add(new AssistantChatMessage(aiResponse));
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error processing your request: {ex.Message}");
            Console.WriteLine();
            
            // Provide helpful error messages for common issues
            if (ex.Message.Contains("401") || ex.Message.Contains("Unauthorized"))
            {
                Console.WriteLine("💡 This might be an authentication issue. Check your API keys.");
            }
            else if (ex.Message.Contains("404") || ex.Message.Contains("NotFound"))
            {
                Console.WriteLine("💡 This might be a configuration issue. Check your endpoints and index name.");
            }
            else if (ex.Message.Contains("403") || ex.Message.Contains("Forbidden"))
            {
                Console.WriteLine("💡 This might be a permissions issue. Check your Azure AI Search access permissions.");
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Failed to initialize: {ex.Message}");
    Console.WriteLine();
    Console.WriteLine("Please check your configuration and try again.");
    
    // Provide setup guidance
    Console.WriteLine();
    Console.WriteLine("🔧 Setup checklist:");
    Console.WriteLine("1. Ensure your Azure OpenAI service is deployed and accessible");
    Console.WriteLine("2. Ensure your Azure AI Search service is created with an index containing data");
    Console.WriteLine("3. Verify all environment variables are correctly set in your .env file");
    Console.WriteLine("4. Check that your search index has a 'content' field (or adjust field mappings in code)");
}

Console.WriteLine("👋 Goodbye!");
