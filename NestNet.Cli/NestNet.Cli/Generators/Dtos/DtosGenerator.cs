using NestNet.Cli.Infra;
using NestNet.Infra.Attributes;
using Spectre.Console;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace NestNet.Cli.Generators.Dtos
{
    internal static class DtosGenerator
    {
        public class InputParams
        {
            /// <summary>
            /// Relative target directory.
            /// </summary>
            public required string RelativeTarDir;
        }

        private class DtosGenerationContext
        {
            public DtosGenerationContext(
                string projectDir,
                string projectName,
                string relativeTarDir,
                string modulesPath,
                Assembly projectAssembly
                )
            {
                ProjectDir = projectDir;
                ProjectName = projectName;
                RelativeTarDir = relativeTarDir;
                ModulesPath = modulesPath;
                ProjectAssembly = projectAssembly;
            }
            public string ProjectDir { get; private set; }
            public string ProjectName { get; private set; }
            public string RelativeTarDir { get; private set; }
            public string ModulesPath { get; private set; }
            public Assembly ProjectAssembly { get; private set; }
        }

        public static void Run(InputParams? inputParams = null)
        {
            AnsiConsole.MarkupLine(Helpers.FormatMessage("\nDtos generation - started\n", "green"));

            try
            {
                var context = CreateDtosGenerationContext(inputParams);
                if (context == null)
                {
                    AnsiConsole.MarkupLine(Helpers.FormatMessage("\nDtos generation - ended, unable to generate/update DTOs", "green"));
                    return;
                }
                CreateDtosFiles(context);
            }
            catch (Exception ex)
            {
                WriteLog(ex.ToString().EscapeMarkup());
                AnsiConsole.MarkupLine(Helpers.FormatMessage("\nDtos generation - failed", "red"));
                return;
            }

            AnsiConsole.MarkupLine(Helpers.FormatMessage("\nDtos generation - ended successfully", "green"));
        }

        private static void CreateDtosFiles(DtosGenerationContext context)
        {
            WriteLog($"Start processing all modules in: {context.ModulesPath}");
            WriteLog($"Relative target directory: {context.RelativeTarDir}");

            foreach (var moduleFolder in Directory.GetDirectories(context.ModulesPath))
            {
                ProcessModuleFolder(context, moduleFolder);
            }

            WriteLog($"Finished processing all modules in: {context.ModulesPath}");
        }

        private static string PromptForDirectory(string promptMessage, string defaultValue)
        {
            return AnsiConsole.Prompt(
                new TextPrompt<string>($"{promptMessage} (press Enter to accept the shown default)")
                    .DefaultValue(defaultValue)
                    .PromptStyle("green")
                    .ValidationErrorMessage(Helpers.FormatMessage("That's not a valid directory path", "red"))
                    .Validate(ValidatePath));
        }

        private static ValidationResult ValidatePath(string path)
        {
            return path.StartsWith("\\")
                ? ValidationResult.Success()
                : ValidationResult.Error("Path must start with '\\'");
        }

        private static bool ValidateInputPath(string path)
        {
            if (!path.StartsWith("\\"))
            {
                AnsiConsole.MarkupLine(Helpers.FormatMessage($"Error: path '{path}' is not valid (path must start with '\\').", "red"));
                return false;
            }
            return true;
        }

        private static DtosGenerationContext? CreateDtosGenerationContext(InputParams? inputParams = null)
        {
            var (projectDir, projectName) = Helpers.GetProjectInfo(ProjType.Core);
            if (projectDir == null || projectName == null)
            {
                return null;
            }

            var projAssembly = LoadAssembly(projectDir, projectName);
            if (projAssembly == null)
            {
                AnsiConsole.MarkupLine(Helpers.FormatMessage("Warnning: Project assembly not found, try to re-build the project.", "yellow"));
                return null;
            }

            string relativeTarDir;
            if (inputParams != null)
            {
                if (!ValidateInputPath(inputParams.RelativeTarDir))
                {
                    return null;
                }
                relativeTarDir = inputParams.RelativeTarDir;
            }
            else
            {

                relativeTarDir = PromptForDirectory("Enter the relative target directory", @"\Dtos");
            }

            var modulesPath = Path.Combine(projectDir, "Modules");
            if (!Directory.Exists(modulesPath))
            {
                AnsiConsole.MarkupLine(Helpers.FormatMessage("Warnning: A 'Modules' directory not found in the current directory, unable to generate/update DTOs.", "yellow"));
                return null;
            }

            return new DtosGenerationContext(projectDir, projectName, relativeTarDir, modulesPath, projAssembly);
        }

        private static void WriteLog(string message)
        {
            var logMessage = $"Dtos generation: {message}";
            AnsiConsole.MarkupLine(Helpers.FormatMessage(logMessage, "grey"));
        }

        private static void ProcessModuleFolder(DtosGenerationContext context, string moduleFolder)
        {
            var pluralizedModuleName = Path.GetFileName(moduleFolder);
            WriteLog($"Start processing module: {pluralizedModuleName}");

            var targetPath = moduleFolder + context.RelativeTarDir;

            Directory.CreateDirectory(targetPath);

            // Find all entity classes in current project assembly
            var entities = context.ProjectAssembly
                .GetTypes()
                .Where(t => t.GetCustomAttribute<TableAttribute>() != null);

            foreach (var entity in entities)
            {
                ProcessEntity(context, entity, pluralizedModuleName, targetPath);
            }

            WriteLog($"Finished processing module: {pluralizedModuleName}");
        }

        private static Assembly? LoadAssembly(string projectRoot, string projectName)
        {
            var binDirectory = Path.Combine(projectRoot, "bin");
            if (!Directory.Exists(binDirectory))
            {
                WriteLog($"Bin directory not found: {binDirectory}");
                return null;
            }

            var configurations = new[] { "Debug", "Release" };

            foreach (var config in configurations)
            {
                var configPath = Path.Combine(binDirectory, config);
                if (!Directory.Exists(configPath)) continue;

                var frameworkFolders = Directory.GetDirectories(configPath, "net*")
                    .Concat(Directory.GetDirectories(configPath, "netcoreapp*"));

                foreach (var frameworkFolder in frameworkFolders)
                {
                    var potentialPath = Path.Combine(frameworkFolder, $"{projectName}.dll");
                    if (File.Exists(potentialPath))
                    {
                        WriteLog($"Assembly found at: {potentialPath}");
                        return Assembly.LoadFrom(potentialPath);
                    }
                }
            }

            return null;
        }

        private static void ProcessEntity(DtosGenerationContext context, Type entity, string pluralizedModuleName, string targetPath)
        {
            WriteLog($"Start processing entity: {entity.Name}");

            foreach (DtoType dtoType in Enum.GetValues(typeof(DtoType)))
            {
                GenerateDto(context, entity, pluralizedModuleName, targetPath, dtoType);
            }

            WriteLog($"Finished processing entity: {entity.Name}");
        }

        private static void GenerateDto(
            DtosGenerationContext context,
            Type entity,
            string pluralizedModuleName,
            string targetPath,
            DtoType dtoType,
            Type? baseClass = null
            )
        {
            var dtoName = Helpers.FormatDtoName(entity.Name, dtoType);
            var dtoBody = ConvertEntityToDto(entity, dtoName, dtoType);
            var dtoFileContent = Helpers.GetDtoContent(context.ProjectName, entity.Name, pluralizedModuleName, dtoType, baseClass, dtoBody);
            var dtoFile = Path.Combine(targetPath, $"{dtoName}.cs");
            WriteLog($"Generated DTO: {dtoName}");
            File.WriteAllText(dtoFile, dtoFileContent);
        }

        private static string ConvertEntityToDto(Type entityType, string dtoName, DtoType dtoType)
        {
            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var dtoProperties = new List<string>();

            foreach (var property in properties)
            {
                var propAttribute = property.GetCustomAttribute<PropAttribute>();
                if (propAttribute != null)
                {
                    GenOpt? opt = null;
                    switch (dtoType)
                    {
                        case DtoType.Create:
                            opt = propAttribute.Create;
                            break;
                        case DtoType.Update:
                            opt = propAttribute.Update;
                            break;
                        case DtoType.Result:
                            opt = propAttribute.Result;
                            break;
                        case DtoType.Query:
                            opt = propAttribute.Result == GenOpt.Ignore
                                ? GenOpt.Ignore
                                : GenOpt.Optional;
                            break;
                        default:
                            throw new Exception("Unexpected DTO Type");
                    }
                    if (opt != GenOpt.Ignore)
                    {
                        dtoProperties.Add(GetDtoProperty(property.PropertyType.Name, property.Name, opt.Value));
                    }
                }
            }

            return string.Join(Environment.NewLine, dtoProperties);
        }

        private static string GetDtoProperty(string propType, string propName, GenOpt opt)
        {
            var optionalOperator = opt == GenOpt.Optional
                ? "?"
                : "";
            var requiredKeyword = opt == GenOpt.Mandatory
                ? "required "
                : "";
            return $"\t\tpublic {requiredKeyword}{propType}{optionalOperator} {propName} {{ get; set; }}";
        }
    }
}