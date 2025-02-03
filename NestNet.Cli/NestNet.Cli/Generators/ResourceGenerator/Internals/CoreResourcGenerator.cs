using NestNet.Cli.Generators.Common;
using NestNet.Cli.Infra;
using Spectre.Console;

namespace NestNet.Cli.Generators.ResourceGenerator
{
    internal static partial class ResourceGenerator
    {
        private class CoreResourceGenerator : MultiProjectsGeneratorBase<ResourceGenerationContext>
        {
            public CoreResourceGenerator()
                : base(ProjectType.Core)
            {
            }

            public override void DoGenerate()
            {
                CreateSampleDtoFile(SampleDtoType.Input);
                CreateSampleDtoFile(SampleDtoType.Output);
                CreateServiceFile();
                CreateServiceTestFile();
            }

            private void CreateSampleDtoFile(SampleDtoType sampleDtoType)
            {
                string dtoName;
                switch (sampleDtoType)
                {
                    case SampleDtoType.Output:
                        dtoName = Context.SampleOutputDtoName;
                        break;
                    case SampleDtoType.Input:
                        dtoName = Context.SampleInputDtoName;
                        break;
                    default:
                        throw new Exception("Invalid  SampleDtoType");
                }
                string dtoContent = GetSampleDtoContent(Context.ProjectContext!.ProjectName, Context.ArtifactName, dtoName);
                string dtoPath = Path.Combine(Context.ProjectContext!.TargetPath, "Dtos", $"{dtoName}.cs");
                Directory.CreateDirectory(GetDirectoryName(dtoPath));
                File.WriteAllText(dtoPath, dtoContent);
                AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {dtoPath}", "grey"));
            }

            private string GetSampleDtoContent(string projectName, string resourceName, string dtoName)
            {
                return $@"namespace {projectName}.Resources.{resourceName}.Dtos
{{
    /// <summary>
    /// This is a sample DTO class.
    /// </summary>
    public class {dtoName}
    {{
        // Add here your required properties.
    }}
}}";
            }

            private void CreateServiceFile()
            {
                string serviceContent = GetServiceContent();
                string servicePath = Path.Combine(Context.ProjectContext!.TargetPath, "Services", $"{Context.ArtifactName}Service.cs");
                Directory.CreateDirectory(GetDirectoryName(servicePath));
                File.WriteAllText(servicePath, serviceContent);
                AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {servicePath}", "grey"));
            }

            private void CreateServiceTestFile()
            {
                string testContent = GetServiceTestContent();
                string testPath = Path.Combine(Context.ProjectContext!.TargetPath, "Tests", "Services", $"{Context.ArtifactName}ServiceTests.cs");
                Directory.CreateDirectory(GetDirectoryName(testPath));
                File.WriteAllText(testPath, testContent);
                AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {testPath}", "grey"));
            }

            private string GetServiceContent()
            {
                return $@"#pragma warning disable IDE0290 // Use primary constructor
using {Context.ProjectContext!.ProjectName}.Resources.{Context.ArtifactName}.Dtos;
using NestNet.Infra.Attributes;

namespace {Context.ProjectContext!.ProjectName}.Resources.{Context.ArtifactName}.Services
{{
    public interface I{Context.ArtifactName}Service
    {{
        Task<IEnumerable<{Context.SampleOutputDtoName}>> SampleOperation({Context.SampleInputDtoName} input);
    }}

    [Injectable(LifetimeType.Scoped)]
    public class {Context.ArtifactName}Service : I{Context.ArtifactName}Service
    {{
        public {Context.ArtifactName}Service()
        {{
        }}

   		public async Task<IEnumerable<{Context.SampleOutputDtoName}>> SampleOperation({Context.SampleInputDtoName} input)
        {{
        	// Replace this sample code with your code.
            return new List<{Context.SampleOutputDtoName}>() {{
                new {Context.SampleOutputDtoName}(),
                new {Context.SampleOutputDtoName}()
            }};
        }}

        // How to customize this class:
		// 1) You can modify the sample method.
        // 2) You can add simmilar methods.
    }}
}}

#pragma warning restore IDE0290 // Use primary constructor";
            }

            private string GetServiceTestContent()
            {
                return $@"using NSubstitute;
using Xunit;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using System.Text.Json;
using {Context.ProjectContext!.ProjectName}.Resources.{Context.ArtifactName}.Services;
using {Context.ProjectContext!.ProjectName}.Resources.{Context.ArtifactName}.Dtos;

namespace {Context.ProjectContext!.ProjectName}.Resources.{Context.ArtifactName}.Tests.Services
{{
    public class {Context.ArtifactName}ServiceTests
    {{
        private readonly IFixture _fixture;
        private readonly {Context.ArtifactName}Service _service;

        public {Context.ArtifactName}ServiceTests()
        {{
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            _service = new {Context.ArtifactName}Service();
        }}

        [Fact]
        public async Task SampleOperation_ReturnsAllItems()
        {{
            // Arrange
            var input = _fixture.Create<{Context.SampleInputDtoName}>();
            var expectedResult = _fixture.CreateMany<{Context.SampleOutputDtoName}>(2).ToList();
       
            // Act
            var result = await _service.SampleOperation(input);
  
            // Assert
            Assert.Equal(expectedResult.Count(), result.Count());
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult),
                JsonSerializer.Serialize(result));

        }}
    }}
}}";
            }
        }
    }
}
