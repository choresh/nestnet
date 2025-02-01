using System.Diagnostics;
using System.Xml.Linq;
using Spectre.Console;
using NestNet.Cli.Infra;
using System.Reflection;
using NestNet.Infra.Enums;
using System.Data;
using AutoMapper.Configuration.Conventions;

namespace NestNet.Cli.Generators
{
    internal class ApiAppGenerator : AppGeneratorBase
    {
        protected override string GeneratorName => "Api";
        protected override string ProjectTemplateCommand => "new webapi -controllers";

        protected override void CustomizeProject(AppGenerationContext context)
        {
            RemoveFsItems(context);
            CreateFsItems(context);
            ModifyWebApiProject(context);
            AddTestPackages(context.ProjectDir);
            AddDtoGenerator(context);
            CopyDocumentation(context.ProjectDir);
        }

        private void RemoveFsItems(AppGenerationContext context)
        {
            AnsiConsole.MarkupLine(Helpers.FormatMessage("\nUnused folders/files will be removed...", "green"));

            var itemsToRemove = new[]
            {
                Path.Combine(context.ProjectDir, "WeatherForecast.cs"),
                Path.Combine(context.ProjectDir, context.ProjectName + ".http"),
                Path.Combine(context.ProjectDir, "Controllers")
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

            AnsiConsole.MarkupLine(Helpers.FormatMessage("New folders has been created" +
                "\n", "green"));
        }

        private void ModifyWebApiProject(AppGenerationContext context)
        {
            AnsiConsole.MarkupLine(Helpers.FormatMessage("Some capabilities (Entity Framework support, automatic registration of DAOs and Services, enhanced Swagger features) will be added to the project...", "green"));

            var dbRelatedContent = GetDbRelatedContent(context.DbType);

            RunDotNetCommand($"add package {dbRelatedContent.NugetsContent}", context.ProjectDir);
            RunDotNetCommand("add package Microsoft.EntityFrameworkCore.Tools", context.ProjectDir);
            RunDotNetCommand("add package Microsoft.EntityFrameworkCore.InMemory", context.ProjectDir);

            CreateApplicationDbContextFile(context);

            UpdateProgramCs(context, dbRelatedContent.ConfigurationContent);

            AnsiConsole.MarkupLine(Helpers.FormatMessage("Some capabilities (Entity Framework support, automatic registration of DAOs and Services, enhanced Swagger features) has been added to the project...", "green"));
        }

        private void AddDtoGenerator(AppGenerationContext context)
        {
            AnsiConsole.MarkupLine(Helpers.FormatMessage("\nPre-build event that runs the Dtos generator will be added to the project...", "green"));

            var csprojPath = Path.Combine(context.ProjectDir, $"{context.ProjectName}.csproj");
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

        private void UpdateProgramCs(AppGenerationContext context, string dbConfigurationContent)
        {
            var programCsPath = Path.Combine(context.ProjectDir, "Program.cs");
            var content = File.ReadAllText(programCsPath);

            content = Replace(content, "var builder = WebApplication.CreateBuilder(args);", GetProgramCsContent1(context.ProjectName, dbConfigurationContent));
     
            content = Replace(content, "builder.Services.AddOpenApi();", GetProgramCsContent2());

            content = Replace(content, "app.MapOpenApi();", GetProgramCsContent3());

            content = Replace(content, "app.Run();", GetProgramCsContent4());

            File.WriteAllText(programCsPath, content);

            AnsiConsole.MarkupLine(Helpers.FormatMessage($"Updated: {programCsPath}", "grey"));
        }

        private string GetProgramCsContent1(string projectName, string dbConfigurationContent)
        {
            return $@"using Microsoft.EntityFrameworkCore;
using System.Reflection;
using {projectName}.Data;
using NestNet.Infra.Helpers;
using NestNet.Infra.Swagger;

var builder = WebApplication.CreateBuilder(args);

{GetConnectionStringSetup()}

{GetDbContextSetup("builder.Services", dbConfigurationContent)}

{GetDependencyInjectionSetup().Replace("services", "builder.Services")}";
        }

        private string GetProgramCsContent2()
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

        private string GetProgramCsContent3()
        {
            return @"app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();";
        }

        private string GetProgramCsContent4()
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

        private string Replace(string content, string oldValue, string newValue)
        {
            if (!content.Contains(oldValue))
            {
                throw new Exception($"Content not contains '{oldValue}'");
            }
            return content.Replace(oldValue, newValue);
        }
    }
}