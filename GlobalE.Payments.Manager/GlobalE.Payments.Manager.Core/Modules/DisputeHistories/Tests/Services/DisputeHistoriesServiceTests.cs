using NSubstitute;
using Xunit;
using NestNet.Infra.Query;
using NestNet.Infra.Paginatation;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using System.Text.Json;
using GlobalE.Payments.Manager.Core.Modules.DisputeHistories.Dtos;
using GlobalE.Payments.Manager.Core.Modules.DisputeHistories.Services;
using GlobalE.Payments.Manager.Core.Modules.DisputeHistories.Entities;
using GlobalE.Payments.Manager.Core.Data;

namespace GlobalE.Payments.Manager.Core.Modules.DisputeHistories.Tests.Services
{
    public class DisputeHistoriesServiceTests
    {
        private readonly IFixture _fixture;
        private readonly IAppRepository _repository;
        private readonly DisputeHistoriesService _service;

        public DisputeHistoriesServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            _repository = _fixture.Freeze<IAppRepository>();
            _service = new DisputeHistoriesService(_repository);
        }

        [Fact]
        public async Task GetAll_ReturnsAllItems()
        {
            // Arrange
            var srcEntities = _fixture.CreateMany<DisputeHistoryEntity>(3).ToList();
            var expectedResult = _service.ToResultDtos(srcEntities);
            _repository.GetAll<DisputeHistoryEntity>().Returns(Task.FromResult<IEnumerable<DisputeHistoryEntity>>(srcEntities));

            // Act
            var result = await _service.GetAll();

            // Assert
            Assert.Equal(expectedResult.Count(), result.Count());
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult),
                JsonSerializer.Serialize(result));
            await _repository.Received(1).GetAll<DisputeHistoryEntity>();
        }

        [Fact]
        public async Task GetById_ReturnsItem_WhenExists()
        {
            // Arrange
            var id = _fixture.Create<long>();
            var entity = _fixture.Create<DisputeHistoryEntity>();
            var expectedResult = _service.ToResultDto(entity);
            _repository.GetById<DisputeHistoryEntity>(Arg.Any<long>()).Returns(Task.FromResult<DisputeHistoryEntity?>(entity));

            // Act
            var result = await _service.GetById(id);

            // Assert
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult),
                JsonSerializer.Serialize(result));
            await _repository.Received(1).GetById<DisputeHistoryEntity>(id);
        }

        [Fact]
        public async Task GetById_ReturnsNull_WhenNotExists()
        {
            // Arrange
            var id = _fixture.Create<long>();
            _repository.GetById<DisputeHistoryEntity>(Arg.Any<long>()).Returns(Task.FromResult<DisputeHistoryEntity?>(null));

            // Act
            var result = await _service.GetById(id);

            // Assert
            Assert.Null(result);
            await _repository.Received(1).GetById<DisputeHistoryEntity>(id);
        }

        [Fact]
        public async Task Create_ReturnsCreatedItem()
        {
            // Arrange
            var createDto = _fixture.Create<DisputeHistoryCreateDto>();
            var createdEntity = _service.ToEntity(createDto);
            var expectedResult = _service.ToResultDto(createdEntity);
            _repository.Create(Arg.Any<DisputeHistoryEntity>()).Returns(Task.FromResult(createdEntity));

            // Act
            var result = await _service.Create(createDto);

            // Assert
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult),
                JsonSerializer.Serialize(result.ResultDto));
            await _repository.Received(1).Create(Arg.Any<DisputeHistoryEntity>());
        }

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_True_ReturnsUpdatedItem_WhenExists()
        {
            // Arrange
            var ignoreMissingOrNullFields = true;
            var id = _fixture.Create<long>();
            var updateDto = _fixture.Create<DisputeHistoryUpdateDto>();
            var updatedEntity = _service.ToEntity(updateDto);
            var expectedResult = _service.ToResultDto(updatedEntity);
            _repository.Update<DisputeHistoryUpdateDto, DisputeHistoryEntity>(Arg.Any<long>(), Arg.Any<DisputeHistoryUpdateDto>(), Arg.Any<bool>()).Returns(Task.FromResult<DisputeHistoryEntity?>(updatedEntity));

            // Act
            var result = await _service.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult),
                JsonSerializer.Serialize(result));
            await _repository.Received(1).Update<DisputeHistoryUpdateDto, DisputeHistoryEntity>(id, updateDto, ignoreMissingOrNullFields);
        }

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_True_ReturnsNull_WhenNotExists()
        {
            // Arrange
            var ignoreMissingOrNullFields = true;
            var id = _fixture.Create<long>();
            var updateDto = _fixture.Create<DisputeHistoryUpdateDto>();
            _repository.Update<DisputeHistoryUpdateDto, DisputeHistoryEntity>(Arg.Any<long>(), Arg.Any<DisputeHistoryUpdateDto>(), Arg.Any<bool>()).Returns(Task.FromResult<DisputeHistoryEntity?>(null));

            // Act
            var result = await _service.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.Null(result);
            await _repository.Received(1).Update<DisputeHistoryUpdateDto, DisputeHistoryEntity>(id, updateDto, ignoreMissingOrNullFields);
        }

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_False_ReturnsUpdatedItem_WhenExists()
        {
            // Arrange
            var ignoreMissingOrNullFields = false;
            var id = _fixture.Create<long>();
            var updateDto = _fixture.Create<DisputeHistoryUpdateDto>();
            var updatedEntity = _service.ToEntity(updateDto);
            var expectedResult = _service.ToResultDto(updatedEntity);
            _repository.Update<DisputeHistoryUpdateDto, DisputeHistoryEntity>(Arg.Any<long>(), Arg.Any<DisputeHistoryUpdateDto>(), Arg.Any<bool>()).Returns(Task.FromResult<DisputeHistoryEntity?>(updatedEntity));

            // Act
            var result = await _service.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult),
                JsonSerializer.Serialize(result));
            await _repository.Received(1).Update<DisputeHistoryUpdateDto, DisputeHistoryEntity>(id, updateDto, ignoreMissingOrNullFields);
        }

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_False_ReturnsNull_WhenNotExists()
        {
            // Arrange
            var ignoreMissingOrNullFields = false;
            var id = _fixture.Create<long>();
            var updateDto = _fixture.Create<DisputeHistoryUpdateDto>();
            _repository.Update<DisputeHistoryUpdateDto, DisputeHistoryEntity>(Arg.Any<long>(), Arg.Any<DisputeHistoryUpdateDto>(), Arg.Any<bool>()).Returns(Task.FromResult<DisputeHistoryEntity?>(null));

            // Act
            var result = await _service.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.Null(result);
            await _repository.Received(1).Update<DisputeHistoryUpdateDto, DisputeHistoryEntity>(id, updateDto, ignoreMissingOrNullFields);
        }

        [Fact]
        public async Task Delete_ReturnsTrue_WhenExists()
        {
            // Arrange
            var id = _fixture.Create<long>();
            var entity = _fixture.Create<DisputeHistoryEntity>();
            _repository.Delete<DisputeHistoryEntity>(Arg.Any<long>()).Returns(Task.FromResult(true));

            // Act
            var found = await _service.Delete(id);

            // Assert
            Assert.True(found);
            await _repository.Received(1).Delete<DisputeHistoryEntity>(id);
        }

        [Fact]
        public async Task Delete_ReturnsFalse_WhenNotExists()
        {
            // Arrange
            var id = _fixture.Create<long>();
            _repository.Delete<DisputeHistoryEntity>(Arg.Any<long>()).Returns(Task.FromResult(false));

            // Act
            var found = await _service.Delete(id);

            // Assert
            Assert.False(found);
            await _repository.Received(1).Delete<DisputeHistoryEntity>(id);
        }

        [Fact]
        public async Task GetPaginated_ReturnsPaginatedItems()
        {
            // Arrange
            var propertyName = "disputeHistoryId";
            var safeRequest = new SafePaginationRequest()
            {
                SortCriteria = new List<SortCriteria>()
                {
                    new SortCriteria()
                    {
                        PropertyName = propertyName,
                        SortDirection = SortDirection.Asc
                    }
                },
                FilterCriteria = new List<FilterCriteria>()
                {
                    new FilterCriteria()
                    {
                        PropertyName = propertyName,
                        Value = "1",
                        Operator = FilterOperator.NotEquals
                    }
                }
            };

            var srcEntities = _fixture.CreateMany<DisputeHistoryEntity>(3)
              .Select((entity, index) => {
                  entity.DisputeHistoryId = index;
                  return entity;
              })
              .ToList();

            var unsafeRequest = new UnsafePaginationRequest()
            {
                PageNumber = safeRequest.PageNumber,
                PageSize = safeRequest.PageSize,
                IncludeTotalCount = safeRequest.IncludeTotalCount,
                SortBy = safeRequest.SortCriteria.Select(c => c.PropertyName).ToArray(),
                SortDirection = safeRequest.SortCriteria.Select(c => c.SortDirection.ToString()).ToArray(),
                FilterBy = safeRequest.FilterCriteria.Select(f => f.PropertyName).ToArray(),
                FilterOperator = safeRequest.FilterCriteria.Select(f => f.Operator.ToString()).ToArray(),
                FilterValue = safeRequest.FilterCriteria.Select(f => f.Value).ToArray()
            };

            var repositoryResult = new PaginatedResult<DisputeHistoryEntity>
            {
                Items = srcEntities,
                TotalCount = srcEntities.Count()
            };

            var expectedResult = _service.ToPaginatedResultDtos(repositoryResult);
            _repository.GetPaginated<DisputeHistoryEntity>(Arg.Any<SafePaginationRequest>()).Returns(Task.FromResult(repositoryResult));

            // Act
            var result = await _service.GetPaginated(unsafeRequest);

            // Assert
            Assert.NotNull(result.Data);
            Assert.Null(result.Error);
            Assert.Equal(expectedResult.TotalCount, result.Data.TotalCount);
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult.Items),
                JsonSerializer.Serialize(result.Data.Items));
            var receivedCalls = _repository.ReceivedCalls();
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
                JsonSerializer.Serialize(safeRequest),
                true
            );
        }

        [Fact]
        public async Task GetPaginated_IncompleteCriteria_ReturnsParametersError()
        {
            // Arrange
            var propertyName = "DisputeHistoryId";
            var safeRequest = new SafePaginationRequest()
            {
                SortCriteria = new List<SortCriteria>()
                {
                    new SortCriteria()
                    {
                        PropertyName = propertyName,
                        SortDirection = SortDirection.Asc
                    }
                },
                FilterCriteria = new List<FilterCriteria>()
                {
                    new FilterCriteria()
                    {
                        PropertyName = propertyName,
                        Value = "1",
                        Operator = FilterOperator.NotEquals
                    }
                }
            };
            var srcEntities = _fixture.CreateMany<DisputeHistoryEntity>(3).ToList();
            var unsafeRequest = new UnsafePaginationRequest()
            {
                PageNumber = safeRequest.PageNumber,
                PageSize = safeRequest.PageSize,
                IncludeTotalCount = safeRequest.IncludeTotalCount,
                // SortBy = safeRequest.SortCriteria.Select(c => c.PropertyName).ToArray(),
                SortDirection = safeRequest.SortCriteria.Select(c => c.SortDirection.ToString()).ToArray(),
                // FilterBy = safeRequest.FilterCriteria.Select(f => f.PropertyName).ToArray(),
                FilterOperator = safeRequest.FilterCriteria.Select(f => f.Operator.ToString()).ToArray(),
                FilterValue = safeRequest.FilterCriteria.Select(f => f.Value).ToArray()
            };
            var repositoryResult = new PaginatedResult<DisputeHistoryEntity>
            {
                Items = srcEntities,
                TotalCount = srcEntities.Count()
            };
            _repository.GetPaginated<DisputeHistoryEntity>(Arg.Any<SafePaginationRequest>()).Returns(Task.FromResult(repositoryResult));

            // Act
            var result = await _service.GetPaginated(unsafeRequest);

            // Assert
            Assert.Null(result.Data);
            Assert.NotNull(result.Error);
            Assert.Contains("Sort criteria is incomplete", result.Error);
            Assert.Contains("Filter criteria is incomplete", result.Error);
        }

        [Fact]
        public async Task GetPaginated_InvalidEnumValue_ReturnsParametersError()
        {
            // Arrange
            var propertyName = "DisputeHistoryId";
            var safeRequest = new SafePaginationRequest()
            {
                SortCriteria = new List<SortCriteria>()
                {
                    new SortCriteria()
                    {
                        PropertyName = propertyName,
                        SortDirection = SortDirection.Asc
                    }
                },
                FilterCriteria = new List<FilterCriteria>()
                {
                    new FilterCriteria()
                    {
                        PropertyName = propertyName,
                        Value = "1",
                        Operator = FilterOperator.NotEquals
                    }
                }
            };
            var srcEntities = _fixture.CreateMany<DisputeHistoryEntity>(3).ToList();
            var unsafeRequest = new UnsafePaginationRequest()
            {
                PageNumber = safeRequest.PageNumber,
                PageSize = safeRequest.PageSize,
                IncludeTotalCount = safeRequest.IncludeTotalCount,
                SortBy = safeRequest.SortCriteria.Select(c => c.PropertyName).ToArray(),
                SortDirection = safeRequest.SortCriteria.Select(c => c.SortDirection.ToString() + "Blabla").ToArray(),
                FilterBy = safeRequest.FilterCriteria.Select(f => f.PropertyName).ToArray(),
                FilterOperator = safeRequest.FilterCriteria.Select(f => f.Operator.ToString() + "Blabla").ToArray(),
                FilterValue = safeRequest.FilterCriteria.Select(f => f.Value).ToArray()
            };
            var repositoryResult = new PaginatedResult<DisputeHistoryEntity>
            {
                Items = srcEntities,
                TotalCount = srcEntities.Count()
            };
            _repository.GetPaginated<DisputeHistoryEntity>(Arg.Any<SafePaginationRequest>()).Returns(Task.FromResult(repositoryResult));

            // Act
            var result = await _service.GetPaginated(unsafeRequest);

            // Assert
            Assert.Null(result.Data);
            Assert.NotNull(result.Error);
            Assert.Contains("Invalid sort direction", result.Error);
            Assert.Contains("Invalid filter operator", result.Error);
        }

        [Fact]
        public async Task GetPaginated_InvalidPropertyName_ReturnsParametersError()
        {
            // Arrange
            var propertyName = "DisputeHistoryId";
            var safeRequest = new SafePaginationRequest()
            {
                SortCriteria = new List<SortCriteria>()
                {
                    new SortCriteria()
                    {
                        PropertyName = propertyName,
                        SortDirection = SortDirection.Asc
                    }
                },
                FilterCriteria = new List<FilterCriteria>()
                {
                    new FilterCriteria()
                    {
                        PropertyName = propertyName,
                        Value = "1",
                        Operator = FilterOperator.NotEquals
                    }
                }
            };
            var unsafeRequest = new UnsafePaginationRequest()
            {
                PageNumber = safeRequest.PageNumber,
                PageSize = safeRequest.PageSize,
                IncludeTotalCount = safeRequest.IncludeTotalCount,
                SortBy = safeRequest.SortCriteria.Select(c => c.PropertyName + "Blabla").ToArray(),
                SortDirection = safeRequest.SortCriteria.Select(c => c.SortDirection.ToString()).ToArray(),
                FilterBy = safeRequest.FilterCriteria.Select(f => f.PropertyName + "Blabla").ToArray(),
                FilterOperator = safeRequest.FilterCriteria.Select(f => f.Operator.ToString()).ToArray(),
                FilterValue = safeRequest.FilterCriteria.Select(f => f.Value).ToArray()
            };
            var srcEntities = _fixture.CreateMany<DisputeHistoryEntity>(3).ToList();
            var repositoryResult = new PaginatedResult<DisputeHistoryEntity>
            {
                Items = srcEntities,
                TotalCount = srcEntities.Count()
            };
            _repository.GetPaginated<DisputeHistoryEntity>(Arg.Any<SafePaginationRequest>()).Returns(Task.FromResult(repositoryResult));

            // Act
            var result = await _service.GetPaginated(unsafeRequest);

            // Assert
            Assert.Null(result.Data);
            Assert.NotNull(result.Error);
            Assert.Contains("Invalid sort properties", result.Error);
            Assert.Contains("Invalid filter properties", result.Error);
        }

        [Fact]
        public async Task GetMany_ReturnsMatchingItems()
        {
            // Arrange
            var srcEntities = _fixture.CreateMany<DisputeHistoryEntity>(3).ToList();
            var expectedResult = _service.ToResultDtos([srcEntities.ToArray()[1]]);
            _repository.GetMany(Arg.Any<FindManyArgs<DisputeHistoryEntity, DisputeHistoryQueryDto>>()).Returns(Task.FromResult<IEnumerable<DisputeHistoryEntity>>([srcEntities.ToArray()[1]]));
            var filter = new FindManyArgs<DisputeHistoryEntity, DisputeHistoryQueryDto>()
            {
                Where = new DisputeHistoryQueryDto
                {
                    DisputeHistoryId = srcEntities[1].DisputeHistoryId
                }
            };

            // Act
            var result = await _service.GetMany(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.NotNull(result.FirstOrDefault());
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult.FirstOrDefault()),
                JsonSerializer.Serialize(result.FirstOrDefault()),
                true
            );
            await _repository.Received(1).GetMany(filter);
        }

        [Fact]
        public async Task GetMany_ReturnsEmptyList_WhenNoIdsMatch()
        {
            // Arrange
            _repository.GetMany(Arg.Any<FindManyArgs<DisputeHistoryEntity, DisputeHistoryQueryDto>>()).Returns(Task.FromResult<IEnumerable<DisputeHistoryEntity>>([]));
            var filter = new FindManyArgs<DisputeHistoryEntity, DisputeHistoryQueryDto>()
            {
                Where = new DisputeHistoryQueryDto
                {
                    DisputeHistoryId = -1
                }
            };

            // Act
            var result = await _service.GetMany(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            await _repository.Received(1).GetMany(filter);
        }

        [Fact]
        public async Task GetMeta_ReturnsCorrectMetadata()
        {
            // Arrange
            var count = _fixture.Create<long>();
            _repository.GetMeta(Arg.Any<FindManyArgs<DisputeHistoryEntity, DisputeHistoryQueryDto>>()).Returns(Task.FromResult(new MetadataDto() { Count = count }));
            var filter = new FindManyArgs<DisputeHistoryEntity, DisputeHistoryQueryDto>()
            {
                Where = new DisputeHistoryQueryDto
                {
                    DisputeHistoryId = -1
                }
            };

            // Act
            var result = await _service.GetMeta(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(count, result.Count);
            await _repository.Received(1).GetMeta(filter);
        }
    }
}