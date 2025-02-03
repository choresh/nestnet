using NestNet.Cli.Generators.Common;
using NestNet.Cli.Generators.ResourceGenerator.Internals;
using NestNet.Cli.Infra;
using Spectre.Console;

namespace NestNet.Cli.Generators.ResourceGenerator
{
    internal class WorkerResourceGenerator : MultiProjectsGeneratorBase<ResourceGenerationContext>
    {
        public WorkerResourceGenerator()
            : base(ProjectType.Worker)
        {
        }

        public override void DoGenerate()
        {
            if (Context.GenerateConsumer)
            {
                // Ensure Worker directory exists
                Directory.CreateDirectory(Context.ProjectContext!.TargetPath);

                CreateConsumerFile();
                CreateConsumerTestFile();
            }
        }

        private void CreateConsumerFile()
        {
            string consumerContent = GetConsumerContent();
            string consumerPath = Path.Combine(Context.ProjectContext!.TargetPath, "Consumers", $"{Context.ArtifactName}Consumer.cs");
            Directory.CreateDirectory(GetDirectoryName(consumerPath));
            File.WriteAllText(consumerPath, consumerContent);
            AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {consumerPath}", "grey"));
        }

        private void CreateConsumerTestFile()
        {
            string testContent = GetConsumerTestContent();
            string testPath = Path.Combine(Context.ProjectContext!.TargetPath, "Tests", "Consumers", $"{Context.ArtifactName}ConsumerTests.cs");
            Directory.CreateDirectory(GetDirectoryName(testPath));
            File.WriteAllText(testPath, testContent);
            AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {testPath}", "grey"));
        }

        private string GetConsumerContent()
        {
            var srcProjectName = Context.ProjectContext!.ProjectName.Replace(".Worker", ".Core");
            return $$"""
                using MassTransit;
                using {{srcProjectName}}.Resources.{{Context.ArtifactName}}.Dtos;
                using {{srcProjectName}}.Resources.{{Context.ArtifactName}}.Services;

                namespace {{Context.ProjectContext!.ProjectName}}.Resources.{{Context.ArtifactName}}.Consumers;

                public class {{Context.ArtifactName}}Consumer : IConsumer<{{Context.SampleInputDtoName}}>
                {
                    private readonly I{{Context.ArtifactName}}Service _{{Context.ParamName}}Service;

                    public {{Context.ArtifactName}}Consumer(I{{Context.ArtifactName}}Service {{Context.ParamName}}Service)
                    {
                        _{{Context.ParamName}}Service = {{Context.ParamName}}Service;
                    }

                    public async Task Consume(ConsumeContext<{{Context.SampleInputDtoName}}> context)
                    {
                        await _{{Context.ParamName}}Service.SampleOperation(context.Message);
                    }
                }
                """;
        }

        private string GetConsumerTestContent()
        {
            var srcProjectName = Context.ProjectContext!.ProjectName.Replace(".Worker", ".Core");
            return $@"using MassTransit;
using NSubstitute;
using Xunit;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using {srcProjectName}.Resources.{Context.ArtifactName}.Services;
using {srcProjectName}.Resources.{Context.ArtifactName}.Dtos;
using {Context.ProjectContext!.ProjectName}.Resources.{Context.ArtifactName}.Consumers;

namespace {Context.ProjectContext!.ProjectName}.Resources.{Context.ArtifactName}.Tests.Consumers
{{
    public class {Context.ArtifactName}ConsumerTests
    {{
        private readonly IFixture _fixture;
        private readonly I{Context.ArtifactName}Service _{Context.ParamName}Service;
        private readonly {Context.ArtifactName}Consumer _consumer;

        public {Context.ArtifactName}ConsumerTests()
        {{
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            _{Context.ParamName}Service = _fixture.Freeze<I{Context.ArtifactName}Service>();
            _consumer = new {Context.ArtifactName}Consumer(_{Context.ParamName}Service);
        }}

        [Fact]
        public async Task Consume_ShouldCallService_WithCorrectParameters()
        {{
            // Arrange
            var message = _fixture.Create<{Context.SampleInputDtoName}>();
            var context = Substitute.For<ConsumeContext<{Context.SampleInputDtoName}>>();
            context.Message.Returns(message);

            // Act
            await _consumer.Consume(context);

            // Assert
            await _{Context.ParamName}Service.Received(1).SampleOperation(Arg.Is<{Context.SampleInputDtoName}>(x => x == message));
        }}
    }}
}}";
        }
    }
}