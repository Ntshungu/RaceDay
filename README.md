# RaceDay — PROG6212 POE Part 2

RaceDay is an ASP.NET Core Web API for managing South African road-running,
walking and cycling events. This repository contains the Part 2 API and its
automated tests.

## Roles

- **Organiser:** creates and manages owned events and categories, views enrolments,
  and captures race results.
- **Participant:** browses events, enrols in an event, manages their profile,
  and views their own results.

## Technology

- C# and ASP.NET Core Web API (.NET 8)
- Entity Framework Core 9
- SQL Server
- Session-based authentication with BCrypt password hashing
- Swagger / OpenAPI
- xUnit and EF Core InMemory for unit tests
- GitHub Actions for continuous integration

## Project structure

- `RaceDay.API/` — API controllers, models, DTOs, database context and migrations.
- `RaceDay.Tests/` — automated unit tests.
- `.github/workflows/dotnet.yml` — build and test workflow.

## Requirements

Install Visual Studio 2022 with the ASP.NET and web development workload,
the .NET 8 SDK, and SQL Server / SQL Server LocalDB.

## Database configuration

The default development connection string is in `RaceDay.API/appsettings.json`.
Change it to match the SQL Server instance installed on your machine. Do not
commit production passwords or secrets.

From the repository root, open a terminal and run:

```bash
dotnet restore RaceDay.sln
dotnet build RaceDay.sln
```

Apply the existing Entity Framework migration using the Package Manager Console
in Visual Studio, with `RaceDay.API` as the Default project:

```powershell
Update-Database
```

If this is a new database and the migration has not yet been applied, this creates
the schema. Do not run the migration against a database that already contains a
different manually-created schema without first reconciling the schemas.

## Run the API

In Visual Studio, set `RaceDay.API` as the startup project and run it. In the
Development environment, Swagger opens at the URL shown in `RaceDay.API/Properties/launchSettings.json`,
for example `https://localhost:7145/swagger`.

## Run tests

From the repository root:

```bash
dotnet test RaceDay.Tests/RaceDay.Tests.csproj
```

## CI

GitHub Actions builds the solution and runs the test project on pushes and pull
requests to `main` or `master`. Add a screenshot of a successful run here after
the workflow passes in your GitHub repository.

## Video presentation

Add your own unlisted YouTube link here after recording the Part 2 demonstration.

## AI-use disclosure

Disclose AI assistance in accordance with the POE instructions and explain the
changes you reviewed, tested and understood before submission.
