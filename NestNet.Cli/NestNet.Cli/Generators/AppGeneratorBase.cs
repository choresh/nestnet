using System.Diagnostics;
using Spectre.Console;
using NestNet.Cli.Infra;
using System.Reflection;
using NestNet.Infra.Enums;

namespace NestNet.Cli.Generators
{
    internal abstract class AppGeneratorBase
    {
        public class InputParams
        {
            public DbType DbType { get; set; }
        }

        protected class AppGenerationContext
        {
            public AppGenerationContext(string rootDir, string projectPrefix, DbType dbType, string generatorName, string projectDir)
            {
                RootDir = rootDir;
                ProjectPrefix = projectPrefix;
                DbType = dbType;
                GeneratorName = generatorName;
                ProjectDir = projectDir;
            }
            public string RootDir { get; set; }
            public string ProjectPrefix { get; private set; }
            public string GeneratorName { get; private set; }
            public string ProjectDir { get; private set; }
            public string ProjectName => $"{ProjectPrefix}.{GeneratorName}";
            public DbType DbType { get; private set; }
        }

        protected class DbRelatedContent
        {
            public string NugetsContent { get; set; } = "";
            public string ConfigurationContent { get; set; } = "";
        }

        protected abstract string GeneratorName { get; }
        protected abstract string ProjectTemplateCommand { get; }

        public void Run(InputParams? inputParams = null)
        {
            AnsiConsole.MarkupLine(Helpers.FormatMessage($"{GeneratorName} generation - started\n", "green"));

            try
            {
                bool goOn = false;
                var context = CreateAppGenerationContext(inputParams);
                if (context != null)
                {
                    goOn = Helpers.CheckTarDir(context.ProjectDir);
                    if (goOn)
                    {
                        goOn = GenerateProject(context);
                    }
                }
                if (!goOn)
                {
                    AnsiConsole.MarkupLine(Helpers.FormatMessage($"\n{GeneratorName} generation - ended, unable to generate the app", "green"));
                    return;
                }

                CustomizeProject(context!);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine(Helpers.FormatMessage($"\n{GeneratorName} generation - failed ({ex.Message})", "red"));
                return;
            }

            AnsiConsole.MarkupLine(Helpers.FormatMessage($"\n{GeneratorName} generation - ended successfully", "green"));
        }

        protected abstract void CustomizeProject(AppGenerationContext context);

        protected AppGenerationContext? CreateAppGenerationContext(InputParams? inputParams = null)
        {
            DbType? dbType = null;
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
                        dbType = DbType.MsSql;
                        break;
                    case "Postgres":
                        dbType = DbType.Postgres;
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

            var rootDir = Directory.GetCurrentDirectory();
            var projectPrefix = Path.GetFileName(rootDir);
            var projectDir = Path.Combine(rootDir, $"{projectPrefix}.{GeneratorName}");
            return new AppGenerationContext(rootDir, projectPrefix, dbType.Value, GeneratorName, projectDir);
        }

        protected bool GenerateProject(AppGenerationContext context)
        {
            Directory.CreateDirectory(context.ProjectDir);
            
            AnsiConsole.MarkupLine(Helpers.FormatMessage($"\n{GeneratorName} project will be generated in {context.ProjectDir}...\n", "green"));
            
            try
            {
                // Create project with the full name and specify output directory
                RunDotNetCommand($"{ProjectTemplateCommand} -o .", context.ProjectDir);
                
                var csprojFile = Directory.GetFiles(context.ProjectDir, "*.csproj").FirstOrDefault();
                if (csprojFile == null)
                {
                    AnsiConsole.MarkupLine(Helpers.FormatMessage("Error: No .csproj file found in the project directory.", "red"));
                    return false;
                }
                
                AnsiConsole.MarkupLine(Helpers.FormatMessage($"{GeneratorName} project has been generated", "green"));
                return true;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine(Helpers.FormatMessage($"Error generating project: {ex.Message}", "red"));
                return false;
            }
        }

        protected void AddTestPackages(string workingDirectory)
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
                RunDotNetCommand($"add package {package}", workingDirectory);
            }

            AnsiConsole.MarkupLine(Helpers.FormatMessage("Test packages has been added to the project", "green"));
        }

        protected DbRelatedContent GetDbRelatedContent(NestNet.Infra.Enums.DbType dbType)
        {
            return dbType switch
            {
                NestNet.Infra.Enums.DbType.MsSql => new DbRelatedContent
                {
                    NugetsContent = "Microsoft.EntityFrameworkCore.SqlServer",
                    ConfigurationContent = "options.UseSqlServer(connectionString)"
                },
                NestNet.Infra.Enums.DbType.Postgres => new DbRelatedContent
                {
                    NugetsContent = "Npgsql.EntityFrameworkCore.PostgreSQL",
                    ConfigurationContent = "options.UseNpgsql(connectionString)"
                },
                _ => throw new ArgumentException($"Unsupported DB type: {dbType}")
            };
        }

        protected void RunDotNetCommand(string command, string workingDirectory)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = command,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory
            };

            // If it's an 'add package' command, find and specify the project file
            if (command.StartsWith("add package"))
            {
                var csprojFile = Directory.GetFiles(workingDirectory, "*.csproj").FirstOrDefault();
                if (csprojFile != null)
                {
                    // Remove "add" from the original command since we're specifying it with the project file
                    var packageCommand = command.Replace("add package", "package");
                    startInfo.Arguments = $"add \"{csprojFile}\" {packageCommand}";
                }
                else
                {
                    throw new Exception($"No .csproj file found in directory: {workingDirectory}");
                }
            }

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                throw new Exception($"Failed to start process for command: dotnet {command}");
            }

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception($"Error executing command: dotnet {command}\n{error}\nRunning: dotnet {startInfo.Arguments}");
            }
        }

        protected void CopyDocumentation(string projectRoot)
        {
            AnsiConsole.MarkupLine(Helpers.FormatMessage("\nCopy documentation started...", "green"));

            string docDir = Path.Combine(projectRoot, "Doc");
            Directory.CreateDirectory(docDir);

            CopyEmbeddedResource("README.md", Path.Combine(docDir, "README.md"));

            AnsiConsole.MarkupLine(Helpers.FormatMessage("Copy documentation ended", "green"));
        }

        protected void CreateApplicationDbContextFile(AppGenerationContext context)
        {
            var dbContextDir = Path.Combine(context.ProjectDir, "Data");
            Directory.CreateDirectory(dbContextDir);

            var dbContextPath = Path.Combine(dbContextDir, "ApplicationDbContext.cs");
            File.WriteAllText(dbContextPath, GetApplicationDbContextContent(context.ProjectName));
            AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {dbContextPath}", "grey"));
        }

        private string GetApplicationDbContextContent(string projectName)
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

        protected string GetConnectionStringSetup()
        {
            return @"// Format connection string
static string CreateConnectionString(string[] args)
{
    var server = ConfigHelper.GetConfigParam(args, ""POSTGRES_SERVER"");
    var dbName = ConfigHelper.GetConfigParam(args, ""POSTGRES_DB_NAME"");
    var user = ConfigHelper.GetConfigParam(args, ""POSTGRES_USER"");
    var password = ConfigHelper.GetConfigParam(args, ""POSTGRES_PASSWORD"");

    return $""Host={server}; Database={dbName}; Username={user}; Password={password}"";
}";
        }

        protected string GetDependencyInjectionSetup()
        {
            return @"// Support Dependency Injection for all classes with [Injectable] attribute
// (Daos, Services, etc), within the given assembleis.
DependencyInjectionHelper.RegisterInjetables(services, [Assembly.GetExecutingAssembly()]);";
        }

        protected string GetDbInitializationCode()
        {
            return @"// Initialize DB
using (var scope = serviceProvider.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // dbContext.Database.Migrate();  // This will create the database and apply migration
    dbContext.Database.EnsureCreated();  // Simpler, doesn't support migrations
}";
        }

        protected string GetDbContextSetup(string servicesName, string dbConfigurationContent)
        {
            return $@"// Add DB Context
var connectionString = CreateConnectionString(args);
{servicesName}.AddDbContext<ApplicationDbContext>(options =>
    {dbConfigurationContent});";
        }

        private void CopyEmbeddedResource(string resourceName, string targetPath)
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