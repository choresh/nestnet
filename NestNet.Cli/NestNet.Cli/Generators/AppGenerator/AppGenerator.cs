using Spectre.Console;
using NestNet.Cli.Infra;

namespace NestNet.Cli.Generators.AppGenerator
{
    internal static partial class AppGenerator
    {
        public class InputParams
        {
            public required IEnumerable<AppType> AppTypes { get; set; }
        }

        public static void Run(InputParams inputParams)
        {
            try
            {
                foreach (var currAppType in inputParams.AppTypes) {

                    AnsiConsole.MarkupLine(Helpers.FormatMessage($"\nApp generation ({currAppType}) - started", "green"));

                    AppGeneratorBase generator;
                    switch (currAppType)
                    {
                        case AppType.Api:
                            generator = new ApiAppGenerator();
                            break;
                        case AppType.Worker:
                            generator = new WorkerAppGenerator();
                            break;
                        default:
                            throw new Exception("Unknown app type");
                    }

                    var context = generator.CreateAppGenerationContext();
                    if (context == null)
                    {
                        AnsiConsole.MarkupLine(Helpers.FormatMessage($"\nApp generation ({currAppType}) - ended, unable to generate the app", "green"));
                        return;
                    }

                    generator.Generate(context);

                    AnsiConsole.MarkupLine(Helpers.FormatMessage($"\nApp generation ({currAppType}) - ended successfully", "green"));
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine(Helpers.FormatMessage($"\nApp generation - failed ({ex.Message})", "red"));
            }
        }
    }
}