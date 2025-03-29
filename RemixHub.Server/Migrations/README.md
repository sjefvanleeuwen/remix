# Entity Framework Core Migrations

This document explains how to use EF Core's built-in migration commands directly to manage your database.

## Prerequisites

First, install the Entity Framework Core tools:

```bash
dotnet tool install --global dotnet-ef
```

Or update if you already have it:

```bash
dotnet tool update --global dotnet-ef
```

## Creating and Applying Migrations

### Initial Setup

From the `RemixHub.Server` directory, run:

```bash
# Create the initial migration
dotnet ef migrations add InitialCreate

# Apply the migration to create the database
dotnet ef database update
```

This will create all necessary tables:
- Identity tables (Users, Roles, etc.)
- Tracks, Stems, and Remixes
- Genres and InstrumentTypes
- All relationships between tables

### When You Change Your Models

After modifying your entity models:

```bash
# Create a new migration with a descriptive name
dotnet ef migrations add AddNewFeatureToTrack

# Apply the migration
dotnet ef database update
```

## Useful EF Core Commands

```bash
# List all migrations and their status
dotnet ef migrations list

# Generate SQL script instead of applying directly
dotnet ef migrations script -o migrate.sql

# Remove the last migration (if not applied)
dotnet ef migrations remove

# Drop the database and start fresh
dotnet ef database drop --force
dotnet ef database update
```

## No Scripts Needed

The beauty of EF Core is that you don't need scripts to manage your database - just use these commands directly in your terminal.
