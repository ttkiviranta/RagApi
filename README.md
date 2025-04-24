# RagAPI - Retrieval Augmented Generation API

RagAPI is a powerful .NET 8 web API that combines document processing, vector search, and AI-powered question answering to provide context-aware responses based on your documents.

## Overview

RagAPI uses the Retrieval Augmented Generation (RAG) pattern to enhance AI responses with relevant information from your documents. This enables more accurate, contextual, and truthful AI-generated answers.

Key features:
- Document processing (PDF support)
- Semantic vector search
- Conversational memory
- Customizable system prompts
- HR-focused job matching capabilities

## Architecture

RagAPI integrates several Azure services:
- **Azure Cognitive Search**: Vector database for semantic document search
- **Azure Document Intelligence**: Extract text from PDF documents
- **Azure OpenAI**: Generate contextual responses
- **Azure Blob Storage**: Store original documents
- **Azure SQL Database**: Store conversations and system settings

## API Endpoints

### Document Management

POST /api/rag/documents

Upload and process a document (currently PDF only).

**Form Data Parameters**:
- `file`: The PDF file to upload
- `DocumentType`: Type of document (Resume, CoverLetter, JobPosting, etc.)
- `EntityId`: (Optional) ID to associate with a specific entity
- `Metadata`: (Optional) Additional metadata about the document

### Query Endpoints

POST /api/rag/query

Make a single query without conversation context.

**Request Body**:

{ "query": "Your question about the documents", "filters": { "optional": "filters" }, "maxResults": 5 }

### Conversation Management

POST /api/rag/conversations

Create a new conversation.

**Request Body**:
{ "title": "Conversation Title" }

GET /api/rag/conversations

Get all conversations for the current user.

GET /api/rag/conversations/{conversationId}

Get a specific conversation by ID.

POST /api/rag/conversations/{conversationId}/query

Make a query in a conversation context (with history).

**Request Body**:
{ "query": "Your follow-up question" }

### Job Matching

POST /api/rag/job-matching

Match job postings with candidate resumes.

**Request Body**:
{ "jobPosting": "Full job description text", "candidateIds": ["id1", "id2"], "maxCandidates": 10 }

POST /api/job-matching/job-postings

Upload a job posting (PDF).

POST /api/job-matching/candidates/{candidateId}/resume

Upload a candidate's resume (PDF).

POST /api/job-matching/candidates/{candidateId}/cover-letter

Upload a candidate's cover letter (PDF).

POST /api/job-matching/match

Match a job with candidate documents.

POST /api/job-matching/interview-questions

Generate interview questions based on job and candidate match.

### System Prompts

GET /api/systemprompt

Get all system prompts.

GET /api/systemprompt/{id}

Get a specific system prompt.

GET /api/systemprompt/default

Get the default system prompt.

POST /api/systemprompt

Create a new system prompt.

PUT /api/systemprompt/{id}

Update an existing system prompt.

DELETE /api/systemprompt/{id}

Delete a system prompt.

### User Management

GET /api/user

Get all users.

GET /api/user/{id}

Get a specific user.

POST /api/user

Create a new user.

PUT /api/user/{id}

Update an existing user.

DELETE /api/user/{id}

Delete a user.

## Authentication

RagAPI uses Azure AD for authentication. To authenticate:

1. Obtain a token from Azure AD using the configured client credentials
2. Include the token in the Authorization header of your requests
3. User roles (Admin/User) are determined based on Azure AD group membership

## Response Format

All responses follow a standardized format:

**Success responses**:
{ "error": false, "data": { /* response data */ } }
**Error responses**:
{ "error": true, "message": "Error description" }


## Setup and Configuration

### Prerequisites

- .NET 8 SDK
- SQL Server or Azure SQL Database
- Azure subscription for cognitive services
- Visual Studio 2022 or other compatible IDE

### Configuration

The application uses the following configuration values in `appsettings.json`:

 d{
  "ConnectionStrings": {
    "DefaultConnection": "Your SQL connection string",
    "AzureBlobStorage": "Your Azure Blob Storage connection string"
  },
  "Azure": {
    "FormRecognizer": {
      "Endpoint": "Your Document Intelligence endpoint",
      "Key": "Your Document Intelligence key"
    },
    "Search": {
      "Endpoint": "Your AI Search endpoint",
      "IndexName": "pdf-documents",
      "Key": "Your AI Search key"
    },
    "OpenAI": {
      "Endpoint": "Your Azure OpenAI endpoint",
      "Key": "Your Azure OpenAI key",
      "EmbeddingDeployment": "text-embedding-ada-002",
      "ChatDeployment": "gpt-35-turbo"
    }
  }
}

### Deployment

The application can be deployed to Azure App Service:

1. Set up required Azure resources
2. Configure connection strings and settings
3. Deploy using Visual Studio or Azure DevOps pipelines

## Development

### Running locally

1. Clone the repository
2. Update `appsettings.json` with your service credentials
3. Run database migrations: `dotnet ef database update`
4. Start the application: `dotnet run`

### Project Structure

- **Controllers**: API endpoints
- **Services**: Core business logic
- **Interfaces**: Service abstractions
- **Models**: Data models
- **Data**: Database context and configuration

## Application Configuration

This project uses the `appsettings.json.template` file to share the configuration structure without sensitive information.

1. Copy `appsettings.json.template` to a file named `appsettings.json`
2. Fill in the missing values with the correct values, which you can obtain from the project administrator
3. Never commit the `appsettings.json` file to the Git repository!

In production, the application uses Azure App Service environment variables.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
