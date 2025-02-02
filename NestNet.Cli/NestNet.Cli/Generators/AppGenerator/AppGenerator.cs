using System.Diagnostics;
using System.Xml.Linq;
using Spectre.Console;
using NestNet.Cli.Infra;
using System.Reflection;
using DbType = NestNet.Infra.Enums.DbType;

namespace NestNet.Cli.Generators.AppGenerator
{
    internal static class AppGenerator
    {
        public class InputParams
        {
            public required IEnumerable<AppType> AppTypes { get; set; }
        }

        private class AppGenerationContext
        {
            public required string CurrentDir { get; set; }
            public required string BaseProjectName { get; set; }
            public required string CurrProjectName { get; set; }
            public required DbType DbType { get; set; }
            public required string AppPath { get; set; }
        }

        public static void Run(InputParams? inputParams = null)
        {
            try
            {
                AnsiConsole.MarkupLine(Helpers.FormatMessage("\nApp generation - started", "green"));

                var context = CreateAppGenerationContext(inputParams);
                if (context == null)
                {
                    AnsiConsole.MarkupLine(Helpers.FormatMessage("\nApp generation - ended, unable to generate the app", "green"));
                    return;
                }

                Directory.CreateDirectory(context.AppPath);
                GenerateProjectFile(context);
                GenerateProgramFile(context);
                GenerateAppConfigFiles(context);

                AnsiConsole.MarkupLine(Helpers.FormatMessage("\nApp generation - ended successfully", "green"));
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine(Helpers.FormatMessage($"\nApp generation - failed ({ex.Message})", "red"));
            }
        }

        private static AppGenerationContext? CreateAppGenerationContext(InputParams? inputParams = null)
        {
            var currentDir = Directory.GetCurrentDirectory();
            var rootFolderName = new DirectoryInfo(currentDir).Name;
            var baseProjectName = rootFolderName;
            var currProjectName = $"{baseProjectName}.Api";
            var appPath = Path.Combine(currentDir, currProjectName);

            if (!Helpers.CheckTarDir(appPath))
            {
                return null;
            }

            // No need to ask for DbType (as it should be determined from Core project).
            var dbType = DetermineDbTypeFromCore(currentDir, baseProjectName);

            return new AppGenerationContext
            {
                CurrentDir = currentDir,
                BaseProjectName = baseProjectName,
                CurrProjectName = currProjectName,
                DbType = dbType,
                AppPath = appPath
            };
        }

        private static DbType DetermineDbTypeFromCore(string currentDir, string baseProjectName)
        {
            var coreCsprojPath = Path.Combine(currentDir, $"{baseProjectName}.Core", $"{baseProjectName}.Core.csproj");
            if (!File.Exists(coreCsprojPath))
            {
                throw new Exception("Core project not found in current directory");
            }

            var csprojContent = File.ReadAllText(coreCsprojPath);

            DbType dbType;
            if (csprojContent.Contains("Npgsql.EntityFrameworkCore.PostgreSQL"))
            {
                dbType = DbType.Postgres;
            }
            else if (csprojContent.Contains("Microsoft.EntityFrameworkCore.SqlServer"))
            {
                dbType = DbType.Postgres;
            }
            else
            {
                throw new Exception("Failed to detect Db Type at core project");
            }

            return dbType;
        }

        // ZZZ
        private static void AddTestPackages(AppGenerationContext context)
        {
            AnsiConsole.MarkupLine(Helpers.FormatMessage("\nTest packages will be added to the project", "green"));

            string[] packages = {
                "NSubstitute",
                "Xunit",
                "Microsoft.TestPlatform.TestHost",
                "xunit.runner.visualstudio",
                "AutoFixture",
                "AutoFixture.AutoNSubstitute"
            };

            foreach (var package in packages)
            {
                RunDotNetCommand(context.CurrentDir, $"add package {package}");
            }

            AnsiConsole.MarkupLine(Helpers.FormatMessage("Test packages has been added to the project", "green"));
        }

        // ZZZ!
        private static void AddDtoGenerator(AppGenerationContext context)
        {
            AnsiConsole.MarkupLine(Helpers.FormatMessage("\nPre-build event that runs the Dtos generator will be added to the project...", "green"));

            var csprojPath = Path.Combine(context.CurrentDir, $"{context.BaseProjectName}.csproj");
            var doc = XDocument.Load(csprojPath);
            if (doc.Root == null)
            {
                AnsiConsole.MarkupLine(Helpers.FormatMessage("\nPre-build event that runs the Dtos generator cannot be added to the project...", "red"));
                return;
            }
            var ns = doc.Root.Name.Namespace;

            var propertyGroup = doc.Root.Elements(ns + "PropertyGroup").First();
            propertyGroup.Add(new XElement(ns + "GenerateDocumentationFile", "true"));

            var target = new XElement(ns + "Target",
               new XAttribute("Name", "PostBuild"),
               new XAttribute("AfterTargets", "PostBuildEvent"),
               new XElement(ns + "Exec",
                   new XAttribute("Command", "nestnet.exe dtos --tar-dir \\Dtos --no-console")
               )
           );

            doc.Root.Add(target);
            doc.Save(csprojPath);

            AnsiConsole.MarkupLine(Helpers.FormatMessage($"Updated: {csprojPath}", "grey"));

            AnsiConsole.MarkupLine(Helpers.FormatMessage("Pre-build event that runs the Dtos generator has been added to the project", "green"));
        }

        private static void RunDotNetCommand(string path, string command)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c dotnet {command}",
                WorkingDirectory = path,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

            process?.WaitForExit();
        }

        // ZZZ
        private static void CopyDocumentation(string projectRoot)
        {
            AnsiConsole.MarkupLine(Helpers.FormatMessage("\nCopy documentation started...", "green"));

            // Create Doc directory
            string docDir = Path.Combine(projectRoot, "Doc");
            Directory.CreateDirectory(docDir);

            // Copy documentation files
            CopyEmbeddedResource("README.md", Path.Combine(docDir, "README.md"));

            AnsiConsole.MarkupLine(Helpers.FormatMessage("Copy documentation ended", "green"));
        }

        private static void CopyEmbeddedResource(string resourceName, string targetPath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourcePath = $"NestNet.Cli.Data.Templates.Doc.{resourceName}";
            try
            {
                using (var stream = assembly.GetManifestResourceStream(resourcePath))
                {
                    if (stream == null)
                    {
                        throw new InvalidOperationException($"Could not find embedded resource: {resourcePath}");
                    }
                    using (var reader = new StreamReader(stream))
                    {
                        string content = reader.ReadToEnd();
                        File.WriteAllText(targetPath, content);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to copy resource {resourceName} to {targetPath}", ex);
            }
        }

        private static void GenerateProjectFile(AppGenerationContext context)
        {
            var csprojContent = $@"<Project Sdk=""Microsoft.NET.Sdk.Web"">

    <PropertyGroup>
        <TargetFramework>net8.0</TargetFramework>
        <Nullable>enable</Nullable>
        <ImplicitUsings>enable</ImplicitUsings>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include=""Microsoft.EntityFrameworkCore.Design"" Version=""*"">
            <PrivateAssets>all</PrivateAssets>
            <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
        </PackageReference>
        <PackageReference Include=""Microsoft.EntityFrameworkCore.Tools"" Version=""*"">
            <PrivateAssets>all</PrivateAssets>
            <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
        </PackageReference>
        <PackageReference Include=""Swashbuckle.AspNetCore"" Version=""*"" />
    </ItemGroup>

    <ItemGroup>
        <ProjectReference Include=""..\{context.BaseProjectName}.Core\{context.BaseProjectName}.Core.csproj"" />
    </ItemGroup>

</Project>";

            File.WriteAllText(Path.Combine(context.AppPath, $"{context.CurrProjectName}.csproj"), csprojContent);
        }

        private static void GenerateProgramFile(AppGenerationContext context)
        {
            var programContent = $@"using {context.BaseProjectName}.Core.Data;
using Microsoft.EntityFrameworkCore;
using NestNet.Infra.Helpers;
using NestNet.Infra.Swagger;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add MVC controllers to the service container.
builder.Services.AddControllers();

// Add API explorer to enable API endpoint discovery and documentation
// This is required for Swagger/OpenAPI to discover API endpoints
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger/OpenAPI documentation.
builder.Services.AddSwaggerGen(c =>
{{
    // Enables Swagger annotations (SwaggerOperationAttribute, SwaggerParameterAttribute etc.).
    c.EnableAnnotations();

    // Add descriptions for enum values in Swagger documentation.
    c.SchemaFilter<EnumSchemaFilter>();

    // Add descriptions for query parameters from QueryDto classes.
    c.SchemaFilter<QueryDtoSchemaFilter>();
}});

// Helper function to construct DB connection string from environment variables
{GetConnectionStringMethod(context.DbType)}

var connectionString = CreateConnectionString(args);

// Configure Entity Framework with database context.
builder.Services.AddDbContext<AppDbContext>(options =>
{{
    {GetDbContextOptionsCode(context.DbType, "connectionString")};
}});

// Configure dependency injection for classes with [Injectable] attribute.
// This scans and register all injectable classes from both the API and Core assemblies.
DependencyInjectionHelper.RegisterInjetables(builder.Services, [
    Assembly.GetExecutingAssembly(),
    Assembly.Load(""{context.BaseProjectName}.Core"")
]);

// Initialize the application
var app = builder.Build();

// Enable Swagger UI only in development environment.
if (app.Environment.IsDevelopment())
{{
    app.UseSwagger();
    app.UseSwaggerUI();
}}

// Configure middleware pipeline
app.UseHttpsRedirection();  // Redirect HTTP requests to HTTPS.
app.UseAuthorization();     // Enable authorization.
app.MapControllers();       // Register controller endpoints.

// Initialize / update the DB (accurding code of our entities).
using (var scope = app.Services.CreateScope())
{{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // dbContext.Database.Migrate();  // This will create the database and apply migration
    dbContext.Database.EnsureCreated();  // Simpler, doesn't support migrations
}}

app.Run();";

            File.WriteAllText(Path.Combine(context.AppPath, "Program.cs"), programContent);
        }

        private static string GetConnectionStringMethod(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.MsSql:
                    return GetMsSqlConnectionStringMethod();
                case DbType.Postgres:
                    return GetPostgresConnectionStringMethod();
                default:
                    throw new ArgumentException($"DB type '{dbType}' not supported");
            }
        }

        private static string GetMsSqlConnectionStringMethod()
        {
            return @"static string CreateConnectionString(string[] args)
{
    var server = ConfigHelper.GetConfigParam(args, ""MSSQL_SERVER"");
    var dbName = ConfigHelper.GetConfigParam(args, ""MSSQL_DB_NAME"");
    var user = ConfigHelper.GetConfigParam(args, ""MSSQL_USER"");
    var password = ConfigHelper.GetConfigParam(args, ""MSSQL_PASSWORD"");
    var trustServerCertificate = ConfigHelper.GetConfigParam(args, ""MSSQL_TRUST_SERVER_CERTIFICATE"", ""false"");
    var trustedConnection = ConfigHelper.GetConfigParam(args, ""MSSQL_TRUSTED_CONNECTION"", ""false"");
    var multipleActiveResultSets = ConfigHelper.GetConfigParam(args, ""MSSQL_MULTIPLE_ACTIVE_RESULT_SETS"", ""false"");

    return $""Server={server}; Database={dbName}; User Id={user}; Password={password}; TrustServerCertificate={trustServerCertificate}; Trusted_Connection={trustedConnection}; MultipleActiveResultSets={multipleActiveResultSets}"";
}";
        }

        private static string GetPostgresConnectionStringMethod()
        {
            return @"static string CreateConnectionString(string[] args)
{
    var server = ConfigHelper.GetConfigParam(args, ""POSTGRES_SERVER"");
    var dbName = ConfigHelper.GetConfigParam(args, ""POSTGRES_DB_NAME"");
    var user = ConfigHelper.GetConfigParam(args, ""POSTGRES_USER"");
    var password = ConfigHelper.GetConfigParam(args, ""POSTGRES_PASSWORD"");

    return $""Host={server}; Database={dbName}; Username={user}; Password={password}"";
}";
        }

        private static string GetDbContextOptionsCode(DbType dbType, string connectionStringCode)
        {
            switch (dbType)
            {
                case DbType.MsSql:
                    return $"options.UseSqlServer({connectionStringCode})";
                case DbType.Postgres:
                    return $"options.UseNpgsql({connectionStringCode})";
                default:
                    throw new ArgumentException($"DB type '{dbType}' not supported");
            }
        }

        private static void GenerateAppConfigFiles(AppGenerationContext context)
        {
            var appSettingsContent = @"{
  ""Logging"": {
    ""LogLevel"": {
      ""Default"": ""Information"",
      ""Microsoft.AspNetCore"": ""Warning""
    }
  }
}";

            var appSettingsDevelopmentContent = @"{
  ""Logging"": {
    ""LogLevel"": {
      ""Default"": ""Information"",
      ""Microsoft.AspNetCore"": ""Warning""
    }
  }
}";

            var launchSettingsContent = $@"{{
    ""profiles"": {{
        ""{context.CurrProjectName}"": {{
            ""commandName"": ""Project"",
            ""launchBrowser"": true,
            ""launchUrl"": ""swagger"",
            ""applicationUrl"": ""https://localhost:7001;http://localhost:5001"",
            ""environmentVariables"": {{
                ""ASPNETCORE_ENVIRONMENT"": ""Development""
            }}
        }}
    }}
}}";

            File.WriteAllText(Path.Combine(context.AppPath, "appsettings.json"), appSettingsContent);
            File.WriteAllText(Path.Combine(context.AppPath, "appsettings.Development.json"), appSettingsDevelopmentContent);

            var tarDir = Path.Combine(context.AppPath, "Properties");
            Directory.CreateDirectory(tarDir);
            File.WriteAllText(Path.Combine(tarDir, "launchSettings.json"), launchSettingsContent);
        }
    }
}