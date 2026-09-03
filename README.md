# Azure RAG Search Services

A modern, high-performance .NET 9 solution implementing Clean Architecture, CQRS, and reliable messaging patterns for Retrieval-Augmented Generation (RAG) and message processing. 🚀

This repository showcases an enterprise-ready architecture that combines vector search capabilities (using local Ollama embeddings and Azure Cosmos DB) with asynchronous message processing via Azure Service Bus.

---

## Architecture Overview

The solution is built following **Clean Architecture** principles to ensure a decoupled, testable, and maintainable codebase.

### Layer Breakdown

1. **`az_rag_search_services.Domain`**
   - Core enterprise entities: `Note` (supporting vector embeddings) and `Order`/`OrderItem` records.
   - Core domain-specific abstractions like `IOrderProcessor`.

2. **`az_rag_search_services.Application`**
   - Implements CQRS (Command Query Responsibility Segregation) pattern with clear messaging abstractions (`ICommandHandler`, `IQueryHandler`).
   - Split into focused use cases under `Features` (e.g., `AddNoteCommand`, `GetNoteByIdQuery`, `SearchNotesByVectorQuery`, and `AddOrderCommand`).
   - Defines infrastructural interfaces to maintain decoupling from concrete technologies.

3. **`az_rag_search_services.Infrastructure`**
   - Concrete implementations of the application abstractions.
   - **Azure Cosmos DB integration** (`AzureCosmosDbService`) for vector and document storage.
   - **Local AI Embedding Integration** via `OllamaEmbeddingService` to compute embeddings for RAG workflows.
   - **Service Bus integration** (`ServiceBusOrderSender`) to publish messages asynchronously.
   - *(Optional)* Entity Framework Core with `pgvector` for PostgreSQL.

4. **`az_rag_search_services.ApiNoteRagSearch`**
   - ASP.NET Core Minimal API with a modular design (`IModule` registration).
   - Integrates **API Versioning** and OpenAPI generation.
   - Powered by **Scalar** for clean, interactive API documentation.

5. **`az_rag_search_services.Worker`**
   - A robust background worker (`OrderProcessingWorker`) implemented as a .NET `BackgroundService`.
   - Listens to Azure Service Bus queues to process orders asynchronously.
   - Features advanced message settlement patterns.

6. **`az_rag_search_services.Test`**
   - Contains unit tests (built with xUnit) to verify business logic and repository integrity.

---

## Core Features

### 1. Vector Embeddings & RAG Workflows
- **Ollama Integration**: Uses a local Ollama instance (`OllamaEmbeddingService`) to generate high-quality text embeddings.
- **Vector Search**: Embeddings are stored alongside note contents in Azure Cosmos DB, allowing efficient vector-based similarity search via `/api/notes/search`.

### 2. Reliable Message Processing & Settlement
The Worker application processes orders from Azure Service Bus with a strict manual-settlement strategy (`AutoCompleteMessages = false`) to guarantee reliable delivery:
- **Immediate Dead-Lettering (Fail-Fast)**: If a message contains poison JSON (deserialization failure) or invalid business data (`InvalidDataException`), it is immediately moved to the Dead-Letter Queue (DLQ) to prevent wasting retry attempts.
- **Transient Failures (Retry-and-Wait)**: If a temporary exception occurs, the message is abandoned. It remains in the queue for a subsequent retry until it either succeeds or exceeds the maximum delivery limit (where Service Bus DLQs it automatically).

---

## Getting Started

### Prerequisites
- **SDK**: .NET 9 SDK
- **Vector Engine**: Ollama running locally (for embedding generation)
- **Database**: Azure Cosmos DB (or Cosmos DB Emulator running locally)
- **Messaging**: Azure Service Bus (or local emulator)

### Configuration
Add your connection strings to your environment variables or update the `appsettings.json` file in the API and Worker projects:

### Running the Services

1. Run the Web API:
   ```bash
   dotnet run --project az_rag_search_services.ApiNoteRagSearch
   ```

2. Run the Worker Background Service:
   ```bash
   dotnet run --project az_rag_search_services.Worker
   ```

3. Run Tests:
   ```bash
   dotnet test
   ```

---

## API Endpoints

Explore and interact with the endpoints via the **Scalar API Playground** at `/scalar/v1` when running in development mode.

### Notes API
- `POST /api/notes` - Creates a new note and generates its vector embedding.
- `GET /api/notes/{id}` - Retrieves a note by its identifier (uses Guid v7).
- `POST /api/notes/search` - Performs a vector search for notes most relevant to a query.

### Orders API
- `POST /api/orders/add` - Publishes a new order message to the Service Bus queue for processing.
