# Sample 15: Azure OpenAI - Use Your Own Data

This demo shows how to use Azure OpenAI's "Use Your Own Data" feature to chat with your own documents through Azure AI Search integration.

## What this demo shows

- Integration of Azure OpenAI with Azure AI Search
- Using semantic search for better document relevance
- Configuring search parameters for optimal results
- Handling responses with source citations
- Error handling for missing configuration

## Prerequisites

1. **Azure OpenAI Service** with a deployed chat model (e.g., GPT-4)
2. **Azure AI Search Service** with an index containing your documents
3. **Search Index** properly configured with content fields

## Required Environment Variables

Add these to your `.env` file in the project root:

```env
# Existing Azure OpenAI configuration
AZURE_OPENAI_ENDPOINT=https://your-openai-service.openai.azure.com/
AZURE_OPENAI_API_KEY=your-openai-api-key
AZURE_OPENAI_DEPLOYMENT_NAME=your-deployment-name

# Azure AI Search configuration for "Use Your Own Data"
AZURE_SEARCH_ENDPOINT=https://your-search-service.search.windows.net
AZURE_SEARCH_API_KEY=your-search-api-key
AZURE_SEARCH_INDEX_NAME=your-index-name
```

## Search Index Schema

Your Azure AI Search index should have fields that match the configuration in the code:

- `content` - The main text content of your documents
- `title` - Document titles (optional)
- `url` - Document URLs or sources (optional)

You can adjust the field names in the `FieldMappingOptions` section of the code to match your index schema.

## Key Features

### Semantic Search
The demo uses `DataSourceQueryType.Semantic` for better search relevance compared to basic keyword search.

### Search Configuration
- **TopNDocuments**: Number of documents to retrieve (default: 5)
- **InScope**: Restricts responses to only information found in search results
- **Strictness**: Controls how strictly the model adheres to search results (1-5 scale)

### Error Handling
- Validates required environment variables
- Handles API errors gracefully
- Provides clear error messages for configuration issues

## Running the Demo

1. Ensure your `.env` file is configured with all required variables
2. Run the application:
   ```bash
   dotnet run
   ```
3. Ask questions about the content in your search index
4. Type 'quit' to exit

## Sample Questions

Depending on your indexed content, you might ask:
- "What is the company's return policy?"
- "How do I configure the API settings?"
- "What are the system requirements?"

## How It Works

1. **Initialization**: Loads environment variables and validates configuration
2. **Client Setup**: Creates Azure OpenAI client with search data source
3. **Search Integration**: Configures Azure AI Search as a data source for chat completions
4. **Query Processing**: Sends user questions to Azure OpenAI, which automatically searches your index
5. **Response Generation**: Returns answers based on the search results with potential source citations

## Customization

You can customize various aspects:

- **Temperature**: Adjust for more creative (higher) or factual (lower) responses
- **Field Mappings**: Match your search index schema
- **Search Parameters**: Tune TopNDocuments, Strictness, etc.
- **Query Type**: Switch between semantic, simple, or full search

## Security Best Practices

- Never commit API keys to source control
- Use Azure Key Vault for production deployments
- Consider using Managed Identity instead of API keys
- Regularly rotate API keys
- Apply principle of least privilege to search service permissions

## Troubleshooting

**Common Issues:**

1. **Missing environment variables**: Ensure all required variables are set in `.env`
2. **Search service permissions**: Verify the API key has read access to the search index
3. **Index schema mismatch**: Check that field names match your index schema
4. **Empty search results**: Verify your index contains data and is accessible

**Debug Tips:**

- Check the Azure portal for search service status
- Test your search index independently using the Azure portal
- Verify network connectivity to both Azure OpenAI and Azure AI Search services
