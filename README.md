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

---

## 🛠️ Tech Stack

- **.NET 8**
- **Azure Functions**
- **Entity Framework Core**
- **MediatR**
- **Azure SQL**
- **Azure API Management**
- **Azure Service Bus**
- **GitHub Actions**

---

## 📣 Event-Driven Messaging

NexOrder.ProductService participates in an **event-driven architecture** using **Azure Service Bus** for asynchronous communication between microservices.

### 🔄 Message Publishing

When a product is updated, the service publishes a domain event to Azure Service Bus:

- **Topic:** `productevents`
- **Event Type:** `ProductUpdated`
- **Message Contract Library:** `NexOrder.ProductService.Messages`

This enables other services (e.g., Order Service, Inventory Service) to react to product changes without tight coupling.

---

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

---

### 📐 Event Flow (Product Update)

1. Client updates a product via API
2. ProductService persists changes using EF Core
3. `ProductUpdated` event is published to Service Bus topic
4. Downstream services consume the event asynchronously

---

### 🧠 Design Rationale

- Avoids synchronous service-to-service dependencies
- Improves scalability and resilience
- Enables future consumers without modifying Product Service
- Mirrors real-world distributed system design

---

## ⚙️ Local Development

### Prerequisites

- .NET SDK 8+
- Azure Functions Core Tools
- SQL Server (local or Azure)
- dotnet-ef CLI

---

### Restore Dependencies

```bash
dotnet restore
```

---

## ⚙️ Application Configuration

### appsettings.json

``` json
{
  "ConnectionStrings": {
    "SystemDbConnectionString": "<Azure SQL Connection String>",
    "ServiceBusConnectionString": "<Azure Service Bus Connection String>",
  }
}
```

### Apply EF Core Migrations

```bash
dotnet ef database update \
  --project NexOrder.ProductService.Infrastructure \
  --startup-project NexOrder.ProductService.Infrastructure
```

---

### Run Locally

```bash
func start
```

---

## 🔐 Security & Authentication

- Authentication is handled by a dedicated **Auth Service**
- JWT tokens are validated at **Azure API Management**
- Product Service assumes authenticated requests from API-M
- No authentication logic is embedded inside the microservice

---

------------------------------------------------------------------------

## 🌐 API Management Integration

- API is added to API Management by referencing the deployed Azure Function App.
- Inbound policy includes CORS configuration.
- `validate-jwt` inbound policy enforced
- API Management becomes the only entry point for clients consuming this authentication service.

------------------------------------------------------------------------

## API Endpoints (Sample)

| Method | Endpoint | Description |
|------|---------|-------------|
| GET | /products | Get all products |
| GET | /products/{id} | Get product by ID |
| POST | /products | Create new product |
| PUT | /products/{id} | Update product |
| DELETE | /products/{id} | Delete product |

---

## 🚢 Deployment

The service is deployed using **GitHub Actions** and Azure services:

- Build & restore
- Apply EF Core migrations (controlled pipeline step)
- Deploy to Azure Function App
- Secured and exposed via Azure API Management

> API Management instances are recreated on demand for cost optimization in non-production environments.

---

## 📌 Notes

- Business functionality is intentionally minimal
- Focus is on architecture, cloud integration, and scalability
- Designed to be consumed by any frontend or service

---
