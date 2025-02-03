using Xunit;
using NestNet.Infra.Query;
using NestNet.Infra.Helpers;
using NestNet.Infra.Paginatation;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SampleApp.Core.Modules.MyModules.Daos;
using SampleApp.Core.Modules.MyModules.Dtos;
using SampleApp.Core.Modules.MyModules.Entities;
using SampleApp.Core.Data;

namespace SampleApp.Core.Modules.MyModules.Tests.Daos
{
    public class MyModulesDaoTests : IDisposable, IAsyncLifetime
    {
        private readonly IFixture _fixture;
        private readonly AppDbContext _context;
        private readonly MyModuleDao _dao;

        public MyModulesDaoTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());

            // Create options for in-memory database
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")  // Unique name per test
                .Options;

            // Create real context with in-memory database
            _context = new AppDbContext(options);

            _dao = new MyModuleDao(_context);
        }

        public async Task InitializeAsync()
        {
            // Clean database before each test
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetAll_ReturnsAllItems()
        {
            // Arrange
            var srcEntities = _fixture.CreateMany<MyModuleEntity>(3).ToList();
            srcEntities.ForEach(async (entity) => await _dao.Create(entity));

            // Act
            var result = await _dao.GetAll();

            // Assert
            Assert.Equal(srcEntities.Count(), result.Count());
            Assert.Equal(
                JsonSerializer.Serialize(srcEntities),
                JsonSerializer.Serialize(result));
        }

        [Fact]
        public async Task GetById_ReturnsItem_WhenExists()
        {
            // Arrange
            var srcEntities = _fixture.CreateMany<MyModuleEntity>(3).ToList();
            srcEntities.ForEach(async (entity) => await _dao.Create(entity));

            // Act
            var result = await _dao.GetById(srcEntities[1].MyModuleId);

            // Assert
            Assert.Equal(
                JsonSerializer.Serialize(srcEntities[1]),
                JsonSerializer.Serialize(result));
        }

        [Fact]
        public async Task GetById_ReturnsNull_WhenNotExists()
        {
            // Arrange
            var id = _fixture.Create<long>();
          
            // Act
            var result = await _dao.GetById(id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Create_ReturnsCreatedItem()
        {
            // Arrange
            var srcEntity = _fixture.Create<MyModuleEntity>();

            // Act
            await _dao.Create(srcEntity);

            // Assert
            var result = await _dao.GetById(srcEntity.MyModuleId);
            Assert.Equal(
              JsonSerializer.Serialize(srcEntity),
              JsonSerializer.Serialize(result));
        }

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_True_ReturnsUpdatedItem_WhenExists()
        {
            // Arrange
            var ignoreMissingOrNullFields = true;
            var srcEntities = _fixture.CreateMany<MyModuleEntity>(3).ToList();
            srcEntities.ForEach(async (entity) => await _dao.Create(entity));
            var updateDto = _fixture.Create<MyModuleUpdateDto>();

            // Act
            var result = await _dao.Update(srcEntities[1].MyModuleId, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.NotNull(result);
            TestsHelper.IsValuesExists(updateDto, result, Assert.Equal);
        }

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_True_ReturnsNull_WhenNotExists()
        {
            // Arrange
            var ignoreMissingOrNullFields = true;
            var id = _fixture.Create<long>();
            var updateDto = _fixture.Create<MyModuleUpdateDto>();

            // Act
            var result = await _dao.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_False_ReturnsUpdatedItem_WhenExists()
        {
            // Arrange
            var ignoreMissingOrNullFields = false;
            var srcEntities = _fixture.CreateMany<MyModuleEntity>(3).ToList();
            srcEntities.ForEach(async (entity) => await _dao.Create(entity));
            var updateDto = _fixture.Create<MyModuleUpdateDto>();

            // Act
            var result = await _dao.Update(srcEntities[1].MyModuleId, updateDto, ignoreMissingOrNullFields);

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

            // Act
            var result = await _dao.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Delete_ReturnsTrue_WhenExists()
        {
            // Arrange
            var srcEntities = _fixture.CreateMany<MyModuleEntity>(3).ToList();
            srcEntities.ForEach(async (entity) => await _dao.Create(entity));

            // Act
            var found = await _dao.Delete(srcEntities[1].MyModuleId);

            // Assert
            Assert.True(found);
        }

        [Fact]
        public async Task Delete_ReturnsFalse_WhenNotExists()
        {
            // Arrange
            var id = _fixture.Create<long>();
     
            // Act
            var found = await _dao.Delete(id);

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
            srcEntities.ForEach(async (entity) => await _dao.Create(entity));
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
            var result = await _dao.GetPaginated(safeRequest);

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
            srcEntities.ForEach(async (entity) => await _dao.Create(entity));
            var filter = new FindManyArgs<MyModuleEntity, MyModuleQueryDto>()
            {
                Where = new MyModuleQueryDto
                {
                    MyModuleId = srcEntities[1].MyModuleId
                }
            };

            // Act
            var result = await _dao.GetMany(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(srcEntities[1], result.FirstOrDefault());
        }

        [Fact]
        public async Task GetMany_ReturnsEmptyList_WhenNoIdsMatch()
        {
            // Arrange
            var srcEntities = _fixture.CreateMany<MyModuleEntity>(3).ToList();
            srcEntities.ForEach(async (entity) => await _dao.Create(entity));
            var filter = new FindManyArgs<MyModuleEntity, MyModuleQueryDto>()
            {
                Where = new MyModuleQueryDto
                {
                    MyModuleId = -1
                }
            };

            // Act
            var result = await _dao.GetMany(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMeta_ReturnsCorrectMetadata()
        {
            // Arrange
            var srcEntities = _fixture.CreateMany<MyModuleEntity>(3).ToList();
            srcEntities.ForEach(async (entity) => await _dao.Create(entity));
            var filter = new FindManyArgs<MyModuleEntity, MyModuleQueryDto>()
            {
                Where = new MyModuleQueryDto
                {
                    MyModuleId = srcEntities[1].MyModuleId
                }
            };

            // Act
            var result = await _dao.GetMeta(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Count);
        }
    }
}