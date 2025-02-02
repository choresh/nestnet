using NestNet.Cli.Generators.Common;
using NestNet.Cli.Infra;
using Spectre.Console;

namespace NestNet.Cli.Generators.ModuleGenerator
{
    internal static partial class ModuleGenerator
    {
        private class CoreModuleGenerator : MultiProjectsGeneratorBase<ModuleGenerationContext>
        {
            public CoreModuleGenerator()
                : base(ProjectType.Core)
            {
            }

            public override void DoGenerate()
            {
                if (Context.GenerateDbSupport)
                {
                    // Ensure Core directory exists
                    Directory.CreateDirectory(Context.ProjectContext!.TargetPath);

                    CreateEntityFile();
                    foreach (DtoType dtoType in Enum.GetValues(typeof(DtoType)))
                    {
                        string? properties = GetInitialDtoProperties(dtoType);
                        CreateDtoFile(dtoType, null, properties);
                    }
                    CreateDaoFile();
                    CreateDaoTestFile();
                }

                if (Context.GenerateService)
                {
                    CreateServiceFile();
                    CreateServiceTestFile();
                }
            }

            private string? GetInitialDtoProperties(DtoType dtoType)
            {
                string? properties;
                switch (dtoType)
                {
                    case DtoType.Result:
                        properties = $@"
        public long {Context.ArtifactName}Id {{ get; set; }}
";
                        break;
                    case DtoType.Query:
                        properties = $@"
        public long? {Context.ArtifactName}Id {{ get; set; }}
";
                        break;
                    default:
                        properties = null;
                        break;
                }
                return properties;
            }

            private void CreateEntityFile()
            {
                string entityContent = GetEntityContent();
                string entityPath = Path.Combine(Context.ProjectContext!.TargetPath, "Entities", $"{Context.ArtifactName}Entity.cs");
                Directory.CreateDirectory(GetDirectoryName(entityPath));
                File.WriteAllText(entityPath, entityContent);
                AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {entityPath}", "grey"));
            }

            private void CreateDtoFile(DtoType dtoType, Type? baseClass = null, string? properties = null)
            {
                string dtoContent = Helpers.GetDtoContent(Context.ProjectContext!.ProjectName, Context.ArtifactName, Context.PluralizedModuleName, dtoType, baseClass, properties);
                string dtoPath = Path.Combine(Context.ProjectContext!.TargetPath, "Dtos", $"{Helpers.FormatDtoName(Context.ArtifactName, dtoType)}.cs");
                Directory.CreateDirectory(GetDirectoryName(dtoPath));
                File.WriteAllText(dtoPath, dtoContent);
                AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {dtoPath}", "grey"));
            }

            private void CreateDaoFile()
            {
                string daoContent = GetDaoContent();
                string daoPath = Path.Combine(Context.ProjectContext!.TargetPath, "Daos", $"{Context.ArtifactName}Dao.cs");
                Directory.CreateDirectory(GetDirectoryName(daoPath));
                File.WriteAllText(daoPath, daoContent);
                AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {daoPath}", "grey"));
            }

            private void CreateServiceFile()
            {
                string serviceContent = GetServiceContent();
                string servicePath = Path.Combine(Context.ProjectContext!.TargetPath, "Services", $"{Context.PluralizedModuleName}Service.cs");
                Directory.CreateDirectory(GetDirectoryName(servicePath));
                File.WriteAllText(servicePath, serviceContent);
                AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {servicePath}", "grey"));
            }

            private void CreateDaoTestFile()
            {
                string testContent = GetDaoTestContent();
                string testPath = Path.Combine(Context.ProjectContext!.TargetPath, "Tests", "Daos", $"{Context.PluralizedModuleName}DaoTests.cs");
                Directory.CreateDirectory(GetDirectoryName(testPath));
                File.WriteAllText(testPath, testContent);
                AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {testPath}", "grey"));
            }

            private void CreateServiceTestFile()
            {
                string testContent = GetServiceTestContent();
                string testPath = Path.Combine(Context.ProjectContext!.TargetPath, "Tests", "Services", $"{Context.PluralizedModuleName}ServiceTests.cs");
                Directory.CreateDirectory(GetDirectoryName(testPath));
                File.WriteAllText(testPath, testContent);
                AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {testPath}", "grey"));
            }

            private string GetEntityContent()
            {
                return $@"using NestNet.Infra.BaseClasses;
using NestNet.Infra.Attributes;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace {Context.ProjectContext!.ProjectName}.Modules.{Context.PluralizedModuleName}.Entities
{{
    [Table(""{Context.PluralizedModuleName}"")]
    public class {Context.ArtifactName}Entity : EntityBase
    {{
        // This property enables code at 'NestNet.Infra' to handle the entity in general 
        // manner (without knowing the specific name '{Context.ArtifactName}Id').
        [Prop(
            create: GenOpt.Ignore,
            update: GenOpt.Ignore,
            result: GenOpt.Ignore
        )]
        [NotMapped] // Exclude property from DB.
        public override long Id
        {{
            get {{ return {Context.ArtifactName}Id; }}
            set {{ {Context.ArtifactName}Id = value; }}
        }}

        [Prop(
            create: GenOpt.Ignore,
            update: GenOpt.Ignore,
            result: GenOpt.Mandatory
        )]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long {Context.ArtifactName}Id {{ get; set; }}

        [Prop(
            create: GenOpt.Mandatory,
            update: GenOpt.Optional,
            result: GenOpt.Mandatory
        )]
        public required string Name {{ get; set; }}

        [Prop(
            create: GenOpt.Mandatory,
            update: GenOpt.Optional,
            result: GenOpt.Mandatory
        )]
        public long Age {{ get; set; }}

        [Prop(
            create: GenOpt.Optional,
            update: GenOpt.Optional,
            result: GenOpt.Optional
        )]
        public string? Email {{ get; set; }}

        [Prop(
            create: GenOpt.Mandatory,
            update: GenOpt.Ignore,
            result: GenOpt.Ignore
        )]
        [NotMapped] // Exclude property from DB.
        public string? MyVirtualField {{ get; set; }}
    }}
}}";
            }

            private string GetDaoContent()
            {
                return $@"#pragma warning disable IDE0290 // Use primary constructor
using {Context.ProjectContext!.ProjectName}.Data;
using NestNet.Infra.BaseClasses;
using NestNet.Infra.Attributes;
using {Context.ProjectContext!.ProjectName}.Modules.{Context.PluralizedModuleName}.Dtos;
using {Context.ProjectContext!.ProjectName}.Modules.{Context.PluralizedModuleName}.Entities;

namespace {Context.ProjectContext!.ProjectName}.Modules.{Context.PluralizedModuleName}.Daos
{{
    public interface I{Context.ArtifactName}Dao: IDao<{Context.EntityName}, {Context.QueryDtoName}>
    {{
        // If you add methods to derived class - expose them here.
    }}

    [Injectable(LifetimeType.Scoped)]
    public class {Context.ArtifactName}Dao : DaoBase<{Context.EntityName}, {Context.QueryDtoName}>, I{Context.ArtifactName}Dao
    {{
        public {Context.ArtifactName}Dao(AppDbContext context)
            : base(context, context.Set<{Context.EntityName}>(), ""{Context.ArtifactName}Id"")
        {{
        }}

        // How to customise this class:
        // 1) You can add here 'custom' methods (methods for operations not supported by the base class).
        // 2) You can override here base class methods if needed:
        //    * Add methods with the same name and signature as in the base class.
        //    * For example:
        //      public override async Task<IEnumerable<{Context.EntityName}>> GetAll()
        //      {{
        //          // Set your custom implementation here.
        //      }}
        // 3) In your methods:
        //    * Base class members '_dbSet' and '_context' are accessible.
        //    * Base class methods (e.g. 'await base.GetAll()') are accesible.
    }}
}}

#pragma warning restore IDE0290 // Use primary constructor";
            }

            private string GetServiceContent()
            {
                return $@"#pragma warning disable IDE0290 // Use primary constructor
using NestNet.Infra.BaseClasses;
using NestNet.Infra.Attributes;
using {Context.ProjectContext!.ProjectName}.Modules.{Context.PluralizedModuleName}.Dtos;
using {Context.ProjectContext!.ProjectName}.Modules.{Context.PluralizedModuleName}.Daos;
using {Context.ProjectContext!.ProjectName}.Modules.{Context.PluralizedModuleName}.Entities;

namespace {Context.ProjectContext!.ProjectName}.Modules.{Context.PluralizedModuleName}.Services
{{
    public interface I{Context.PluralizedModuleName}Service: ICrudService<{Context.EntityName}, {Context.CreateDtoName}, {Context.UpdateDtoName}, {Context.ResultDtoName}, {Context.QueryDtoName}>
    {{
        // If you add methods to derived class - expose them here.
    }}

    [Injectable(LifetimeType.Scoped)]
    public class {Context.PluralizedModuleName}Service : CrudServiceBase<{Context.EntityName}, {Context.CreateDtoName}, {Context.UpdateDtoName}, {Context.ResultDtoName}, {Context.QueryDtoName}>, I{Context.PluralizedModuleName}Service
    {{
        public {Context.PluralizedModuleName}Service(I{Context.ArtifactName}Dao {Context.ParamName}Dao)
            : base({Context.ParamName}Dao)
        {{
        }}

        // How to customise this class:
        // 1) You can add here 'custom' methods (methods for operations not supported by the base class).
        // 2) You can override here base class methods if needed:
        //    * Add methods with the same name and signature as in the base class.
        //    * For example:
        //      public override async Task<IEnumerable<{Context.ResultDtoName}>> GetAll()
        //      {{
        //          // Set your custom implementation here.
        //      }}
        // 3) In your methods:
        //    * Base class member '_dao' is accessible.
        //    * Base class methods (e.g. 'await base.GetAll()') are accesible.
    }}
}}

#pragma warning restore IDE0290 // Use primary constructor";
            }

            private string GetDaoTestContent()
            {
                return $@"using Xunit;
using NestNet.Infra.Query;
using NestNet.Infra.Helpers;
using NestNet.Infra.Paginatation;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using System.Text.Json;
using {Context.ProjectContext!.ProjectName}.Modules.{Context.PluralizedModuleName}.Dtos;
using {Context.ProjectContext!.ProjectName}.Modules.{Context.PluralizedModuleName}.Daos;
using {Context.ProjectContext!.ProjectName}.Modules.{Context.PluralizedModuleName}.Entities;
using {Context.ProjectContext!.ProjectName}.Data;
using Microsoft.EntityFrameworkCore;

namespace {Context.ProjectContext!.ProjectName}.Modules.{Context.PluralizedModuleName}.Tests.Daos
{{
    public class {Context.PluralizedModuleName}DaoTests : IDisposable, IAsyncLifetime
    {{
        private readonly IFixture _fixture;
        private readonly AppDbContext _context;
        private readonly {Context.ArtifactName}Dao _dao;

        public {Context.PluralizedModuleName}DaoTests()
        {{
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());

            // Create options for in-memory database
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $""TestDb_{{Guid.NewGuid()}}"")  // Unique name per test
                .Options;

            // Create real context with in-memory database
            _context = new AppDbContext(options);

            _dao = new {Context.ArtifactName}Dao(_context);
        }}

        public async Task InitializeAsync()
        {{
            // Clean database before each test
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();
        }}

        public Task DisposeAsync()
        {{
            return Task.CompletedTask;
        }}
    
        public void Dispose()
        {{
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }}

        [Fact]
        public async Task GetAll_ReturnsAllItems()
        {{
            // Arrange
            var srcEntities = _fixture.CreateMany<{Context.EntityName}>(3).ToList();
            srcEntities.ForEach(async (entity) => await _dao.Create(entity));

            // Act
            var result = await _dao.GetAll();

            // Assert
            Assert.Equal(srcEntities.Count(), result.Count());
            Assert.Equal(
                JsonSerializer.Serialize(srcEntities),
                JsonSerializer.Serialize(result));
        }}

        [Fact]
        public async Task GetById_ReturnsItem_WhenExists()
        {{
            // Arrange
            var srcEntities = _fixture.CreateMany<{Context.EntityName}>(3).ToList();
            srcEntities.ForEach(async (entity) => await _dao.Create(entity));

            // Act
            var result = await _dao.GetById(srcEntities[1].{Context.ArtifactName}Id);

            // Assert
            Assert.Equal(
                JsonSerializer.Serialize(srcEntities[1]),
                JsonSerializer.Serialize(result));
        }}

        [Fact]
        public async Task GetById_ReturnsNull_WhenNotExists()
        {{
            // Arrange
            var id = _fixture.Create<long>();
          
            // Act
            var result = await _dao.GetById(id);

            // Assert
            Assert.Null(result);
        }}

        [Fact]
        public async Task Create_ReturnsCreatedItem()
        {{
            // Arrange
            var srcEntity = _fixture.Create<{Context.EntityName}>();

            // Act
            await _dao.Create(srcEntity);

            // Assert
            var result = await _dao.GetById(srcEntity.{Context.ArtifactName}Id);
            Assert.Equal(
              JsonSerializer.Serialize(srcEntity),
              JsonSerializer.Serialize(result));
        }}

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_True_ReturnsUpdatedItem_WhenExists()
        {{
            // Arrange
            var ignoreMissingOrNullFields = true;
            var srcEntities = _fixture.CreateMany<{Context.EntityName}>(3).ToList();
            srcEntities.ForEach(async (entity) => await _dao.Create(entity));
            var updateDto = _fixture.Create<{Context.UpdateDtoName}>();

            // Act
            var result = await _dao.Update(srcEntities[1].{Context.ArtifactName}Id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.NotNull(result);
            TestsHelper.IsValuesExists(updateDto, result, Assert.Equal);
        }}

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_True_ReturnsNull_WhenNotExists()
        {{
            // Arrange
            var ignoreMissingOrNullFields = true;
            var id = _fixture.Create<long>();
            var updateDto = _fixture.Create<{Context.UpdateDtoName}>();

            // Act
            var result = await _dao.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.Null(result);
        }}

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_False_ReturnsUpdatedItem_WhenExists()
        {{
            // Arrange
            var ignoreMissingOrNullFields = false;
            var srcEntities = _fixture.CreateMany<{Context.EntityName}>(3).ToList();
            srcEntities.ForEach(async (entity) => await _dao.Create(entity));
            var updateDto = _fixture.Create<{Context.UpdateDtoName}>();

            // Act
            var result = await _dao.Update(srcEntities[1].{Context.ArtifactName}Id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.NotNull(result);
            TestsHelper.IsValuesExists(updateDto, result, Assert.Equal);
        }}

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_False_ReturnsNull_WhenNotExists()
        {{
            // Arrange
            var ignoreMissingOrNullFields = false;
            var id = _fixture.Create<long>();
            var updateDto = _fixture.Create<{Context.UpdateDtoName}>();

            // Act
            var result = await _dao.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.Null(result);
        }}

        [Fact]
        public async Task Delete_ReturnsTrue_WhenExists()
        {{
            // Arrange
            var srcEntities = _fixture.CreateMany<{Context.EntityName}>(3).ToList();
            srcEntities.ForEach(async (entity) => await _dao.Create(entity));

            // Act
            var found = await _dao.Delete(srcEntities[1].{Context.ArtifactName}Id);

            // Assert
            Assert.True(found);
        }}

        [Fact]
        public async Task Delete_ReturnsFalse_WhenNotExists()
        {{
            // Arrange
            var id = _fixture.Create<long>();
     
            // Act
            var found = await _dao.Delete(id);

            // Assert
            Assert.False(found);
        }}

        [Fact]
        public async Task GetPaginated_ReturnsPaginatedItems()
        {{
            // Arrange
            var srcEntities = _fixture.CreateMany<{Context.EntityName}>(3)
               .Select((entity, index) => {{
                   entity.{Context.ArtifactName}Id = index + 1;
                   return entity;
               }})
               .ToList();
            srcEntities.ForEach(async (entity) => await _dao.Create(entity));
            var value = srcEntities[1].{Context.ArtifactName}Id;
            var propertyName = ""{Context.ArtifactName}Id"";
            var resultItems = srcEntities
                  .Where(e => (e.{Context.ArtifactName}Id != value))
                  .OrderByDescending(e => e.{Context.ArtifactName}Id);
            var safeRequest = new SafePaginationRequest()
            {{
                IncludeTotalCount = true,
                SortCriteria = new List<SortCriteria>()
                {{
                    new SortCriteria()
                    {{
                        PropertyName = propertyName,
                        SortDirection = SortDirection.Desc
                    }}
                }},
                FilterCriteria = new List<FilterCriteria>()
                {{
                    new FilterCriteria()
                    {{
                        PropertyName = propertyName,
                        Value = value.ToString(),
                        Operator = FilterOperator.NotEquals
                    }}
                }}
            }};
            var expectedResult = new PaginatedResult<{Context.EntityName}>
            {{
                Items = resultItems,
                TotalCount = resultItems.Count()
            }};

            // Act
            var result = await _dao.GetPaginated(safeRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult.TotalCount, result.TotalCount);
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult.Items),
                JsonSerializer.Serialize(result.Items));
        }}

        [Fact]
        public async Task GetMany_ReturnsMatchingItems()
        {{
            // Arrange
            var srcEntities = _fixture.CreateMany<{Context.EntityName}>(3).ToList();
            srcEntities.ForEach(async (entity) => await _dao.Create(entity));
            var filter = new FindManyArgs<{Context.EntityName}, {Context.QueryDtoName}>()
            {{
                Where = new {Context.QueryDtoName}
                {{
                    {Context.ArtifactName}Id = srcEntities[1].{Context.ArtifactName}Id
                }}
            }};

            // Act
            var result = await _dao.GetMany(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.NotNull(result.FirstOrDefault());
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult.FirstOrDefault()),
                JsonSerializer.Serialize(result.FirstOrDefault()),
                true
            );
        }}

        [Fact]
        public async Task GetMany_ReturnsEmptyList_WhenNoIdsMatch()
        {{
            // Arrange
            var srcEntities = _fixture.CreateMany<{Context.EntityName}>(3).ToList();
            srcEntities.ForEach(async (entity) => await _dao.Create(entity));
            var filter = new FindManyArgs<{Context.EntityName}, {Context.QueryDtoName}>()
            {{
                Where = new {Context.QueryDtoName}
                {{
                    {Context.ArtifactName}Id = -1
                }}
            }};

            // Act
            var result = await _dao.GetMany(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }}

        [Fact]
        public async Task GetMeta_ReturnsCorrectMetadata()
        {{
            // Arrange
            var srcEntities = _fixture.CreateMany<{Context.EntityName}>(3).ToList();
            srcEntities.ForEach(async (entity) => await _dao.Create(entity));
            var filter = new FindManyArgs<{Context.EntityName}, {Context.QueryDtoName}>()
            {{
                Where = new {Context.QueryDtoName}
                {{
                    {Context.ArtifactName}Id = srcEntities[1].{Context.ArtifactName}Id
                }}
            }};

            // Act
            var result = await _dao.GetMeta(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Count);
        }}
    }}
}}";
            }

            private string GetServiceTestContent()
            {
                return $@"using NSubstitute;
using Xunit;
using NestNet.Infra.Query;
using NestNet.Infra.BaseClasses;
using NestNet.Infra.Paginatation;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using System.Text.Json;
using {Context.ProjectContext!.ProjectName}.Modules.{Context.PluralizedModuleName}.Services;
using {Context.ProjectContext!.ProjectName}.Modules.{Context.PluralizedModuleName}.Dtos;
using {Context.ProjectContext!.ProjectName}.Modules.{Context.PluralizedModuleName}.Daos;

namespace {Context.ProjectContext!.ProjectName}.Modules.{Context.PluralizedModuleName}.Tests.Services
{{
    public class {Context.PluralizedModuleName}ServiceTests
    {{
        private readonly IFixture _fixture;
        private readonly I{Context.ArtifactName}Dao _{Context.ParamName}Dao;
        private readonly {Context.PluralizedModuleName}Service _service;

        public {Context.PluralizedModuleName}ServiceTests()
        {{
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            _{Context.ParamName}Dao = _fixture.Freeze<I{Context.ArtifactName}Dao>();
            _service = new {Context.PluralizedModuleName}Service(_{Context.ParamName}Dao);
        }}

        [Fact]
        public async Task GetAll_ReturnsAllItems()
        {{
            // Arrange
            var srcEntities = _fixture.CreateMany<{Context.EntityName}>(3).ToList();
            var expectedResult = _service.ToResultDtos(srcEntities);
            _{Context.ParamName}Dao.GetAll().Returns(Task.FromResult<IEnumerable<{Context.EntityName}>>(srcEntities));

            // Act
            var result = await _service.GetAll();
  
            // Assert
            Assert.Equal(expectedResult.Count(), result.Count());
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult),
                JsonSerializer.Serialize(result));
            await _{Context.ParamName}Dao.Received(1).GetAll();
        }}

        [Fact]
        public async Task GetById_ReturnsItem_WhenExists()
        {{
            // Arrange
            var id = _fixture.Create<long>();
            var entity = _fixture.Create<{Context.EntityName}>();
            var expectedResult = _service.ToResultDto(entity);
            _{Context.ParamName}Dao.GetById(Arg.Any<long>()).Returns(Task.FromResult<{Context.EntityName}?>(entity));

            // Act
            var result = await _service.GetById(id);

            // Assert
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult),
                JsonSerializer.Serialize(result));
            await _{Context.ParamName}Dao.Received(1).GetById(id);
        }}

        [Fact]
        public async Task GetById_ReturnsNull_WhenNotExists()
        {{
            // Arrange
            var id = _fixture.Create<long>();
            _{Context.ParamName}Dao.GetById(Arg.Any<long>()).Returns(Task.FromResult<{Context.EntityName}?>(null));

            // Act
            var result = await _service.GetById(id);

            // Assert
            Assert.Null(result);
            await _{Context.ParamName}Dao.Received(1).GetById(id);
        }}

        [Fact]
        public async Task Create_ReturnsCreatedItem()
        {{
            // Arrange
            var createDto = _fixture.Create<{Context.CreateDtoName}>();
            var createdEntity = _service.ToEntity(createDto);
            var expectedResult = _service.ToResultDto(createdEntity);
            _{Context.ParamName}Dao.Create(Arg.Any<{Context.EntityName}>()).Returns(Task.FromResult(createdEntity));

            // Act
            var result = await _service.Create(createDto);

            // Assert
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult),
                JsonSerializer.Serialize(result.ResultDto));
            await _{Context.ParamName}Dao.Received(1).Create(Arg.Any<{Context.EntityName}>());
        }}

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_True_ReturnsUpdatedItem_WhenExists()
        {{
            // Arrange
            var ignoreMissingOrNullFields = true;
            var id = _fixture.Create<long>();
            var updateDto = _fixture.Create<{Context.UpdateDtoName}>();
            var updatedEntity = _service.ToEntity(updateDto);
            var expectedResult = _service.ToResultDto(updatedEntity);           
            _{Context.ParamName}Dao.Update(Arg.Any<long>(), Arg.Any<{Context.UpdateDtoName}>(), Arg.Any<bool>()).Returns(Task.FromResult<{Context.EntityName}?>(updatedEntity));
            
            // Act
            var result = await _service.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult),
                JsonSerializer.Serialize(result));
            await _{Context.ParamName}Dao.Received(1).Update(id, updateDto, ignoreMissingOrNullFields);
        }}

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_True_ReturnsNull_WhenNotExists()
        {{
            // Arrange
            var ignoreMissingOrNullFields = true;
            var id = _fixture.Create<long>();
            var updateDto = _fixture.Create<{Context.UpdateDtoName}>();
            _{Context.ParamName}Dao.Update(Arg.Any<long>(), Arg.Any<{Context.UpdateDtoName}>(), Arg.Any<bool>()).Returns(Task.FromResult<{Context.EntityName}?>(null));

            // Act
            var result = await _service.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.Null(result);
            await _{Context.ParamName}Dao.Received(1).Update(id, updateDto, ignoreMissingOrNullFields);
        }}

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_False_ReturnsUpdatedItem_WhenExists()
        {{
            // Arrange
            var ignoreMissingOrNullFields = false;
            var srcEntities = _fixture.CreateMany<{Context.EntityName}>(3).ToList();
            srcEntities.ForEach(async (entity) => await _{Context.ParamName}Dao.Create(entity));
            var updateDto = _fixture.Create<{Context.UpdateDtoName}>();

            // Act
            var result = await _service.Update(srcEntities[1].{Context.ArtifactName}Id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.NotNull(result);
            TestsHelper.IsValuesExists(updateDto, result, Assert.Equal);
        }}

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_False_ReturnsNull_WhenNotExists()
        {{
            // Arrange
            var ignoreMissingOrNullFields = false;
            var id = _fixture.Create<long>();
            var updateDto = _fixture.Create<{Context.UpdateDtoName}>();
            _{Context.ParamName}Dao.Update(Arg.Any<long>(), Arg.Any<{Context.UpdateDtoName}>(), Arg.Any<bool>()).Returns(Task.FromResult<{Context.EntityName}?>(null));

            // Act
            var result = await _service.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.Null(result);
            await _{Context.ParamName}Dao.Received(1).Update(id, updateDto, ignoreMissingOrNullFields);
        }}

        [Fact]
        public async Task Delete_ReturnsTrue_WhenExists()
        {{
            // Arrange
            var srcEntities = _fixture.CreateMany<{Context.EntityName}>(3).ToList();
            srcEntities.ForEach(async (entity) => await _{Context.ParamName}Dao.Create(entity));

            // Act
            var found = await _service.Delete(srcEntities[1].{Context.ArtifactName}Id);

            // Assert
            Assert.True(found);
        }}

        [Fact]
        public async Task Delete_ReturnsFalse_WhenNotExists()
        {{
            // Arrange
            var id = _fixture.Create<long>();
     
            // Act
            var found = await _service.Delete(id);

            // Assert
            Assert.False(found);
        }}

        [Fact]
        public async Task GetPaginated_ReturnsPaginatedItems()
        {{
            // Arrange
            var srcEntities = _fixture.CreateMany<{Context.EntityName}>(3)
               .Select((entity, index) => {{
                   entity.{Context.ArtifactName}Id = index + 1;
                   return entity;
               }})
               .ToList();
            srcEntities.ForEach(async (entity) => await _{Context.ParamName}Dao.Create(entity));
            var value = srcEntities[1].{Context.ArtifactName}Id;
            var propertyName = ""{Context.ArtifactName}Id"";
            var resultItems = srcEntities
                  .Where(e => (e.{Context.ArtifactName}Id != value))
                  .OrderByDescending(e => e.{Context.ArtifactName}Id);
            var safeRequest = new SafePaginationRequest()
            {{
                IncludeTotalCount = true,
                SortCriteria = new List<SortCriteria>()
                {{
                    new SortCriteria()
                    {{
                        PropertyName = propertyName,
                        SortDirection = SortDirection.Desc
                    }}
                }},
                FilterCriteria = new List<FilterCriteria>()
                {{
                    new FilterCriteria()
                    {{
                        PropertyName = propertyName,
                        Value = value.ToString(),
                        Operator = FilterOperator.NotEquals
                    }}
                }}
            }};
            var expectedResult = new PaginatedResult<{Context.EntityName}>
            {{
                Items = resultItems,
                TotalCount = resultItems.Count()
            }};

            // Act
            var result = await _service.GetPaginated(safeRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult.TotalCount, result.TotalCount);
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult.Items),
                JsonSerializer.Serialize(result.Items));
        }}

        [Fact]
        public async Task GetMany_ReturnsMatchingItems()
        {{
            // Arrange
            var srcEntities = _fixture.CreateMany<{Context.EntityName}>(3).ToList();
            srcEntities.ForEach(async (entity) => await _{Context.ParamName}Dao.Create(entity));
            var filter = new FindManyArgs<{Context.EntityName}, {Context.QueryDtoName}>()
            {{
                Where = new {Context.QueryDtoName}
                {{
                    {Context.ArtifactName}Id = srcEntities[1].{Context.ArtifactName}Id
                }}
            }};

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
            await _{Context.ParamName}Dao.Received(1).GetMany(filter);
        }}

        [Fact]
        public async Task GetMany_ReturnsEmptyList_WhenNoIdsMatch()
        {{
            // Arrange
            var srcEntities = _fixture.CreateMany<{Context.EntityName}>(3).ToList();
            srcEntities.ForEach(async (entity) => await _{Context.ParamName}Dao.Create(entity));
            var filter = new FindManyArgs<{Context.EntityName}, {Context.QueryDtoName}>()
            {{
                Where = new {Context.QueryDtoName}
                {{
                    {Context.ArtifactName}Id = -1
                }}
            }};

            // Act
            var result = await _service.GetMany(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            await _{Context.ParamName}Dao.Received(1).GetMany(filter);
        }}

        [Fact]
        public async Task GetMeta_ReturnsCorrectMetadata()
        {{
            // Arrange
            var srcEntities = _fixture.CreateMany<{Context.EntityName}>(3).ToList();
            srcEntities.ForEach(async (entity) => await _{Context.ParamName}Dao.Create(entity));
            var filter = new FindManyArgs<{Context.EntityName}, {Context.QueryDtoName}>()
            {{
                Where = new {Context.QueryDtoName}
                {{
                    {Context.ArtifactName}Id = srcEntities[1].{Context.ArtifactName}Id
                }}
            }};

            // Act
            var result = await _service.GetMeta(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Count);
        }}
    }}
}}";
            }
/* ZZZ
            private static string GetDaoTestContent(ModuleGenerationContext context)
            {
                return $@"using Xunit;
using NestNet.Infra.Query;
using NestNet.Infra.Helpers;
using NestNet.Infra.Paginatation;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using System.Text.Json;
using {Context.ProjectContext!.ProjectName}.Modules.{context.PluralizedModuleName}.Dtos;
using {Context.ProjectContext!.ProjectName}.Modules.{context.PluralizedModuleName}.Daos;
using {Context.ProjectContext!.ProjectName}.Modules.{context.PluralizedModuleName}.Entities;
using {context.ProjectContext!.ProjectName}.Data;
using Microsoft.EntityFrameworkCore;

namespace {Context.ProjectContext!.ProjectName}.Modules.{context.PluralizedModuleName}.Tests.Daos
{{
    public class {context.PluralizedModuleName}DaoTests : IDisposable, IAsyncLifetime
    {{
        private readonly IFixture _fixture;
        private readonly AppDbContext _context;
        private readonly {context.ArtifactName}Dao _dao;

        public {context.PluralizedModuleName}DaoTests()
        {{
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());

            // Create options for in-memory database
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $""TestDb_{{Guid.NewGuid()}}"")  // Unique name per test
                .Options;

            // Create real context with in-memory database
            _context = new AppDbContext(options);

            _dao = new {context.ArtifactName}Dao(_context);
        }}

        public async Task InitializeAsync()
        {{
            // Clean database before each test
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();
        }}

        public Task DisposeAsync()
        {{
            return Task.CompletedTask;
        }}
    
        public void Dispose()
        {{
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }}

        [Fact]
        public async Task GetAll_ReturnsAllItems()
        {{
            // Arrange
            var srcEntities = _fixture.CreateMany<{context.EntityName}>(3).ToList();
            srcEntities.ForEach(async (entity) => await _dao.Create(entity));

            // Act
            var result = await _dao.GetAll();

            // Assert
            Assert.Equal(srcEntities.Count(), result.Count());
            Assert.Equal(
                JsonSerializer.Serialize(srcEntities),
                JsonSerializer.Serialize(result));
        }}

        [Fact]
        public async Task GetById_ReturnsItem_WhenExists()
        {{
            // Arrange
            var srcEntities = _fixture.CreateMany<{context.EntityName}>(3).ToList();
            srcEntities.ForEach(async (entity) => await _dao.Create(entity));

            // Act
            var result = await _dao.GetById(srcEntities[1].{context.ArtifactName}Id);

            // Assert
            Assert.Equal(
                JsonSerializer.Serialize(srcEntities[1]),
                JsonSerializer.Serialize(result));
        }}

        [Fact]
        public async Task GetById_ReturnsNull_WhenNotExists()
        {{
            // Arrange
            var id = _fixture.Create<long>();
          
            // Act
            var result = await _dao.GetById(id);

            // Assert
            Assert.Null(result);
        }}

        [Fact]
        public async Task Create_ReturnsCreatedItem()
        {{
            // Arrange
            var srcEntity = _fixture.Create<{context.EntityName}>();

            // Act
            await _dao.Create(srcEntity);

            // Assert
            var result = await _dao.GetById(srcEntity.{context.ArtifactName}Id);
            Assert.Equal(
              JsonSerializer.Serialize(srcEntity),
              JsonSerializer.Serialize(result));
        }}

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_True_ReturnsUpdatedItem_WhenExists()
        {{
            // Arrange
            var ignoreMissingOrNullFields = true;
            var srcEntities = _fixture.CreateMany<{context.EntityName}>(3).ToList();
            srcEntities.ForEach(async (entity) => await _dao.Create(entity));
            var updateDto = _fixture.Create<{context.UpdateDtoName}>();

            // Act
            var result = await _dao.Update(srcEntities[1].{context.ArtifactName}Id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.NotNull(result);
            TestsHelper.IsValuesExists(updateDto, result, Assert.Equal);
        }}

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_True_ReturnsNull_WhenNotExists()
        {{
            // Arrange
            var ignoreMissingOrNullFields = true;
            var id = _fixture.Create<long>();
            var updateDto = _fixture.Create<{context.UpdateDtoName}>();

            // Act
            var result = await _dao.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.Null(result);
        }}

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_False_ReturnsUpdatedItem_WhenExists()
        {{
            // Arrange
            var ignoreMissingOrNullFields = false;
            var srcEntities = _fixture.CreateMany<{context.EntityName}>(3).ToList();
            srcEntities.ForEach(async (entity) => await _dao.Create(entity));
            var updateDto = _fixture.Create<{context.UpdateDtoName}>();

            // Act
            var result = await _dao.Update(srcEntities[1].{context.ArtifactName}Id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.NotNull(result);
            TestsHelper.IsValuesExists(updateDto, result, Assert.Equal);
        }}

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_False_ReturnsNull_WhenNotExists()
        {{
            // Arrange
            var ignoreMissingOrNullFields = false;
            var id = _fixture.Create<long>();
            var updateDto = _fixture.Create<{context.UpdateDtoName}>();

            // Act
            var result = await _dao.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.Null(result);
        }}

        [Fact]
        public async Task Delete_ReturnsTrue_WhenExists()
        {{
            // Arrange
            var srcEntities = _fixture.CreateMany<{context.EntityName}>(3).ToList();
            srcEntities.ForEach(async (entity) => await _dao.Create(entity));

            // Act
            var found = await _dao.Delete(srcEntities[1].{context.ArtifactName}Id);

            // Assert
            Assert.True(found);
        }}

        [Fact]
        public async Task Delete_ReturnsFalse_WhenNotExists()
        {{
            // Arrange
            var id = _fixture.Create<long>();
     
            // Act
            var found = await _dao.Delete(id);

            // Assert
            Assert.False(found);
        }}

        [Fact]
        public async Task GetPaginated_ReturnsPaginatedItems()
        {{
            // Arrange
            var srcEntities = _fixture.CreateMany<{context.EntityName}>(3)
               .Select((entity, index) => {{
                   entity.{context.ArtifactName}Id = index + 1;
                   return entity;
               }})
               .ToList();
            srcEntities.ForEach(async (entity) => await _dao.Create(entity));
            var value = srcEntities[1].{context.ArtifactName}Id;
            var propertyName = ""{context.ArtifactName}Id"";
            var resultItems = srcEntities
                  .Where(e => (e.{context.ArtifactName}Id != value))
                  .OrderByDescending(e => e.{context.ArtifactName}Id);
            var safeRequest = new SafePaginationRequest()
            {{
                IncludeTotalCount = true,
                SortCriteria = new List<SortCriteria>()
                {{
                    new SortCriteria()
                    {{
                        PropertyName = propertyName,
                        SortDirection = SortDirection.Desc
                    }}
                }},
                FilterCriteria = new List<FilterCriteria>()
                {{
                    new FilterCriteria()
                    {{
                        PropertyName = propertyName,
                        Value = value.ToString(),
                        Operator = FilterOperator.NotEquals
                    }}
                }}
            }};
            var expectedResult = new PaginatedResult<{context.EntityName}>
            {{
                Items = resultItems,
                TotalCount = resultItems.Count()
            }};

            // Act
            var result = await _dao.GetPaginated(safeRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult.TotalCount, result.TotalCount);
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult.Items),
                JsonSerializer.Serialize(result.Items));
        }}

        [Fact]
        public async Task GetMany_ReturnsMatchingItems()
        {{
            // Arrange
            var srcEntities = _fixture.CreateMany<{context.EntityName}>(3).ToList();
            srcEntities.ForEach(async (entity) => await _dao.Create(entity));
            var filter = new FindManyArgs<{context.EntityName}, {context.QueryDtoName}>()
            {{
                Where = new {context.QueryDtoName}
                {{
                    {context.ArtifactName}Id = srcEntities[1].{context.ArtifactName}Id
                }}
            }};

            // Act
            var result = await _dao.GetMany(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.NotNull(result.FirstOrDefault());
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult.FirstOrDefault()),
                JsonSerializer.Serialize(result.FirstOrDefault()),
                true
            );
        }}

        [Fact]
        public async Task GetMany_ReturnsEmptyList_WhenNoIdsMatch()
        {{
            // Arrange
            var srcEntities = _fixture.CreateMany<{context.EntityName}>(3).ToList();
            srcEntities.ForEach(async (entity) => await _dao.Create(entity));
            var filter = new FindManyArgs<{context.EntityName}, {context.QueryDtoName}>()
            {{
                Where = new {context.QueryDtoName}
                {{
                    {context.ArtifactName}Id = -1
                }}
            }};

            // Act
            var result = await _dao.GetMany(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }}

        [Fact]
        public async Task GetMeta_ReturnsCorrectMetadata()
        {{
            // Arrange
            var srcEntities = _fixture.CreateMany<{context.EntityName}>(3).ToList();
            srcEntities.ForEach(async (entity) => await _dao.Create(entity));
            var filter = new FindManyArgs<{context.EntityName}, {context.QueryDtoName}>()
            {{
                Where = new {context.QueryDtoName}
                {{
                    {context.ArtifactName}Id = srcEntities[1].{context.ArtifactName}Id
                }}
            }};

            // Act
            var result = await _dao.GetMeta(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Count);
        }}
    }}
}}";
            }
*/
        }
    }
}
