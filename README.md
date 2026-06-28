# NexOrder.ProductService

This repository contains the **Product Service** microservice for the NexOrder platform — a cloud-native .NET microservices solution built using Clean Architecture principles and Azure services.

---

## 🧱 Overview

NexOrder.ProductService is responsible for **product management** within the NexOrder ecosystem.

The service intentionally keeps business functionality simple (CRUD) while demonstrating **real-world backend architecture, cloud-native patterns, security, CI/CD, and messaging**.

---

## 🧩 Key Concepts Demonstrated

- Clean Architecture (Domain / Application / Infrastructure)
- Azure Functions (serverless microservice)
- MediatR (CQRS-style command/query separation)
- Entity Framework Core
- Azure SQL Database
- Azure API Management (API Gateway)
- JWT-based authentication (validated at API-M)
- **Azure Service Bus (event-driven messaging)**
- GitHub Actions CI/CD
- Azure Open AI for quick add product
- Cloud-ready configuration & secrets handling
- **Docker + containerized deployments**

---

## 📁 Project Structure

```
NexOrder.ProductService
├── NexOrder.ProductService               # Azure Functions host
├── NexOrder.ProductService.Domain        # Domain entities & business rules
├── NexOrder.ProductService.Application   # Use cases, handlers, interfaces
├── NexOrder.ProductService.Infrastructure# EF Core, DB context, migrations
├── NexOrder.ProductService.Messages      # Integration message contracts
├── NexOrder.ProductService.Shared        # Shared utilities & common models
```

---

## 🚀 Features

- Create, update, delete, and query products
- Clean separation of concerns across layers
- Designed for scalability and extensibility
- Secured behind Azure API Management
- Event publication for downstream services
- Runs locally via **Azure Functions Core Tools** or **Docker**
- Redis-based caching for optimized performance

---

## 🧠 Caching Strategy (Redis)

To improve performance and reduce database load, Redis caching is
implemented for the **Product List API**.

### ✅ What is cached?

-   Paginated product list responses
-   Filtered results

### 🔑 Cache Key Structure

products:v{version}:page={pageNumber}:size={pageSize}

-   Includes pagination parameters
-   Uses versioning to handle invalidation

------------------------------------------------------------------------

### 🔄 Cache Invalidation Strategy

Instead of deleting cache keys manually:

-   A **version-based approach** is used
-   A global key is maintained:

products:version

-   On any **Add / Update / Delete**:
    -   Version is incremented
    -   Old cache becomes obsolete automatically
    -   
To ensure high performance and data consistency, NexOrder uses a **Versioned Redis Cache** strategy:

* **Caching:** Product data is cached using a versioned key pattern.
* **Invalidation (Event-Driven):** We use **Azure Service Bus** to maintain consistency across instances.
* **Workflow:**
    * 1. An `Add/Update/Delete` operation occurs.
    * 2. A command (`UpdateProductsCache`) is published to the `productservicecommands` queue.
    * 3. The `ProductService` consumes its own command and increments the **Cache Version**.
    * 4. Subsequent reads fetch the new version, effectively invalidating the old cache.

------------------------------------------------------------------------
### 🛡️ Resilience & Performance
To handle high-load scenarios, **NexOrder** implements advanced caching patterns:

#### Cache Stampede Protection (FusionCache)
When a popular cache key expires, multiple concurrent requests often try to re-generate the same data simultaneously, hitting the database all at once. 
- **Solution:** We use **FusionCache's** built-in "Fail-Safe" and "Soft-Expiration" mechanisms.
- **Implementation:** By utilizing **Probabilistic Propagation** and **Optimistic Locking**, FusionCache ensures that only one factory execution happens at a time for a specific key, while other concurrent requests wait for the result or receive a slightly stale (fail-safe) value.


------------------------------------------------------------------------
### ⏱ Cache Expiry

-   TTL (Time-To-Live) for now 5 minutes is applied to avoid stale data
-   Ensures eventual consistency

------------------------------------------------------------------------

### ⚙️ Technology Used

-   IFusionCache abstraction
-   Redis via StackExchange.Redis

------------------------------------------------------------------------

### 🏥 Health Checks Implementation
To ensure system resilience and support container orchestration platforms (like Kubernetes or Azure Container Apps), this service exposes detailed health monitoring endpoints.

#### Configured Dependencies
The service monitors the connectivity of three critical infrastructure dependencies:
 - **Database**: EF Core / SQL Server connectivity check.
 - **Cache**: Redis distributed cache validation.
 - **Messaging**: Azure Service Bus queue connectivity.

 **Refer to Program.cs file for Healthchecks middleware registrations**
 
 A separate endpoint for Health check is defined in `HealthFunction.cs` with url: `/health`

------------------------------------------------------------------------

## 🤖 AI-Powered Capabilities

`NexOrder.ProductService` leverages **Microsoft Semantic Kernel** to provide intelligent, natural language capabilities that bridge the gap between unstructured user intents and structured domain operations.

Instead of traditional hardcoded logic, the service uses an extensible, plugin-based architecture where the kernel orchestrates native C# plugins to execute business logic.

---

### 1. "Quick Add" Feature (`AddProductPlugin`)
This feature allows users to create new products instantly using natural language inputs (e.g., *"Add a premium leather wallet with a sleek design for $45, category Accessories, stock 150"*). 

* **Intent Extraction:** The kernel processes the unstructured text, automatically extracting and mapping relevant product attributes (`Name`, `Description`, `Price`).
* **Deterministic Execution:** The data is passed directly into the **`AddProductPlugin`**, ensuring that the actual validation and persistence layers remain strictly controlled by native C# business services.

### 2. Semantic Product Search (`SearchProductsPlugin`)
Finding products is no longer limited to exact keyword matching or rigid database filters. This feature allows users to discover products via natural, conversational queries (e.g., *"Find me gadgets under $100 that are currently in stock"* or *"Show me popular summer clothes"*).

* **Intelligent Filtering:** The **`SearchProductsPlugin`** utilizes native kernel functions to analyze the user's criteria, intelligently translating context (like price thresholds, stock availability, and categories) into structured database query filters.
* **Context-Aware Results:** The AI ensures relevant product matching based on the semantic intent behind the search query, rather than just simple text matching.

   **Refer to Program.cs file for OpenAI and Kernel registrations**
------------------------------------------------------------------------

## 🛠️ Tech Stack

- **.NET 8**
- **Azure Functions**
- **Entity Framework Core**
- **MediatR**
- **Azure SQL**
- **Azure Open AI**
- **Microsoft Semantic Kernel**
- **Azure API Management**
- **Azure Service Bus**
- **Azure Managed Redis**
- **Docker / Docker Compose**
- **GitHub Actions** 

---

## 📣 Event-Driven Messaging

NexOrder.ProductService participates in an **event-driven architecture** using **Azure Service Bus** for asynchronous communication between microservices.

### 🔄 Message Publishing

When a product is updated, the service publishes a domain event to Azure Service Bus:

- **Topic:** `productserviceevents`
- **Event Type:** `ProductUpdated`
- **Message Contract Library:** `NexOrder.ProductService.Messages`

This enables other services (e.g., Order Service, Inventory Service) to react to product changes without tight coupling.

### 🧾 Message Contract

Message contracts are defined in a dedicated shared library:

```
NexOrder.ProductService.Messages
└── ProductUpdated
```

Benefits:

- Strongly typed event contracts
- Clear ownership of integration boundaries
- Easy versioning and reuse across services

### 📐 Event Flow (Product Update)

1. Client updates a product via API
2. ProductService persists changes using EF Core
3. `ProductUpdated` event is published to Service Bus topic
4. Downstream services consume the event asynchronously

### 🧠 Design Rationale

- Avoids synchronous service-to-service dependencies
- Improves scalability and resilience
- Enables future consumers without modifying Product Service
- Mirrors real-world distributed system design

---

## Private Nuget Packages

This project depends on the **NexOrder.Framework** package, which is hosted via GitHub Packages. To successfully build the project in a GitHub Actions environment, the workflow must be configured to authenticate with the private NuGet source.

### GitHub Actions Workflow Update

An additional step is required before the `dotnet restore` command to register the private source using the `GITHUB_TOKEN`.

Add the following step to your `.github/workflows/main_nexorder-productservice.yml` file:

```yaml
- name: Add Private NuGet Source
  run: |
    dotnet nuget add source "[https://nuget.pkg.github.com/mitanshu-patel/index.json](https://nuget.pkg.github.com/mitanshu-patel/index.json)" \
      --name "github" \
      --username "${{ github.actor }}" \
      --password "${{ secrets.GITHUB_TOKEN }}" \
      --store-password-in-clear-text

- name: Restore dependencies
  run: dotnet restore
```

### Local Development

For local development, developer will need add new Nuget source with the url of index.json as mentioned above and use PAT(Personal Access Token) created via Developer settings, for more refer ```Readme.md``` of **NexOrder.Framework**.

---



## ⚙️ Local Development (without Docker)

### Prerequisites

- .NET SDK 8+
- Azure Functions Core Tools
- SQL Server (local or Azure)
- `dotnet-ef` CLI

### Restore Dependencies

```bash
dotnet restore
```

### ⚙️ Application Configuration

#### appsettings.json

```json
{
  "ConnectionStrings": {
    "SystemDbConnectionString": "<Azure SQL Connection String>",
    "ServiceBusConnectionString": "<Azure Service Bus Connection String>"
  }
}
```

### Apply EF Core Migrations

```bash
dotnet ef database update \
  --project NexOrder.ProductService.Infrastructure \
  --startup-project NexOrder.ProductService.Infrastructure
```

### Run Locally

```bash
func start
```

---

## 🐳 Docker Support

This service can be run locally using **Docker** and **Docker Compose**.

### Prerequisites

- Docker Desktop (or Docker Engine)
- Docker Compose v2

### 🧱 Dockerfile

A `Dockerfile` is included to build a container image for the service.

Build an image locally:

```bash
docker build -t nexorder-productservice:local .
```

Run the container (example):

```bash
docker run --rm -p 8080:80 \
  -e ConnectionStrings__SystemDbConnectionString="<connection-string>" \
  -e ConnectionStrings__ServiceBusConnectionString="<servicebus-connection-string>" \
  -e RedisCacheOptions_Configuration="<redis-connection-string>" \
  -e RedisCacheOptions_InstanceName="<redis-instance-name>" \
  -e OpenAIAPIKey="<your-api-key>" \
  -e OpenAIDeployment="<open-ai-deployment-name>" \
  -e OpenAIEndpoint="<open-ai-endpoint-url>" \
  -e OpenAIModel="<open-ai-model-name>" \
  -e GITHUB_USERNAME="<github-username>" \
  -e GITHUB_TOKEN="<personal-access-token>"
  nexorder-productservice:local
```

> Note: Actual port bindings and hosting settings depend on how the Function host is configured in the container.
> 

### 🧩 Docker Compose

A `docker-compose.yml` is included to simplify local orchestration.

Start services:

```bash
docker compose up --build
```

Stop services:

```bash
docker compose down
```

### 🔐 Configuration in Containers

For local containers, prefer **environment variables** (or a local `.env` file referenced by Compose) rather than committing secrets.

Common keys:

- `ConnectionStrings__SystemDbConnectionString`
- `ConnectionStrings__ServiceBusConnectionString`
- `RedisCacheOptions_Configuration`
- `RedisCacheOptions_InstanceName`
- `OpenAIAPIKey`
- `OpenAIDeployment`
- `OpenAIEndpoint`
- `OpenAIModel`

---

## 🚢 Deployment

### GitHub Actions

The service supports two deployment workflows using **GitHub Actions** with Azure:

1. **Standard deployment (without containerization)** — builds and deploys the Function App directly
2. **Containerized deployment** — builds a Docker image, pushes to Azure Container Registry, and deploys to Azure Web App for Containers

> **Currently, only the containerized deployment workflow is enabled.**
> 

### Standard Deployment Workflow (Disabled)

When enabled, this workflow:

- Builds & restores the .NET project
- Applies EF Core migrations (controlled pipeline step)
- Deploys directly to Azure Functions

> API Management instances are recreated on demand for cost optimization in non-production environments.
> 

### 🧊 Containerized Deployment Workflow (Active)

This service is deployed as a container to **Azure Web App for Containers**.

High-level flow:

1. Build the Docker image via GitHub Actions
2. Push image to **Azure Container Registry**
3. Configure Azure Web App for Containers to pull and run the image
4. Provide required configuration via **App Settings** (environment variables)

Recommended App Settings (examples):

- `ConnectionStrings__SystemDbConnectionString`
- `ConnectionStrings__ServiceBusConnectionString`
- Any other runtime configuration used by the Function host

---

## 🔐 Security & Authentication

- Authentication is handled by a dedicated **Auth Service**
- JWT tokens are validated at **Azure API Management**
- Product Service assumes authenticated requests from API-M
- No authentication logic is embedded inside the microservice

---

## 🌐 API Management Integration

- API is added to API Management by referencing the deployed Azure Function App.
- Inbound policy includes CORS configuration.
- `validate-jwt` inbound policy enforced.
- API Management becomes the only entry point for clients consuming this service.

---

## API Endpoints (Sample)

| Method | Endpoint | Description |
| --- | --- | --- |
| POST | /products/search | Search products |
| GET | /products/{id} | Get product by ID |
| POST | /products | Create new product |
| POST | /products/quick-add | Add product based on user prompt |
| PUT | /products/{id} | Update product |
| DELETE | /products/{id} | Delete product |

---

## 📌 Notes

- Business functionality is intentionally minimal.
- Focus is on architecture, cloud integration, and scalability.
- Designed to be consumed by any frontend or service.
