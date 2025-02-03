using NestNet.Cli.Infra;
using NestNet.Infra.Enums;
using Spectre.Console;
using static NestNet.Cli.Generators.AppGenerator.AppGenerator;
using System.Xml.Linq;

namespace NestNet.Cli.Generators.CoreGenerator
{
    internal static class CoreGenerator
    {
        public class InputParams
        {
            public DbType? DbType { get; set; }
        }

        private class CoreGenerationContext
        {
            public required string CurrentDir { get; set; }
            public required string BaseProjectName { get; set; }
            public required string CurrProjectName { get; set; }
            public required DbType DbType { get; set; }
            public required string CorePath { get; set; }
        }

        public static void Run(InputParams? inputParams = null)
        {
            try
            {
                AnsiConsole.MarkupLine(Helpers.FormatMessage("\nCore generation - started", "green"));

                var context = CreateCoreGenerationContext(inputParams);
                if (context == null)
                {
                    AnsiConsole.MarkupLine(Helpers.FormatMessage("\nCore generation - ended, unable to generate the app", "green"));
                    return;
                }

                GenerateRootFiles(context);

                // Create core directory structure
                Directory.CreateDirectory(context.CorePath);
                GenerateProjectFile(context);
                GenerateDbContext(context);

                AnsiConsole.MarkupLine(Helpers.FormatMessage("\nCore generation - ended successfully", "green"));
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine(Helpers.FormatMessage($"\nCore generation - failed ({ex.Message})", "red"));
            }
        }

        private static void GenerateRootFiles(CoreGenerationContext context)
        {
            var gitignoreContent = @"
bin
lib
            ";
            File.WriteAllText(Path.Combine(context.CurrentDir, ".gitignore"), gitignoreContent);
        }

        /*
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
        */


        private static void GenerateProjectFile(CoreGenerationContext context)
        {
            string dbPackage;
            switch (context.DbType)
            {
                case DbType.MsSql:
                    dbPackage = "Microsoft.EntityFrameworkCore.SqlServer";
                    break;
                case DbType.Postgres:
                    dbPackage = "Npgsql.EntityFrameworkCore.PostgreSQL";
                    break;
                default:
                    throw new ArgumentException($"Unsupported database type: {context.DbType}");
            }

            var csprojContent = $@"<Project Sdk=""Microsoft.NET.Sdk"">

    <PropertyGroup>
        <TargetFramework>net8.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include=""Microsoft.EntityFrameworkCore"" Version=""*"" />
        <PackageReference Include=""{dbPackage}"" Version=""*"" />

        <PackageReference Include=""Xunit"" Version=""*"" />
        <PackageReference Include=""Microsoft.TestPlatform.TestHost"" Version=""*"" />
        <PackageReference Include=""AutoFixture"" Version=""*"" />
        <PackageReference Include=""AutoFixture.AutoNSubstitute"" Version=""*"" />
        <PackageReference Include=""Microsoft.EntityFrameworkCore.InMemory"" Version=""*"" />
        <PackageReference Include=""NSubstitute"" Version=""*"" />
        <PackageReference Include=""xunit.runner.visualstudio"" Version=""*"">
            <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
            <PrivateAssets>all</PrivateAssets>
        </PackageReference>
        <PackageReference Include=""Microsoft.NET.Test.Sdk"" Version=""*"" />
    </ItemGroup>

</Project>";

            File.WriteAllText(Path.Combine(context.CorePath, $"{context.CurrProjectName}.csproj"), csprojContent);
        }

        private static void GenerateDbContext(CoreGenerationContext context)
        {
            var dbContextContent = $@"using Microsoft.EntityFrameworkCore;
using NestNet.Infra.BaseClasses;
using System.Reflection;

namespace {context.CurrProjectName}.Data
{{
    public class AppDbContext : AppDbContextBase
    {{
         public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(
                options,
                [Assembly.GetExecutingAssembly()] // If your entities not located (only) at current assembly - customise here
            )
        {{
        }}
    }}
}}";

            var tarDir = Path.Combine(context.CorePath, "Data");
            Directory.CreateDirectory(tarDir);
            File.WriteAllText(Path.Combine(tarDir, "AppDbContext.cs"), dbContextContent);
        }

        private static CoreGenerationContext? CreateCoreGenerationContext(InputParams? inputParams = null)
        {
            var dbType = DbType.MsSql; // Default value
            var currentDir = Directory.GetCurrentDirectory();

            // Get project name from root folder and add Core suffix
            var rootFolderName = new DirectoryInfo(currentDir).Name;
            var baseProjectName = rootFolderName;
            var currProjectName = $"{baseProjectName}.Core";
            var corePath = Path.Combine(currentDir, currProjectName);

            if (!Helpers.CheckTarDir(corePath))
            {
                return null;
            }

            if (inputParams == null)
            {
                var dbChoice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select database type:")
                        .AddChoices("MSSQL", "PostgreSQL", "Exit"));

                switch (dbChoice)
                {
                    case "MSSQL":
                        dbType = DbType.MsSql;
                        break;
                    case "PostgreSQL":
                        dbType = DbType.Postgres;
                        break;
                    case "Exit":
                        return null;
                }
            }
            else if (inputParams.DbType.HasValue)
            {
                dbType = inputParams.DbType.Value;
            }

            return new CoreGenerationContext
            {
                CurrentDir = currentDir,
                BaseProjectName = baseProjectName,
                CurrProjectName = currProjectName,
                DbType = dbType,
                CorePath = corePath
            };
        }

        /*
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
        */
    }
}