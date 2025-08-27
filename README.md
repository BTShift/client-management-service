# Client Management Service

Microservice for managing clients within tenant databases in the multi-tenant SaaS accounting platform.

## Overview

This service handles:
- Client CRUD operations (within tenant context)
- Client groups management
- Client user associations
- Client categories and tags
- Department and team assignments

## Architecture

- **Technology**: .NET 8, gRPC
- **Database**: PostgreSQL (tenant-isolated databases)
- **Communication**: gRPC (internal), REST via API Gateway (external)
- **Messaging**: RabbitMQ with MassTransit

## Project Structure

```
src/
├── ClientManagement.Api/           # gRPC service implementation
├── ClientManagement.Contract/      # Proto files and event contracts
├── ClientManagement.Domain/        # Domain entities and value objects
├── ClientManagement.Application/   # Business logic and use cases
└── ClientManagement.Infrastructure/ # Data access and external services

tests/
├── ClientManagement.UnitTests/
└── ClientManagement.IntegrationTests/
```

## Development

### Prerequisites
- .NET 8 SDK
- Docker & Docker Compose
- PostgreSQL (for local development)

### Running Locally

```bash
# Restore dependencies
dotnet restore

# Run the service
dotnet run --project src/ClientManagement.Api

# Run tests
dotnet test
```

### Environment Variables

```bash
# Database Connection (PG* variables)
PGHOST=localhost
PGPORT=5432
PGDATABASE=clientmanagement
PGUSER=postgres
PGPASSWORD=password

# Service Configuration
GRPC_PORT=5003
ASPNETCORE_ENVIRONMENT=Development

# RabbitMQ
RABBITMQ_HOST=localhost
RABBITMQ_PORT=5672
RABBITMQ_USERNAME=guest
RABBITMQ_PASSWORD=guest

# Identity Service
IDENTITY_SERVICE_HOST=localhost
IDENTITY_SERVICE_GRPC_PORT=5001
```

## API Documentation

The service exposes gRPC endpoints for:
- Client management operations
- Client group operations
- Client user associations

See `src/ClientManagement.Contract/Protos/client.proto` for the complete API definition.

## Contract Publishing

The `ClientManagement.Contract` package is published to GitHub Package Registry and contains:
- Proto definitions
- Event contracts
- Shared DTOs

## Deployment

This service is deployed on Railway with automatic deployments from the `master` branch.