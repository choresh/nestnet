using NestNet.Cli.Generators.Common;
using NestNet.Cli.Generators.ResourceGenerator.Internals;
using NestNet.Cli.Infra;
using NestNet.Infra.Helpers;
using Spectre.Console;
using System.Text.RegularExpressions;

namespace NestNet.Cli.Generators.ResourceGenerator
{
    internal static partial class ResourceGenerator
    {
        public class InputParams
        {
            /// <summary>
            /// Name of the resource.
            /// </summary>
            public required string ResourceName { get; set; }
        }

        public static void Run(InputParams? inputParams = null)
        {
            ResourceGenerationContext context;
            if (inputParams != null)
            {
                // Silent mode
                context = CreateSilentResourceGenerationContext(inputParams);
            }
            else
            {
                // Interactive mode
                context = CreateInteractiveResourceGenerationContext();
            }

            foreach (var projectType in Enum.GetValues<ProjectType>())
            {
                AnsiConsole.MarkupLine(Helpers.FormatMessage($"\nResource generation ({projectType}) - started\n", "green"));

                MultiProjectsGeneratorBase<ResourceGenerationContext> generator;
                switch (projectType)
                {
                    case ProjectType.Core:
                        generator = new CoreResourceGenerator();
                        break;
                    case ProjectType.Api:
                        generator = new ApiResourceGenerator();
                        break;
                    case ProjectType.Worker:
                        generator = new WorkerResourceGenerator();
                        break;
                    default:
                        throw new Exception("Unknown project type");
                }

                var success = generator.Generate(context, "Resources");

                if (!success)
                {
                    AnsiConsole.MarkupLine(Helpers.FormatMessage($"\nResource generation ({projectType}) - ended, unable to generate the resource", "green"));
                    return;
                }

                AnsiConsole.MarkupLine(Helpers.FormatMessage($"\nResource generation ({projectType}) - ended successfully\n", "green"));
            }
        }

        private static ResourceGenerationContext CreateSilentResourceGenerationContext(InputParams inputParams)
        {
            var paramName = StringHelper.ToCamelCase(inputParams.ResourceName);
            var kebabCaseResourceName = Helpers.ToKebabCase(inputParams.ResourceName);

            return new ResourceGenerationContext
            {
                GenerateController = true, // ZZZ
                GenerateConsumer = true, // ZZZ
                ArtifactName = inputParams.ResourceName,
                ParamName = paramName,
                KebabCaseResourceName = kebabCaseResourceName,
                SampleInputDtoName = "SampleInputDto",
                SampleOutputDtoName = "SampleOutputDto",
            };
        }

        private static ResourceGenerationContext CreateInteractiveResourceGenerationContext()
        {
            string resourceName = GetResourceName();
            var paramName = StringHelper.ToCamelCase(resourceName);
            var kebabCaseResourceName = Helpers.ToKebabCase(resourceName);

            return new ResourceGenerationContext
            {
                GenerateController = true, // ZZZ
                GenerateConsumer = true, // ZZZ
                ArtifactName = resourceName,
                ParamName = paramName,
                KebabCaseResourceName = kebabCaseResourceName,
                SampleInputDtoName = "SampleInputDto",
                SampleOutputDtoName = "SampleOutputDto"
            };
        }

        private static string GetResourceName()
        {
            return AnsiConsole.Prompt(
                new TextPrompt<string>("Enter the resource name (capitalized, with alphanumeric\n characters only. e.g. 'Payments', 'FlowManager', etc.):")
                .PromptStyle("green")
                .ValidationErrorMessage(Helpers.FormatMessage("That's not a valid resource name", "red"))
                .Validate(ValidateResorceName));
        }

        private static ValidationResult ValidateResorceName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return ValidationResult.Error("Resource name cannot be empty.");
            }
            if (!char.IsUpper(input[0]))
            {
                return ValidationResult.Error("Resource name must start with a capital letter.");
            }
            if (!Regex.IsMatch(input, "^[A-Z][a-zA-Z0-9]*$"))
            {
                return ValidationResult.Error("Resource name must contain alphanumeric characters only.");
            }
            return ValidationResult.Success();
        }

    }
}