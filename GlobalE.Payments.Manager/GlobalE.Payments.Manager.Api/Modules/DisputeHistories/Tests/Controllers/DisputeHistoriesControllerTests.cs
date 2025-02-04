using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using NestNet.Infra.Query;
using NestNet.Infra.BaseClasses;
using NestNet.Infra.Paginatation;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using System.Text.Json;
using GlobalE.Payments.Manager.Api.Modules.DisputeHistories.Controllers;
using GlobalE.Payments.Manager.Core.Modules.DisputeHistories.Dtos;
using GlobalE.Payments.Manager.Core.Modules.DisputeHistories.Services;
using GlobalE.Payments.Manager.Core.Modules.DisputeHistories.Entities;

namespace GlobalE.Payments.Manager.Api.Modules.DisputeHistories.Tests.Controllers
{
    public class DisputeHistoriesControllerTests
    {
        private readonly IFixture _fixture;
        private readonly IDisputeHistoriesService _disputeHistoriesService;
        private readonly DisputeHistoriesController _controller;

        public DisputeHistoriesControllerTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            _disputeHistoriesService = _fixture.Freeze<IDisputeHistoriesService>();
            _controller = new DisputeHistoriesController(_disputeHistoriesService);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithAllItems()
        {
            // Arrange
            var expectedResult = _fixture.CreateMany<DisputeHistoryResultDto>(2).ToList();
            _disputeHistoriesService.GetAll().Returns(Task.FromResult<IEnumerable<DisputeHistoryResultDto>>(expectedResult));

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            var resultData = Assert.IsAssignableFrom<IEnumerable<DisputeHistoryResultDto>>(okResult.Value);
            Assert.Equal(expectedResult.Count, resultData.Count());
            await _disputeHistoriesService.Received(1).GetAll();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult_WhenItemExists()
        {
            // Arrange
            var id = _fixture.Create<long>();
            var expectedResult = _fixture.Create<DisputeHistoryResultDto>();
            _disputeHistoriesService.GetById(Arg.Any<long>()).Returns(Task.FromResult<DisputeHistoryResultDto?>(expectedResult));

            // Act
            var result = await _controller.GetById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            var resultData = Assert.IsType<DisputeHistoryResultDto>(okResult.Value);
            Assert.Equal(expectedResult, resultData);
            await _disputeHistoriesService.Received(1).GetById(id);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenItemDoesNotExist()
        {
            // Arrange
            var id = _fixture.Create<long>();
            _disputeHistoriesService.GetById(Arg.Any<long>()).Returns(Task.FromResult<DisputeHistoryResultDto?>(null));

            // Act
            var result = await _controller.GetById(id);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Value);
            var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
            Assert.NotNull(notFoundResult);
            Assert.Equal(404, notFoundResult.StatusCode);
            await _disputeHistoriesService.Received(1).GetById(id);
        }

        [Fact]
        public async Task Create_ReturnsCreatedResult_WithNewItem()
        {
            // Arrange
            var id = _fixture.Create<long>();
            var createDto = _fixture.Create<DisputeHistoryCreateDto>();
            var expectedResult = _fixture.Create<DisputeHistoryResultDto>();
            var internalCreateResult = new InternalCreateResult<DisputeHistoryResultDto>
            {
                Id = id,
                ResultDto = expectedResult
            };
            _disputeHistoriesService.Create(Arg.Any<DisputeHistoryCreateDto>()).Returns(Task.FromResult(internalCreateResult));

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.NotNull(createdResult);
            Assert.Equal(201, createdResult.StatusCode);
            var resultData = Assert.IsType<DisputeHistoryResultDto>(createdResult.Value);
            Assert.Equal(expectedResult, resultData);
            await _disputeHistoriesService.Received(1).Create(createDto);
        }

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_True_ReturnsOkResult_WhenUpdateSuccessful()
        {
            // Arrange
            var ignoreMissingOrNullFields = true;
            var id = _fixture.Create<long>();
            var updateDto = _fixture.Create<DisputeHistoryUpdateDto>();
            var expectedResult = _fixture.Create<DisputeHistoryResultDto>();
            _disputeHistoriesService.Update(Arg.Any<long>(), Arg.Any<DisputeHistoryUpdateDto>(), Arg.Any<bool>()).Returns(Task.FromResult<DisputeHistoryResultDto?>(expectedResult));

            // Act
            var result = await _controller.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            var resultData = Assert.IsType<DisputeHistoryResultDto>(okResult.Value);
            Assert.Equal(expectedResult, resultData);
            await _disputeHistoriesService.Received(1).Update(id, updateDto, ignoreMissingOrNullFields);
        }

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_True_ReturnsNotFound_WhenItemDoesNotExist()
        {
            // Arrange
            var ignoreMissingOrNullFields = true;
            var id = _fixture.Create<long>();
            var updateDto = _fixture.Create<DisputeHistoryUpdateDto>();
            _disputeHistoriesService.Update(Arg.Any<long>(), Arg.Any<DisputeHistoryUpdateDto>(), Arg.Any<bool>()).Returns(Task.FromResult<DisputeHistoryResultDto?>(null));

            // Act
            var result = await _controller.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Value);
            var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
            Assert.NotNull(notFoundResult);
            Assert.Equal(404, notFoundResult.StatusCode);
            await _disputeHistoriesService.Received(1).Update(id, updateDto, ignoreMissingOrNullFields);
        }

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_False_ReturnsOkResult_WhenUpdateSuccessful()
        {
            // Arrange
            var ignoreMissingOrNullFields = false;
            var id = _fixture.Create<long>();
            var updateDto = _fixture.Create<DisputeHistoryUpdateDto>();
            var expectedResult = _fixture.Create<DisputeHistoryResultDto>();
            _disputeHistoriesService.Update(Arg.Any<long>(), Arg.Any<DisputeHistoryUpdateDto>(), Arg.Any<bool>()).Returns(Task.FromResult<DisputeHistoryResultDto?>(expectedResult));

            // Act
            var result = await _controller.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            var resultData = Assert.IsType<DisputeHistoryResultDto>(okResult.Value);
            Assert.Equal(expectedResult, resultData);
            await _disputeHistoriesService.Received(1).Update(id, updateDto, ignoreMissingOrNullFields);
        }

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_False_ReturnsNotFound_WhenItemDoesNotExist()
        {
            // Arrange
            var ignoreMissingOrNullFields = false;
            var id = _fixture.Create<long>();
            var updateDto = _fixture.Create<DisputeHistoryUpdateDto>();
            _disputeHistoriesService.Update(Arg.Any<long>(), Arg.Any<DisputeHistoryUpdateDto>(), Arg.Any<bool>()).Returns(Task.FromResult<DisputeHistoryResultDto?>(null));

            // Act
            var result = await _controller.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Value);
            var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
            Assert.NotNull(notFoundResult);
            Assert.Equal(404, notFoundResult.StatusCode);
            await _disputeHistoriesService.Received(1).Update(id, updateDto, ignoreMissingOrNullFields);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenDeleteSuccessful()
        {
            // Arrange
            var id = _fixture.Create<long>();
            _disputeHistoriesService.Delete(Arg.Any<long>()).Returns(Task.FromResult(true));

            // Act
            var result = await _controller.Delete(id);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.NotNull(noContentResult);
            Assert.Equal(204, noContentResult.StatusCode);
            await _disputeHistoriesService.Received(1).Delete(id);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenItemDoesNotExist()
        {
            // Arrange
            var id = _fixture.Create<long>();
            _disputeHistoriesService.Delete(Arg.Any<long>()).Returns(Task.FromResult(false));

            // Act
            var result = await _controller.Delete(id);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
            Assert.NotNull(notFoundResult);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.IsType<NotFoundResult>(result);
            await _disputeHistoriesService.Received(1).Delete(id);
        }

        [Fact]
        public async Task GetPaginated_ReturnsOkResult_WithPaginatedItems()
        {
            // Arrange
            var request = _fixture.Create<UnsafePaginationRequest>();
            var dtos = _fixture.CreateMany<DisputeHistoryResultDto>(3).ToList();
            var expectedResult = new DataWithOptionalError<PaginatedResult<DisputeHistoryResultDto>>
            {
                Data = new PaginatedResult<DisputeHistoryResultDto>()
                {
                    Items = dtos,
                    TotalCount = dtos.Count()
                }
            };
            _disputeHistoriesService.GetPaginated(Arg.Any<UnsafePaginationRequest>()).Returns(Task.FromResult(expectedResult));

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
            var resultData = Assert.IsType<PaginatedResult<DisputeHistoryResultDto>>(okResult.Value);
            Assert.Equal(expectedResult.Data.Items.Count(), resultData.Items.Count());
            Assert.Equal(expectedResult.Data.TotalCount, resultData.TotalCount);
            var receivedCalls = _disputeHistoriesService.ReceivedCalls();
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
            var dtos = _fixture.CreateMany<DisputeHistoryResultDto>(3).ToList();
            var expectedResult = new DataWithOptionalError<PaginatedResult<DisputeHistoryResultDto>>
            {
                Error = "Blabla"
            };
            _disputeHistoriesService.GetPaginated(Arg.Any<UnsafePaginationRequest>()).Returns(Task.FromResult(expectedResult));

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
            var expectedResult = _fixture.CreateMany<DisputeHistoryResultDto>(3).ToList();
            _disputeHistoriesService.GetMany(Arg.Any<FindManyArgs<DisputeHistoryEntity, DisputeHistoryQueryDto>>()).Returns(Task.FromResult<IEnumerable<DisputeHistoryResultDto>>([expectedResult.ToArray()[1]]));
            var filter = new FindManyArgs<DisputeHistoryEntity, DisputeHistoryQueryDto>()
            {
                Where = new DisputeHistoryQueryDto
                {
                    DisputeHistoryId = expectedResult[1].DisputeHistoryId
                }
            };

            // Act
            var result = await _controller.GetMany(filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            var resultData = Assert.IsAssignableFrom<IEnumerable<DisputeHistoryResultDto>>(okResult.Value);
            Assert.Single(resultData);
            Assert.NotNull(resultData.FirstOrDefault());
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult[1]),
                JsonSerializer.Serialize(resultData.FirstOrDefault()),
                true
            );
            await _disputeHistoriesService.Received(1).GetMany(filter);
        }

        [Fact]
        public async Task GetMany_ReturnsEmptyList_WhenNoIdsMatch()
        {
            // Arrange
            _disputeHistoriesService.GetMany(Arg.Any<FindManyArgs<DisputeHistoryEntity, DisputeHistoryQueryDto>>()).Returns(Task.FromResult<IEnumerable<DisputeHistoryResultDto>>([]));
            var filter = new FindManyArgs<DisputeHistoryEntity, DisputeHistoryQueryDto>()
            {
                Where = new DisputeHistoryQueryDto
                {
                    DisputeHistoryId = -1
                }
            };

            // Act
            var result = await _controller.GetMany(filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            var resultData = Assert.IsAssignableFrom<IEnumerable<DisputeHistoryResultDto>>(okResult.Value);
            Assert.Empty(resultData);
            await _disputeHistoriesService.Received(1).GetMany(filter);
        }

        [Fact]
        public async Task GetMeta_ReturnsCorrectMetadata()
        {
            // Arrange
            var count = _fixture.Create<long>();
            _disputeHistoriesService.GetMeta(Arg.Any<FindManyArgs<DisputeHistoryEntity, DisputeHistoryQueryDto>>()).Returns(Task.FromResult(new MetadataDto() { Count = count }));
            var filter = new FindManyArgs<DisputeHistoryEntity, DisputeHistoryQueryDto>()
            {
                Where = new DisputeHistoryQueryDto
                {
                    DisputeHistoryId = -1
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
            await _disputeHistoriesService.Received(1).GetMeta(filter);
        }
    }
}