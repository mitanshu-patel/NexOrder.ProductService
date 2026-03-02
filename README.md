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

---

## 🛠️ Tech Stack

- **.NET 8**
- **Azure Functions**
- **Entity Framework Core**
- **MediatR**
- **Azure SQL**
- **Azure API Management**
- **Azure Service Bus**
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
| PUT | /products/{id} | Update product |
| DELETE | /products/{id} | Delete product |

---

## 📌 Notes

- Business functionality is intentionally minimal.
- Focus is on architecture, cloud integration, and scalability.
- Designed to be consumed by any frontend or service.
