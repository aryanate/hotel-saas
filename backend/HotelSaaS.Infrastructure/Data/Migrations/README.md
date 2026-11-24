# Entity Framework Core Migrations

This folder contains database migrations for the Hotel Management SaaS application using Entity Framework Core.

## Running Migrations

To add a new migration, run the following command from the solution root:

```bash
dotnet ef migrations add InitialCreate --project backend/HotelSaaS.Infrastructure --startup-project backend/HotelSaaS.API
```

To apply migrations to the database:

```bash
dotnet ef database update --project backend/HotelSaaS.Infrastructure --startup-project backend/HotelSaaS.API
```

## Prerequisites

Ensure you have the `dotnet-ef` global tool installed:

```bash
dotnet tool install --global dotnet-ef
```
