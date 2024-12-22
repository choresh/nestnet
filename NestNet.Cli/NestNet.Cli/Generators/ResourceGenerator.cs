using NestNet.Cli.Infra;
using NestNet.Infra.Helpers;
using Spectre.Console;
using System.Text.RegularExpressions;

namespace NestNet.Cli.Generators
{
    internal static class ResourceGenerator
    {
        public class InputParams
        {
            /// <summary>
            /// Name of the resource.
            /// </summary>
            public required string ResourceName { get; set; }
        }

        private enum SampleDtoType
        {
            Input,
            Output
        }

        private class ResourceGenerationContext
        {
            public required string CurrentDir { get; set; }
            public required string ProjectName { get; set; }
            public required string ResourceName { get; set; }
            public required string ParamName { get; set; }
            public required string KebabCaseResourceName { get; set; }
            public required string ResourcePath { get; set; }
            public required string SampleInputDtoName { get; set; }
            public required string SampleOutputDtoName { get; set; }
        }

        public static void Run(InputParams? inputParams = null)
        {
            AnsiConsole.MarkupLine(Helpers.FormatMessage("\nResource generation - started\n", "green"));

            try
            {
                ResourceGenerationContext? context;
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
                if (context == null || !Helpers.CheckTarDir(context.ResourcePath))
                {
                    AnsiConsole.MarkupLine(Helpers.FormatMessage("\nResource generation - ended, unable to generate the resource", "green"));
                    return;
                }
                CreateResourceFiles(context);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine(Helpers.FormatMessage($"\nResource generation - failed ({ex.Message})", "red"));
                return;
            }

            AnsiConsole.MarkupLine(Helpers.FormatMessage("\nResource generation - ended successfully", "green"));
        }

        private static ResourceGenerationContext? CreateSilentResourceGenerationContext(InputParams inputParams)
        {
            var (currentDir, projectName) = Helpers.GetProjectInfo();
            if (currentDir == null || projectName == null)
            {
                return null;
            }

            var paramName = StringHelper.ToCamelCase(inputParams.ResourceName);
            var kebabCaseResourceName = Helpers.ToKebabCase(inputParams.ResourceName);
            string resourcePath = Path.Combine(currentDir, "Resources", inputParams.ResourceName);

            return new ResourceGenerationContext
            {
                CurrentDir = currentDir,
                ProjectName = projectName,
                ResourceName = inputParams.ResourceName,
                ParamName = paramName,
                KebabCaseResourceName = kebabCaseResourceName,
                ResourcePath = resourcePath,
                SampleInputDtoName = "SampleInputDto",
                SampleOutputDtoName = "SampleOutputDto"
            };
        }

        private static ResourceGenerationContext? CreateInteractiveResourceGenerationContext()
        {
            string resourceName = GetResourceName();
            var (currentDir, projectName) = Helpers.GetProjectInfo();
            if (currentDir == null || projectName == null)
            {
                return null;
            }
            var paramName = StringHelper.ToCamelCase(resourceName);
            var kebabCaseResourceName = Helpers.ToKebabCase(resourceName);
            string resourcePath = Path.Combine(currentDir, "Resources", resourceName);
            return new ResourceGenerationContext
            {
                CurrentDir = currentDir,
                ProjectName = projectName,
                ResourceName = resourceName,
                ParamName = paramName,
                KebabCaseResourceName = kebabCaseResourceName,
                ResourcePath = resourcePath,
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


        private static void CreateResourceFiles(ResourceGenerationContext context)
        {
            CreateSampleDtoFile(context, SampleDtoType.Input);
            CreateSampleDtoFile(context, SampleDtoType.Output);
            CreateServiceFile(context);
            CreateControllerFile(context);
            CreateControllerTestFile(context);
            CreateServiceTestFile(context);
        }

        private static void CreateSampleDtoFile(ResourceGenerationContext context, SampleDtoType sampleDtoType)
        {
            string dtoName;
            switch (sampleDtoType)
            {
                case SampleDtoType.Output:
                    dtoName = context.SampleOutputDtoName;
                    break;
                case SampleDtoType.Input:
                    dtoName = context.SampleInputDtoName;
                    break;
                default:
                    throw new Exception("Invalid  SampleDtoType");
            }
            string dtoContent = GetSampleDtoContent(context.ProjectName, context.ResourceName, dtoName);
            string dtoPath = Path.Combine(context.ResourcePath, "Dtos", $"{dtoName}.cs");
            Directory.CreateDirectory(GetDirectoryName(dtoPath));
            File.WriteAllText(dtoPath, dtoContent);
            AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {dtoPath}", "grey"));
        }

        private static string GetSampleDtoContent(string projectName, string resourceName, string dtoName)
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

        private static void CreateServiceFile(ResourceGenerationContext context)
        {
            string serviceContent = GetServiceContent(context);
            string servicePath = Path.Combine(context.ResourcePath, "Services", $"{context.ResourceName}Service.cs");
            Directory.CreateDirectory(GetDirectoryName(servicePath));
            File.WriteAllText(servicePath, serviceContent);
            AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {servicePath}", "grey"));
        }

        private static void CreateControllerFile(ResourceGenerationContext context)
        {
            string controllerContent = GetControllerContent(context);
            string controllerPath = Path.Combine(context.ResourcePath, "Controllers", $"{context.ResourceName}Controller.cs");
            Directory.CreateDirectory(GetDirectoryName(controllerPath));
            File.WriteAllText(controllerPath, controllerContent);
            AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {controllerPath}", "grey"));
        }

        private static void CreateControllerTestFile(ResourceGenerationContext context)
        {
            string testContent = GetControllerTestContent(context);
            string testPath = Path.Combine(context.ResourcePath, "Tests", "Controllers", $"{context.ResourceName}ControllerTests.cs");
            Directory.CreateDirectory(GetDirectoryName(testPath));
            File.WriteAllText(testPath, testContent);
            AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {testPath}", "grey"));
        }

        private static void CreateServiceTestFile(ResourceGenerationContext context)
        {
            string testContent = GetServiceTestContent(context);
            string testPath = Path.Combine(context.ResourcePath, "Tests", "Services", $"{context.ResourceName}ServiceTests.cs");
            Directory.CreateDirectory(GetDirectoryName(testPath));
            File.WriteAllText(testPath, testContent);
            AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {testPath}", "grey"));
        }

        private static string GetDirectoryName(string path)
        {
            var dirName = Path.GetDirectoryName(path);
            if (dirName == null)
            {
                throw new Exception($"Directory name not found, path: {path}");
            }
            return dirName;
        }

        private static string GetServiceContent(ResourceGenerationContext context)
        {
            return $@"#pragma warning disable IDE0290 // Use primary constructor
using {context.ProjectName}.Resources.{context.ResourceName}.Dtos;
using NestNet.Infra.Attributes;

namespace {context.ProjectName}.Resources.{context.ResourceName}.Services
{{
    public interface I{context.ResourceName}Service
    {{
        Task<IEnumerable<{context.SampleOutputDtoName}>> SampleOperation({context.SampleInputDtoName} input);
    }}

    [Injectable(LifetimeType.Scoped)]
    public class {context.ResourceName}Service : I{context.ResourceName}Service
    {{
        public {context.ResourceName}Service()
        {{
        }}

   		public async Task<IEnumerable<{context.SampleOutputDtoName}>> SampleOperation({context.SampleInputDtoName} input)
        {{
        	// Replace this sample code with your code.
            return new List<{context.SampleOutputDtoName}>() {{
                new {context.SampleOutputDtoName}(),
                new {context.SampleOutputDtoName}()
            }};
        }}

        // How to customize this class:
		// 1) You can modify the sample method.
        // 2) You can add simmilar methods.
    }}
}}

#pragma warning restore IDE0290 // Use primary constructor";
        }

        private static string GetControllerContent(ResourceGenerationContext context)
        {
            return $@"#pragma warning disable IDE0290 // Use primary constructor
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using {context.ProjectName}.Resources.{context.ResourceName}.Dtos;
using {context.ProjectName}.Resources.{context.ResourceName}.Services;

namespace {context.ProjectName}.Resources.{context.ResourceName}.Controllers
{{
    [Route(""api/{context.KebabCaseResourceName}"")]
    public class {context.ResourceName}Controller : ControllerBase
    {{
        private I{context.ResourceName}Service _{context.ParamName}Service;

        public {context.ResourceName}Controller(I{context.ResourceName}Service {context.ParamName}Service)
        {{
            _{context.ParamName}Service = {context.ParamName}Service;
        }}

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<{context.SampleOutputDtoName}>>> SampleOperation({context.SampleInputDtoName} input)
        {{
            var result = await _{context.ParamName}Service.SampleOperation(input);
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

        private static string GetControllerTestContent(ResourceGenerationContext context)
        {
            return $@"using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using {context.ProjectName}.Resources.{context.ResourceName}.Controllers;
using {context.ProjectName}.Resources.{context.ResourceName}.Dtos;
using {context.ProjectName}.Resources.{context.ResourceName}.Services;

namespace {context.ProjectName}.Resources.{context.ResourceName}.Tests.Controllers
{{
    public class {context.ResourceName}ControllerTests
    {{
        private readonly IFixture _fixture;
        private readonly I{context.ResourceName}Service _{context.ParamName}Service;
        private readonly {context.ResourceName}Controller _controller;

        public {context.ResourceName}ControllerTests()
        {{
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            _{context.ParamName}Service = _fixture.Freeze<I{context.ResourceName}Service>();
            _controller = new {context.ResourceName}Controller(_{context.ParamName}Service);
        }}

        [Fact]
        public async Task SampleOperation_ReturnsOkResult_WithAllItems()
        {{
            // Arrange
            var input = _fixture.Create<{context.SampleInputDtoName}>();
            var expectedResult = _fixture.CreateMany<{context.SampleOutputDtoName}>(2).ToList();
            _{context.ParamName}Service.SampleOperation(Arg.Any<{context.SampleInputDtoName}>()).Returns(Task.FromResult<IEnumerable<{context.SampleOutputDtoName}>>(expectedResult));

            // Act
            var result = await _controller.SampleOperation(input);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            var resultData = Assert.IsAssignableFrom<IEnumerable<{context.SampleOutputDtoName}>>(okResult.Value);
            Assert.Equal(expectedResult.Count, resultData.Count());
            await _{context.ParamName}Service.Received(1).SampleOperation(Arg.Any<{context.SampleInputDtoName}>());
        }}
    }}
}}";
        }

        private static string GetServiceTestContent(ResourceGenerationContext context)
        {
            return $@"using NSubstitute;
using Xunit;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using System.Text.Json;
using {context.ProjectName}.Resources.{context.ResourceName}.Services;
using {context.ProjectName}.Resources.{context.ResourceName}.Dtos;

namespace {context.ProjectName}.Resources.{context.ResourceName}.Tests.Services
{{
    public class {context.ResourceName}ServiceTests
    {{
        private readonly IFixture _fixture;
        private readonly {context.ResourceName}Service _service;

        public {context.ResourceName}ServiceTests()
        {{
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            _service = new {context.ResourceName}Service();
        }}

        [Fact]
        public async Task SampleOperation_ReturnsAllItems()
        {{
            // Arrange
            var input = _fixture.Create<{context.SampleInputDtoName}>();
            var expectedResult = _fixture.CreateMany<{context.SampleOutputDtoName}>(2).ToList();
       
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