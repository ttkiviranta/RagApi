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
- Interview management
- Application tracking
- Candidate management

## Recent Code Modernizations

The codebase has recently undergone several modernizations to take advantage of C# 12 and .NET 8 features:

### Model Improvements
- **File-scoped namespaces** - Replaced traditional namespace blocks with cleaner file-scoped syntax
- **Non-nullable string defaults** - Added `= string.Empty` to string properties to avoid null reference issues
- **DateTime defaults** - Standardized UTC datetime defaults with `= DateTime.UtcNow`
- **Nullable navigation properties** - Added `?` to reference-type navigation properties for better null safety
- **Helper properties** - Added computed properties like `IsActive`, `FullName`, `DisplayName` and others to facilitate common operations
- **Collection initializers** - Used modern collection initializers like `= []` where appropriate

### Controller Enhancements
- **Replaced AutoMapper with Mapperly** - Switched from runtime reflection-based mapping to compile-time source generation for better performance and type safety
- **Standardized responses** - All controllers now use the BaseController methods for consistent API responses
- **Improved logging** - Added structured logging with context information across controllers

These changes improve type safety, reduce runtime errors, and enhance code readability without altering database schema or requiring migrations. The modernizations follow best practices for C# 12 and .NET 8, such as nullable reference types and file-scoped namespaces.

## Document Processing

PDF documents are processed asynchronously using Azure Service Bus:
1. Documents are uploaded to Azure Blob Storage
2. A message is sent to Service Bus queue
3. Background service processes the document:
   - Extracts text using Azure Document Intelligence
   - Creates vector embeddings using Azure OpenAI
   - Indexes content in Azure Cognitive Search

## Architecture

RagAPI integrates several Azure services:
- **Azure Cognitive Search**: Vector database for semantic document search
- **Azure Document Intelligence**: Extract text from PDF documents
- **Azure OpenAI**: Generate contextual responses
- **Azure Blob Storage**: Store original documents
- **Azure SQL Database**: Store conversations, applications, interviews, and system settings
- **Azure Service Bus**: Message queue for asynchronous PDF processing

## API Endpoints

### Document Management

POST /api/documents
- Upload and process a document (currently PDF only)
- Form Data Parameters:
  - `file`: The PDF file to upload
  - `DocumentType`: Type of document (Resume, CoverLetter, JobPosting, etc.)
  - `EntityId`: (Optional) ID to associate with a specific entity
  - `Metadata`: (Optional) Additional metadata about the document

GET /api/documents/{id}
- Get document details by ID

### RAG Query Endpoints

POST /api/rag/query
- Make a single query without conversation context
- Request Body: `{ "query": "Your question", "filters": { "optional": "filters" }, "maxResults": 5 }`

### Conversation Management

POST /api/rag/conversations
- Create a new conversation
- Request Body: `{ "title": "Conversation Title" }`

GET /api/rag/conversations
- Get all conversations for the current user

GET /api/rag/conversations/{conversationId}
- Get a specific conversation by ID

POST /api/rag/conversations/{conversationId}/query
- Make a query in a conversation context (with history)
- Request Body: `{ "query": "Your follow-up question" }`

### Job Matching and Applications

POST /api/rag/job-matching
- Match job postings with candidate resumes
- Request Body: `{ "jobPosting": "Full job description text", "candidateIds": ["id1", "id2"], "maxCandidates": 10 }`

POST /api/job-matching/candidates/{candidateId}/match-jobs
- Match a candidate with available job postings
- Query Parameter: `limit` (default: 10)

### Applications Management

GET /api/applications
- Get all applications

GET /api/applications/{id}
- Get application details by ID

POST /api/applications
- Create a new application

PUT /api/applications/{id}/status
- Update application status

### Interview Management

GET /api/interviews
- Get all interviews

GET /api/interviews/{id}
- Get interview details by ID

POST /api/interviews
- Create a new interview

PUT /api/interviews/{id}
- Update interview details

GET /api/interviews/application/{applicationId}
- Get all interviews for an application

POST /api/interviews/{id}/questions
- Generate interview questions for an interview

### System Prompts

GET /api/systemprompt
- Get all system prompts

GET /api/systemprompt/{id}
- Get a specific system prompt

GET /api/systemprompt/default
- Get the default system prompt

POST /api/systemprompt
- Create a new system prompt

PUT /api/systemprompt/{id}
- Update an existing system prompt

DELETE /api/systemprompt/{id}
- Delete a system prompt

### User Management

GET /api/user
- Get all users

GET /api/user/{id}
- Get a specific user

POST /api/user
- Create a new user

## Authentication

RagAPI uses Azure AD for authentication. To authenticate:

1. Obtain a token from Azure AD using the configured client credentials
2. Include the token in the Authorization header of your requests
3. User roles (Admin/User) are determined based on Azure AD group membership

## Response Format

All responses follow a standardized format:

**Success responses**:
```json
{
    "error": false,
    "data": { /* response data */ }
}
```

**Error responses**:
```json
{
    "error": true,
    "message": "Error description",
    "timestamp": "2024-03-21T12:00:00Z",
    "path": "/api/endpoint"
}
```

## Setup and Configuration

### Prerequisites

- .NET 8 SDK
- SQL Server or Azure SQL Database
- Azure subscription for cognitive services
- Visual Studio 2022 or other compatible IDE

### Configuration

The application uses the following configuration values in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your SQL connection string",
    "AzureBlobStorage": "Your Azure Blob Storage connection string",
    "ServiceBus": "Your Azure Service Bus connection string"
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
```

### Development

1. Clone the repository
2. Copy `appsettings.json.template` to `appsettings.json`
3. Update `appsettings.json` with your service credentials
4. Run database migrations: `dotnet ef database update`
5. Start the application: `dotnet run`

### Project Structure

- **Api/**: API controllers and models
- **Services/**: Core business logic
- **Interfaces/**: Service abstractions
- **Models/**: Data models and DTOs
- **Data/**: Database context and configuration
- **Helpers/**: Utility classes and helpers

## License

This project is licensed under the MIT License - see the LICENSE file for details.