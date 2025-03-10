﻿using System.Diagnostics;
using System.Xml.Linq;
using Spectre.Console;
using NestNet.Cli.Infra;
using System.Reflection;
using NestNet.Infra.Enums;
using System.Data;
using AutoMapper.Configuration.Conventions;

namespace NestNet.Cli.Generators
{
    internal static class AppGenerator
    {
        public class InputParams
        {
            public NestNet.Infra.Enums.DbType DbType { get; set; }
        }

        private class AppGenerationContext
        {
            public AppGenerationContext(string currentDir, string projectName, NestNet.Infra.Enums.DbType dbType)
            {
                CurrentDir = currentDir;
                ProjectName = projectName;
                DbType = dbType;
            }
            public string CurrentDir { get; private set; }
            public string ProjectName { get; private set; }
            public NestNet.Infra.Enums.DbType DbType { get; private set; }
        }

        public static void Run(InputParams? inputParams = null)
        {
            AnsiConsole.MarkupLine(Helpers.FormatMessage("App generation - startrd\n", "green"));

            try
            {
                bool goOn = false;
                var context = CreateAppGenerationContext(inputParams);
                if (context != null)
                {
                    goOn = Helpers.CheckTarDir(context.CurrentDir);
                    if (goOn)
                    {
                        goOn = GenerateWebApiProject(context);
                    }
                }
                if (!goOn)
                {
                    AnsiConsole.MarkupLine(Helpers.FormatMessage("\nApp generation - ended, unable to generate the app", "green"));
                    return;
                }
                RemoveFsItems(context!);
                CreateFsItems(context!);
                ModifyWebApiProject(context!);
                AddTestPackages();
                AddDtoGenerator(context!);
                CopyDocumentation(context!.CurrentDir);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine(Helpers.FormatMessage($"\nApp generation - failed ({ex.Message})", "red"));
                return;
            }

            AnsiConsole.MarkupLine(Helpers.FormatMessage("\nApp generation - ended successfully", "green"));
        }

        private static AppGenerationContext? CreateAppGenerationContext(InputParams? inputParams = null)
        {
            NestNet.Infra.Enums.DbType? dbType = null;
            if (inputParams == null)
            {
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[green]NestNet.Cli[/] - Use arrow keys to select DB type and press Enter:")
                        .AddChoices(new[] {
                            "MsSql",
                            "Postgres",
                            "Exit"
                        }));
                switch (choice)
                {
                    case "MsSql":
                        dbType = NestNet.Infra.Enums.DbType.MsSql;
                        break;
                    case "Postgres":
                        dbType = NestNet.Infra.Enums.DbType.Postgres;
                        break;
                    case "Exit":
                        break;
                }
            }
            else
            {
                dbType = inputParams.DbType;
            }

            if (dbType == null)
            {
                return null;
            }

            var currentDir = Directory.GetCurrentDirectory();
            var projectName = Path.GetFileName(currentDir);
            return new AppGenerationContext(currentDir, projectName, dbType.Value);
        }

        private static bool GenerateWebApiProject(AppGenerationContext context)
        {
            AnsiConsole.MarkupLine(Helpers.FormatMessage("\nStandard ASP.NET Core Web API project will be generated...\n", "green"));
            RunDotNetCommand("new webapi -controllers");
            var csprojFile = Directory.GetFiles(context.CurrentDir, "*.csproj").FirstOrDefault();
            if (csprojFile == null)
            {
                AnsiConsole.MarkupLine(Helpers.FormatMessage("Error: No .csproj file found in the current directory.", "red"));
                return false;
            }
            AnsiConsole.MarkupLine(Helpers.FormatMessage("Standard ASP.NET Core Web API project has been generated", "green"));
            return true;
        }

        private static void AddTestPackages()
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
                RunDotNetCommand($"add package {package}");
            }

            AnsiConsole.MarkupLine(Helpers.FormatMessage("Test packages has been added to the project", "green"));
        }

        private static void AddDtoGenerator(AppGenerationContext context)
        {
            AnsiConsole.MarkupLine(Helpers.FormatMessage("\nPre-build event that runs the Dtos generator will be added to the project...", "green"));

            var csprojPath = Path.Combine(context.CurrentDir, $"{context.ProjectName}.csproj");
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

        private static void ModifyWebApiProject(AppGenerationContext context)
        {
            AnsiConsole.MarkupLine(Helpers.FormatMessage("Some capabilities (Entity Framework support, automatic registration of DAOs and Services, enhanced Swagger features) will be added to the project...", "green"));

            var dbRelatedContent = GetDbRelatedContent(context.DbType);

            RunDotNetCommand($"add package {dbRelatedContent.NugetsContent}");
            RunDotNetCommand("add package Microsoft.EntityFrameworkCore.Tools");
            RunDotNetCommand("add package Microsoft.EntityFrameworkCore.InMemory");

            CreateApplicationDbContextFile(context);

            UpdateProgramCs(context, dbRelatedContent.ConfigurationContent);

            AnsiConsole.MarkupLine(Helpers.FormatMessage("Some capabilities (Entity Framework support, automatic registration of DAOs and Services, enhanced Swagger features) has been added to the project...", "green"));
        }

        private static void CreateFsItems(AppGenerationContext context)
        {
            AnsiConsole.MarkupLine(Helpers.FormatMessage("New folders will be created...", "green"));

            var itemsToAdd = new[]
           {
                Path.Combine(context.CurrentDir, "Data")
            };

            foreach (var item in itemsToAdd)
            {
                Directory.CreateDirectory(item);
                AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {item}", "grey"));
            }

            AnsiConsole.MarkupLine(Helpers.FormatMessage("New folders has been created" +
                "\n", "green"));
        }

        private static void RemoveFsItems(AppGenerationContext context)
        {
            AnsiConsole.MarkupLine(Helpers.FormatMessage("\nUnused folders/files will be removed...", "green"));

            var itemsToRemove = new[]
            {
                Path.Combine(context.CurrentDir, "WeatherForecast.cs"),
                Path.Combine(context.CurrentDir, context.ProjectName + ".http"),
                Path.Combine(context.CurrentDir, "Controllers")
            };

            foreach (var item in itemsToRemove)
            {
                if (File.Exists(item))
                {
                    File.Delete(item);
                    AnsiConsole.MarkupLine(Helpers.FormatMessage($"File removed: {item}", "grey"));
                }
                else if (Directory.Exists(item))
                {
                    Directory.Delete(item, true);
                    AnsiConsole.MarkupLine(Helpers.FormatMessage($"Directory removed: {item}", "grey"));
                }
                else
                {
                    throw new Exception($"Failed to remove item '{item}' (not found in file system)");
                }
            }
            AnsiConsole.MarkupLine(Helpers.FormatMessage("Unused folders/files has been removed\n", "green"));
        }

        class DbRelatedContent
        {
            public string ConfigurationContent { get; set; }
            public string NugetsContent { get; set; }

        }

        private static DbRelatedContent GetDbRelatedContent(NestNet.Infra.Enums.DbType dbType)
        {
            string configurationContent;
            string nugetsContent;
            switch (dbType)
            {
                case NestNet.Infra.Enums.DbType.MsSql:
                    configurationContent = GetMsSqlConfigurationContent();
                    nugetsContent = "Microsoft.EntityFrameworkCore.SqlServer";
                    break;
                case NestNet.Infra.Enums.DbType.Postgres:
                    configurationContent = GetPostgresConfigurationContent();
                    nugetsContent = "Npgsql.EntityFrameworkCore.PostgreSQL";
                    break;
                default:
                    throw new ArgumentException($"DB type '{dbType}' not soppurted");
            }
            return new DbRelatedContent()
            {
                ConfigurationContent = configurationContent,
                NugetsContent = nugetsContent
            };
        }

        private static void UpdateProgramCs(AppGenerationContext context, string dbConfigurationContent)
        {
            var programCsPath = Path.Combine(context.CurrentDir, "Program.cs");
            var content = File.ReadAllText(programCsPath);

            content = Replace(content, "var builder = WebApplication.CreateBuilder(args);", GetProgramCsContent1(context.ProjectName, dbConfigurationContent));
     
            content = Replace(content, "builder.Services.AddOpenApi();", GetProgramCsContent2());

            content = Replace(content, "app.MapOpenApi();", GetProgramCsContent3());

            content = Replace(content, "app.Run();", GetProgramCsContent4());

            File.WriteAllText(programCsPath, content);

            AnsiConsole.MarkupLine(Helpers.FormatMessage($"Updated: {programCsPath}", "grey"));
        }

        private static void CreateApplicationDbContextFile(AppGenerationContext context)
        {
            var contextPath = Path.Combine(context.CurrentDir, "Data", "ApplicationDbContext.cs");
            var content = GetApplicationDbContextContent(context.ProjectName);
            File.WriteAllText(contextPath, content);
            AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {contextPath}", "grey"));
        }

        private static void RunDotNetCommand(string command)
        {
            AnsiConsole.Status()
                .Start($"Running: dotnet {command}", ctx =>
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "dotnet",
                            Arguments = command,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };

                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        AnsiConsole.MarkupLine(Helpers.FormatMessage($"Error executing command: dotnet {command}", "red"));
                        AnsiConsole.MarkupLine(Helpers.FormatMessage(error, "red"));
                    }
                    else
                    {
                        ctx.Status($"Completed: dotnet {command}");
                    }

                    ctx.Spinner(Spinner.Known.Star);
                    ctx.SpinnerStyle(Style.Parse("green"));
                });
        }

        private static string GetMsSqlConfigurationContent()
        {
            return $@"
// Format connection string
static string CreateConnectionString(string[] args)
{{
    var erver = ConfigHelper.GetConfigParam(args, ""MSSQL_SERVER"");
    var dbName = ConfigHelper.GetConfigParam(args, ""MSSQL_DB_NAME"");
    var user = ConfigHelper.GetConfigParam(args, ""MSSQL_USER"");
    var password = ConfigHelper.GetConfigParam(args, ""MSSQL_PASSWORD"");
    var trustServerCertificate = ConfigHelper.GetConfigParam(args, ""MSSQL_TRUST_SERVER_CERTIFICATE"", ""false"");
    var trustedConnection = ConfigHelper.GetConfigParam(args, ""MSSQL_TRUSTED_CONNECTION"", ""false"");
    var multipleActiveResultSets = ConfigHelper.GetConfigParam(args, ""MSSQL_MULTIPLE_ACTIVE_RESULT_SETS"", ""false"");

    return $""Server={{erver}}; Database={{dbName}}; User Id={{user}}; Password={{password}}; TrustServerCertificate={{trustServerCertificate}}; Trusted_Connection={{trustedConnection}}; MultipleActiveResultSets={{multipleActiveResultSets}}"";
}}

// * Add Entity Framework Core.
// * If your entities not located (only) at current assembly - customise via C'tor of 'ApplicationDbContext'.
var connectionString = CreateConnectionString(args);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
";
        }

        private static string GetPostgresConfigurationContent()
        {
            return $@"
// Format connection string
static string CreateConnectionString(string[] args)
{{
    var server = ConfigHelper.GetConfigParam(args, ""POSTGRES_SERVER"");
    var dbName = ConfigHelper.GetConfigParam(args, ""POSTGRES_DB_NAME"");
    var user = ConfigHelper.GetConfigParam(args, ""POSTGRES_USER"");
    var password = ConfigHelper.GetConfigParam(args, ""POSTGRES_PASSWORD"");

    return $""Host={{server}}; Database={{dbName}}; Username={{user}}; Password={{password}}"";
}}

// * Add Entity Framework Core.
// * If your entities not located (only) at current assembly - customise via C'tor of 'ApplicationDbContext'.
var connectionString = CreateConnectionString(args);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
";
        }

        private static string GetProgramCsContent1(string projectName, string dbConfigurationContent)
        {
            return $@"using Microsoft.EntityFrameworkCore;
using System.Reflection;
using {projectName}.Data;
using NestNet.Infra.Helpers;
using NestNet.Infra.Swagger;

var builder = WebApplication.CreateBuilder(args);

{dbConfigurationContent}
// Support Dependency Injection for all classes with [Injectable] attribute
// (Daos, Services, etc), within the given assembleis.
DependencyInjectionHelper.RegisterInjetables(builder.Services, [Assembly.GetExecutingAssembly()]);";
        }

        private static string GetProgramCsContent2()
        {
            return @"builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();

    // Add enum descriptions
    c.SchemaFilter<EnumSchemaFilter>();

    // Add QueryDto properties descriptions
    c.SchemaFilter<QueryDtoSchemaFilter>();
});";
        }

        private static string GetProgramCsContent3()
        {
            return @"app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();";
        }

        private static string GetProgramCsContent4()
        {
            return @"// Initialize / update the DB
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // dbContext.Database.Migrate();  // This will create the database and apply migration
    dbContext.Database.EnsureCreated();  // Simpler, doesn't support migrations
}

app.Run();";
        }

        private static string GetApplicationDbContextContent(string projectName)
        {
            return $@"using Microsoft.EntityFrameworkCore;
using NestNet.Infra.BaseClasses;
using System.Reflection;

namespace {projectName}.Data
{{
    public class ApplicationDbContext : ApplicationDbContextBase
    {{
         public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(
                options,
                [Assembly.GetExecutingAssembly()] // If your entities not located (only) at current assembly - customise here
            )
        {{
        }}
    }}
}}";
        }

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

        private static string Replace(string content, string oldValue, string newValue)
        {
            if (!content.Contains(oldValue))
            {
                throw new Exception($"Content not contains '{oldValue}'");
            }
            return content.Replace(oldValue, newValue);
        }
    }
}