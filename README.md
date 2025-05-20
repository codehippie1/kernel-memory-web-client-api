# Personal Memory Management System

A comprehensive memory management system built with LangGraph, Kernel Memory, and Azure AI Search.

## Projects

This repository contains three main projects:

1. **langgraph-memory-manager**: A Node.js application that uses LangGraph to manage memory operations
2. **kernel-memory-web-client-api**: Web client API for interacting with the memory system
3. **kernel-memory-main**: Core Kernel Memory implementation

## Setup

### Prerequisites

- Node.js (v16 or later)
- Python 3.8 or later
- Azure AI Search service
- Azure OpenAI service

### Environment Variables

Create `.env` files in each project directory with the following variables:

```env
# Azure AI Search Configuration
AZURE_SEARCH_ENDPOINT=your_search_endpoint
AZURE_SEARCH_API_KEY=your_search_api_key
AZURE_SEARCH_INDEX_NAME=default

# Azure OpenAI Configuration
AZURE_OPENAI_API_KEY=your_openai_api_key
AZURE_OPENAI_DEPLOYMENT_NAME=your_deployment_name
AZURE_OPENAI_BASE_URL=your_base_url
AZURE_OPENAI_API_VERSION=2024-02-15-preview
```

## Development

1. Install dependencies:
   ```bash
   # For Node.js projects
   npm install

   # For Python projects
   python -m venv venv
   source venv/bin/activate  # or `venv\Scripts\activate` on Windows
   pip install -r requirements.txt
   ```

2. Start the services:
   ```bash
   # Start Kernel Memory service
   cd kernel-memory-main
   dotnet run

   # Start web client API
   cd kernel-memory-web-client-api
   npm start

   # Start LangGraph memory manager
   cd langgraph-memory-manager
   npm start
   ```

## License

MIT 