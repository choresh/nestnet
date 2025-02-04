using NestNet.Cli.Infra;
using NestNet.Infra.Enums;
using Spectre.Console;
using static NestNet.Cli.Generators.AppGenerator.AppGenerator;
using System.Xml.Linq;

namespace NestNet.Cli.Generators.CoreGenerator
{
    internal static class CoreGenerator
    {
        public class InputParams
        {
            public DbType? DbType { get; set; }
        }

        private class CoreGenerationContext
        {
            public required string CurrentDir { get; set; }
            public required string BaseProjectName { get; set; }
            public required string CurrProjectName { get; set; }
            public required DbType DbType { get; set; }
            public required string CorePath { get; set; }
        }

        public static void Run(InputParams? inputParams = null)
        {
            try
            {
                AnsiConsole.MarkupLine(Helpers.FormatMessage("\nCore generation - started", "green"));

                var context = CreateCoreGenerationContext(inputParams);
                if (context == null)
                {
                    AnsiConsole.MarkupLine(Helpers.FormatMessage("\nCore generation - ended, unable to generate the app", "green"));
                    return;
                }

                var tarDataDir = Path.Combine(context.CorePath, "Data");
                Directory.CreateDirectory(tarDataDir);

                GenerateRootFiles(context);

                // Create core directory structure
                Directory.CreateDirectory(context.CorePath);
                GenerateProjectFile(context);
                GenerateAppDbContext(context, tarDataDir);
                GenerateAppRepository(context, tarDataDir);
                GenerateAppRepositoryTest(context, tarDataDir);

                AnsiConsole.MarkupLine(Helpers.FormatMessage("\nCore generation - ended successfully", "green"));
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine(Helpers.FormatMessage($"\nCore generation - failed ({ex.Message})", "red"));
            }
        }

        private static void GenerateRootFiles(CoreGenerationContext context)
        {
            var gitignoreContent = @"
bin
lib
            ";
            File.WriteAllText(Path.Combine(context.CurrentDir, ".gitignore"), gitignoreContent);
        }

        /*
        // ZZZ
        private static void CopyDocumentation(string projectRoot)
        {
            AnsiConsole.MarkupLine(Helpers.FormatMessage("\nCopy documentation started...", "green"));

            // Create Doc directory
            string docDir = Path.Combine(projectRoot, "Doc");
            Directory.CreateDirectory(docDir);

            // Copy documentation files
            CopyEmbeddedResource("README.md", Path.Combine(docDir, "README.md"));

            AnsiConsole.MarkupLine(Helpers.FormatMessage("Copy documentation ended", "green"));
        }

        private static void CopyEmbeddedResource(string resourceName, string targetPath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourcePath = $"NestNet.Cli.Data.Templates.Doc.{resourceName}";
            try
            {
                using (var stream = assembly.GetManifestResourceStream(resourcePath))
                {
                    if (stream == null)
                    {
                        throw new InvalidOperationException($"Could not find embedded resource: {resourcePath}");
                    }
                    using (var reader = new StreamReader(stream))
                    {
                        string content = reader.ReadToEnd();
                        File.WriteAllText(targetPath, content);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to copy resource {resourceName} to {targetPath}", ex);
            }
        }
        */


        private static void GenerateProjectFile(CoreGenerationContext context)
        {
            string dbPackage;
            switch (context.DbType)
            {
                case DbType.MsSql:
                    dbPackage = "Microsoft.EntityFrameworkCore.SqlServer";
                    break;
                case DbType.Postgres:
                    dbPackage = "Npgsql.EntityFrameworkCore.PostgreSQL";
                    break;
                default:
                    throw new ArgumentException($"Unsupported database type: {context.DbType}");
            }

            var csprojContent = $@"<Project Sdk=""Microsoft.NET.Sdk"">

    <PropertyGroup>
        <TargetFramework>net8.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include=""Microsoft.EntityFrameworkCore"" Version=""*"" />
        <PackageReference Include=""{dbPackage}"" Version=""*"" />

        <PackageReference Include=""Xunit"" Version=""*"" />
        <PackageReference Include=""Microsoft.TestPlatform.TestHost"" Version=""*"" />
        <PackageReference Include=""AutoFixture"" Version=""*"" />
        <PackageReference Include=""AutoFixture.AutoNSubstitute"" Version=""*"" />
        <PackageReference Include=""Microsoft.EntityFrameworkCore.InMemory"" Version=""*"" />
        <PackageReference Include=""NSubstitute"" Version=""*"" />
        <PackageReference Include=""xunit.runner.visualstudio"" Version=""*"">
            <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
            <PrivateAssets>all</PrivateAssets>
        </PackageReference>
        <PackageReference Include=""Microsoft.NET.Test.Sdk"" Version=""*"" />
    </ItemGroup>

</Project>";

            File.WriteAllText(Path.Combine(context.CorePath, $"{context.CurrProjectName}.csproj"), csprojContent);
        }

        private static void GenerateAppDbContext(CoreGenerationContext context, string tarDataDir)
        {
            var appDbContextContent = $@"using Microsoft.EntityFrameworkCore;
using NestNet.Infra.BaseClasses;
using System.Reflection;

namespace {context.CurrProjectName}.Data
{{
    public class AppDbContext : AppDbContextBase
    {{
         public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(
                options,
                [Assembly.GetExecutingAssembly()] // If your entities not located (only) at current assembly - customise here
            )
        {{
        }}
    }}
}}";

            File.WriteAllText(Path.Combine(tarDataDir, "AppDbContext.cs"), appDbContextContent);
        }

        private static void GenerateAppRepository(CoreGenerationContext context, string tarDataDir)
        {
            var appRepositoryContent = $@"#pragma warning disable IDE0290 // Use primary constructor
using NestNet.Infra.BaseClasses;
using NestNet.Infra.Attributes;

namespace {context.CurrProjectName}.Data
{{
    public interface IAppRepository : IAppRepositoryBase
    {{
        // If you add methods to derived class - expose them here.
    }}

    [Injectable(LifetimeType.Scoped)]
    public class AppRepository : AppRepositoryBase, IAppRepository
    {{
        public AppRepository(AppDbContext context)
            : base(context)
        {{
        }}

        // How to customise this class:
        // 1) You can add here 'custom' methods (methods for operations not supported by the base class).
        // 2) You can override here base class methods if needed:
        //    * Add methods with the same name and signature as in the base class.
        //    * For example:
        //      public virtual async Task<IEnumerable<TEntity>> GetAll<TEntity>() where TEntity : class, IEntity
        //      {{
        //          // Set your custom implementation here.
        //      }}
        // 3) In your methods:
        //    * Base class member '_context' are accessible.
        //    * Base class methods (e.g. 'await base.GetAll()') are accesible.
    }}
}}

#pragma warning restore IDE0290 // Use primary constructor";

            File.WriteAllText(Path.Combine(tarDataDir, "AppRepository.cs"), appRepositoryContent);
        }

        private static void GenerateAppRepositoryTest(CoreGenerationContext context, string tarDataDir)
        {
            var appRepositoryTestContent = $@"using Xunit;
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

namespace {context.CurrProjectName}.Data.Tests
{{
    public class AppRepositoryTests : IDisposable, IAsyncLifetime
    {{
        [Table(""Examples"")]
        private class ExampleEntity : EntityBase
        {{
            public override long Id
            {{
                get {{ return ExampleId; }}
                set {{ ExampleId = value; }}
            }}

            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public long ExampleId {{get; set;}}

            public required string Name {{ get; set; }}
        }}

        private class ExampleUpdateDto
        {{
            public string? Name {{ get; set; }}
        }}

        private class ExampleQueryDto
        {{
            public long? ExampleId {{ get; set; }}
            public string? Name {{ get; set; }}
        }}

        private readonly IFixture _fixture;
        private readonly AppDbContext _context;
        private readonly AppRepository _repository;

        public AppRepositoryTests()
        {{
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());

            // Create options for in-memory database
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $""TestDb_{{Guid.NewGuid()}}"")  // Unique name per test
                .Options;

            // Create real context with in-memory database
            _context = new AppDbContext(options);

            _repository = new AppRepository(_context);
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
            var srcEntities = _fixture.CreateMany<ExampleEntity>(3).ToList();
            srcEntities.ForEach(async (entity) => await _repository.Create(entity));

            // Act
            var result = await _repository.GetAll<ExampleEntity>();

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
            var srcEntities = _fixture.CreateMany<ExampleEntity>(3).ToList();
            srcEntities.ForEach(async (entity) => await _repository.Create(entity));

            // Act
            var result = await _repository.GetById<ExampleEntity>(srcEntities[1].ExampleId);

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
            var result = await _repository.GetById<ExampleEntity>(id);

            // Assert
            Assert.Null(result);
        }}

        [Fact]
        public async Task Create_ReturnsCreatedItem()
        {{
            // Arrange
            var srcEntity = _fixture.Create<ExampleEntity>();

            // Act
            await _repository.Create(srcEntity);

            // Assert
            var result = await _repository.GetById<ExampleEntity>(srcEntity.ExampleId);
            Assert.Equal(
              JsonSerializer.Serialize(srcEntity),
              JsonSerializer.Serialize(result));
        }}

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_True_ReturnsUpdatedItem_WhenExists()
        {{
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
        }}

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_True_ReturnsNull_WhenNotExists()
        {{
            // Arrange
            var ignoreMissingOrNullFields = true;
            var id = _fixture.Create<long>();
            var updateDto = _fixture.Create<ExampleUpdateDto>();

            // Act
            var result = await _repository.Update<ExampleUpdateDto, ExampleEntity>(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.Null(result);
        }}

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_False_ReturnsUpdatedItem_WhenExists()
        {{
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
        }}

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_False_ReturnsNull_WhenNotExists()
        {{
            // Arrange
            var ignoreMissingOrNullFields = false;
            var id = _fixture.Create<long>();
            var updateDto = _fixture.Create<ExampleUpdateDto>();

            // Act
            var result = await _repository.Update<ExampleUpdateDto, ExampleEntity>(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.Null(result);
        }}

        [Fact]
        public async Task Delete_ReturnsTrue_WhenExists()
        {{
            // Arrange
            var srcEntities = _fixture.CreateMany<ExampleEntity>(3).ToList();
            srcEntities.ForEach(async (entity) => await _repository.Create(entity));

            // Act
            var found = await _repository.Delete<ExampleEntity>(srcEntities[1].ExampleId);

            // Assert
            Assert.True(found);
        }}

        [Fact]
        public async Task Delete_ReturnsFalse_WhenNotExists()
        {{
            // Arrange
            var id = _fixture.Create<long>();

            // Act
            var found = await _repository.Delete<ExampleEntity>(id);

            // Assert
            Assert.False(found);
        }}

        [Fact]
        public async Task GetPaginated_ReturnsPaginatedItems()
        {{
            // Arrange
            var srcEntities = _fixture.CreateMany<ExampleEntity>(3)
               .Select((entity, index) =>
               {{
                   entity.ExampleId = index + 1;
                   return entity;
               }})
               .ToList();
            srcEntities.ForEach(async (entity) => await _repository.Create(entity));
            var value = srcEntities[1].ExampleId;
            var propertyName = ""ExampleId"";
            var resultItems = srcEntities
                  .Where(e => e.ExampleId != value)
                  .OrderByDescending(e => e.ExampleId);
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
            var expectedResult = new PaginatedResult<ExampleEntity>
            {{
                Items = resultItems,
                TotalCount = resultItems.Count()
            }};

            // Act
            var result = await _repository.GetPaginated<ExampleEntity>(safeRequest);

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
            var srcEntities = _fixture.CreateMany<ExampleEntity>(3).ToList();
            srcEntities.ForEach(async (entity) => await _repository.Create(entity));
            var filter = new FindManyArgs<ExampleEntity, ExampleQueryDto>()
            {{
                Where = new ExampleQueryDto
                {{
                    ExampleId = srcEntities[1].ExampleId
                }}
            }};

            // Act
            var result = await _repository.GetMany(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(srcEntities[1], result.FirstOrDefault());
        }}

        [Fact]
        public async Task GetMany_ReturnsEmptyList_WhenNoIdsMatch()
        {{
            // Arrange
            var srcEntities = _fixture.CreateMany<ExampleEntity>(3).ToList();
            srcEntities.ForEach(async (entity) => await _repository.Create(entity));
            var filter = new FindManyArgs<ExampleEntity, ExampleQueryDto>()
            {{
                Where = new ExampleQueryDto
                {{
                    ExampleId = -1
                }}
            }};

            // Act
            var result = await _repository.GetMany(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }}

        [Fact]
        public async Task GetMeta_ReturnsCorrectMetadata()
        {{
            // Arrange
            var srcEntities = _fixture.CreateMany<ExampleEntity>(3).ToList();
            srcEntities.ForEach(async (entity) => await _repository.Create(entity));
            var filter = new FindManyArgs<ExampleEntity, ExampleQueryDto>()
            {{
                Where = new ExampleQueryDto
                {{
                    ExampleId = srcEntities[1].ExampleId
                }}
            }};

            // Act
            var result = await _repository.GetMeta(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Count);
        }}
    }}
}}";

            var tarDataTestsDir = Path.Combine(tarDataDir, "Tests");
            Directory.CreateDirectory(tarDataTestsDir);
            File.WriteAllText(Path.Combine(tarDataTestsDir, "AppRepositoryTests.cs"), appRepositoryTestContent);
        }

        private static CoreGenerationContext? CreateCoreGenerationContext(InputParams? inputParams = null)
        {
            var dbType = DbType.MsSql; // Default value
            var currentDir = Directory.GetCurrentDirectory();

            // Get project name from root folder and add Core suffix
            var rootFolderName = new DirectoryInfo(currentDir).Name;
            var baseProjectName = rootFolderName;
            var currProjectName = $"{baseProjectName}.Core";
            var corePath = Path.Combine(currentDir, currProjectName);

            if (!Helpers.CheckTarDir(corePath))
            {
                return null;
            }

            if (inputParams == null)
            {
                var dbChoice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select database type:")
                        .AddChoices("MSSQL", "PostgreSQL", "Exit"));

                switch (dbChoice)
                {
                    case "MSSQL":
                        dbType = DbType.MsSql;
                        break;
                    case "PostgreSQL":
                        dbType = DbType.Postgres;
                        break;
                    case "Exit":
                        return null;
                }
            }
            else if (inputParams.DbType.HasValue)
            {
                dbType = inputParams.DbType.Value;
            }

            return new CoreGenerationContext
            {
                CurrentDir = currentDir,
                BaseProjectName = baseProjectName,
                CurrProjectName = currProjectName,
                DbType = dbType,
                CorePath = corePath
            };
        }

        /*
        // ZZZ!
        private static void AddDtoGenerator(AppGenerationContext context)
        {
            AnsiConsole.MarkupLine(Helpers.FormatMessage("\nPre-build event that runs the Dtos generator will be added to the project...", "green"));

            var csprojPath = Path.Combine(context.CurrentDir, $"{context.BaseProjectName}.csproj");
            var doc = XDocument.Load(csprojPath);
            if (doc.Root == null)
            {
                AnsiConsole.MarkupLine(Helpers.FormatMessage("\nPre-build event that runs the Dtos generator cannot be added to the project...", "red"));
                return;
            }
            var ns = doc.Root.Name.Namespace;

            var propertyGroup = doc.Root.Elements(ns + "PropertyGroup").First();
            propertyGroup.Add(new XElement(ns + "GenerateDocumentationFile", "true"));

            var target = new XElement(ns + "Target",
               new XAttribute("Name", "PostBuild"),
               new XAttribute("AfterTargets", "PostBuildEvent"),
               new XElement(ns + "Exec",
                   new XAttribute("Command", "nestnet.exe dtos --tar-dir \\Dtos --no-console")
               )
           );

            doc.Root.Add(target);
            doc.Save(csprojPath);

            AnsiConsole.MarkupLine(Helpers.FormatMessage($"Updated: {csprojPath}", "grey"));

            AnsiConsole.MarkupLine(Helpers.FormatMessage("Pre-build event that runs the Dtos generator has been added to the project", "green"));
        }
        */
    }
}