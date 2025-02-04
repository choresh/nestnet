using NestNet.Cli.Generators.Common;
using NestNet.Cli.Infra;
using Spectre.Console;

namespace NestNet.Cli.Generators.ModuleGenerator
{
    internal class CoreModuleGenerator : MultiProjectsGeneratorBase<ModuleGenerationContext>
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
            string dtoContent = Helpers.GetDtoContent(Context.ProjectContext!.ProjectName, Context.ArtifactName, Context.PluralizedModuleName, Context.ArtifactName + ".Entity", dtoType, baseClass, properties);
            string dtoPath = Path.Combine(Context.ProjectContext!.TargetPath, "Dtos", $"{Helpers.FormatDtoName(Context.ArtifactName, dtoType)}.cs");
            Directory.CreateDirectory(GetDirectoryName(dtoPath));
            File.WriteAllText(dtoPath, dtoContent);
            AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {dtoPath}", "grey"));
        }

        private void CreateServiceFile()
        {
            string serviceContent = GetServiceContent();
            string servicePath = Path.Combine(Context.ProjectContext!.TargetPath, "Services", $"{Context.PluralizedModuleName}Service.cs");
            Directory.CreateDirectory(GetDirectoryName(servicePath));
            File.WriteAllText(servicePath, serviceContent);
            AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {servicePath}", "grey"));
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

        private string GetServiceContent()
        {
            return $@"#pragma warning disable IDE0290 // Use primary constructor
using NestNet.Infra.BaseClasses;
using NestNet.Infra.Attributes;
using NestNet.Infra.Interfaces;
using {Context.ProjectContext!.ProjectName}.Modules.{Context.PluralizedModuleName}.Dtos;
using {Context.ProjectContext!.ProjectName}.Modules.{Context.PluralizedModuleName}.Entities;
using {Context.ProjectContext!.ProjectName}.Data;

namespace {Context.ProjectContext!.ProjectName}.Modules.{Context.PluralizedModuleName}.Services
{{
    public interface I{Context.PluralizedModuleName}Service: ICrudService<{Context.EntityName}, {Context.CreateDtoName}, {Context.UpdateDtoName}, {Context.ResultDtoName}, {Context.QueryDtoName}>
    {{
        // If you add methods to derived class - expose them here.
    }}

    [Injectable(LifetimeType.Scoped)]
    public class {Context.PluralizedModuleName}Service : CrudServiceBase<{Context.EntityName}, {Context.CreateDtoName}, {Context.UpdateDtoName}, {Context.ResultDtoName}, {Context.QueryDtoName}>, I{Context.PluralizedModuleName}Service
    {{
        public {Context.PluralizedModuleName}Service(IAppRepository appRepository)
            : base(appRepository)
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
        //    * Base class member '_repository' is accessible.
        //    * Base class methods (e.g. 'await base.GetAll()') are accesible.
    }}
}}

#pragma warning restore IDE0290 // Use primary constructor";
        }

        private string GetServiceTestContent()
        {
            return $@"using NSubstitute;
using Xunit;
using NestNet.Infra.Query;
using NestNet.Infra.Paginatation;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using System.Text.Json;
using {Context.ProjectContext!.ProjectName}.Modules.{Context.PluralizedModuleName}.Dtos;
using {Context.ProjectContext!.ProjectName}.Modules.{Context.PluralizedModuleName}.Services;
using {Context.ProjectContext!.ProjectName}.Modules.{Context.PluralizedModuleName}.Entities;
using {Context.ProjectContext!.ProjectName}.Data;

namespace {Context.ProjectContext!.ProjectName}.Modules.{Context.PluralizedModuleName}.Tests.Services
{{
    public class {Context.PluralizedModuleName}ServiceTests
    {{
        private readonly IFixture _fixture;
        private readonly IAppRepository _repository;
        private readonly {Context.PluralizedModuleName}Service _service;

        public {Context.PluralizedModuleName}ServiceTests()
        {{
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            _repository = _fixture.Freeze<IAppRepository>();
            _service = new {Context.PluralizedModuleName}Service(_repository);
        }}

        [Fact]
        public async Task GetAll_ReturnsAllItems()
        {{
            // Arrange
            var srcEntities = _fixture.CreateMany<{Context.EntityName}>(3).ToList();
            var expectedResult = _service.ToResultDtos(srcEntities);
            _repository.GetAll<{Context.EntityName}>().Returns(Task.FromResult<IEnumerable<{Context.EntityName}>>(srcEntities));

            // Act
            var result = await _service.GetAll();

            // Assert
            Assert.Equal(expectedResult.Count(), result.Count());
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult),
                JsonSerializer.Serialize(result));
            await _repository.Received(1).GetAll<{Context.EntityName}>();
        }}

        [Fact]
        public async Task GetById_ReturnsItem_WhenExists()
        {{
            // Arrange
            var id = _fixture.Create<long>();
            var entity = _fixture.Create<{Context.EntityName}>();
            var expectedResult = _service.ToResultDto(entity);
            _repository.GetById<{Context.EntityName}>(Arg.Any<long>()).Returns(Task.FromResult<{Context.EntityName}?>(entity));

            // Act
            var result = await _service.GetById(id);

            // Assert
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult),
                JsonSerializer.Serialize(result));
            await _repository.Received(1).GetById<{Context.EntityName}>(id);
        }}

        [Fact]
        public async Task GetById_ReturnsNull_WhenNotExists()
        {{
            // Arrange
            var id = _fixture.Create<long>();
            _repository.GetById<{Context.EntityName}>(Arg.Any<long>()).Returns(Task.FromResult<{Context.EntityName}?>(null));

            // Act
            var result = await _service.GetById(id);

            // Assert
            Assert.Null(result);
            await _repository.Received(1).GetById<{Context.EntityName}>(id);
        }}

        [Fact]
        public async Task Create_ReturnsCreatedItem()
        {{
            // Arrange
            var createDto = _fixture.Create<{Context.CreateDtoName}>();
            var createdEntity = _service.ToEntity(createDto);
            var expectedResult = _service.ToResultDto(createdEntity);
            _repository.Create(Arg.Any<{Context.EntityName}>()).Returns(Task.FromResult(createdEntity));

            // Act
            var result = await _service.Create(createDto);

            // Assert
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult),
                JsonSerializer.Serialize(result.ResultDto));
            await _repository.Received(1).Create(Arg.Any<{Context.EntityName}>());
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
            _repository.Update<{Context.UpdateDtoName}, {Context.EntityName}>(Arg.Any<long>(), Arg.Any<{Context.UpdateDtoName}>(), Arg.Any<bool>()).Returns(Task.FromResult<{Context.EntityName}?>(updatedEntity));

            // Act
            var result = await _service.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult),
                JsonSerializer.Serialize(result));
            await _repository.Received(1).Update<{Context.UpdateDtoName}, {Context.EntityName}>(id, updateDto, ignoreMissingOrNullFields);
        }}

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_True_ReturnsNull_WhenNotExists()
        {{
            // Arrange
            var ignoreMissingOrNullFields = true;
            var id = _fixture.Create<long>();
            var updateDto = _fixture.Create<{Context.UpdateDtoName}>();
            _repository.Update<{Context.UpdateDtoName}, {Context.EntityName}>(Arg.Any<long>(), Arg.Any<{Context.UpdateDtoName}>(), Arg.Any<bool>()).Returns(Task.FromResult<{Context.EntityName}?>(null));

            // Act
            var result = await _service.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.Null(result);
            await _repository.Received(1).Update<{Context.UpdateDtoName}, {Context.EntityName}>(id, updateDto, ignoreMissingOrNullFields);
        }}

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_False_ReturnsUpdatedItem_WhenExists()
        {{
            // Arrange
            var ignoreMissingOrNullFields = false;
            var id = _fixture.Create<long>();
            var updateDto = _fixture.Create<{Context.UpdateDtoName}>();
            var updatedEntity = _service.ToEntity(updateDto);
            var expectedResult = _service.ToResultDto(updatedEntity);
            _repository.Update<{Context.UpdateDtoName}, {Context.EntityName}>(Arg.Any<long>(), Arg.Any<{Context.UpdateDtoName}>(), Arg.Any<bool>()).Returns(Task.FromResult<{Context.EntityName}?>(updatedEntity));

            // Act
            var result = await _service.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult),
                JsonSerializer.Serialize(result));
            await _repository.Received(1).Update<{Context.UpdateDtoName}, {Context.EntityName}>(id, updateDto, ignoreMissingOrNullFields);
        }}

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_False_ReturnsNull_WhenNotExists()
        {{
            // Arrange
            var ignoreMissingOrNullFields = false;
            var id = _fixture.Create<long>();
            var updateDto = _fixture.Create<{Context.UpdateDtoName}>();
            _repository.Update<{Context.UpdateDtoName}, {Context.EntityName}>(Arg.Any<long>(), Arg.Any<{Context.UpdateDtoName}>(), Arg.Any<bool>()).Returns(Task.FromResult<{Context.EntityName}?>(null));

            // Act
            var result = await _service.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.Null(result);
            await _repository.Received(1).Update<{Context.UpdateDtoName}, {Context.EntityName}>(id, updateDto, ignoreMissingOrNullFields);
        }}

        [Fact]
        public async Task Delete_ReturnsTrue_WhenExists()
        {{
            // Arrange
            var id = _fixture.Create<long>();
            var entity = _fixture.Create<{Context.EntityName}>();
            _repository.Delete<{Context.EntityName}>(Arg.Any<long>()).Returns(Task.FromResult(true));

            // Act
            var found = await _service.Delete(id);

            // Assert
            Assert.True(found);
            await _repository.Received(1).Delete<{Context.EntityName}>(id);
        }}

        [Fact]
        public async Task Delete_ReturnsFalse_WhenNotExists()
        {{
            // Arrange
            var id = _fixture.Create<long>();
            _repository.Delete<{Context.EntityName}>(Arg.Any<long>()).Returns(Task.FromResult(false));

            // Act
            var found = await _service.Delete(id);

            // Assert
            Assert.False(found);
            await _repository.Received(1).Delete<{Context.EntityName}>(id);
        }}

        [Fact]
        public async Task GetPaginated_ReturnsPaginatedItems()
        {{
            // Arrange
            var propertyName = ""{Context.ParamName}Id"";
            var safeRequest = new SafePaginationRequest()
            {{
                SortCriteria = new List<SortCriteria>()
                {{
                    new SortCriteria()
                    {{
                        PropertyName = propertyName,
                        SortDirection = SortDirection.Asc
                    }}
                }},
                FilterCriteria = new List<FilterCriteria>()
                {{
                    new FilterCriteria()
                    {{
                        PropertyName = propertyName,
                        Value = ""1"",
                        Operator = FilterOperator.NotEquals
                    }}
                }}
            }};

            var srcEntities = _fixture.CreateMany<{Context.EntityName}>(3)
              .Select((entity, index) => {{
                  entity.{Context.ArtifactName}Id = index;
                  return entity;
              }})
              .ToList();

            var unsafeRequest = new UnsafePaginationRequest()
            {{
                PageNumber = safeRequest.PageNumber,
                PageSize = safeRequest.PageSize,
                IncludeTotalCount = safeRequest.IncludeTotalCount,
                SortBy = safeRequest.SortCriteria.Select(c => c.PropertyName).ToArray(),
                SortDirection = safeRequest.SortCriteria.Select(c => c.SortDirection.ToString()).ToArray(),
                FilterBy = safeRequest.FilterCriteria.Select(f => f.PropertyName).ToArray(),
                FilterOperator = safeRequest.FilterCriteria.Select(f => f.Operator.ToString()).ToArray(),
                FilterValue = safeRequest.FilterCriteria.Select(f => f.Value).ToArray()
            }};

            var repositoryResult = new PaginatedResult<{Context.EntityName}>
            {{
                Items = srcEntities,
                TotalCount = srcEntities.Count()
            }};

            var expectedResult = _service.ToPaginatedResultDtos(repositoryResult);
            _repository.GetPaginated<{Context.EntityName}>(Arg.Any<SafePaginationRequest>()).Returns(Task.FromResult(repositoryResult));

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
        }}

        [Fact]
        public async Task GetPaginated_IncompleteCriteria_ReturnsParametersError()
        {{
            // Arrange
            var propertyName = ""{Context.ArtifactName}Id"";
            var safeRequest = new SafePaginationRequest()
            {{
                SortCriteria = new List<SortCriteria>()
                {{
                    new SortCriteria()
                    {{
                        PropertyName = propertyName,
                        SortDirection = SortDirection.Asc
                    }}
                }},
                FilterCriteria = new List<FilterCriteria>()
                {{
                    new FilterCriteria()
                    {{
                        PropertyName = propertyName,
                        Value = ""1"",
                        Operator = FilterOperator.NotEquals
                    }}
                }}
            }};
            var srcEntities = _fixture.CreateMany<{Context.EntityName}>(3).ToList();
            var unsafeRequest = new UnsafePaginationRequest()
            {{
                PageNumber = safeRequest.PageNumber,
                PageSize = safeRequest.PageSize,
                IncludeTotalCount = safeRequest.IncludeTotalCount,
                // SortBy = safeRequest.SortCriteria.Select(c => c.PropertyName).ToArray(),
                SortDirection = safeRequest.SortCriteria.Select(c => c.SortDirection.ToString()).ToArray(),
                // FilterBy = safeRequest.FilterCriteria.Select(f => f.PropertyName).ToArray(),
                FilterOperator = safeRequest.FilterCriteria.Select(f => f.Operator.ToString()).ToArray(),
                FilterValue = safeRequest.FilterCriteria.Select(f => f.Value).ToArray()
            }};
            var repositoryResult = new PaginatedResult<{Context.EntityName}>
            {{
                Items = srcEntities,
                TotalCount = srcEntities.Count()
            }};
            _repository.GetPaginated<{Context.EntityName}>(Arg.Any<SafePaginationRequest>()).Returns(Task.FromResult(repositoryResult));

            // Act
            var result = await _service.GetPaginated(unsafeRequest);

            // Assert
            Assert.Null(result.Data);
            Assert.NotNull(result.Error);
            Assert.Contains(""Sort criteria is incomplete"", result.Error);
            Assert.Contains(""Filter criteria is incomplete"", result.Error);
        }}

        [Fact]
        public async Task GetPaginated_InvalidEnumValue_ReturnsParametersError()
        {{
            // Arrange
            var propertyName = ""{Context.ArtifactName}Id"";
            var safeRequest = new SafePaginationRequest()
            {{
                SortCriteria = new List<SortCriteria>()
                {{
                    new SortCriteria()
                    {{
                        PropertyName = propertyName,
                        SortDirection = SortDirection.Asc
                    }}
                }},
                FilterCriteria = new List<FilterCriteria>()
                {{
                    new FilterCriteria()
                    {{
                        PropertyName = propertyName,
                        Value = ""1"",
                        Operator = FilterOperator.NotEquals
                    }}
                }}
            }};
            var srcEntities = _fixture.CreateMany<{Context.EntityName}>(3).ToList();
            var unsafeRequest = new UnsafePaginationRequest()
            {{
                PageNumber = safeRequest.PageNumber,
                PageSize = safeRequest.PageSize,
                IncludeTotalCount = safeRequest.IncludeTotalCount,
                SortBy = safeRequest.SortCriteria.Select(c => c.PropertyName).ToArray(),
                SortDirection = safeRequest.SortCriteria.Select(c => c.SortDirection.ToString() + ""Blabla"").ToArray(),
                FilterBy = safeRequest.FilterCriteria.Select(f => f.PropertyName).ToArray(),
                FilterOperator = safeRequest.FilterCriteria.Select(f => f.Operator.ToString() + ""Blabla"").ToArray(),
                FilterValue = safeRequest.FilterCriteria.Select(f => f.Value).ToArray()
            }};
            var repositoryResult = new PaginatedResult<{Context.EntityName}>
            {{
                Items = srcEntities,
                TotalCount = srcEntities.Count()
            }};
            _repository.GetPaginated<{Context.EntityName}>(Arg.Any<SafePaginationRequest>()).Returns(Task.FromResult(repositoryResult));

            // Act
            var result = await _service.GetPaginated(unsafeRequest);

            // Assert
            Assert.Null(result.Data);
            Assert.NotNull(result.Error);
            Assert.Contains(""Invalid sort direction"", result.Error);
            Assert.Contains(""Invalid filter operator"", result.Error);
        }}

        [Fact]
        public async Task GetPaginated_InvalidPropertyName_ReturnsParametersError()
        {{
            // Arrange
            var propertyName = ""{Context.ArtifactName}Id"";
            var safeRequest = new SafePaginationRequest()
            {{
                SortCriteria = new List<SortCriteria>()
                {{
                    new SortCriteria()
                    {{
                        PropertyName = propertyName,
                        SortDirection = SortDirection.Asc
                    }}
                }},
                FilterCriteria = new List<FilterCriteria>()
                {{
                    new FilterCriteria()
                    {{
                        PropertyName = propertyName,
                        Value = ""1"",
                        Operator = FilterOperator.NotEquals
                    }}
                }}
            }};
            var unsafeRequest = new UnsafePaginationRequest()
            {{
                PageNumber = safeRequest.PageNumber,
                PageSize = safeRequest.PageSize,
                IncludeTotalCount = safeRequest.IncludeTotalCount,
                SortBy = safeRequest.SortCriteria.Select(c => c.PropertyName + ""Blabla"").ToArray(),
                SortDirection = safeRequest.SortCriteria.Select(c => c.SortDirection.ToString()).ToArray(),
                FilterBy = safeRequest.FilterCriteria.Select(f => f.PropertyName + ""Blabla"").ToArray(),
                FilterOperator = safeRequest.FilterCriteria.Select(f => f.Operator.ToString()).ToArray(),
                FilterValue = safeRequest.FilterCriteria.Select(f => f.Value).ToArray()
            }};
            var srcEntities = _fixture.CreateMany<{Context.EntityName}>(3).ToList();
            var repositoryResult = new PaginatedResult<{Context.EntityName}>
            {{
                Items = srcEntities,
                TotalCount = srcEntities.Count()
            }};
            _repository.GetPaginated<{Context.EntityName}>(Arg.Any<SafePaginationRequest>()).Returns(Task.FromResult(repositoryResult));

            // Act
            var result = await _service.GetPaginated(unsafeRequest);

            // Assert
            Assert.Null(result.Data);
            Assert.NotNull(result.Error);
            Assert.Contains(""Invalid sort properties"", result.Error);
            Assert.Contains(""Invalid filter properties"", result.Error);
        }}

        [Fact]
        public async Task GetMany_ReturnsMatchingItems()
        {{
            // Arrange
            var srcEntities = _fixture.CreateMany<{Context.EntityName}>(3).ToList();
            var expectedResult = _service.ToResultDtos([srcEntities.ToArray()[1]]);
            _repository.GetMany(Arg.Any<FindManyArgs<{Context.EntityName}, {Context.QueryDtoName}>>()).Returns(Task.FromResult<IEnumerable<{Context.EntityName}>>([srcEntities.ToArray()[1]]));
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
            await _repository.Received(1).GetMany(filter);
        }}

        [Fact]
        public async Task GetMany_ReturnsEmptyList_WhenNoIdsMatch()
        {{
            // Arrange
            _repository.GetMany(Arg.Any<FindManyArgs<{Context.EntityName}, {Context.QueryDtoName}>>()).Returns(Task.FromResult<IEnumerable<{Context.EntityName}>>([]));
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
            await _repository.Received(1).GetMany(filter);
        }}

        [Fact]
        public async Task GetMeta_ReturnsCorrectMetadata()
        {{
            // Arrange
            var count = _fixture.Create<long>();
            _repository.GetMeta(Arg.Any<FindManyArgs<{Context.EntityName}, {Context.QueryDtoName}>>()).Returns(Task.FromResult(new MetadataDto() {{ Count = count }}));
            var filter = new FindManyArgs<{Context.EntityName}, {Context.QueryDtoName}>()
            {{
                Where = new {Context.QueryDtoName}
                {{
                    {Context.ArtifactName}Id = -1
                }}
            }};

            // Act
            var result = await _service.GetMeta(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(count, result.Count);
            await _repository.Received(1).GetMeta(filter);
        }}
    }}
}}";
        }
    }
}