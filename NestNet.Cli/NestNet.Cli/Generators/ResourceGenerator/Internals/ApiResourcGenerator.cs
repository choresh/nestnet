using NestNet.Cli.Generators.Common;
using NestNet.Cli.Generators.ResourceGenerator.Internals;
using NestNet.Cli.Infra;
using Spectre.Console;

namespace NestNet.Cli.Generators.ResourceGenerator
{
    internal class ApiResourceGenerator : MultiProjectsGeneratorBase<ResourceGenerationContext>
    {
        public ApiResourceGenerator()
            : base(ProjectType.Api)
        {
        }

        public override void DoGenerate()
        {
            if (Context.GenerateController)
            {
                // Ensure API directory exists
                Directory.CreateDirectory(Context.ProjectContext!.TargetPath);

                CreateControllerFile();
                CreateControllerTestFile();
            }
        }

        private void CreateControllerFile()
        {
            string controllerContent = GetControllerContent();
            string controllerPath = Path.Combine(Context.ProjectContext!.TargetPath, "Controllers", $"{Context.ArtifactName}Controller.cs");
            Directory.CreateDirectory(GetDirectoryName(controllerPath));
            File.WriteAllText(controllerPath, controllerContent);
            AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {controllerPath}", "grey"));
        }

        private void CreateControllerTestFile()
        {
            string testContent = GetControllerTestContent();
            string testPath = Path.Combine(Context.ProjectContext!.TargetPath, "Tests", "Controllers", $"{Context.ArtifactName}ControllerTests.cs");
            Directory.CreateDirectory(GetDirectoryName(testPath));
            File.WriteAllText(testPath, testContent);
            AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {testPath}", "grey"));
        }

        private string GetControllerContent()
        {
            var srcProjectName = Context.ProjectContext!.ProjectName.Replace(".Api", ".Core");
            return $@"#pragma warning disable IDE0290 // Use primary constructor
using Microsoft.AspNetCore.Mvc;
using {srcProjectName}.Resources.{Context.ArtifactName}.Dtos;
using {srcProjectName}.Resources.{Context.ArtifactName}.Services;

namespace {Context.ProjectContext!.ProjectName}.Resources.{Context.ArtifactName}.Controllers
{{
    [Route(""api/{Context.KebabCaseResourceName}"")]
    public class {Context.ArtifactName}Controller : ControllerBase
    {{
        private I{Context.ArtifactName}Service _{Context.ParamName}Service;

        public {Context.ArtifactName}Controller(I{Context.ArtifactName}Service {Context.ParamName}Service)
        {{
            _{Context.ParamName}Service = {Context.ParamName}Service;
        }}

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<{Context.SampleOutputDtoName}>>> SampleOperation({Context.SampleInputDtoName} input)
        {{
            var result = await _{Context.ParamName}Service.SampleOperation(input);
            return Ok(result);
        }}

        // How to customize this class:
		// 1) You can modify the sample method.
        // 2) You can add simmilar methods.
        // 3) Instead 'Ok()' - you can use other returnning options, e.g.:       
        //    * NotFound() (then - decorate your method with '[ProducesResponseType(StatusCodes.Status404NotFound)]')
        //    * BadRequest() (then - decorate your method with '[ProducesResponseType(StatusCodes.Status400BadRequest)]')
        //    * CreatedAtAction(...) (then - decorate your method with '[ProducesResponseType(StatusCodes.Status201Created)]')
    }}
}}

#pragma warning restore IDE0290 // Use primary constructor";
        }

        private string GetControllerTestContent()
        {
            var srcProjectName = Context.ProjectContext!.ProjectName.Replace(".Api", ".Core");
            return $@"using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using {Context.ProjectContext!.ProjectName}.Resources.{Context.ArtifactName}.Controllers;
using {srcProjectName}.Resources.{Context.ArtifactName}.Dtos;
using {srcProjectName}.Resources.{Context.ArtifactName}.Services;

namespace {Context.ProjectContext!.ProjectName}.Resources.{Context.ArtifactName}.Tests.Controllers
{{
    public class {Context.ArtifactName}ControllerTests
    {{
        private readonly IFixture _fixture;
        private readonly I{Context.ArtifactName}Service _{Context.ParamName}Service;
        private readonly {Context.ArtifactName}Controller _controller;

        public {Context.ArtifactName}ControllerTests()
        {{
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            _{Context.ParamName}Service = _fixture.Freeze<I{Context.ArtifactName}Service>();
            _controller = new {Context.ArtifactName}Controller(_{Context.ParamName}Service);
        }}

        [Fact]
        public async Task SampleOperation_ReturnsOkResult_WithAllItems()
        {{
            // Arrange
            var input = _fixture.Create<{Context.SampleInputDtoName}>();
            var expectedResult = _fixture.CreateMany<{Context.SampleOutputDtoName}>(2).ToList();
            _{Context.ParamName}Service.SampleOperation(Arg.Any<{Context.SampleInputDtoName}>()).Returns(Task.FromResult<IEnumerable<{Context.SampleOutputDtoName}>>(expectedResult));

            // Act
            var result = await _controller.SampleOperation(input);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            var resultData = Assert.IsAssignableFrom<IEnumerable<{Context.SampleOutputDtoName}>>(okResult.Value);
            Assert.Equal(expectedResult.Count, resultData.Count());
            await _{Context.ParamName}Service.Received(1).SampleOperation(Arg.Any<{Context.SampleInputDtoName}>());
        }}
    }}
}}";

        }
    }
}