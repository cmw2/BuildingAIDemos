using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Azure.AI.Projects;
using Azure.AI.Inference;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Azure.Storage;
using Azure.Identity;
using Azure.Core;
using DotNetEnv;

// Sample 09: Multi-Modal Image Analysis with Connected Azure Storage
//
// Upload images to connected Azure Storage, analyze them with AI Foundry vision models,
// and demonstrate how AI Foundry coordinates multi-service workflows.

// Load environment variables
Env.Load(".env");

// Get configuration from environment
var projectEndpoint = Environment.GetEnvironmentVariable("AI_FOUNDRY_PROJECT_CONNECTION_STRING")!;
var modelDeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")!;
var modelDeploymentEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_ENDPOINT")!;
var connectedStorageName = Environment.GetEnvironmentVariable("AI_FOUNDRY_STORAGE_CONNECTION_NAME") ?? "image-storage";
var containerName = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONTAINER_NAME") ?? "processed-images";

Console.WriteLine("📸 Multi-Modal Image Analysis with Connected Storage");
Console.WriteLine("Upload images and get AI analysis with structured data extraction\n");

// Set up authentication
var credential = new DefaultAzureCredential();
var tokenCredential = new TokenCredentialWrapper(credential);
var openAiTokenCredential = new OpenAITokenCredentialWrapper(credential);

try
{
    // Create AI Project client
    AIProjectClient aiProjectClient = new AIProjectClient(new Uri(projectEndpoint), tokenCredential);
    
    // Use direct Azure OpenAI client instead of AI Foundry client
    // ChatCompletionsClient visionModel = aiProjectClient.GetChatCompletionsClient();
    ChatCompletionsClient visionModel = new ChatCompletionsClient(new Uri(modelDeploymentEndpoint), openAiTokenCredential);

    // Get connected Azure Storage
    var (containerClient, blobServiceClient) = await GetStorageContainerClient(aiProjectClient, connectedStorageName, containerName, credential);

    Console.WriteLine("✅ Connected to AI Foundry and Azure Storage");
    Console.WriteLine($"📁 Using container: {containerClient.Name}\n");

    // Interactive mode: Let user upload images
    Console.WriteLine("📝 Note: This demo requires real images for meaningful AI analysis.");
    Console.WriteLine("The AI can analyze receipts, invoices, photos, documents, etc.\n");
    
    // Main interactive loop
    Console.WriteLine("🖼️  Interactive Image Processing");
    Console.WriteLine("Enter the path to an image file (or 'quit', 'exit', or 'q' to exit):");
    
    while (true)
    {
        Console.Write("Image path: ");
        var imagePath = Console.ReadLine()?.Trim();
        
        // Remove surrounding quotes if present (Windows Explorer adds these)
        if (!string.IsNullOrEmpty(imagePath) && imagePath.StartsWith('"') && imagePath.EndsWith('"'))
        {
            imagePath = imagePath.Substring(1, imagePath.Length - 2);
        }
        
        if (string.IsNullOrEmpty(imagePath) || 
            imagePath.Equals("quit", StringComparison.OrdinalIgnoreCase) ||
            imagePath.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
            imagePath.Equals("q", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("👋 Goodbye!");
            break;
        }
        
        if (!File.Exists(imagePath))
        {
            Console.WriteLine("❌ File not found. Please try again.");
            continue;
        }
        
        try
        {
            // Upload user's image to connected storage
            var fileName = Path.GetFileName(imagePath);
            var blobName = $"user-upload-{DateTime.Now:yyyyMMdd-HHmmss}-{fileName}";
            var blobClient = containerClient.GetBlobClient(blobName);
            
            await blobClient.UploadAsync(imagePath, overwrite: true);
            Console.WriteLine($"📤 Uploaded: {blobName}");
            
            // Generate accessible URL for the image
            var imageUrl = await GenerateImageAccessUrl(blobClient, containerClient, blobServiceClient);
            
            // Get user's analysis request
            Console.Write("What would you like to know about this image? ");
            var userQuery = Console.ReadLine()?.Trim();
            
            if (string.IsNullOrEmpty(userQuery))
                userQuery = "Describe what you see in this image and extract any text or data.";
            
            // Analyze with AI - Include the image in the request
            var analysisRequest = new ChatCompletionsOptions()
            {
                Model = modelDeploymentName,
                Temperature = 0.3f,
                MaxTokens = 800
            };
            
            analysisRequest.Messages.Add(new ChatRequestSystemMessage("You are an expert image analyst. Provide detailed, accurate analysis of images."));
            
            // Create a user message that includes both text and the image via SAS URL
            var userMessage = new ChatRequestUserMessage(new ChatMessageContentItem[]
            {
                new ChatMessageTextContentItem(userQuery),
                new ChatMessageImageContentItem(imageUrl)
            });
            analysisRequest.Messages.Add(userMessage);
            
            var analysis = await visionModel.CompleteAsync(analysisRequest);
            var result = analysis.Value.Content;
            
            Console.WriteLine("\n🧠 AI Analysis:");
            Console.WriteLine(result);
            Console.WriteLine($"\n{new string('─', 40)}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Processing failed: {ex.Message}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    if (ex.InnerException != null)
        Console.WriteLine($"   Inner: {ex.InnerException.Message}");
}

static async Task<(BlobContainerClient, BlobServiceClient)> GetStorageContainerClient(AIProjectClient aiProjectClient, string connectedStorageName, string containerName, DefaultAzureCredential credential)
{
    Console.WriteLine("🔍 Checking for AI Foundry connections...");
    
    try
    {        
        // Try to get the specific storage connection
        ConnectionProperties storageConnection = aiProjectClient.Connections.Get(connectedStorageName, includeCredentials: true);
        
        Console.WriteLine($"✅ Found connection: {storageConnection.Name}");
        Console.WriteLine($"   Type: {storageConnection.Type}");
        Console.WriteLine($"   Target: {storageConnection.Target}");
        
        // Use the Target URL as the storage account URL
        var storageAccountUrl = storageConnection.Target;
        
        // Use the connection's credentials instead of DefaultAzureCredential
        BlobServiceClient blobServiceClient;
        
        if (storageConnection.Credentials != null)
        {
            Console.WriteLine($"   Using connection credentials: {storageConnection.Credentials.GetType().Name}");
            
            // Handle different credential types from the connection
            switch (storageConnection.Credentials)
            {
                case SASCredentials sasCredentials:
                    // For SAS credentials, the URL likely already includes the SAS token
                    Console.WriteLine("   Using SAS-based authentication");
                    blobServiceClient = new BlobServiceClient(new Uri(storageAccountUrl));
                    break;
                    
                case EntraIDCredentials entraCredentials:
                    // Use Entra ID authentication
                    Console.WriteLine("   Using Entra ID authentication");
                    blobServiceClient = new BlobServiceClient(new Uri(storageAccountUrl), credential);
                    break;
                    
                case ApiKeyCredentials apiKeyCredentials:
                    // For storage account key - use the ApiKey property
                    Console.WriteLine("   Using API Key authentication");
                    var storageCredential = new StorageSharedKeyCredential(
                        GetStorageAccountName(storageAccountUrl), 
                        apiKeyCredentials.ApiKey);
                    blobServiceClient = new BlobServiceClient(new Uri(storageAccountUrl), storageCredential);
                    break;
                    
                default:
                    Console.WriteLine($"   Unsupported credential type: {storageConnection.Credentials.GetType().Name}");
                    Console.WriteLine("   Note: AI Foundry SDK is in preview - some credential types not yet fully implemented");
                    Console.WriteLine("   Falling back to DefaultAzureCredential");
                    blobServiceClient = new BlobServiceClient(new Uri(storageAccountUrl), credential);
                    break;
            }
        }
        else
        {
            Console.WriteLine("   No credentials found, using DefaultAzureCredential");
            blobServiceClient = new BlobServiceClient(new Uri(storageAccountUrl), credential);
        }
        
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();
        
        Console.WriteLine($"   Using connected resource: {connectedStorageName}");
        return (containerClient, blobServiceClient);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️  Could not use AI Foundry connection: {ex.Message}");
        Console.WriteLine("Falling back to direct storage account approach...\n");
        
        // Fallback to direct storage account approach
        var storageAccountName = Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT_NAME");
        if (string.IsNullOrEmpty(storageAccountName))
        {
            throw new InvalidOperationException("Please set either:\n   1. Configure a storage connection in AI Foundry, or\n   2. Set AZURE_STORAGE_ACCOUNT_NAME environment variable");
        }
        
        var storageAccountUrl = $"https://{storageAccountName}.blob.core.windows.net";
        var blobServiceClient = new BlobServiceClient(new Uri(storageAccountUrl), credential);
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();

        Console.WriteLine($"   Using storage account: {storageAccountName}");
        return (containerClient, blobServiceClient);
    }
}

static string GetStorageAccountName(string storageAccountUrl)
{
    var uri = new Uri(storageAccountUrl);
    var host = uri.Host;
    // Extract account name from URL like "https://mystorageaccount.blob.core.windows.net"
    return host.Split('.')[0];
}

static async Task<Uri> GenerateImageAccessUrl(BlobClient blobClient, BlobContainerClient containerClient, BlobServiceClient blobServiceClient)
{
    if (blobClient.CanGenerateSasUri)
    {
        // We can generate SAS directly (storage account key or SAS credential)
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerClient.Name,
            BlobName = blobClient.Name,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);
        
        var imageUrl = blobClient.GenerateSasUri(sasBuilder);
        Console.WriteLine($"🔗 Generated SAS URL for AI access");
        return imageUrl;
    }
    else
    {
        // Can't generate SAS directly, try user delegation SAS
        try
        {
            Console.WriteLine("🔑 Generating user delegation SAS token...");
            
            // Get user delegation key
            var delegationKey = await blobServiceClient.GetUserDelegationKeyAsync(
                DateTimeOffset.UtcNow, 
                DateTimeOffset.UtcNow.AddHours(1));
            
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerClient.Name,
                BlobName = blobClient.Name,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);
            
            // Generate user delegation SAS URI
            var sasQueryParameters = sasBuilder.ToSasQueryParameters(delegationKey.Value, blobServiceClient.AccountName);
            var sasUri = new UriBuilder(blobClient.Uri) { Query = sasQueryParameters.ToString() };
            Console.WriteLine($"🔗 Generated user delegation SAS URL for AI access");
            return sasUri.Uri;
        }
        catch (Exception sasEx)
        {
            Console.WriteLine($"⚠️  Could not generate user delegation SAS: {sasEx.Message}");
            Console.WriteLine("🔗 Using direct blob URL (blob must be publicly accessible)");
            return blobClient.Uri;
        }
    }
}

// Custom wrapper to handle token scope (same as Sample 7)
public class TokenCredentialWrapper : TokenCredential
{
    private readonly TokenCredential _innerCredential;

    public TokenCredentialWrapper(TokenCredential innerCredential)
    {
        _innerCredential = innerCredential;
    }

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        var context = new TokenRequestContext(new[] { "https://ai.azure.com/.default" });
        return _innerCredential.GetToken(context, cancellationToken);
    }

    public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        var context = new TokenRequestContext(new[] { "https://ai.azure.com/.default" });
        return _innerCredential.GetTokenAsync(context, cancellationToken);
    }
}

// Token wrapper for Azure OpenAI (uses different scope)
public class OpenAITokenCredentialWrapper : TokenCredential
{
    private readonly TokenCredential _innerCredential;

    public OpenAITokenCredentialWrapper(TokenCredential innerCredential)
    {
        _innerCredential = innerCredential;
    }

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        var context = new TokenRequestContext(new[] { "https://cognitiveservices.azure.com/.default" });
        return _innerCredential.GetToken(context, cancellationToken);
    }

    public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        var context = new TokenRequestContext(new[] { "https://cognitiveservices.azure.com/.default" });
        return _innerCredential.GetTokenAsync(context, cancellationToken);
    }
}
