using NSubstitute;
using Xunit;
using NestNet.Infra.Query;
using NestNet.Infra.BaseClasses;
using NestNet.Infra.Paginatation;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using System.Text.Json;
using SampleApp.Core.Modules.MyModules.Services;
using SampleApp.Core.Modules.MyModules.Dtos;
using SampleApp.Core.Modules.MyModules.Daos;

namespace SampleApp.Core.Modules.MyModules.Tests.Services
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
            var srcEntities = _fixture.CreateMany<MyModuleEntity>(3).ToList();
            var expectedResult = _service.ToResultDtos(srcEntities);
            _myModuleDao.GetAll().Returns(Task.FromResult<IEnumerable<MyModuleEntity>>(srcEntities));

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
            var entity = _fixture.Create<MyModuleEntity>();
            var expectedResult = _service.ToResultDto(entity);
            _myModuleDao.GetById(Arg.Any<long>()).Returns(Task.FromResult<MyModuleEntity?>(entity));

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
            _myModuleDao.GetById(Arg.Any<long>()).Returns(Task.FromResult<MyModuleEntity?>(null));

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
            _myModuleDao.Create(Arg.Any<MyModuleEntity>()).Returns(Task.FromResult(createdEntity));

            // Act
            var result = await _service.Create(createDto);

            // Assert
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult),
                JsonSerializer.Serialize(result.ResultDto));
            await _myModuleDao.Received(1).Create(Arg.Any<MyModuleEntity>());
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
            _myModuleDao.Update(Arg.Any<long>(), Arg.Any<MyModuleUpdateDto>(), Arg.Any<bool>()).Returns(Task.FromResult<MyModuleEntity?>(updatedEntity));
            
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
            _myModuleDao.Update(Arg.Any<long>(), Arg.Any<MyModuleUpdateDto>(), Arg.Any<bool>()).Returns(Task.FromResult<MyModuleEntity?>(null));

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
            var srcEntities = _fixture.CreateMany<MyModuleEntity>(3).ToList();
            srcEntities.ForEach(async (entity) => await _myModuleDao.Create(entity));
            var updateDto = _fixture.Create<MyModuleUpdateDto>();

            // Act
            var result = await _service.Update(srcEntities[1].MyModuleId, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.NotNull(result);
            TestsHelper.IsValuesExists(updateDto, result, Assert.Equal);
        }

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_False_ReturnsNull_WhenNotExists()
        {
            // Arrange
            var ignoreMissingOrNullFields = false;
            var id = _fixture.Create<long>();
            var updateDto = _fixture.Create<MyModuleUpdateDto>();
            _myModuleDao.Update(Arg.Any<long>(), Arg.Any<MyModuleUpdateDto>(), Arg.Any<bool>()).Returns(Task.FromResult<MyModuleEntity?>(null));

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
            var srcEntities = _fixture.CreateMany<MyModuleEntity>(3).ToList();
            srcEntities.ForEach(async (entity) => await _myModuleDao.Create(entity));

            // Act
            var found = await _service.Delete(srcEntities[1].MyModuleId);

            // Assert
            Assert.True(found);
        }

        [Fact]
        public async Task Delete_ReturnsFalse_WhenNotExists()
        {
            // Arrange
            var id = _fixture.Create<long>();
     
            // Act
            var found = await _service.Delete(id);

            // Assert
            Assert.False(found);
        }

        [Fact]
        public async Task GetPaginated_ReturnsPaginatedItems()
        {
            // Arrange
            var srcEntities = _fixture.CreateMany<MyModuleEntity>(3)
               .Select((entity, index) => {
                   entity.MyModuleId = index + 1;
                   return entity;
               })
               .ToList();
            srcEntities.ForEach(async (entity) => await _myModuleDao.Create(entity));
            var value = srcEntities[1].MyModuleId;
            var propertyName = "MyModuleId";
            var resultItems = srcEntities
                  .Where(e => (e.MyModuleId != value))
                  .OrderByDescending(e => e.MyModuleId);
            var safeRequest = new SafePaginationRequest()
            {
                IncludeTotalCount = true,
                SortCriteria = new List<SortCriteria>()
                {
                    new SortCriteria()
                    {
                        PropertyName = propertyName,
                        SortDirection = SortDirection.Desc
                    }
                },
                FilterCriteria = new List<FilterCriteria>()
                {
                    new FilterCriteria()
                    {
                        PropertyName = propertyName,
                        Value = value.ToString(),
                        Operator = FilterOperator.NotEquals
                    }
                }
            };
            var expectedResult = new PaginatedResult<MyModuleEntity>
            {
                Items = resultItems,
                TotalCount = resultItems.Count()
            };

            // Act
            var result = await _service.GetPaginated(safeRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult.TotalCount, result.TotalCount);
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult.Items),
                JsonSerializer.Serialize(result.Items));
        }

        [Fact]
        public async Task GetMany_ReturnsMatchingItems()
        {
            // Arrange
            var srcEntities = _fixture.CreateMany<MyModuleEntity>(3).ToList();
            srcEntities.ForEach(async (entity) => await _myModuleDao.Create(entity));
            var filter = new FindManyArgs<MyModuleEntity, MyModuleQueryDto>()
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
            var srcEntities = _fixture.CreateMany<MyModuleEntity>(3).ToList();
            srcEntities.ForEach(async (entity) => await _myModuleDao.Create(entity));
            var filter = new FindManyArgs<MyModuleEntity, MyModuleQueryDto>()
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
            var srcEntities = _fixture.CreateMany<MyModuleEntity>(3).ToList();
            srcEntities.ForEach(async (entity) => await _myModuleDao.Create(entity));
            var filter = new FindManyArgs<MyModuleEntity, MyModuleQueryDto>()
            {
                Where = new MyModuleQueryDto
                {
                    MyModuleId = srcEntities[1].MyModuleId
                }
            };

            // Act
            var result = await _service.GetMeta(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Count);
        }
    }
}