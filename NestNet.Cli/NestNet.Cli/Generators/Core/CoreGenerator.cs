using NestNet.Cli.Infra;
using NestNet.Infra.Enums;
using Spectre.Console;

namespace NestNet.Cli.Generators.Core
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
    }
}