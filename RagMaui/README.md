# RagMaui - Cross-Platform Desktop and Mobile Client for RAG API

## Introduction

RagMaui is a .NET 8 MAUI application that serves as a cross-platform client for the RagAPI service. This application provides a user-friendly interface for interacting with the Retrieval Augmented Generation capabilities, enabling users to engage in AI-powered conversations with context from uploaded documents.

## Features

- **Cross-Platform Support**: Windows, macOS, iOS, and Android compatibility
- **SSO Authentication**: Azure AD integrated authentication with MSAL library
- **Conversation Management**: Create, view, and continue conversations
- **Document Upload**: Upload PDF documents to the RAG system
- **Semantic Search**: Chat with AI using document-based context
- **Source Transparency**: View source documents used in AI responses

## Architecture

RagMaui follows the MVVM architectural pattern:
- **Models**: Data structures for conversations, messages, and search results
- **ViewModels**: Logic for different screens and user interactions
- **Views**: UI components and pages
- **Services**: Communication with the RagApi backend and Azure services

## Key Components

### Authentication

RagMaui implements Microsoft Authentication Library (MSAL) for secure single sign-on via Azure AD:
- Supports token acquisition, refresh, and persistent authentication
- Securely manages user credentials and tokens
- Integrates with the application's HTTP client for authenticated API calls

### Conversation Interface

The chat interface provides:
- Real-time message exchange with the RAG system
- Visual distinction between user and AI messages
- Citation of source documents used in responses
- Conversation history and context preservation

### Document Management

Users can upload PDFs directly from the app:
- Document type selection
- Progress feedback
- Error handling and validation

## Integration with RagApi

RagMaui integrates with the RagApi through a RESTful interface:
- **Authentication**: Bearer token authorization using Azure AD tokens
- **Conversation Management**: Creating and retrieving conversations
- **Queries**: Sending user prompts and receiving AI responses
- **Document Processing**: Uploading documents for indexing

## Changes to RagApi Project

The RagApi project has been enhanced to support the RagMaui client:
- **Authentication Improvements**: Enhanced Azure AD integration for cross-platform clients
- **CORS Configuration**: Updated to allow mobile and desktop clients
- **API Optimization**: Endpoints optimized for mobile bandwidth considerations
- **Secure Token Handling**: Improved token validation and refresh mechanisms

## Setup and Configuration

### Prerequisites

- .NET 8 SDK with MAUI workload
- Visual Studio 2022 with MAUI development support
- Azure Account with AD tenant for authentication
- Running instance of RagApi backend

### Configuration

The application requires Azure AD configuration in the `AuthConfig.cs` file:
- ClientId: Azure AD application client ID
- TenantId: Azure AD tenant ID
- Authority: Authentication authority URL
- ApiScope: Access scope for the RagApi
- ApiBaseUrl: URL of the RagApi backend

For security, a template file (`AuthConfig.cs.template`) is provided, and the actual configuration file should be excluded from version control.

### Platform-Specific Setup

For each platform, additional configuration is required to enable SSO authentication:

#### iOS
- URL scheme registration for authentication callbacks
- Keychain security group configuration

#### Android
- Custom tabs and intent filters for authentication flows

#### Windows
- Protocol activation for authentication redirect URIs

## Getting Started

1. Clone the repository
2. Copy `AuthConfig.cs.template` to `AuthConfig.cs` and update with your Azure AD values
3. Configure the API base URL to point to your RagApi instance
4. Build and run the application on your preferred platform

## License

This project is licensed under the MIT License - see the LICENSE file for details.