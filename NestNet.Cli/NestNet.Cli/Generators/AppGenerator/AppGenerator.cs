using System.Diagnostics;
using System.Xml.Linq;
using Spectre.Console;
using NestNet.Cli.Infra;
using System.Reflection;
using DbType = NestNet.Infra.Enums.DbType;
using NestNet.Cli.Generators.Common;

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

        /*
        // ZZZ
        private static void AddTestPackages(AppGenerationContext context)
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
                RunDotNetCommand(context.CurrentDir, $"add package {package}");
            }

            AnsiConsole.MarkupLine(Helpers.FormatMessage("Test packages has been added to the project", "green"));
        }

        private static void RunDotNetCommand(string path, string command)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c dotnet {command}",
                WorkingDirectory = path,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

            process?.WaitForExit();
        }
        */

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
    }
}