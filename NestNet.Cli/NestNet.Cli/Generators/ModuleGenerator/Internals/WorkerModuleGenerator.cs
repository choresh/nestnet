using NestNet.Cli.Generators.Common;
using NestNet.Cli.Infra;

namespace NestNet.Cli.Generators.ModuleGenerator
{
    internal class WorkerModuleGenerator : MultiProjectsGeneratorBase<ModuleGenerationContext>
    {
        public WorkerModuleGenerator()
            : base(ProjectType.Worker)
        {
        }

        public override void DoGenerate()
        {

            // Worker-specific generation logic will be implemented in the future
            /*
            if (Context.GenerateWorker)
            {
                // Ensure Worker directory exists
                Directory.CreateDirectory(Context.TargetPath);

                CreateConsumerFile();
                CreateConsumerTestFile();
            }
            */
        }

        // Placeholder for future worker-specific methods
        /*
        private void CreateConsumerFile()
        {
            string consumerContent = GetConsumerContent();
            string consumerPath = Path.Combine(Context.TargetPath, "Consumers", $"{Context.ModuleName}Consumer.cs");
            Directory.CreateDirectory(GetDirectoryName(consumerPath));
            File.WriteAllText(consumerPath, consumerContent);
            AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {consumerPath}", "grey"));
        }

        // ZZZZ
        private void CreateConsumerTestFile()
        {
            string testContent = GetConsumerTestContent();
            string testPath = Path.Combine(Context.TargetPath, "Tests", "Consumers", $"{Context.ModuleName}ConsumerTests.cs");
            Directory.CreateDirectory(GetDirectoryName(testPath));
            File.WriteAllText(testPath, testContent);
            AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {testPath}", "grey"));
        }
        */
    }
}