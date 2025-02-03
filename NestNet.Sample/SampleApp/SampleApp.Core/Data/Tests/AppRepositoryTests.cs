using Xunit;
using NestNet.Infra.Query;
using NestNet.Infra.Helpers;
using NestNet.Infra.Paginatation;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using NestNet.Infra.BaseClasses;
using System.ComponentModel.DataAnnotations;

namespace SampleApp.Core.Data.Tests
{
    public class AppRepositoryTests : IDisposable, IAsyncLifetime
    {
        [Table("Examples")]
        private class ExampleEntity : EntityBase
        {
            public override long Id
            {
                get { return ExampleId; }
                set { ExampleId = value; }
            }

            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public long ExampleId {get; set;}

            public required string Name { get; set; }
        }

        private class ExampleUpdateDto
        {
            public string? Name { get; set; }
        }

        private class ExampleQueryDto
        {
            public long? ExampleId { get; set; }
            public string? Name { get; set; }
        }

        private readonly IFixture _fixture;
        private readonly AppDbContext _context;
        private readonly AppRepository _repository;

        public AppRepositoryTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());

            // Create options for in-memory database
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")  // Unique name per test
                .Options;

            // Create real context with in-memory database
            _context = new AppDbContext(options);

            _repository = new AppRepository(_context);
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
            var srcEntities = _fixture.CreateMany<ExampleEntity>(3).ToList();
            srcEntities.ForEach(async (entity) => await _repository.Create(entity));

            // Act
            var result = await _repository.GetAll<ExampleEntity>();

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
            var srcEntities = _fixture.CreateMany<ExampleEntity>(3).ToList();
            srcEntities.ForEach(async (entity) => await _repository.Create(entity));

            // Act
            var result = await _repository.GetById<ExampleEntity>(srcEntities[1].ExampleId);

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
            var result = await _repository.GetById<ExampleEntity>(id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Create_ReturnsCreatedItem()
        {
            // Arrange
            var srcEntity = _fixture.Create<ExampleEntity>();

            // Act
            await _repository.Create(srcEntity);

            // Assert
            var result = await _repository.GetById<ExampleEntity>(srcEntity.ExampleId);
            Assert.Equal(
              JsonSerializer.Serialize(srcEntity),
              JsonSerializer.Serialize(result));
        }

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_True_ReturnsUpdatedItem_WhenExists()
        {
            // Arrange
            var ignoreMissingOrNullFields = true;
            var srcEntities = _fixture.CreateMany<ExampleEntity>(3).ToList();
            srcEntities.ForEach(async (entity) => await _repository.Create(entity));
            var updateDto = _fixture.Create<ExampleUpdateDto>();

            // Act
            var result = await _repository.Update<ExampleUpdateDto, ExampleEntity>(srcEntities[1].ExampleId, updateDto, ignoreMissingOrNullFields);

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
            var updateDto = _fixture.Create<ExampleUpdateDto>();

            // Act
            var result = await _repository.Update<ExampleUpdateDto, ExampleEntity>(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_False_ReturnsUpdatedItem_WhenExists()
        {
            // Arrange
            var ignoreMissingOrNullFields = false;
            var srcEntities = _fixture.CreateMany<ExampleEntity>(3).ToList();
            srcEntities.ForEach(async (entity) => await _repository.Create(entity));
            var updateDto = _fixture.Create<ExampleUpdateDto>();

            // Act
            var result = await _repository.Update<ExampleUpdateDto, ExampleEntity>(srcEntities[1].ExampleId, updateDto, ignoreMissingOrNullFields);

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
            var updateDto = _fixture.Create<ExampleUpdateDto>();

            // Act
            var result = await _repository.Update<ExampleUpdateDto, ExampleEntity>(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Delete_ReturnsTrue_WhenExists()
        {
            // Arrange
            var srcEntities = _fixture.CreateMany<ExampleEntity>(3).ToList();
            srcEntities.ForEach(async (entity) => await _repository.Create(entity));

            // Act
            var found = await _repository.Delete<ExampleEntity>(srcEntities[1].ExampleId);

            // Assert
            Assert.True(found);
        }

        [Fact]
        public async Task Delete_ReturnsFalse_WhenNotExists()
        {
            // Arrange
            var id = _fixture.Create<long>();

            // Act
            var found = await _repository.Delete<ExampleEntity>(id);

            // Assert
            Assert.False(found);
        }

        [Fact]
        public async Task GetPaginated_ReturnsPaginatedItems()
        {
            // Arrange
            var srcEntities = _fixture.CreateMany<ExampleEntity>(3)
               .Select((entity, index) =>
               {
                   entity.ExampleId = index + 1;
                   return entity;
               })
               .ToList();
            srcEntities.ForEach(async (entity) => await _repository.Create(entity));
            var value = srcEntities[1].ExampleId;
            var propertyName = "ExampleId";
            var resultItems = srcEntities
                  .Where(e => e.ExampleId != value)
                  .OrderByDescending(e => e.ExampleId);
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
            var expectedResult = new PaginatedResult<ExampleEntity>
            {
                Items = resultItems,
                TotalCount = resultItems.Count()
            };

            // Act
            var result = await _repository.GetPaginated<ExampleEntity>(safeRequest);

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
            var srcEntities = _fixture.CreateMany<ExampleEntity>(3).ToList();
            srcEntities.ForEach(async (entity) => await _repository.Create(entity));
            var filter = new FindManyArgs<ExampleEntity, ExampleQueryDto>()
            {
                Where = new ExampleQueryDto
                {
                    ExampleId = srcEntities[1].ExampleId
                }
            };

            // Act
            var result = await _repository.GetMany(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(srcEntities[1], result.FirstOrDefault());
        }

        [Fact]
        public async Task GetMany_ReturnsEmptyList_WhenNoIdsMatch()
        {
            // Arrange
            var srcEntities = _fixture.CreateMany<ExampleEntity>(3).ToList();
            srcEntities.ForEach(async (entity) => await _repository.Create(entity));
            var filter = new FindManyArgs<ExampleEntity, ExampleQueryDto>()
            {
                Where = new ExampleQueryDto
                {
                    ExampleId = -1
                }
            };

            // Act
            var result = await _repository.GetMany(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMeta_ReturnsCorrectMetadata()
        {
            // Arrange
            var srcEntities = _fixture.CreateMany<ExampleEntity>(3).ToList();
            srcEntities.ForEach(async (entity) => await _repository.Create(entity));
            var filter = new FindManyArgs<ExampleEntity, ExampleQueryDto>()
            {
                Where = new ExampleQueryDto
                {
                    ExampleId = srcEntities[1].ExampleId
                }
            };

            // Act
            var result = await _repository.GetMeta(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Count);
        }
    }
}