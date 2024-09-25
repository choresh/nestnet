using System.Diagnostics;
using System.Xml.Linq;
using Spectre.Console;
using NestNet.Cli.Infra;
using System.Reflection;

namespace NestNet.Cli.Generators
{
    internal static class AppGenerator
    {
        public class InputParams
        {
            /// <summary>
            /// Force regeneration of folder content.
            /// </summary>
            public bool Force { get; set; }
        }

        private class AppGenerationContext
        {
            public AppGenerationContext(bool force, string currentDir, string projectName) {
                Force = force;
                CurrentDir = currentDir;
                ProjectName = projectName; 
               
            }
            public bool Force { get; private set; }
            public string CurrentDir { get; private set; }
            public string ProjectName { get; private set; }
        }

        public static void Run(InputParams? inputParams = null)
        {
            AnsiConsole.MarkupLine(Helpers.FormatMessage("App generation - startrd\n", "green"));

            var context = CreateAppGenerationContext(inputParams);

            var goOn = PrepareDirectory(context);
            if (!goOn) {
                return;
            }
            goOn = GenerateWebApiProject(context);
            if (!goOn)
            {
                return;
            }
            RemoveFsItems(context);
            CreateFsItems(context);
            ModifyWebApiProject(context);
            AddTestPackages();
            AddDtoGenerator(context);
            CopyDocumentation(context.CurrentDir);

            AnsiConsole.MarkupLine(Helpers.FormatMessage("\nApp generation - ended successfully", "green"));
        }

        private static AppGenerationContext CreateAppGenerationContext(InputParams? inputParams = null)
        {
            var currentDir = Directory.GetCurrentDirectory();
            var projectName = Path.GetFileName(currentDir);
            var force = (inputParams != null) && inputParams.Force;
            return new AppGenerationContext(force, currentDir, projectName);
        }

        private static bool PrepareDirectory(AppGenerationContext context)
        {
            string[] items = Directory.GetFileSystemEntries(context.CurrentDir);

            if (items.Length > 0)
            {
                AnsiConsole.MarkupLine(Helpers.FormatMessage($"The current folder ('{context.CurrentDir}') is not empty.", "green"));
                if (AnsiConsole.Confirm("Do you want to regenerate the folder content?"))
                {
                    AnsiConsole.Status()
                        .Start("Deleting folder contents...", ctx =>
                        {
                            Directory.Delete(context.CurrentDir, true);
                            Directory.CreateDirectory(context.CurrentDir);
                            ctx.Status("Folder contents deleted.");
                            ctx.Spinner(Spinner.Known.Star);
                            ctx.SpinnerStyle(Style.Parse("green"));
                        });
                }
                else
                {
                    AnsiConsole.MarkupLine(Helpers.FormatMessage("Operation cancelled. Exiting without making changes.", "red"));
                    return false;
                }
            }
            else
            {
                AnsiConsole.MarkupLine(Helpers.FormatMessage($"The current folder ('{context.CurrentDir}') is empty, folder content will be generated.", "green"));
            }
            return true;
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
            RunDotNetCommand("add package Microsoft.EntityFrameworkCore.SqlServer");
            RunDotNetCommand("add package Microsoft.EntityFrameworkCore.Tools");
            RunDotNetCommand("add package Microsoft.EntityFrameworkCore.InMemory");
            CreateApplicationDbContextFile(context);
            UpdateProgramCs(context);
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

        private static void UpdateProgramCs(AppGenerationContext context)
        {
            var programCsPath = Path.Combine(context.CurrentDir, "Program.cs");
            var content = File.ReadAllText(programCsPath);

            content = content.Replace("var builder = WebApplication.CreateBuilder(args);", GetProgramCsStartContent(context.ProjectName));

            content = content.Replace("builder.Services.AddSwaggerGen();", GetProgramCsMiddleContent());

            content = content.Replace("app.Run();", GetProgramCsEndContent());

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

        private static string GetProgramCsStartContent(string projectName)
        {
            return $@"using Microsoft.EntityFrameworkCore;
using System.Reflection;
using {projectName}.Data;
using NestNet.Infra.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Format connection string
static string CreateConnectionString(string[] args)
{{
    var dbServer = ConfigHelper.GetConfigParam(args, ""DB_SERVER"");
    var dbName = ConfigHelper.GetConfigParam(args, ""DB_NAME"");
    var dbUser = ConfigHelper.GetConfigParam(args, ""DB_USER"");
    var dbPassword = ConfigHelper.GetConfigParam(args, ""DB_PASSWORD"");
    var dbTrustServerCertificate = ConfigHelper.GetConfigParam(args, ""DB_TRUST_SERVER_CERTIFICATE"", ""false"");
    var dbTrustedConnection = ConfigHelper.GetConfigParam(args, ""DB_TRUSTED_CONNECTION"", ""false"");
    var dbMultipleActiveResultSets = ConfigHelper.GetConfigParam(args, ""DB_MULTIPLE_ACTIVE_RESULT_SETS"", ""false"");

    return $""Server={{dbServer}}; Database={{dbName}}; User Id={{dbUser}}; Password={{dbPassword}}; TrustServerCertificate={{dbTrustServerCertificate}}; Trusted_Connection={{dbTrustedConnection}}; MultipleActiveResultSets={{dbMultipleActiveResultSets}}"";
}}

// * Add Entity Framework Core.
// * If namespace of your entities is not standard (not ended with '.Entities') - customise via C'tor of 'ApplicationDbContext'.
// * If location of your entities is not at the standard folder ('.\Modules\<module name>\Entities') - customise
//   parametrs for post-build command that execute the 'GenerateDtos.ps1' script.
var connectionString = CreateConnectionString(args);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Support Dependency Injection for all classes with [Injectable] attribute
// (Daos, Services, etc), within the given assembleis.
DependencyInjectionHelper.RegisterInjetables(builder.Services, [Assembly.GetExecutingAssembly()]);";
        }

        private static string GetProgramCsMiddleContent()
        {
            return @"
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();

    // Add enum descriptions
    c.SchemaFilter<SwaggerHelper.EnumSchemaFilter>();
});";
        }

        private static string GetProgramCsEndContent()
        {
            return @"
// Initialize/update the DB
DbHelper.InitDb<ApplicationDbContext>(app.Services);

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
    }
}