using System.Diagnostics;
using System.Xml.Linq;
using Spectre.Console;
using NestNet.Cli.Infra;
using System.Reflection;
using NestNet.Infra.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NestNet.Infra.Helpers;

namespace NestNet.Cli.Generators
{
    internal class WorkerAppGenerator : AppGeneratorBase
    {
        protected override string GeneratorName => "Worker";
        protected override string ProjectTemplateCommand => "new console";

        protected override void CustomizeProject(AppGenerationContext context)
        {
            CreateFsItems(context);
            ModifyWorkerProject(context);
            AddTestPackages(context.ProjectDir);
            CopyDocumentation(context.ProjectDir);
        }

        private void CreateFsItems(AppGenerationContext context)
        {
            AnsiConsole.MarkupLine(Helpers.FormatMessage("New folders will be created...", "green"));

            var itemsToAdd = new[]
            {
                Path.Combine(context.ProjectDir, "Data")
            };

            foreach (var item in itemsToAdd)
            {
                Directory.CreateDirectory(item);
                AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {item}", "grey"));
            }

            AnsiConsole.MarkupLine(Helpers.FormatMessage("New folders has been created\n", "green"));
        }

        private void ModifyWorkerProject(AppGenerationContext context)
        {
            AnsiConsole.MarkupLine(Helpers.FormatMessage("Adding Entity Framework support...", "green"));

            var dbRelatedContent = GetDbRelatedContent(context.DbType);

            RunDotNetCommand($"add package {dbRelatedContent.NugetsContent}", context.ProjectDir);
            RunDotNetCommand("add package Microsoft.EntityFrameworkCore.Tools", context.ProjectDir);
            RunDotNetCommand("add package Microsoft.EntityFrameworkCore.InMemory", context.ProjectDir);

            CreateApplicationDbContextFile(context);
            UpdateProgramCs(context, dbRelatedContent.ConfigurationContent);

            AnsiConsole.MarkupLine(Helpers.FormatMessage("Entity Framework support has been added", "green"));
        }

        private void UpdateProgramCs(AppGenerationContext context, string dbConfigurationContent)
        {
            var programCsPath = Path.Combine(context.ProjectDir, "Program.cs");
            var content = GetProgramCsContent(context.ProjectName, dbConfigurationContent);
            File.WriteAllText(programCsPath, content);
            AnsiConsole.MarkupLine(Helpers.FormatMessage($"Updated: {programCsPath}", "grey"));
        }

        private string GetProgramCsContent(string projectName, string dbConfigurationContent)
        {
            return $@"using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using {projectName}.Data;
using NestNet.Infra.Helpers;

{GetConnectionStringSetup()}

// Create service collection
var services = new ServiceCollection();

{GetDbContextSetup("services", dbConfigurationContent)}

{GetDependencyInjectionSetup()}

// Build service provider
var serviceProvider = services.BuildServiceProvider();

{GetDbInitializationCode()}

// Your worker logic goes here
Console.WriteLine(""Worker application started..."");

Console.WriteLine(""Press any key to exit..."");
Console.ReadKey();";
        }
    }
} 