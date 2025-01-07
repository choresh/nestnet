using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using NestNet.Infra.Query;
using NestNet.Infra.BaseClasses;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using System.Text.Json;
using SampleApp.Modules.MyModules.Services;
using SampleApp.Modules.MyModules.Controllers;
using SampleApp.Modules.MyModules.Dtos;

namespace SampleApp.Modules.MyModules.Tests.Controllers
{
    public class MyModulesControllerTests
    {
        private readonly IFixture _fixture;
        private readonly IMyModulesService _myModulesService;
        private readonly MyModulesController _controller;

        public MyModulesControllerTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            _myModulesService = _fixture.Freeze<IMyModulesService>();
            _controller = new MyModulesController(_myModulesService);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithAllItems()
        {
            // Arrange
            var expectedResult = _fixture.CreateMany<MyModuleResultDto>(2).ToList();
            _myModulesService.GetAll().Returns(Task.FromResult<IEnumerable<MyModuleResultDto>>(expectedResult));

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            var resultData = Assert.IsAssignableFrom<IEnumerable<MyModuleResultDto>>(okResult.Value);
            Assert.Equal(expectedResult.Count, resultData.Count());
            await _myModulesService.Received(1).GetAll();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult_WhenItemExists()
        {
            // Arrange
            var id = _fixture.Create<int>();
            var expectedResult = _fixture.Create<MyModuleResultDto>();
            _myModulesService.GetById(Arg.Any<int>()).Returns(Task.FromResult<MyModuleResultDto?>(expectedResult));

            // Act
            var result = await _controller.GetById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            var resultData = Assert.IsType<MyModuleResultDto>(okResult.Value);
            Assert.Equal(expectedResult, resultData);
            await _myModulesService.Received(1).GetById(id);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenItemDoesNotExist()
        {
            // Arrange
            var id = _fixture.Create<int>();
            _myModulesService.GetById(Arg.Any<int>()).Returns(Task.FromResult<MyModuleResultDto?>(null));

            // Act
            var result = await _controller.GetById(id);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Value);
            var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
            Assert.NotNull(notFoundResult);
            Assert.Equal(404, notFoundResult.StatusCode);
            await _myModulesService.Received(1).GetById(id);
        }

        [Fact]
        public async Task Create_ReturnsCreatedResult_WithNewItem()
        {
            // Arrange
            var id = _fixture.Create<int>();
            var createDto = _fixture.Create<MyModuleCreateDto>();
            var expectedResult = _fixture.Create<MyModuleResultDto>();
            var internalCreateResult = new InternalCreateResult<MyModuleResultDto>
            {
                Id = id,
                ResultDto = expectedResult
            };
            _myModulesService.Create(Arg.Any<MyModuleCreateDto>()).Returns(Task.FromResult(internalCreateResult));

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.NotNull(createdResult);
            Assert.Equal(201, createdResult.StatusCode);
            var resultData = Assert.IsType<MyModuleResultDto>(createdResult.Value);
            Assert.Equal(expectedResult, resultData);
            await _myModulesService.Received(1).Create(createDto);
        }

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_True_ReturnsOkResult_WhenUpdateSuccessful()
        {
            // Arrange
            var ignoreMissingOrNullFields = true;
            var id = _fixture.Create<int>();
            var updateDto = _fixture.Create<MyModuleUpdateDto>();
            var expectedResult = _fixture.Create<MyModuleResultDto>();
            _myModulesService.Update(Arg.Any<int>(), Arg.Any<MyModuleUpdateDto>(), Arg.Any<bool>()).Returns(Task.FromResult<MyModuleResultDto?>(expectedResult));

            // Act
            var result = await _controller.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            var resultData = Assert.IsType<MyModuleResultDto>(okResult.Value);
            Assert.Equal(expectedResult, resultData);
            await _myModulesService.Received(1).Update(id, updateDto, ignoreMissingOrNullFields);
        }

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_True_ReturnsNotFound_WhenItemDoesNotExist()
        {
            // Arrange
            var ignoreMissingOrNullFields = true;
            var id = _fixture.Create<int>();
            var updateDto = _fixture.Create<MyModuleUpdateDto>();
            _myModulesService.Update(Arg.Any<int>(), Arg.Any<MyModuleUpdateDto>(), Arg.Any<bool>()).Returns(Task.FromResult<MyModuleResultDto?>(null));
         
            // Act
            var result = await _controller.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Value);
            var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
            Assert.NotNull(notFoundResult);
            Assert.Equal(404, notFoundResult.StatusCode);
            await _myModulesService.Received(1).Update(id, updateDto, ignoreMissingOrNullFields);
        }

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_False_ReturnsOkResult_WhenUpdateSuccessful()
        {
            // Arrange
            var ignoreMissingOrNullFields = false;
            var id = _fixture.Create<int>();
            var updateDto = _fixture.Create<MyModuleUpdateDto>();
            var expectedResult = _fixture.Create<MyModuleResultDto>();
            _myModulesService.Update(Arg.Any<int>(), Arg.Any<MyModuleUpdateDto>(), Arg.Any<bool>()).Returns(Task.FromResult<MyModuleResultDto?>(expectedResult));

            // Act
            var result = await _controller.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            var resultData = Assert.IsType<MyModuleResultDto>(okResult.Value);
            Assert.Equal(expectedResult, resultData);
            await _myModulesService.Received(1).Update(id, updateDto, ignoreMissingOrNullFields);
        }

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_False_ReturnsNotFound_WhenItemDoesNotExist()
        {
            // Arrange
            var ignoreMissingOrNullFields = false;
            var id = _fixture.Create<int>();
            var updateDto = _fixture.Create<MyModuleUpdateDto>();
            _myModulesService.Update(Arg.Any<int>(), Arg.Any<MyModuleUpdateDto>(), Arg.Any<bool>()).Returns(Task.FromResult<MyModuleResultDto?>(null));
         
            // Act
            var result = await _controller.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Value);
            var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
            Assert.NotNull(notFoundResult);
            Assert.Equal(404, notFoundResult.StatusCode);
            await _myModulesService.Received(1).Update(id, updateDto, ignoreMissingOrNullFields);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenDeleteSuccessful()
        {
            // Arrange
            var id = _fixture.Create<int>();
            _myModulesService.Delete(Arg.Any<int>()).Returns(Task.FromResult(true));

            // Act
            var result = await _controller.Delete(id);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.NotNull(noContentResult);
            Assert.Equal(204, noContentResult.StatusCode);
            await _myModulesService.Received(1).Delete(id);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenItemDoesNotExist()
        {
            // Arrange
            var id = _fixture.Create<int>();
            _myModulesService.Delete(Arg.Any<int>()).Returns(Task.FromResult(false));

            // Act
            var result = await _controller.Delete(id);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
            Assert.NotNull(notFoundResult);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.IsType<NotFoundResult>(result);
            await _myModulesService.Received(1).Delete(id);
        }

        [Fact]
        public async Task GetPaginated_ReturnsOkResult_WithPaginatedItems()
        {
            // Arrange
            var request = _fixture.Create<UnsafePaginationRequest>();
            var dtos = _fixture.CreateMany<MyModuleResultDto>(3).ToList();
            var expectedResult = new DataWithOptionalError<PaginatedResult<MyModuleResultDto>>
            {
                Data = new PaginatedResult<MyModuleResultDto>()
                {
                    Items = dtos,
                    TotalCount = dtos.Count()
                }
            };
            _myModulesService.GetPaginated(Arg.Any<UnsafePaginationRequest>()).Returns(Task.FromResult(expectedResult));

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
            var resultData = Assert.IsType<PaginatedResult<MyModuleResultDto>>(okResult.Value);
            Assert.Equal(expectedResult.Data.Items.Count(), resultData.Items.Count());
            Assert.Equal(expectedResult.Data.TotalCount, resultData.TotalCount);
            var receivedCalls = _myModulesService.ReceivedCalls();
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
        }

        [Fact]
        public async Task GetPaginated_ReturnsParametersError()
        {
            // Arrange
            var request = _fixture.Create<UnsafePaginationRequest>();
            var dtos = _fixture.CreateMany<MyModuleResultDto>(3).ToList();
            var expectedResult = new DataWithOptionalError<PaginatedResult<MyModuleResultDto>>
            {
                Error = "Blabla"
            };
            _myModulesService.GetPaginated(Arg.Any<UnsafePaginationRequest>()).Returns(Task.FromResult(expectedResult));

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
        }

        [Fact]
        public async Task GetMany_ReturnsMatchingItems()
        {
            // Arrange
            var expectedResult = _fixture.CreateMany<MyModuleResultDto>(3).ToList();
            _myModulesService.GetMany(Arg.Any<FindManyArgs<Entities.MyModule, MyModuleQueryDto>>()).Returns(Task.FromResult<IEnumerable<MyModuleResultDto>>([expectedResult.ToArray()[1]]));
             var filter = new FindManyArgs<Entities.MyModule, MyModuleQueryDto>()
            {
                Where = new MyModuleQueryDto
                {
                    MyModuleId = expectedResult[1].MyModuleId
                }
            };

            // Act
            var result = await _controller.GetMany(filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            var resultData = Assert.IsAssignableFrom<IEnumerable<MyModuleResultDto>>(okResult.Value);
            Assert.Single(resultData);
            Assert.NotNull(resultData.FirstOrDefault());
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult[1]),
                JsonSerializer.Serialize(resultData.FirstOrDefault()),
                true
            );
            await _myModulesService.Received(1).GetMany(filter);
        }

        [Fact]
        public async Task GetMany_ReturnsEmptyList_WhenNoIdsMatch()
        {
            // Arrange
            _myModulesService.GetMany(Arg.Any<FindManyArgs<Entities.MyModule, MyModuleQueryDto>>()).Returns(Task.FromResult<IEnumerable<MyModuleResultDto>>([]));
        
            var filter = new FindManyArgs<Entities.MyModule, MyModuleQueryDto>()
            {
                Where = new MyModuleQueryDto
                {
                    MyModuleId = -1
                }
            };

            // Act
            var result = await _controller.GetMany(filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            var resultData = Assert.IsAssignableFrom<IEnumerable<MyModuleResultDto>>(okResult.Value);
            Assert.Empty(resultData);
            await _myModulesService.Received(1).GetMany(filter);
        }

        [Fact]
        public async Task GetMeta_ReturnsCorrectMetadata()
        {
            // Arrange
            var count = _fixture.Create<int>();
            _myModulesService.GetMeta(Arg.Any<FindManyArgs<Entities.MyModule, MyModuleQueryDto>>()).Returns(Task.FromResult(new MetadataDto() { Count = count }));
            var filter = new FindManyArgs<Entities.MyModule, MyModuleQueryDto>()
            {
                Where = new MyModuleQueryDto
                {
                    MyModuleId = -1
                }
            };

            // Act
            var result = await _controller.GetMeta(filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            var resultData = Assert.IsAssignableFrom<MetadataDto>(okResult.Value);
            Assert.Equal(count, resultData.Count);
            await _myModulesService.Received(1).GetMeta(filter);
        }
    }
}