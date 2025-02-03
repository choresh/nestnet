using NestNet.Cli.Generators.Common;
using NestNet.Cli.Infra;
using NestNet.Infra.Helpers;
using Spectre.Console;
using System.Text.RegularExpressions;

namespace NestNet.Cli.Generators.ModuleGenerator
{
    internal static partial class ModuleGenerator
    {
        public class InputParams
        {
            /// <summary>
            /// Name of the module.
            /// </summary>
            public required string ModuleName { get; set; }

            /// <summary>
            /// Pluralized name of the module.
            /// </summary>
            public required string PluralizedModuleName { get; set; }

            /// <summary>
            /// Generate database support (entity).
            /// </summary>
            public bool GenerateDbSupport { get; set; } = true;

            /// <summary>
            /// Generate service.
            /// </summary>
            public bool GenerateService { get; set; } = true;

            /// <summary>
            /// Generate controlle.
            /// </summary>
            public bool GenerateController { get; set; } = true;
        }

        public static void Run(InputParams? inputParams = null)
        {
            ModuleGenerationContext context;
            if (inputParams != null)
            {
                // Silent mode
                context = CreateSilentModuleGenerationContext(inputParams);
            }
            else
            {
                // Interactive mode
                context = CreateInteractiveModuleGenerationContext();
            }

            foreach (var projectType in Enum.GetValues<ProjectType>())
            {
                if (projectType == ProjectType.Worker)
                { 
                    continue; // This type not soppurted yet
                }
                AnsiConsole.MarkupLine(Helpers.FormatMessage($"\nModule generation ({projectType}) - started\n", "green"));

                MultiProjectsGeneratorBase<ModuleGenerationContext> generator;
                switch (projectType)
                {
                    case ProjectType.Core:
                        generator = new CoreModuleGenerator();
                        break;
                    case ProjectType.Api:
                        generator = new ApiModuleGenerator();
                        break;
                    case ProjectType.Worker:
                        generator = new WorkerModuleGenerator();
                        break;
                    default:
                        throw new Exception("Unknown project type");
                }

                var success = generator.Generate(context, "Modules");

                if (!success)
                {
                    AnsiConsole.MarkupLine(Helpers.FormatMessage($"\nModule generation ({projectType}) - ended, unable to generate the module", "green"));
                    return;
                }

                AnsiConsole.MarkupLine(Helpers.FormatMessage($"\nModule generation ({projectType}) - ended successfully\n", "green"));
            }
        }

        private static ModuleGenerationContext CreateSilentModuleGenerationContext(InputParams? inputParams = null)
        {
            string moduleName;
            if (inputParams == null)
            {
                moduleName = AnsiConsole.Ask<string>("Enter the module name:");
            }
            else
            {
                moduleName = inputParams.ModuleName;
            }

            return new ModuleGenerationContext
            {
                ArtifactName = moduleName,
                PluralizedModuleName = inputParams?.PluralizedModuleName ?? moduleName + "s",
                ParamName = StringHelper.ToCamelCase(moduleName),
                PluralizedParamName = StringHelper.ToCamelCase(moduleName + "s"),
                KebabCasePluralizedModuleName = Helpers.ToKebabCase(moduleName + "s"),
                EntityName = $"Entities.{moduleName}",
                NullableEntityName = $"{moduleName}?",
                CreateDtoName = Helpers.FormatDtoName(moduleName, DtoType.Create),
                UpdateDtoName = Helpers.FormatDtoName(moduleName, DtoType.Update),
                ResultDtoName = Helpers.FormatDtoName(moduleName, DtoType.Result),
                QueryDtoName = Helpers.FormatDtoName(moduleName, DtoType.Query),
                GenerateController = inputParams?.GenerateController ?? true,
                GenerateService = inputParams?.GenerateService ?? true,
                GenerateDbSupport = inputParams?.GenerateDbSupport ?? true
            };
        }

        private static ModuleGenerationContext CreateInteractiveModuleGenerationContext()
        {
            string moduleName = GetModuleName();

            // First ask about DB support since it's the foundation
            bool generateDbSupport = true; // AnsiConsole.Confirm("Generate database support (entity)?", true);

            // Only ask about service if DB support is enabled
            bool generateService = false;
            if (generateDbSupport)
            {
                generateService = AnsiConsole.Confirm("Generate service?", true);
            }

            // Only ask about controller if service is enabled
            bool generateController = false;
            if (generateService)
            {
                generateController = AnsiConsole.Confirm("Generate controller?", true);
            }

            string pluralizedModuleName = GetPluralizedModuleName(moduleName);
            var paramName = StringHelper.ToCamelCase(moduleName);
            var pluralizedParamName = StringHelper.ToCamelCase(pluralizedModuleName);
            var kebabCasePluralizedModuleName = Helpers.ToKebabCase(pluralizedModuleName);
            var entityName = $"{moduleName}Entity";
            var nullableEntityFullName = $"{entityName}?";

            return new ModuleGenerationContext
            {
                ArtifactName = moduleName,
                PluralizedModuleName = pluralizedModuleName,
                ParamName = paramName,
                PluralizedParamName = pluralizedParamName,
                KebabCasePluralizedModuleName = kebabCasePluralizedModuleName,
                EntityName = entityName,
                NullableEntityName = nullableEntityFullName,
                CreateDtoName = Helpers.FormatDtoName(moduleName, DtoType.Create),
                UpdateDtoName = Helpers.FormatDtoName(moduleName, DtoType.Update),
                ResultDtoName = Helpers.FormatDtoName(moduleName, DtoType.Result),
                QueryDtoName = Helpers.FormatDtoName(moduleName, DtoType.Query),
                GenerateDbSupport = generateDbSupport,
                GenerateService = generateService,
                GenerateController = generateController
            };
        }

        private static string GetPluralizedModuleName(string moduleName)
        {
            return AnsiConsole.Prompt(
                new TextPrompt<string>("Enter the pluralized module name (press Enter to except the shown default)")
                .DefaultValue(moduleName + "s")
                .PromptStyle("green")
                .ValidationErrorMessage(Helpers.FormatMessage("That's not a valid pluralized module name", "red"))
                .Validate(ValidateResorceName));
        }

        private static string GetModuleName()
        {
            return AnsiConsole.Prompt(
                new TextPrompt<string>("Enter the module name (singular, capitalized, with alphanumeric\n characters only. e.g. 'Product', 'CardHolder', etc.):")
                .PromptStyle("green")
                .ValidationErrorMessage(Helpers.FormatMessage("That's not a valid module name", "red"))
                .Validate(ValidateResorceName));
        }

        private static ValidationResult ValidateResorceName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return ValidationResult.Error("Module name cannot be empty.");
            }
            if (!char.IsUpper(input[0]))
            {
                return ValidationResult.Error("Module name must start with a capital letter.");
            }
            if (!Regex.IsMatch(input, "^[A-Z][a-zA-Z0-9]*$"))
            {
                return ValidationResult.Error("Module name must contain alphanumeric characters only.");
            }
            return ValidationResult.Success();
        }
    }
}