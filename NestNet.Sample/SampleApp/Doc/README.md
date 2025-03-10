# NestNet Documentation

NestNet is a powerful tool for creating well-designed ASP.NET Core microservices with standardized architecture and best practices.

## Table of Contents
- [Prerequisites](#prerequisites)
- [CLI Installation](#cli-installation)
- [CLI Usage](#cli-usage)
  - [Interactive Mode](#interactive-mode)
  - [Non-interactive Mode](#non-interactive-mode)
- [Project Structure](#project-structure)
- [Database Configuration](#database-configuration)
- [Features](#features)
  - [CRUD Support](#crud-support)
  - [DTO Support](#dto-support)
  - [Pagination](#pagination)
  - [Dependency Injection](#dependency-injection)
  - [Database Support](#database-support)
  - [Testing](#testing)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)

## Prerequisites

- Windows 10 or later
- .NET 7.0 or later
- .NET IDE (Visual Studio, Rider, etc.)
- If you are going to use MSSQL in your microservice(s):
    * SQL Server Developer Edition
    * SQL Server Management Studio (SSMS)
- If you are going to use POSTGRES in your microservice(s):
    * PostgreSQL
    * PgAdmin

## CLI Installation

1. Download the latest release of [NestNet.Cli](https://github.com/choresh/nestnet/blob/main/NestNet.Cli/NestNet.Cli.Installer/Debug/NestNet.Cli.Installer.msi)
2. Run the installer and select the 'Everyone' option when prompted
   > ⚠️ Note: The 'Just Me' option is currently not working due to a known path manipulation issue

## CLI Usage

### Interactive Mode

Launch the CLI in interactive mode:
```bash
nestnet
```

This opens a menu with the following options:

#### 1. Generate App
Creates a new microservice application scaffold.

**Steps:**
1. Create a root folder with a descriptive name (e.g., `OrderProcessor`, `PaymentService`)
2. Open terminal in this folder
3. Run `nestnet` and select `Generate App`
4. Open the generated solution in your IDE
5. Download and add existing project [NestNet.Infra](https://github.com/choresh/nestnet/tree/main/NestNet.Infra) to the solution.
6. Add reference to `NestNet.Infra` in your new project
7. Run the project to verify the Swagger page loads

#### 2. Generate Module
Creates a full-stack module with database integration.

**Generated components:**
- Entity class
- Data Access Object (DAO)
- Data Transfer Objects (DTOs)
- CRUD Service
- CRUD Controller
- Test structure

#### 3. Generate Resource
Creates a high-level feature manager without database integration.

**Generated components:**
- Service class
- Controller
- Test structure

#### 4. Generate DTOs
Force regeneration of DTOs based on entity classes.
> Note: This runs automatically during build. Manual execution rarely needed.

### Non-interactive Mode

View all available commands:
```bash
nestnet -?
```

Common commands:
```bash
nestnet app
nestnet module
nestnet resource
nestnet dtos
```

## Project Structure

```
Project/
├── Data/
├── Doc/│
├── Modules/               # Database-bound features
│   └── {ModuleName}/
│       ├── Controllers/
│       ├── Daos/
│       ├── Dtos/
│       ├── Entities/
│       ├── Services/
│       └── Tests/
├── Resources/            # Non-database features
│   └── {ResourceName}/
│       ├── Controllers/
│       ├── Dtos/
│       ├── Services/
│       └── Tests/
├── appsettings.json
└── Program.cs
```

## Database Configuration
- Note: to configure the required environment variables (see bellow) - you can update and use PowerShell scripts located [here](https://github.com/choresh/nestnet/tree/main/Scripts).

### If you going to use MSSQL in your microservice(s):

1. Set up SQL Server login with admin role:
   - Enable "SQL Server and Windows Authentication mode"
   - Create SQL Server authentication user with sysadmin role

2. Configure environment variables:
   - MSSQL_SERVER
   - MSSQL_DB_NAME
   - MSSQL_USER
   - MSSQL_PASSWORD
   - MSSQL_TRUST_SERVER_CERTIFICATE
   - MSSQL_TRUSTED_CONNECTION
   - MSSQL_MULTIPLE_ACTIVE_RESULT_SETS

### If you going to use POSTGRES in your microservice(s):

1. Set up PostgresSQL login with admin role:
   - During installation, set a password for the default 'postgres' superuser

2. Configure environment variables:
   - POSTGRES_SERVER
   - POSTGRES_DB_NAME
   - POSTGRES_USER
   - POSTGRES_PASSWORD
   
## Features

### CRUD Support
All module endpoints automatically support standard CRUD operations:

| Operation | Method | Endpoint | Description |
|-----------|--------|----------|-------------|
| GetAll | GET | /api/{resource} | Retrieves all entities |
| GetById | GET | /api/{resource}/{id} | Retrieves an entity by ID |
| Create | POST | /api/{resource} | Creates a new entity |
| Update | PUT | /api/{resource}/{id} | Updates an existing entity |
| Delete | DELETE | /api/{resource}/{id} | Deletes an entity by ID |
| GetPaginated | GET | /api/{resource}/paginated | Retrieves paginated results |
| GetMany | GET | /api/{resource}/many | Retrieves many entities |
| GetMeta | GET | /api/{resource}/meta | Retrieves many entities metadata |


### DTO Support
- Data Transfer Objects (DTOs) are automatically generated to ensure type safety and clear API contracts.
- DTOs are automatically generated based on entity properties marked with the `Prop` attribute.
- Generation occurs during build time to ensure DTOs stay synchronized with entity changes.

#### DTO Types
Each entity in your modules automatically receives 4 DTO classes:
- `CreateDto`: Contains all properties required for entity creation
- `UpdateDto`: Contains all modifiable properties for entity updates
- `ResultDto`: Contains all properties of entity, as returned by controller/service operations
- `QueryDto`: Contains all properties of entity, as may passed to controller/service queries

#### DTOs Generation Example
Sample source entity:
```csharp
public class User
{
   [Prop(create: GenOpt.Ignore, update: GenOpt.Ignore, result: GenOpt.Mandatory)] // To be generated by DB, hince 'ignore' at 'create'
   public int MyModuleId { get; set; }
   
   [Prop(create: GenOpt.Mandatory, update: GenOpt.Ignore, result: GenOpt.Mandatory)] // Mandatory at create/result
   public string Username { get; set; }

   [Prop(create: GenOpt.Optional, update: GenOpt.Optional, result: GenOpt.Optional)] // Optional (nullable) at all cases
   public string? Email { get; set; } // The nulable operator - has meanning just in the DB context

   [Prop(create: GenOpt.Mandatory, update: GenOpt.Ignore, result: GenOpt.Ignore)] // Excluded from ResultDto
   public string Password { get; set; }

   [Prop(create: GenOpt.Ignore, update: GenOpt.Ignore, result: GenOpt.Mandatory)] // Generated by DB
   public DateTime CreatedAt { get; set; }
}
```

The generate DTOs
```csharp
public class UserCreateDto
{
    public string Username { get; set; } // Mandatory
    public string? Email { get; set; } // Optional
}

public class UserUpdateDto
{
    public string? Email { get; set; } // Optional
}

public class UserResultDto
{
    public int UserId { get; set; } // Mandatory
    public string Username { get; set; } // Mandatory
    public string? Email { get; set; } // Optional
    public DateTime CreatedAt { get; set; } // Mandatory
}

public class UserQueryDto
{
    public int? UserId { get; set; } // Optional
    public string? Username { get; set; } // Optional
    public string? Email { get; set; } // Optional
    public DateTime? CreatedAt { get; set; } // Optional
}
```
Note: all properties in 'QueryDto' are based on properties of 'ResultDto', but they will all be generated as optional (this kind of generation is NOT controllable via the 'Prop' attribute, it is done under the hood).

### Pagination
All module endpoints support standardized pagination through the `GetPaginated()` method.

#### Pagination Parameters
```http
GET /api/users/paginated?pageNumber=1&pageSize=10&sortBy=lastName&sortDirection=desc&filterBy=email&filterOperator=Contains&filterValue=@company.com
```

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| pageNumber | integer | 1 | The page number to retrieve |
| pageSize | integer | 20 | Number of items in each page |
| sortBy | string | Id | Property name to sort by |
| sortDirection | string | asc | Sort direction (asc or desc) |
| filterBy | string | - | Property name to filter on |
| filterOperator | string | - | Comparison operator |
| filterValue | string | - | Value to compare against |

Available Filter Operators:
- `Equals`: Exact match
- `NotEquals`: Inverse match
- `GreaterThan`: Greater than comparison
- `LessThan`: Less than comparison
- `Contains`: Substring match
- `NotContains`: Inverse substring match
- `GreaterThanOrEqual`: Greater than or equal comparison // TODO
- `LessThanOrEqual`: Less than or equal comparison // TODO
- `StartsWith`: Prefix match // TODO
- `EndsWith`: Suffix match // TODO
- `IsNull`: Null check // TODO
- `IsNotNull`: Non-null check // TODO

Example Response:
```json
{
    "items": [...],
    "totalItems": 100,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 10,
    "hasNextPage": true,
    "hasPreviousPage": false
}
```

### Dependency Injection
The application includes built-in support for Dependency Injection:
- Use the `Injectable` attribute to mark services for automatic registration
- Services are automatically registered in `Program.cs` using `DependencyInjectionHelper.RegisterInjectables()`
- Supports Scoped, Singleton, and Transient lifetimes

Example:
```csharp
[Injectable(LifetimeType.Scoped)]
public class UserService : IUserService
{
    // Service implementation
}
```

### Database Support
Entity Framework Core integration (with Microsoft SQL Server or with PosgresSQL) is pre-configured:
- Use the `Entity` attribute to mark classes for database mapping
- Customize column properties using the `Prop` attribute's `store` parameter

Example:
```csharp
[Entity]
public class User
{
    [Prop(store: DbOpt.PrimaryKey)] // Generated by DB
    public int UserId { get; set; }

    [Prop(store: DbOpt.Standard)] 
    public string Username { get; set; } // Required

    [Prop(store: DbOpt.Standard)] 
    public string? Email { get; set; } // Optional (e.i. nullable)
}
```

### Testing
The project includes comprehensive test support.

### Unit Tests
- Automatic test generation for all public methods
- Tests cover Controllers, Services, and DAOs
- DAO tests use in-memory database
- Uses xUnit as the testing framework

Example test location:

```
Modules/
└── Users/
    └── Tests/
        ├── Controllers/
        │   └── UserControllerTests.cs
        ├── Services/
        │   └── UserServiceTests.cs
        └── Daos/
            └── UserDaoTests.cs
```

## Best Practices

1. Use descriptive, noun-based names for your microservices
2. Generate modules for database-bound features
3. Generate resources for orchestration or complex business logic
4. DTOs are automatically generated - modify the entity class to change DTO structure
5. Always run the application after generation to verify the setup

## Troubleshooting

### Common Issues

1. **Installation Issues**
   - Ensure you selected 'Everyone' during installation
   - Verify `NestNet.Infra` is properly referenced in your solution

2. **Database Connection**
   - Verify environment variables are set correctly
   - Ensure SQL Server is running
   - Check Windows Authentication settings

3. **Build Errors**
   - Clean solution and rebuild
   - Verify all NuGet packages are restored
   - Check for syntax errors in entity attributes

4. **DTO Generation**
   - Ensure entities are properly marked with attributes
   - Rebuild solution to trigger DTO generation
   - Check build output for generation errors

### Getting Help
- Check the [GitHub Issues](https://github.com/choresh/nestnet/issues)
- Join our [Discord Community (TODO)](https://discord.gg/yourdiscord)
- Review the [FAQ (TODO)](https://your-docs-url.com/faq)