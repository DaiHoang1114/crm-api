# MyCRM Backend

A CRM backend application built with .NET 9, PostgreSQL, and Keycloak.

## Prerequisites

- .NET 9 SDK
- Docker Desktop
- PostgreSQL (via Docker)
- Keycloak (via Docker)

## Getting Started

1. Start infrastructure:
   ```bash
   cd infrastructure
   docker compose up -d
   ```

2. Run migrations:
   ```bash
   cd MyCRM.Client
   dotnet ef database update
   ```

3. Run the application:
   ```bash
   dotnet run
   ```

## Database Connection

- Host: localhost
- Port: 5433
- Database: postgres
- Username: postgres
- Password: postgres