# Bulk Insert performance testing using .NET 8 web API with EF Core

This project is a .NET 8 web API that demonstrates the use of Entity Framework Core with a Weather Forecast example & Bulk Insert performance testing.

## Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB is used in this example)

## .Net 8 Web API for Testing

1. **Install SQL Server Developer / Visual Studio:**

   - Download: [SQL Server Developer](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) | [Visual Studio](https://visualstudio.microsoft.com/downloads/)

1. **Create a controller-based web API with ASP.NET Core:**

   - [Tutorial](https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-web-api)

1. **Install the required NuGet packages:**

   The necessary packages are already included in the `.csproj` file:
   - Microsoft.EntityFrameworkCore
   - Microsoft.EntityFrameworkCore.SqlServer
   - Microsoft.EntityFrameworkCore.Tools

   ```cmd
   dotnet add package Microsoft.EntityFrameworkCore
   dotnet add package Microsoft.EntityFrameworkCore.SqlServer
   dotnet add package Microsoft.EntityFrameworkCore.Tools
   ```

1. **Configure the database connection:**

   The connection string is configured in `appsettings.json`:

   ```json
   {
   "ConnectionStrings": {
      "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=d-report-lab;Trusted_Connection=True;MultipleActiveResultSets=true"
   },
   "Logging": {
      "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
      }
   },
   "AllowedHosts": "*"
   }
   ```

1. **Add the DbContext to the service container:**

   In `Program.cs`, add the following code to register the `ApplicationDbContext`:

   ```csharp
   // Add this near the top
   using Microsoft.EntityFrameworkCore;
   using d_report_Lab.Data;

   var builder = WebApplication.CreateBuilder(args);

   // Add DbContext
   builder.Services.AddDbContext<ApplicationDbContext>(options =>
      options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

   // Existing services
   builder.Services.AddControllers();
   // ...
   ```

1. **Create the initial migration and update the database:**

   Run the following commands in the Package Manager Console or .NET CLI:

   -  Package Manager Console

   ```cmd
   Add-Migration InitialCreate
   Update-Database
   ```

   - Console Output when executing `Update-Database` command. This command will create initial data defined in the migrations.

      <details>
      <summary>Expand</summary>

      ```cmd
      PM> Update-Database
      Build started...
      Build succeeded.
      Microsoft.EntityFrameworkCore.Database.Command[20101]
            Executed DbCommand (277ms) [Parameters=[], CommandType='Text', CommandTimeout='60']
            CREATE DATABASE [d-report-lab];
      Microsoft.EntityFrameworkCore.Database.Command[20101]
            Executed DbCommand (126ms) [Parameters=[], CommandType='Text', CommandTimeout='60']
            IF SERVERPROPERTY('EngineEdition') <> 5
            BEGIN
               ALTER DATABASE [d-report-lab] SET READ_COMMITTED_SNAPSHOT ON;
            END;
      Microsoft.EntityFrameworkCore.Database.Command[20101]
            Executed DbCommand (22ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
            SELECT 1
      Microsoft.EntityFrameworkCore.Migrations[20411]
            Acquiring an exclusive lock for migration application. See https://aka.ms/efcore-docs-migrations-lock for more information if this takes too long.
      Acquiring an exclusive lock for migration application. See https://aka.ms/efcore-docs-migrations-lock for more information if this takes too long.
      Microsoft.EntityFrameworkCore.Database.Command[20101]
            Executed DbCommand (28ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
            DECLARE @result int;
            EXEC @result = sp_getapplock @Resource = '__EFMigrationsLock', @LockOwner = 'Session', @LockMode = 'Exclusive';
            SELECT @result
      Microsoft.EntityFrameworkCore.Database.Command[20101]
            Executed DbCommand (15ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
            IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
            BEGIN
               CREATE TABLE [__EFMigrationsHistory] (
                  [MigrationId] nvarchar(150) NOT NULL,
                  [ProductVersion] nvarchar(32) NOT NULL,
                  CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
               );
            END;
      Microsoft.EntityFrameworkCore.Database.Command[20101]
            Executed DbCommand (1ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
            SELECT 1
      Microsoft.EntityFrameworkCore.Database.Command[20101]
            Executed DbCommand (1ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
            SELECT OBJECT_ID(N'[__EFMigrationsHistory]');
      Microsoft.EntityFrameworkCore.Database.Command[20101]
            Executed DbCommand (4ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
            SELECT [MigrationId], [ProductVersion]
            FROM [__EFMigrationsHistory]
            ORDER BY [MigrationId];
      Microsoft.EntityFrameworkCore.Migrations[20402]
            Applying migration '20250328021118_InitialCreate'.
      Applying migration '20250328021118_InitialCreate'.
      Microsoft.EntityFrameworkCore.Database.Command[20101]
            Executed DbCommand (2ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
            CREATE TABLE [WeatherForecasts] (
               [Id] int NOT NULL IDENTITY,
               [Date] date NOT NULL,
               [TemperatureC] int NOT NULL,
               [Summary] nvarchar(100) NULL,
               CONSTRAINT [PK_WeatherForecasts] PRIMARY KEY ([Id])
            );
      Microsoft.EntityFrameworkCore.Database.Command[20101]
            Executed DbCommand (67ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
            IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Date', N'Summary', N'TemperatureC') AND [object_id] = OBJECT_ID(N'[WeatherForecasts]'))
               SET IDENTITY_INSERT [WeatherForecasts] ON;
            INSERT INTO [WeatherForecasts] ([Id], [Date], [Summary], [TemperatureC])
            VALUES (1, '2025-03-28', N'Warm', 25);
            IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Date', N'Summary', N'TemperatureC') AND [object_id] = OBJECT_ID(N'[WeatherForecasts]'))
               SET IDENTITY_INSERT [WeatherForecasts] OFF;
      Microsoft.EntityFrameworkCore.Database.Command[20101]
            Executed DbCommand (3ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
            INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
            VALUES (N'20250328021118_InitialCreate', N'9.0.3');
      Microsoft.EntityFrameworkCore.Database.Command[20101]
            Executed DbCommand (6ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
            DECLARE @result int;
            EXEC @result = sp_releaseapplock @Resource = '__EFMigrationsLock', @LockOwner = 'Session';
            SELECT @result
      Done.
      ```
      </details>

   - .NET CLI:

   ```cmd
   dotnet ef migrations add InitialCreate 
   dotnet ef database update
   ```

   This will create the database and apply the initial schema.

## Usage

The `WeatherForecastController` provides CRUD operations for the `WeatherForecast` model.

- **Get all weather forecasts:**

   `GET /WeatherForecast`

- **Get a weather forecast by ID:**

   `GET /WeatherForecast/{id}`

- **Create a new weather forecast:**

   `POST /WeatherForecast`

   Example request body:

   { "date": "2025-03-28", "temperatureC": 25, "summary": "Warm" }
  
- **Update an existing weather forecast:**

   `PUT /WeatherForecast/{id}`

   Example request body:

   { "id": 1, "date": "2025-03-28", "temperatureC": 30, "summary": "Hot" }
 
- **Delete a weather forecast:**

   `DELETE /WeatherForecast/{id}`

## Bulk Insert performance testing

This section explains how to perform and test different bulk insert methods with Entity Framework Core using WeatherForecastTest model.

- [Fast SQL Bulk Inserts With C# and EF Core](https://www.milanjovanovic.tech/blog/fast-sql-bulk-inserts-with-csharp-and-ef-core)
- [How to Perform Bulk Insert with EF Core](https://dev.to/iamcymentho/how-to-perform-bulk-insert-with-ef-core-c44)

### Setup Process

**1. Generate Test Tables**

First, we need to create a dedicated test table in the database using the Package Manager Console or .NET CLI:

```cmd
dotnet ef migrations add AddWeatherForecastTestTable
dotnet ef database update
```

```cmd
Add-Migration AddWeatherForecastTestTable
Update-Database
```

**2. Running Bulk Insert Tests**

The BulkInsertTests class provides three different methods for bulk inserts:

1.	AddRange: Standard EF Core AddRange method
2.	ExecuteSqlRaw: Raw SQL with parameterized queries in batches
3.	SqlBulkCopy: Using SqlBulkCopy for maximum performance

PerfTestRunner handles the execution of the test code.

```cmd
cmd> dotnet run --project PerfTest / PerfTestRunner.cs -- 10000
```

```cmd
cmd> dotnet run --project PerfTest / PerfTestRunner.cs
```

<!-- Option 2. Execute the Main method by temporarily updating the project file.

```xml
< PropertyGroup >
      < OutputType > Exe </ OutputType >
      < TargetFramework > net8.0 </ TargetFramework >
      < StartupObject > d_report_Lab.PerfTest.PerfTestRunner </ StartupObject >
</ PropertyGroup >
``` -->

**3. Test Data**

The tests generate 10,000 random weather forecast records for testing. You can adjust this number in the code to test with different data volumes:

   ```csharp
   // Create test data - adjust the number as needed
   var weatherForecasts = GenerateTestData(10000);
   ```

#### Performance Comparison
The test results will be displayed in the test output, showing the execution time for each method:

   - EF Core AddRange: Standard EF Core bulk insert
   - EF Core ExecuteSqlRaw: Using parameterized SQL queries in batches
   - SqlBulkCopy: Using SqlBulkCopy for maximum performance

#### Implementation Details

1.	TestEntityFrameworkCoreAddRange:
   - Uses EF Core's AddRange method to add entities
   - Calls SaveChanges once to commit all changes

2.	TestEntityFrameworkCoreExecuteSqlRaw:
   - Uses raw SQL INSERT statements
   - Processes records in batches of 1,000
   - Uses parameterized queries to prevent SQL injection

3.	TestSqlBulkCopy:
   - Creates a DataTable with the same structure as the entity
   - Uses SqlBulkCopy with a batch size of 5,000

  
   