using Spectre.Console;
using System.Text.RegularExpressions;

namespace NestNet.Cli.Infra
{
    internal static class Helpers
    {
        public static bool CheckTarDir(string tarDirPath)
        {
            if (!Directory.Exists(tarDirPath))
            {
                AnsiConsole.MarkupLine(Helpers.FormatMessage($"The target directory ('{tarDirPath}') not exists, directory content will be generated.", "green"));
                return true;
            }
            string[] items = Directory.GetFileSystemEntries(tarDirPath);
            if (items.Length == 0)
            {
                AnsiConsole.MarkupLine(Helpers.FormatMessage($"The target directory ('{tarDirPath}') is empty, directory content will be generated.", "green"));
                return true;
            }
            AnsiConsole.MarkupLine(Helpers.FormatMessage($"The target directory ('{tarDirPath}') is not empty, clean it or use another directory", "yellow"));
            return false;
        }

        public static string ToKebabCase(string str)
        {
            return Regex.Replace(str, "([a-z])([A-Z])", "$1-$2").ToLower();
        }

        public static string FormatMessage(string message, string style)
        {
            return $"[{style}]{message}[/]";
        }

        public static (string? projectDir, string? projectName) GetProjectInfo(ProjectType projectType)
        {
            var currentDir = Directory.GetCurrentDirectory();
            var baseProjectName = Path.GetFileName(currentDir);
            var projectName = $"{baseProjectName}.{projectType}";
            var projectDir = Path.Join(currentDir, projectName);
            if (!Directory.Exists(projectDir))
            {
                AnsiConsole.MarkupLine(Helpers.FormatMessage($"Warnning: No correspond directory found for '{projectType}' project.", "yellow"));
                return (null, null);

            }
            var csprojFile = Directory.GetFiles(projectDir, "*.csproj").FirstOrDefault();
            if (csprojFile == null)
            {
                AnsiConsole.MarkupLine(Helpers.FormatMessage($"Error: No .csproj file found in the '{projectType}' project directory.", "red"));
                return (null, null);
            }
            return (projectDir, projectName);
        }

        public static string FormatDtoName(string baseName, DtoType dtoType)
        {
            return $"{baseName}{dtoType}Dto";
        }

        public static string GetDtoContent(string projectName, string moduleName, string pluralizedModuleName, DtoType dtoType, Type? baseClass = null, string? properties = null)
        {
            var baseSyntax = baseClass == null
                ? ""
                : $" : {baseClass.Name}";

            var usingSyntax = baseClass == null
                ? ""
                : $"using {baseClass.Namespace};{Environment.NewLine}";

            return $@"{usingSyntax}namespace {projectName}.Modules.{pluralizedModuleName}.Dtos
{{
    /// <summary>
    /// * This is an auto-generated DTO class.
    /// * Do not modify this file directly as it will be regenerated.
    /// * To modify the properties, please update properties/attributes at the corresponding entity class.
    /// </summary>
    public class {FormatDtoName(moduleName, dtoType)}{baseSyntax}
    {{
{properties}
    }}
}}";
        }
    }
}
