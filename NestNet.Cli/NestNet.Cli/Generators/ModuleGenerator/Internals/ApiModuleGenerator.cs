using NestNet.Cli.Generators.Common;
using NestNet.Cli.Infra;
using Spectre.Console;

namespace NestNet.Cli.Generators.ModuleGenerator
{
    internal static partial class ModuleGenerator
    {
        private class ApiModuleGenerator : MultiProjectsGeneratorBase<ModuleGenerationContext>
        {
            public ApiModuleGenerator()
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
                string controllerPath = Path.Combine(Context.ProjectContext!.TargetPath, "Controllers", $"{Context.PluralizedModuleName}Controller.cs");
                Directory.CreateDirectory(GetDirectoryName(controllerPath));
                File.WriteAllText(controllerPath, controllerContent);
                AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {controllerPath}", "grey"));
            }

            private void CreateControllerTestFile()
            {
                string testContent = GetControllerTestContent();
                string testPath = Path.Combine(Context.ProjectContext!.TargetPath, "Tests", "Controllers", $"{Context.PluralizedModuleName}ControllerTests.cs");
                Directory.CreateDirectory(GetDirectoryName(testPath));
                File.WriteAllText(testPath, testContent);
                AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {testPath}", "grey"));
            }

            private string GetControllerContent()
            {
                var srcProjectName = Context.ProjectContext!.ProjectName.Replace(".Api", ".Core");
                return $@"#pragma warning disable IDE0290 // Use primary constructor
using Microsoft.AspNetCore.Mvc;
using NestNet.Infra.BaseClasses;
using {srcProjectName}.Modules.{Context.PluralizedModuleName}.Dtos;
using {srcProjectName}.Modules.{Context.PluralizedModuleName}.Services;
using {srcProjectName}.Modules.{Context.PluralizedModuleName}.Entities;

namespace {Context.ProjectContext!.ProjectName}.Modules.{Context.PluralizedModuleName}.Controllers
{{
    [Route(""api/{Context.KebabCasePluralizedModuleName}"")]
    public class {Context.PluralizedModuleName}Controller : CrudControllerBase<{Context.EntityName}, {Context.CreateDtoName}, {Context.UpdateDtoName}, {Context.ResultDtoName}, {Context.QueryDtoName}>
    {{
        public {Context.PluralizedModuleName}Controller(I{Context.PluralizedModuleName}Service {Context.PluralizedParamName}Service)
            : base({Context.PluralizedParamName}Service, ""{Context.ArtifactName}Id"")
        {{
        }}

        [HttpGet]
        public override async Task<ActionResult<IEnumerable<{Context.ResultDtoName}>>> GetAll()
        {{
            return await base.GetAll();
        }}

        [HttpGet(""{{{Context.ParamName}Id}}"")]
        public override async Task<ActionResult<{Context.ResultDtoName}>> GetById(long {Context.ParamName}Id)
        {{
            return await base.GetById({Context.ParamName}Id);
        }}

        [HttpPost]
        public override async Task<ActionResult<{Context.ResultDtoName}>> Create({Context.CreateDtoName} {Context.ParamName})
        {{
            return await base.Create({Context.ParamName});
        }}

        [HttpPut(""{{{Context.ParamName}Id}}"")]
        public override async Task<ActionResult<{Context.ResultDtoName}>> Update(long {Context.ParamName}Id, {Context.UpdateDtoName} {Context.ParamName}, bool ignoreMissingOrNullFields)
        {{
            return await base.Update({Context.ParamName}Id, {Context.ParamName}, ignoreMissingOrNullFields);
        }}

        [HttpDelete(""{{{Context.ParamName}Id}}"")]
        public override async Task<IActionResult> Delete(long {Context.ParamName}Id)
        {{
            return await base.Delete({Context.ParamName}Id);
        }}
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
using NestNet.Infra.Query;
using NestNet.Infra.BaseClasses;
using NestNet.Infra.Paginatation;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using System.Text.Json;
using {Context.ProjectContext!.ProjectName}.Modules.{Context.PluralizedModuleName}.Controllers;
using {srcProjectName}.Modules.{Context.PluralizedModuleName}.Dtos;
using {srcProjectName}.Modules.{Context.PluralizedModuleName}.Services;
using {srcProjectName}.Modules.{Context.PluralizedModuleName}.Entities;

namespace {Context.ProjectContext!.ProjectName}.Modules.{Context.PluralizedModuleName}.Tests.Controllers
{{
    public class {Context.PluralizedModuleName}ControllerTests
    {{
        private readonly IFixture _fixture;
        private readonly I{Context.PluralizedModuleName}Service _{Context.PluralizedParamName}Service;
        private readonly {Context.PluralizedModuleName}Controller _controller;

        public {Context.PluralizedModuleName}ControllerTests()
        {{
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            _{Context.PluralizedParamName}Service = _fixture.Freeze<I{Context.PluralizedModuleName}Service>();
            _controller = new {Context.PluralizedModuleName}Controller(_{Context.PluralizedParamName}Service);
        }}

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithAllItems()
        {{
            // Arrange
            var expectedResult = _fixture.CreateMany<{Context.ResultDtoName}>(2).ToList();
            _{Context.PluralizedParamName}Service.GetAll().Returns(Task.FromResult<IEnumerable<{Context.ResultDtoName}>>(expectedResult));

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            var resultData = Assert.IsAssignableFrom<IEnumerable<{Context.ResultDtoName}>>(okResult.Value);
            Assert.Equal(expectedResult.Count, resultData.Count());
            await _{Context.PluralizedParamName}Service.Received(1).GetAll();
        }}

        [Fact]
        public async Task GetById_ReturnsOkResult_WhenItemExists()
        {{
            // Arrange
            var id = _fixture.Create<long>();
            var expectedResult = _fixture.Create<{Context.ResultDtoName}>();
            _{Context.PluralizedParamName}Service.GetById(Arg.Any<long>()).Returns(Task.FromResult<{Context.ResultDtoName}?>(expectedResult));

            // Act
            var result = await _controller.GetById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            var resultData = Assert.IsType<{Context.ResultDtoName}>(okResult.Value);
            Assert.Equal(expectedResult, resultData);
            await _{Context.PluralizedParamName}Service.Received(1).GetById(id);
        }}

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenItemDoesNotExist()
        {{
            // Arrange
            var id = _fixture.Create<long>();
            _{Context.PluralizedParamName}Service.GetById(Arg.Any<long>()).Returns(Task.FromResult<{Context.ResultDtoName}?>(null));

            // Act
            var result = await _controller.GetById(id);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Value);
            var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
            Assert.NotNull(notFoundResult);
            Assert.Equal(404, notFoundResult.StatusCode);
            await _{Context.PluralizedParamName}Service.Received(1).GetById(id);
        }}

        [Fact]
        public async Task Create_ReturnsCreatedResult_WithNewItem()
        {{
            // Arrange
            var id = _fixture.Create<long>();
            var createDto = _fixture.Create<{Context.CreateDtoName}>();
            var expectedResult = _fixture.Create<{Context.ResultDtoName}>();
            var internalCreateResult = new InternalCreateResult<{Context.ResultDtoName}>
            {{
                Id = id,
                ResultDto = expectedResult
            }};
            _{Context.PluralizedParamName}Service.Create(Arg.Any<{Context.CreateDtoName}>()).Returns(Task.FromResult(internalCreateResult));

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.NotNull(createdResult);
            Assert.Equal(201, createdResult.StatusCode);
            var resultData = Assert.IsType<{Context.ResultDtoName}>(createdResult.Value);
            Assert.Equal(expectedResult, resultData);
            await _{Context.PluralizedParamName}Service.Received(1).Create(createDto);
        }}

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_True_ReturnsOkResult_WhenUpdateSuccessful()
        {{
            // Arrange
            var ignoreMissingOrNullFields = true;
            var id = _fixture.Create<long>();
            var updateDto = _fixture.Create<{Context.UpdateDtoName}>();
            var expectedResult = _fixture.Create<{Context.ResultDtoName}>();
            _{Context.PluralizedParamName}Service.Update(Arg.Any<long>(), Arg.Any<{Context.UpdateDtoName}>(), Arg.Any<bool>()).Returns(Task.FromResult<{Context.ResultDtoName}?>(expectedResult));

            // Act
            var result = await _controller.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            var resultData = Assert.IsType<{Context.ResultDtoName}>(okResult.Value);
            Assert.Equal(expectedResult, resultData);
            await _{Context.PluralizedParamName}Service.Received(1).Update(id, updateDto, ignoreMissingOrNullFields);
        }}

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_True_ReturnsNotFound_WhenItemDoesNotExist()
        {{
            // Arrange
            var ignoreMissingOrNullFields = true;
            var id = _fixture.Create<long>();
            var updateDto = _fixture.Create<{Context.UpdateDtoName}>();
            _{Context.PluralizedParamName}Service.Update(Arg.Any<long>(), Arg.Any<{Context.UpdateDtoName}>(), Arg.Any<bool>()).Returns(Task.FromResult<{Context.ResultDtoName}?>(null));

            // Act
            var result = await _controller.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Value);
            var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
            Assert.NotNull(notFoundResult);
            Assert.Equal(404, notFoundResult.StatusCode);
            await _{Context.PluralizedParamName}Service.Received(1).Update(id, updateDto, ignoreMissingOrNullFields);
        }}

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_False_ReturnsOkResult_WhenUpdateSuccessful()
        {{
            // Arrange
            var ignoreMissingOrNullFields = false;
            var id = _fixture.Create<long>();
            var updateDto = _fixture.Create<{Context.UpdateDtoName}>();
            var expectedResult = _fixture.Create<{Context.ResultDtoName}>();
            _{Context.PluralizedParamName}Service.Update(Arg.Any<long>(), Arg.Any<{Context.UpdateDtoName}>(), Arg.Any<bool>()).Returns(Task.FromResult<{Context.ResultDtoName}?>(expectedResult));

            // Act
            var result = await _controller.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            var resultData = Assert.IsType<{Context.ResultDtoName}>(okResult.Value);
            Assert.Equal(expectedResult, resultData);
            await _{Context.PluralizedParamName}Service.Received(1).Update(id, updateDto, ignoreMissingOrNullFields);
        }}

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_False_ReturnsNotFound_WhenItemDoesNotExist()
        {{
            // Arrange
            var ignoreMissingOrNullFields = false;
            var id = _fixture.Create<long>();
            var updateDto = _fixture.Create<{Context.UpdateDtoName}>();
            _{Context.PluralizedParamName}Service.Update(Arg.Any<long>(), Arg.Any<{Context.UpdateDtoName}>(), Arg.Any<bool>()).Returns(Task.FromResult<{Context.ResultDtoName}?>(null));

            // Act
            var result = await _controller.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Value);
            var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
            Assert.NotNull(notFoundResult);
            Assert.Equal(404, notFoundResult.StatusCode);
            await _{Context.PluralizedParamName}Service.Received(1).Update(id, updateDto, ignoreMissingOrNullFields);
        }}

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenDeleteSuccessful()
        {{
            // Arrange
            var id = _fixture.Create<long>();
            _{Context.PluralizedParamName}Service.Delete(Arg.Any<long>()).Returns(Task.FromResult(true));

            // Act
            var result = await _controller.Delete(id);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.NotNull(noContentResult);
            Assert.Equal(204, noContentResult.StatusCode);
            await _{Context.PluralizedParamName}Service.Received(1).Delete(id);
        }}

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenItemDoesNotExist()
        {{
            // Arrange
            var id = _fixture.Create<long>();
            _{Context.PluralizedParamName}Service.Delete(Arg.Any<long>()).Returns(Task.FromResult(false));

            // Act
            var result = await _controller.Delete(id);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
            Assert.NotNull(notFoundResult);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.IsType<NotFoundResult>(result);
            await _{Context.PluralizedParamName}Service.Received(1).Delete(id);
        }}

        [Fact]
        public async Task GetPaginated_ReturnsOkResult_WithPaginatedItems()
        {{
            // Arrange
            var request = _fixture.Create<UnsafePaginationRequest>();
            var dtos = _fixture.CreateMany<{Context.ResultDtoName}>(3).ToList();
            var expectedResult = new DataWithOptionalError<PaginatedResult<{Context.ResultDtoName}>>
            {{
                Data = new PaginatedResult<{Context.ResultDtoName}>()
                {{
                    Items = dtos,
                    TotalCount = dtos.Count()
                }}
            }};
            _{Context.PluralizedParamName}Service.GetPaginated(Arg.Any<UnsafePaginationRequest>()).Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _controller.GetPaginated(
                request.PageNumber,
                request.PageSize,
                request.IncludeTotalCount,
                request.SortBy,
                request.SortDirection,
                request.FilterBy,
                request.FilterOperator,
                request.FilterValue
            );

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            var resultData = Assert.IsType<PaginatedResult<{Context.ResultDtoName}>>(okResult.Value);
            Assert.Equal(expectedResult.Data.Items.Count(), resultData.Items.Count());
            Assert.Equal(expectedResult.Data.TotalCount, resultData.TotalCount);
            var receivedCalls = _{Context.PluralizedParamName}Service.ReceivedCalls();
            Assert.NotNull(receivedCalls);
            Assert.Single(receivedCalls);
            var receivedCall = receivedCalls.FirstOrDefault();
            Assert.NotNull(receivedCall);
            var receivedCallArguments = receivedCall.GetArguments();
            Assert.Single(receivedCallArguments);
            var receivedCallArgument = receivedCallArguments.FirstOrDefault();
            Assert.NotNull(receivedCallArgument);
            Assert.Equal(
                JsonSerializer.Serialize(receivedCallArgument),
                JsonSerializer.Serialize(request)
            );
        }}

        [Fact]
        public async Task GetPaginated_ReturnsParametersError()
        {{
            // Arrange
            var request = _fixture.Create<UnsafePaginationRequest>();
            var dtos = _fixture.CreateMany<{Context.ResultDtoName}>(3).ToList();
            var expectedResult = new DataWithOptionalError<PaginatedResult<{Context.ResultDtoName}>>
            {{
                Error = ""Blabla""
            }};
            _{Context.PluralizedParamName}Service.GetPaginated(Arg.Any<UnsafePaginationRequest>()).Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _controller.GetPaginated(
                request.PageNumber,
                request.PageSize,
                request.IncludeTotalCount,
                request.SortBy,
                request.SortDirection,
                request.FilterBy,
                request.FilterOperator,
                request.FilterValue
            );

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal(badRequestResult.Value, expectedResult.Error);
        }}

        [Fact]
        public async Task GetMany_ReturnsMatchingItems()
        {{
            // Arrange
            var expectedResult = _fixture.CreateMany<{Context.ResultDtoName}>(3).ToList();
            _{Context.PluralizedParamName}Service.GetMany(Arg.Any<FindManyArgs<{Context.EntityName}, {Context.QueryDtoName}>>()).Returns(Task.FromResult<IEnumerable<{Context.ResultDtoName}>>([expectedResult.ToArray()[1]]));
            var filter = new FindManyArgs<{Context.EntityName}, {Context.QueryDtoName}>()
            {{
                Where = new {Context.QueryDtoName}
                {{
                    {Context.ArtifactName}Id = expectedResult[1].{Context.ArtifactName}Id
                }}
            }};

            // Act
            var result = await _controller.GetMany(filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            var resultData = Assert.IsAssignableFrom<IEnumerable<{Context.ResultDtoName}>>(okResult.Value);
            Assert.Single(resultData);
            Assert.NotNull(resultData.FirstOrDefault());
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult[1]),
                JsonSerializer.Serialize(resultData.FirstOrDefault()),
                true
            );
            await _{Context.PluralizedParamName}Service.Received(1).GetMany(filter);
        }}

        [Fact]
        public async Task GetMany_ReturnsEmptyList_WhenNoIdsMatch()
        {{
            // Arrange
            _{Context.PluralizedParamName}Service.GetMany(Arg.Any<FindManyArgs<{Context.EntityName}, {Context.QueryDtoName}>>()).Returns(Task.FromResult<IEnumerable<{Context.ResultDtoName}>>([]));
            var filter = new FindManyArgs<{Context.EntityName}, {Context.QueryDtoName}>()
            {{
                Where = new {Context.QueryDtoName}
                {{
                    {Context.ArtifactName}Id = -1
                }}
            }};

            // Act
            var result = await _controller.GetMany(filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            var resultData = Assert.IsAssignableFrom<IEnumerable<{Context.ResultDtoName}>>(okResult.Value);
            Assert.Empty(resultData);
            await _{Context.PluralizedParamName}Service.Received(1).GetMany(filter);
        }}

        [Fact]
        public async Task GetMeta_ReturnsCorrectMetadata()
        {{
            // Arrange
            var count = _fixture.Create<long>();
            _{Context.PluralizedParamName}Service.GetMeta(Arg.Any<FindManyArgs<{Context.EntityName}, {Context.QueryDtoName}>>()).Returns(Task.FromResult(new MetadataDto() {{ Count = count }}));
            var filter = new FindManyArgs<{Context.EntityName}, {Context.QueryDtoName}>()
            {{
                Where = new {Context.QueryDtoName}
                {{
                    {Context.ArtifactName}Id = -1
                }}
            }};

            // Act
            var result = await _controller.GetMeta(filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            var resultData = Assert.IsAssignableFrom<MetadataDto>(okResult.Value);
            Assert.Equal(count, resultData.Count);
            await _{Context.PluralizedParamName}Service.Received(1).GetMeta(filter);
        }}
    }}
}}";
            }
        }
    }
}