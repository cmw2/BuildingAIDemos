# Sample 09: Multi-Modal Image Analysis with Connected Storage

Demonstrates AI Foundry integration with Azure Storage for image processing workflows.

## Features
- Upload images to Azure Blob Storage
- Analyze images using AI Foundry vision models
- Extract structured data from business documents
- Store analysis results back to connected storage
- Interactive image processing mode

## Setup

1. **Environment Variables** (add to `.env`):
```env
AI_FOUNDRY_ENDPOINT=https://your-project.ai.azure.com
AZURE_OPENAI_DEPLOYMENT_NAME=gpt-4o

# Connected Storage Configuration
AI_FOUNDRY_STORAGE_CONNECTION_NAME=image-storage
AZURE_STORAGE_CONTAINER_NAME=processed-images
```

2. **Azure Resources Needed**:
   - AI Foundry project with vision-capable model (gpt-4o, gpt-4-vision)
   - Azure Storage account connected to the AI Foundry project as "image-storage" (or your custom name)
   - The storage account should be added as a "Connected Resource" in AI Foundry portal

## Usage

```bash
dotnet run
```

The demo will:
1. Connect to AI Foundry and linked Azure Storage
2. Allow you to upload your own images for analysis
3. Store all images and analysis results in connected Azure Storage

## Demo Scenarios

### Business Documents
- Upload receipts, invoices, forms
- Extract: amounts, dates, line items, vendor info
- Format results as structured JSON

### General Images
- Describe scenes, objects, people
- Extract any visible text (OCR)
- Identify brands, logos, products

### Multi-Modal Workflow
Shows how AI Foundry coordinates:
- Connected Azure Storage (no connection strings needed)
- AI processing (Vision models)  
- Results storage (JSON files)
- All managed through AI Foundry's connected resources
