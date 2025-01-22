using NSubstitute;
using Xunit;
using NestNet.Infra.Query;
using NestNet.Infra.BaseClasses;
using NestNet.Infra.Paginatation;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using System.Text.Json;
using SampleApp.Modules.MyModules.Services;
using SampleApp.Modules.MyModules.Dtos;
using SampleApp.Modules.MyModules.Daos;

namespace SampleApp.Modules.MyModules.Tests.Services
{
    public class MyModulesServiceTests
    {
        private readonly IFixture _fixture;
        private readonly IMyModuleDao _myModuleDao;
        private readonly MyModulesService _service;

        public MyModulesServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            _myModuleDao = _fixture.Freeze<IMyModuleDao>();
            _service = new MyModulesService(_myModuleDao);
        }

        [Fact]
        public async Task GetAll_ReturnsAllItems()
        {
            // Arrange
            var srcEntities = _fixture.CreateMany<Entities.MyModule>(3).ToList();
            var expectedResult = _service.ToResultDtos(srcEntities);
            _myModuleDao.GetAll().Returns(Task.FromResult<IEnumerable<Entities.MyModule>>(srcEntities));

            // Act
            var result = await _service.GetAll();
  
            // Assert
            Assert.Equal(expectedResult.Count(), result.Count());
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult),
                JsonSerializer.Serialize(result));
            await _myModuleDao.Received(1).GetAll();
        }

        [Fact]
        public async Task GetById_ReturnsItem_WhenExists()
        {
            // Arrange
            var id = _fixture.Create<long>();
            var entity = _fixture.Create<Entities.MyModule>();
            var expectedResult = _service.ToResultDto(entity);
            _myModuleDao.GetById(Arg.Any<long>()).Returns(Task.FromResult<Entities.MyModule?>(entity));

            // Act
            var result = await _service.GetById(id);

            // Assert
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult),
                JsonSerializer.Serialize(result));
            await _myModuleDao.Received(1).GetById(id);
        }

        [Fact]
        public async Task GetById_ReturnsNull_WhenNotExists()
        {
            // Arrange
            var id = _fixture.Create<long>();
            _myModuleDao.GetById(Arg.Any<long>()).Returns(Task.FromResult<Entities.MyModule?>(null));

            // Act
            var result = await _service.GetById(id);

            // Assert
            Assert.Null(result);
            await _myModuleDao.Received(1).GetById(id);
        }

        [Fact]
        public async Task Create_ReturnsCreatedItem()
        {
            // Arrange
            var createDto = _fixture.Create<MyModuleCreateDto>();
            var createdEntity = _service.ToEntity(createDto);
            var expectedResult = _service.ToResultDto(createdEntity);
            _myModuleDao.Create(Arg.Any<Entities.MyModule>()).Returns(Task.FromResult(createdEntity));

            // Act
            var result = await _service.Create(createDto);

            // Assert
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult),
                JsonSerializer.Serialize(result.ResultDto));
            await _myModuleDao.Received(1).Create(Arg.Any<Entities.MyModule>());
        }

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_True_ReturnsUpdatedItem_WhenExists()
        {
            // Arrange
            var ignoreMissingOrNullFields = true;
            var id = _fixture.Create<long>();
            var updateDto = _fixture.Create<MyModuleUpdateDto>();
            var updatedEntity = _service.ToEntity(updateDto);
            var expectedResult = _service.ToResultDto(updatedEntity);           
            _myModuleDao.Update(Arg.Any<long>(), Arg.Any<MyModuleUpdateDto>(), Arg.Any<bool>()).Returns(Task.FromResult<Entities.MyModule?>(updatedEntity));
            
            // Act
            var result = await _service.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult),
                JsonSerializer.Serialize(result));
            await _myModuleDao.Received(1).Update(id, updateDto, ignoreMissingOrNullFields);
        }

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_True_ReturnsNull_WhenNotExists()
        {
            // Arrange
            var ignoreMissingOrNullFields = true;
            var id = _fixture.Create<long>();
            var updateDto = _fixture.Create<MyModuleUpdateDto>();
            _myModuleDao.Update(Arg.Any<long>(), Arg.Any<MyModuleUpdateDto>(), Arg.Any<bool>()).Returns(Task.FromResult<Entities.MyModule?>(null));

            // Act
            var result = await _service.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.Null(result);
            await _myModuleDao.Received(1).Update(id, updateDto, ignoreMissingOrNullFields);
        }

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_False_ReturnsUpdatedItem_WhenExists()
        {
            // Arrange
            var ignoreMissingOrNullFields = false;
            var id = _fixture.Create<long>();
            var updateDto = _fixture.Create<MyModuleUpdateDto>();
            var updatedEntity = _service.ToEntity(updateDto);
            var expectedResult = _service.ToResultDto(updatedEntity);           
            _myModuleDao.Update(Arg.Any<long>(), Arg.Any<MyModuleUpdateDto>(), Arg.Any<bool>()).Returns(Task.FromResult<Entities.MyModule?>(updatedEntity));
            
            // Act
            var result = await _service.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult),
                JsonSerializer.Serialize(result));
            await _myModuleDao.Received(1).Update(id, updateDto, ignoreMissingOrNullFields);
        }

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_False_ReturnsNull_WhenNotExists()
        {
            // Arrange
            var ignoreMissingOrNullFields = false;
            var id = _fixture.Create<long>();
            var updateDto = _fixture.Create<MyModuleUpdateDto>();
            _myModuleDao.Update(Arg.Any<long>(), Arg.Any<MyModuleUpdateDto>(), Arg.Any<bool>()).Returns(Task.FromResult<Entities.MyModule?>(null));

            // Act
            var result = await _service.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.Null(result);
            await _myModuleDao.Received(1).Update(id, updateDto, ignoreMissingOrNullFields);
        }

        [Fact]
        public async Task Delete_ReturnsTrue_WhenExists()
        {
            // Arrange
            var id = _fixture.Create<long>();
            var entity = _fixture.Create<Entities.MyModule>();
            _myModuleDao.Delete(Arg.Any<long>()).Returns(Task.FromResult(true));

            // Act
            var found = await _service.Delete(id);

            // Assert
            Assert.True(found);
            await _myModuleDao.Received(1).Delete(id);
        }

        [Fact]
        public async Task Delete_ReturnsFalse_WhenNotExists()
        {
            // Arrange
            var id = _fixture.Create<long>();
            _myModuleDao.Delete(Arg.Any<long>()).Returns(Task.FromResult(false));

            // Act
             var found = await _service.Delete(id);

            // Assert
            Assert.False(found);
            await _myModuleDao.Received(1).Delete(id);
        }

        [Fact]
        public async Task GetPaginated_ReturnsPaginatedItems()
        {
            // Arrange
            var propertyName = "myModuleId";
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

            var srcEntities = _fixture.CreateMany<Entities.MyModule>(3)
              .Select((entity, index) => {
                  entity.MyModuleId = index;
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

            var daoResult = new PaginatedResult<Entities.MyModule>
            {
                Items = srcEntities,
                TotalCount = srcEntities.Count()
            };
          
            var expectedResult = _service.ToPaginatedResultDtos(daoResult);
            _myModuleDao.GetPaginated(Arg.Any<SafePaginationRequest>()).Returns(Task.FromResult(daoResult));

            // Act
            var result = await _service.GetPaginated(unsafeRequest);

            // Assert
            Assert.NotNull(result.Data);
            Assert.Null(result.Error);
            Assert.Equal(expectedResult.TotalCount, result.Data.TotalCount);
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult.Items),
                JsonSerializer.Serialize(result.Data.Items));
            var receivedCalls = _myModuleDao.ReceivedCalls();
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
            var propertyName = "MyModuleId";
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
            var srcEntities = _fixture.CreateMany<Entities.MyModule>(3).ToList();
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
            var daoResult = new PaginatedResult<Entities.MyModule>
            {
                Items = srcEntities,
                TotalCount = srcEntities.Count()
            };
            _myModuleDao.GetPaginated(Arg.Any<SafePaginationRequest>()).Returns(Task.FromResult(daoResult));

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
            var propertyName = "MyModuleId";
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
            var srcEntities = _fixture.CreateMany<Entities.MyModule>(3).ToList();
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
            var daoResult = new PaginatedResult<Entities.MyModule>
            {
                Items = srcEntities,
                TotalCount = srcEntities.Count()
            };
            _myModuleDao.GetPaginated(Arg.Any<SafePaginationRequest>()).Returns(Task.FromResult(daoResult));

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
            var propertyName = "MyModuleId";
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
            var srcEntities = _fixture.CreateMany<Entities.MyModule>(3).ToList();
            var daoResult = new PaginatedResult<Entities.MyModule>
            {
                Items = srcEntities,
                TotalCount = srcEntities.Count()
            };  
            _myModuleDao.GetPaginated(Arg.Any<SafePaginationRequest>()).Returns(Task.FromResult(daoResult));

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
            var srcEntities = _fixture.CreateMany<Entities.MyModule>(3).ToList();
            var expectedResult = _service.ToResultDtos([srcEntities.ToArray()[1]]);
            _myModuleDao.GetMany(Arg.Any<FindManyArgs<Entities.MyModule, MyModuleQueryDto>>()).Returns(Task.FromResult<IEnumerable<Entities.MyModule>>([srcEntities.ToArray()[1]]));
            var filter = new FindManyArgs<Entities.MyModule, MyModuleQueryDto>()
            {
                Where = new MyModuleQueryDto
                {
                    MyModuleId = srcEntities[1].MyModuleId
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
            await _myModuleDao.Received(1).GetMany(filter);
        }

        [Fact]
        public async Task GetMany_ReturnsEmptyList_WhenNoIdsMatch()
        {
            // Arrange
            _myModuleDao.GetMany(Arg.Any<FindManyArgs<Entities.MyModule, MyModuleQueryDto>>()).Returns(Task.FromResult<IEnumerable<Entities.MyModule>>([]));
            var filter = new FindManyArgs<Entities.MyModule, MyModuleQueryDto>()
            {
                Where = new MyModuleQueryDto
                {
                    MyModuleId = -1
                }
            };

            // Act
            var result = await _service.GetMany(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            await _myModuleDao.Received(1).GetMany(filter);
        }

        [Fact]
        public async Task GetMeta_ReturnsCorrectMetadata()
        {
            // Arrange
            var count = _fixture.Create<long>();
            _myModuleDao.GetMeta(Arg.Any<FindManyArgs<Entities.MyModule, MyModuleQueryDto>>()).Returns(Task.FromResult(new MetadataDto() { Count = count}));
            var filter = new FindManyArgs<Entities.MyModule, MyModuleQueryDto>()
            {
                Where = new MyModuleQueryDto
                {
                    MyModuleId = -1
                }
            };

            // Act
            var result = await _service.GetMeta(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(count, result.Count);
            await _myModuleDao.Received(1).GetMeta(filter);
        }
    }
}