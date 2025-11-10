# RagAPI - Retrieval Augmented Generation API with AI Agent Orchestration

RagAPI is a powerful .NET 9 web API that combines document processing, vector search, AI-powered question answering, and **intelligent agent orchestration** to provide context-aware responses and automated workflows based on your documents.

## 🆕 **New: AI Agent Orchestration System**

RagAPI now features a comprehensive AI Agent system that enables automated workflows for document processing, candidate management, and job matching:

### **Available Agents:**
- **DocumentAnalysisAgent**: Intelligent PDF analysis and metadata extraction
- **CandidateCreationAgent**: Automated candidate profile creation from CVs
- **JobMatchingAgent**: AI-powered job-candidate matching
- **AgentOrchestrator**: Workflow management and agent coordination

### **Pre-built Workflows:**
- **cv-processing**: Complete CV → Candidate profile creation
- **job-posting-processing**: Job posting analysis and candidate matching

## Overview

RagAPI uses the Retrieval Augmented Generation (RAG) pattern enhanced with agent orchestration to provide:
- Automated document processing workflows
- Intelligent candidate and job management
- Context-aware AI responses
- Multi-agent coordination for complex tasks

Key features:
- 🤖 **AI Agent Orchestration System** (NEW!)
- 📄 Document processing (PDF support) 
- 🔍 Semantic vector search
- 💬 Conversational memory
- 🎯 Customizable system prompts
- 👥 HR-focused job matching capabilities
- 📝 Interview management
- 📊 Application tracking
- 🎯 Automated candidate management

## Recent Code Modernizations (.NET 9 & C# 13)

The codebase has been modernized to take advantage of C# 13 and .NET 9 features:

### Model Improvements
- **File-scoped namespaces** - Clean syntax with .NET 9 optimizations
- **Non-nullable string defaults** - Enhanced null safety with C# 13
- **Collection expressions** - Modern `= []` syntax
- **Primary constructors** - Simplified object creation
- **Required properties** - Compile-time safety improvements

### Agent System Architecture
- **AgentOrchestrator**: Centralized workflow management
- **Workflow definitions**: Declarative multi-step processes  
- **Parallel execution**: Optimized agent coordination
- **Error handling**: Robust failure recovery
- **Extensible design**: Easy addition of new agents

### Service Integrations
- **Azure Service Bus**: Asynchronous message processing
- **Background services**: Continuous PDF processing
- **Agent registration**: Automatic service discovery
- **Dependency injection**: Clean service architecture

## AI Agent System

### Agent Endpoints

**GET /api/agents**
- List all registered agents with their capabilities

**GET /api/agents/workflows** 
- Get available workflow definitions

**POST /api/agents/{agentName}/execute**
- Execute a single agent with custom parameters

**POST /api/agents/workflows/{workflowName}/execute**
- Run a predefined workflow (e.g., "cv-processing")

**POST /api/agents/process-cv**
- Shortcut endpoint: Upload CV → Create candidate → Find job matches

### Example Workflow Usage

```json
POST /api/agents/workflows/cv-processing/execute
{
  "parameters": {
    "blobName": "resume-john-doe.pdf"
  }
}
```

Response includes:
- Extracted candidate information
- Created candidate profile
- Potential job matches
- Workflow execution details

## Document Processing

PDF documents are processed asynchronously using Azure Service Bus and AI Agents:
1. Documents uploaded to Azure Blob Storage
2. Message sent to Service Bus queue  
3. **DocumentAnalysisAgent** processes the document:
   - Extracts text using Azure Document Intelligence
   - Analyzes content structure and metadata
   - Creates vector embeddings using Azure OpenAI
   - Indexes content in Azure Cognitive Search
4. **CandidateCreationAgent** creates profiles (for CVs)
5. **JobMatchingAgent** finds suitable opportunities

## Architecture

RagAPI integrates several Azure services with agent orchestration:
- **Azure Cognitive Search**: Vector database for semantic search
- **Azure Document Intelligence**: Text extraction from PDFs
- **Azure OpenAI**: AI-powered analysis and responses
- **Azure Blob Storage**: Document storage
- **Azure SQL Database**: Structured data and workflows
- **Azure Service Bus**: Asynchronous processing
- **Agent Orchestrator**: Workflow coordination

## MAUI Client Application

RagAPI includes a cross-platform .NET MAUI application with:
- **Multi-platform support**: Windows, macOS, iOS, Android
- **Modern UI**: .NET 9 MAUI with updated controls
- **Comprehensive testing**: Unit tests and mock services
- **Agent integration**: Direct workflow execution from mobile/desktop

### MAUI Features:
- Document upload and processing
- Real-time chat interface
- Candidate and job management
- Interview scheduling
- Agent workflow monitoring

## API Endpoints

### Agent Management (NEW!)

**GET /api/agents**
- List registered agents and their capabilities

**POST /api/agents/process-cv**
- Complete CV processing workflow
- Parameters: `{ "blobName": "filename.pdf" }`

**POST /api/agents/workflows/{workflowName}/execute**
- Execute predefined workflows
- Available workflows: "cv-processing", "job-posting-processing"

### Document Management

**POST /api/documents**
- Upload and process documents
- Triggers agent workflows automatically

**GET /api/documents/{id}**
- Get document details and processing status

### RAG Query Endpoints

**POST /api/rag/query**
- Enhanced with agent insights
- Request: `{ "query": "question", "useAgents": true }`

### Conversation Management
[Previous conversation endpoints remain the same...]

### Job Matching (Enhanced with Agents)

**POST /api/rag/job-matching**
- AI-agent enhanced matching
- Automatic candidate scoring and recommendations

### Applications & Interview Management
[Previous endpoints enhanced with agent insights...]

### System Prompts
[Previous endpoints remain the same...]

## Authentication

RagAPI uses Azure AD for authentication:
- Agent endpoints require authentication
- Role-based access (Admin/User)
- Secure workflow execution

## Setup and Configuration

### Prerequisites

- **.NET 9 SDK** (Updated!)
- SQL Server or Azure SQL Database
- Azure subscription for cognitive services
- Visual Studio 2022 (17.8+) or other compatible IDE

### Configuration

Update `appsettings.json` with your Azure service credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "[Your SQL connection]",
    "AzureBlobStorage": "[Your Blob Storage connection]", 
    "ServiceBus": "[Your Service Bus connection]"
  },
"Azure": {
    "FormRecognizer": {
      "Endpoint": "[Document Intelligence endpoint]",
      "Key": "[Your key]"
    },
    "Search": {
"Endpoint": "[AI Search endpoint]",
      "Key": "[Your key]"
    },
    "OpenAI": {
      "Endpoint": "[Azure OpenAI endpoint]",
      "Key": "[Your key]"
 }
  }
}
```

### Quick Start

1. Clone the repository
2. Copy `appsettings.json.template` to `appsettings.json`
3. Configure Azure service credentials
4. Run: `dotnet ef database update` (RagApi project)
5. Start both applications: `.\start-both.ps1`

### Development Scripts

- **start-both.ps1** - Start both API and MAUI app
- **start-both.bat** - Windows batch alternative

### Project Structure

```
├── RagApi/   # Main Web API with Agents
│   ├── Services/Agents/    # Agent implementations  
│   ├── Api/Controllers/    # REST endpoints
│   ├── Models/Agents/      # Agent models and workflows
│   └── Interfaces/Agents/  # Agent abstractions
├── RagMaui/      # Cross-platform client
│   ├── Views/             # XAML pages
│├── ViewModels/        # MVVM view models
│   └── Services/  # API integration
├── RagApi.Tests/          # API unit tests
├── RagMaui.Tests/         # MAUI unit tests
└── React.UI/   # React client (legacy)
```

## Testing

### Run Tests
```bash
# API tests
dotnet test RagApi.Tests

# MAUI tests  
dotnet test RagMaui.Tests

# All tests
dotnet test
```

### Agent Testing
The system includes comprehensive agent testing with:
- Mock services for Azure integrations
- Workflow simulation
- Error handling validation
- Performance testing

## License

This project is licensed under the MIT License - see the LICENSE file for details.

---

## 🚀 Getting Started with Agents

1. **Upload a CV**: `POST /api/agents/process-cv`
2. **View results**: Automatic candidate creation + job matches
3. **Monitor workflows**: Check agent execution status
4. **Extend system**: Add custom agents and workflows

**Experience the power of AI-driven automation in recruitment!** 🎯