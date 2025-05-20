# Kernel Memory Web Client API

A minimal ASP.NET Core API that serves as a web client for Kernel Memory operations.

## Features

- Memory upload endpoint with support for content, tags, and document IDs
- Memory query endpoint for asking questions
- CORS enabled for cross-origin requests
- Swagger/OpenAPI documentation

## Prerequisites

- .NET 8.0 SDK
- Kernel Memory service running (default: http://127.0.0.1:9001)

## Getting Started

1. Clone the repository
2. Navigate to the project directory
3. Run the following commands:

```bash
dotnet restore
dotnet run
```

The API will be available at:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001

## API Endpoints

### Upload Memory
```
POST /memory/upload
Content-Type: multipart/form-data

Parameters:
- content: The content to store
- tags: JSON string of tags
- documentId: Unique identifier for the document
```

### Ask Question
```
POST /memory/ask
Content-Type: application/json

Body:
{
    "question": "Your question here"
}
```

## Development

To add new features or modify existing ones:

1. Update the Program.cs file with new endpoints
2. Add any required models in separate files
3. Test the endpoints using Swagger UI at `/swagger`

## License

MIT 