using NestNet.Cli.Infra;
using NestNet.Infra.Helpers;
using Spectre.Console;
using System.Text.RegularExpressions;

namespace NestNet.Cli.Generators
{
    internal static class ModuleGenerator
    {
        public class InputParams
        {
            /// <summary>
            /// Name of the module.
            /// </summary>
            public required string ModuleName { get; set; }

            /// <summary>
            /// Pluralized name of the module.
            /// </summary>
            public required string PluralizedModuleName { get; set; }

            /// <summary>
            /// Generate database support (entity + dao).
            /// </summary>
            public bool GenerateDbSupport { get; set; } = true;

            /// <summary>
            /// Generate service.
            /// </summary>
            public bool GenerateService { get; set; } = true;

            /// <summary>
            /// Generate controlle.
            /// </summary>
            public bool GenerateController { get; set; } = true;
        }

        private class ModuleGenerationContext
        {
            public required string CurrentDir { get; set; }
            public required string ProjectName { get; set; }
            public required string ModuleName { get; set; }
            public required string PluralizedModuleName { get; set; }
            public required string ParamName { get; set; }
            public required string PluralizedParamName { get; set; }
            public required string KebabCasePluralizedModuleName { get; set; }
            public required string EntityFullName { get; set; }
            public required string NullableEntityFullName { get; set; }
            public required string ModulePath { get; set; }
            public required string CreateDtoName { get; set; }
            public required string UpdateDtoName { get; set; }
            public required string ResultDtoName { get; set; }
            public required string QueryDtoName { get; set; }
            public required bool GenerateDbSupport { get; set; }
            public required bool GenerateService { get; set; }
            public required bool GenerateController { get; set; }
         }

        public static void Run(InputParams? inputParams = null)
        {
            AnsiConsole.MarkupLine(Helpers.FormatMessage("\nModule generation - started\n", "green"));

            try
            {
                ModuleGenerationContext? context;
                if (inputParams != null)
                {
                    // Silent mode
                    context = CreateSilentModuleGenerationContext(inputParams);
                }
                else
                {
                    // Interactive mode
                    context = CreateInteractiveModuleGenerationContext();
                }
                if (context == null || !Helpers.CheckTarDir(context.ModulePath))
                {
                    AnsiConsole.MarkupLine(Helpers.FormatMessage("\nModule generation - ended, unable to generate the module", "green"));
                    return;
                }
                CreateModuleFiles(context);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine(Helpers.FormatMessage($"\nModule generation - failed ({ex.Message})", "red"));
                return;
            }

            AnsiConsole.MarkupLine(Helpers.FormatMessage("\nModule generation - ended successfully", "green"));
        }

        private static ModuleGenerationContext? CreateSilentModuleGenerationContext(InputParams inputParams)
        {
            var (currentDir, projectName) = Helpers.GetProjectInfo();
            if (currentDir == null || projectName == null)
            {
                return null;
            }

            var paramName = StringHelper.ToCamelCase(inputParams.ModuleName);
            var pluralizedParamName = StringHelper.ToCamelCase(inputParams.PluralizedModuleName);
            var kebabCasePluralizedModuleName = Helpers.ToKebabCase(inputParams.PluralizedModuleName);
            var entityFullName = $"Entities.{inputParams.ModuleName}";
            var nullableEntityFullName = $"{entityFullName}?";
            string modulePath = Path.Combine(currentDir, "Modules", inputParams.PluralizedModuleName);

            return new ModuleGenerationContext
            {
                CurrentDir = currentDir,
                ProjectName = projectName,
                ModuleName = inputParams.ModuleName,
                PluralizedModuleName = inputParams.PluralizedModuleName,
                ParamName = paramName,
                PluralizedParamName = pluralizedParamName,
                KebabCasePluralizedModuleName = kebabCasePluralizedModuleName,
                EntityFullName = entityFullName,
                NullableEntityFullName = nullableEntityFullName,
                ModulePath = modulePath,
                CreateDtoName = Helpers.FormatDtoName(inputParams.ModuleName, DtoType.Create),
                UpdateDtoName = Helpers.FormatDtoName(inputParams.ModuleName, DtoType.Update),
                ResultDtoName = Helpers.FormatDtoName(inputParams.ModuleName, DtoType.Result),
                QueryDtoName = Helpers.FormatDtoName(inputParams.ModuleName, DtoType.Query),
                GenerateController = inputParams.GenerateController,
                GenerateService = inputParams.GenerateService,
                GenerateDbSupport = inputParams.GenerateDbSupport
            };
        }

        private static ModuleGenerationContext? CreateInteractiveModuleGenerationContext()
        {
            string moduleName = GetModuleName();
            var (currentDir, projectName) = Helpers.GetProjectInfo();
            if (currentDir == null || projectName == null)
            {
                return null;
            }

            // First ask about DB support since it's the foundation
            bool generateDbSupport = true; // AnsiConsole.Confirm("Generate database support (entity + dao)?", true);

            // Only ask about service if DB support is enabled
            bool generateService = false;
            if (generateDbSupport)
            {
                generateService = AnsiConsole.Confirm("Generate service?", true);
            }

            // Only ask about controller if service is enabled
            bool generateController = false;
            if (generateService)
            {
                generateController = AnsiConsole.Confirm("Generate controller?", true);
            }

            string pluralizedModuleName = GetPluralizedModuleName(moduleName);
            var paramName = StringHelper.ToCamelCase(moduleName);
            var pluralizedParamName = StringHelper.ToCamelCase(pluralizedModuleName);
            var kebabCasePluralizedModuleName = Helpers.ToKebabCase(pluralizedModuleName);
            var entityFullName = $"Entities.{moduleName}";
            var nullableEntityFullName = $"{entityFullName}?";
            string modulePath = Path.Combine(currentDir, "Modules", pluralizedModuleName);
            return new ModuleGenerationContext
            {
                CurrentDir = currentDir,
                ProjectName = projectName,
                ModuleName = moduleName,
                PluralizedModuleName = pluralizedModuleName,
                ParamName = paramName,
                PluralizedParamName = pluralizedParamName,
                KebabCasePluralizedModuleName = kebabCasePluralizedModuleName,
                EntityFullName = entityFullName,
                NullableEntityFullName = nullableEntityFullName,
                ModulePath = modulePath,
                CreateDtoName = Helpers.FormatDtoName(moduleName, DtoType.Create),
                UpdateDtoName = Helpers.FormatDtoName(moduleName, DtoType.Update),
                ResultDtoName = Helpers.FormatDtoName(moduleName, DtoType.Result),
                QueryDtoName = Helpers.FormatDtoName(moduleName, DtoType.Query),
                GenerateDbSupport = generateDbSupport,
                GenerateService = generateService,
                GenerateController = generateController
            };
        }

        private static string GetPluralizedModuleName(string moduleName)
        {
            return AnsiConsole.Prompt(
                new TextPrompt<string>("Enter the pluralized module name (press Enter to except the shown default)")
                .DefaultValue(moduleName + "s")
                .PromptStyle("green")
                .ValidationErrorMessage(Helpers.FormatMessage("That's not a valid pluralized module name", "red"))
                .Validate(ValidateResorceName));
        }

        private static string GetModuleName()
        {
            return AnsiConsole.Prompt(
                new TextPrompt<string>("Enter the module name (singular, capitalized, with alphanumeric\n characters only. e.g. 'Product', 'CardHolder', etc.):")
                .PromptStyle("green")
                .ValidationErrorMessage(Helpers.FormatMessage("That's not a valid module name", "red"))
                .Validate(ValidateResorceName));
        }

        private static ValidationResult ValidateResorceName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return ValidationResult.Error("Module name cannot be empty.");
            }
            if (!char.IsUpper(input[0]))
            {
                return ValidationResult.Error("Module name must start with a capital letter.");
            }
            if (!Regex.IsMatch(input, "^[A-Z][a-zA-Z0-9]*$"))
            {
                return ValidationResult.Error("Module name must contain alphanumeric characters only.");
            }
            return ValidationResult.Success();
        }

        private static void CreateModuleFiles(ModuleGenerationContext context)
        {
            if (context.GenerateDbSupport)
            {
                CreateEntityFile(context);
                foreach (DtoType dtoType in Enum.GetValues(typeof(DtoType)))
                {
                    CreateDtoFile(context, dtoType);
                }
                CreateDaoFile(context);
                CreateDaoTestFile(context);
            }

            if (context.GenerateService)
            {
                CreateServiceFile(context);
                CreateServiceTestFile(context);
            }

            if (context.GenerateController)
            {
                CreateControllerFile(context);
                CreateControllerTestFile(context);
            }
        }

        private static void CreateEntityFile(ModuleGenerationContext context)
        {
            string entityContent = GetEntityContent(context);
            string entityPath = Path.Combine(context.ModulePath, "Entities", $"{context.ModuleName}.cs");
            Directory.CreateDirectory(GetDirectoryName(entityPath));
            File.WriteAllText(entityPath, entityContent);
            AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {entityPath}", "grey"));
        }

        private static void CreateDtoFile(ModuleGenerationContext context, DtoType dtoType, Type? baseClass = null)
        {
            string dtoContent = Helpers.GetDtoContent(context.ProjectName, context.ModuleName, context.PluralizedModuleName, dtoType, baseClass);
            string dtoPath = Path.Combine(context.ModulePath, "Dtos", $"{Helpers.FormatDtoName(context.ModuleName, dtoType)}.cs");
            Directory.CreateDirectory(GetDirectoryName(dtoPath));
            File.WriteAllText(dtoPath, dtoContent);
            AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {dtoPath}", "grey"));
        }

        private static void CreateDaoFile(ModuleGenerationContext context)
        {
            string daoContent = GetDaoContent(context);
            string daoPath = Path.Combine(context.ModulePath, "Daos", $"{context.ModuleName}Dao.cs");
            Directory.CreateDirectory(GetDirectoryName(daoPath));
            File.WriteAllText(daoPath, daoContent);
            AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {daoPath}", "grey"));
        }

        private static void CreateServiceFile(ModuleGenerationContext context)
        {
            string serviceContent = GetServiceContent(context);
            string servicePath = Path.Combine(context.ModulePath, "Services", $"{context.PluralizedModuleName}Service.cs");
            Directory.CreateDirectory(GetDirectoryName(servicePath));
            File.WriteAllText(servicePath, serviceContent);
            AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {servicePath}", "grey"));
        }

        private static void CreateControllerFile(ModuleGenerationContext context)
        {
            string controllerContent = GetControllerContent(context);
            string controllerPath = Path.Combine(context.ModulePath, "Controllers", $"{context.PluralizedModuleName}Controller.cs");
            Directory.CreateDirectory(GetDirectoryName(controllerPath));
            File.WriteAllText(controllerPath, controllerContent);
            AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {controllerPath}", "grey"));
        }

        private static void CreateControllerTestFile(ModuleGenerationContext context)
        {
            string testContent = GetControllerTestContent(context);
            string testPath = Path.Combine(context.ModulePath, "Tests", "Controllers", $"{context.PluralizedModuleName}ControllerTests.cs");
            Directory.CreateDirectory(GetDirectoryName(testPath));
            File.WriteAllText(testPath, testContent);
            AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {testPath}", "grey"));
        }

        private static void CreateServiceTestFile(ModuleGenerationContext context)
        {
            string testContent = GetServiceTestContent(context);
            string testPath = Path.Combine(context.ModulePath, "Tests", "Services", $"{context.PluralizedModuleName}ServiceTests.cs");
            Directory.CreateDirectory(GetDirectoryName(testPath));
            File.WriteAllText(testPath, testContent);
            AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {testPath}", "grey"));
        }

        private static void CreateDaoTestFile(ModuleGenerationContext context)
        {
            string testContent = GetDaoTestContent(context);
            string testPath = Path.Combine(context.ModulePath, "Tests", "Daos", $"{context.PluralizedModuleName}DaoTests.cs");
            Directory.CreateDirectory(GetDirectoryName(testPath));
            File.WriteAllText(testPath, testContent);
            AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {testPath}", "grey"));
        }

        private static string GetDirectoryName(string path)
        {
            var dirName = Path.GetDirectoryName(path);
            if (dirName == null)
            {
                throw new Exception($"Directory name not found, path: {path}");
            }
            return dirName;
        }

        private static string GetEntityContent(ModuleGenerationContext context)
        {
            return $@"using NestNet.Infra.BaseClasses;
using NestNet.Infra.Attributes;

namespace {context.ProjectName}.Modules.{context.PluralizedModuleName}.Entities
{{
    [Entity(""{context.PluralizedModuleName}"")]
    public class {context.ModuleName} : EntityBase
    {{
        // This property enables code at 'NestNet.Infra' to handle the entity in general 
        // manner (without knowing the specific name '{context.ModuleName}Id').
        [Prop(
            create: GenOpt.Ignore,
            update: GenOpt.Ignore,
            result: GenOpt.Ignore,
            store: DbOpt.Ignore
        )]
        public override int Id
        {{
            get {{ return {context.ModuleName}Id; }}
            set {{ {context.ModuleName}Id = value; }}
        }}

        // Value of this property is auto generated by the DB.
        [Prop(
            create: GenOpt.Ignore,
            update: GenOpt.Ignore,
            result: GenOpt.Mandatory,
            store: DbOpt.PrimaryKey
        )]
        public int {context.ModuleName}Id {{ get; set; }}

        [Prop(
            create: GenOpt.Mandatory,
            update: GenOpt.Optional,
            result: GenOpt.Mandatory,
            store: DbOpt.Standard
        )]
        public required string Name {{ get; set; }}

        [Prop(
            create: GenOpt.Mandatory,
            update: GenOpt.Optional,
            result: GenOpt.Mandatory,
            store: DbOpt.Standard
        )]
        public int Age {{ get; set; }}

        [Prop(
            create: GenOpt.Optional,
            update: GenOpt.Optional,
            result: GenOpt.Optional,
            store: DbOpt.Standard
        )]
        public string? Email {{ get; set; }}

        [Prop(
            create: GenOpt.Mandatory,
            update: GenOpt.Ignore,
            result: GenOpt.Ignore,
            store: DbOpt.Ignore
        )]
        public string? MyVirtualField {{ get; set; }}
    }}
}}";
        }

        private static string GetDaoContent(ModuleGenerationContext context)
        {
            return $@"#pragma warning disable IDE0290 // Use primary constructor
using {context.ProjectName}.Data;
using NestNet.Infra.BaseClasses;
using NestNet.Infra.Attributes;

namespace {context.ProjectName}.Modules.{context.PluralizedModuleName}.Daos
{{
    public interface I{context.ModuleName}Dao: IDao<{context.EntityFullName}, {context.QueryDtoName}>
    {{
        // If you add methods to derived class - expose them here.
    }}

    [Injectable(LifetimeType.Scoped)]
    public class {context.ModuleName}Dao : DaoBase<{context.EntityFullName}, {context.QueryDtoName}>, I{context.ModuleName}Dao
    {{
        public {context.ModuleName}Dao(ApplicationDbContext context)
            : base(context, context.GetDbSet<{context.EntityFullName}>(), ""{context.ParamName}Id"")
        {{
        }}

        // How to customise this class:
        // 1) You can add here 'custom' methods (methods for operations not supported by the base class).
        // 2) You can override here base class methods if needed:
        //    * Add methods with the same name and signature as in the base class.
        //    * For example:
        //      public override async Task<IEnumerable<{context.EntityFullName}>> GetAll()
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

        private static string GetServiceContent(ModuleGenerationContext context)
        {
            return $@"#pragma warning disable IDE0290 // Use primary constructor
using NestNet.Infra.BaseClasses;
using NestNet.Infra.Attributes;
using {context.ProjectName}.Modules.{context.PluralizedModuleName}.Dtos;
using {context.ProjectName}.Modules.{context.PluralizedModuleName}.Daos;

namespace {context.ProjectName}.Modules.{context.PluralizedModuleName}.Services
{{
    public interface I{context.PluralizedModuleName}Service: ICrudService<{context.EntityFullName}, {context.CreateDtoName}, {context.UpdateDtoName}, {context.ResultDtoName}, {context.QueryDtoName}>
    {{
        // If you add methods to derived class - expose them here.
    }}

    [Injectable(LifetimeType.Scoped)]
    public class {context.PluralizedModuleName}Service : CrudServiceBase<{context.EntityFullName}, {context.CreateDtoName}, {context.UpdateDtoName}, {context.ResultDtoName}, {context.QueryDtoName}>, I{context.PluralizedModuleName}Service
    {{
        public {context.PluralizedModuleName}Service(I{context.ModuleName}Dao {context.ParamName}Dao)
            : base({context.ParamName}Dao)
        {{
        }}

        // How to customise this class:
        // 1) You can add here 'custom' methods (methods for operations not supported by the base class).
        // 2) You can override here base class methods if needed:
        //    * Add methods with the same name and signature as in the base class.
        //    * For example:
        //      public override async Task<IEnumerable<{context.ResultDtoName}>> GetAll()
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

        private static string GetControllerContent(ModuleGenerationContext context)
        {
            return $@"#pragma warning disable IDE0290 // Use primary constructor
using Microsoft.AspNetCore.Mvc;
using NestNet.Infra.BaseClasses;
using {context.ProjectName}.Modules.{context.PluralizedModuleName}.Dtos;
using {context.ProjectName}.Modules.{context.PluralizedModuleName}.Services;

namespace {context.ProjectName}.Modules.{context.PluralizedModuleName}.Controllers
{{
    [Route(""api/{context.KebabCasePluralizedModuleName}"")]
    public class {context.PluralizedModuleName}Controller : CrudControllerBase<{context.EntityFullName}, {context.CreateDtoName}, {context.UpdateDtoName}, {context.ResultDtoName}, {context.QueryDtoName}>
    {{
        public {context.PluralizedModuleName}Controller(I{context.PluralizedModuleName}Service {context.PluralizedParamName}Service)
            : base({context.PluralizedParamName}Service, ""{context.ParamName}Id"")
        {{
        }}

        [HttpGet]
        public override async Task<ActionResult<IEnumerable<{context.ResultDtoName}>>> GetAll()
        {{
            return await base.GetAll();
        }}

        [HttpGet(""{{{context.ParamName}Id}}"")]
        public override async Task<ActionResult<{context.ResultDtoName}>> GetById(int {context.ParamName}Id)
        {{
            return await base.GetById({context.ParamName}Id);
        }}

        [HttpPost]
        public override async Task<ActionResult<{context.ResultDtoName}>> Create({context.CreateDtoName} {context.ParamName})
        {{
            return await base.Create({context.ParamName});
        }}

        [HttpPut(""{{{context.ParamName}Id}}"")]
        public override async Task<ActionResult<{context.ResultDtoName}>> Update(int {context.ParamName}Id, {context.UpdateDtoName} {context.ParamName}, bool ignoreMissingOrNullFields)
        {{
            return await base.Update({context.ParamName}Id, {context.ParamName}, ignoreMissingOrNullFields);
        }}

        [HttpDelete(""{{{context.ParamName}Id}}"")]
        public override async Task<IActionResult> Delete(int {context.ParamName}Id)
        {{
            return await base.Delete({context.ParamName}Id);
        }}

        // How to customize this class:
        // 1) You can modify the auto-generated CRUD methods (e.g. by
        //     adding code before or after the base class method calls).
        // 2) You can add custom methods for operations not covered by
        //    the auto-generated CRUD methods. The base class's '_service'
        //    member is accessible for use in these custom methods.
    }}
}}

#pragma warning restore IDE0290 // Use primary constructor";
        }

        private static string GetControllerTestContent(ModuleGenerationContext context)
        {
            return $@"using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using NestNet.Infra.Query;
using NestNet.Infra.BaseClasses;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using System.Text.Json;
using {context.ProjectName}.Modules.{context.PluralizedModuleName}.Services;
using {context.ProjectName}.Modules.{context.PluralizedModuleName}.Controllers;
using {context.ProjectName}.Modules.{context.PluralizedModuleName}.Dtos;

namespace {context.ProjectName}.Modules.{context.PluralizedModuleName}.Tests.Controllers
{{
    public class {context.PluralizedModuleName}ControllerTests
    {{
        private readonly IFixture _fixture;
        private readonly I{context.PluralizedModuleName}Service _{context.PluralizedParamName}Service;
        private readonly {context.PluralizedModuleName}Controller _controller;

        public {context.PluralizedModuleName}ControllerTests()
        {{
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            _{context.PluralizedParamName}Service = _fixture.Freeze<I{context.PluralizedModuleName}Service>();
            _controller = new {context.PluralizedModuleName}Controller(_{context.PluralizedParamName}Service);
        }}

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithAllItems()
        {{
            // Arrange
            var expectedResult = _fixture.CreateMany<{context.ResultDtoName}>(2).ToList();
            _{context.PluralizedParamName}Service.GetAll().Returns(Task.FromResult<IEnumerable<{context.ResultDtoName}>>(expectedResult));

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            var resultData = Assert.IsAssignableFrom<IEnumerable<{context.ResultDtoName}>>(okResult.Value);
            Assert.Equal(expectedResult.Count, resultData.Count());
            await _{context.PluralizedParamName}Service.Received(1).GetAll();
        }}

        [Fact]
        public async Task GetById_ReturnsOkResult_WhenItemExists()
        {{
            // Arrange
            var id = _fixture.Create<int>();
            var expectedResult = _fixture.Create<{context.ResultDtoName}>();
            _{context.PluralizedParamName}Service.GetById(Arg.Any<int>()).Returns(Task.FromResult<{context.ResultDtoName}?>(expectedResult));

            // Act
            var result = await _controller.GetById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            var resultData = Assert.IsType<{context.ResultDtoName}>(okResult.Value);
            Assert.Equal(expectedResult, resultData);
            await _{context.PluralizedParamName}Service.Received(1).GetById(id);
        }}

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenItemDoesNotExist()
        {{
            // Arrange
            var id = _fixture.Create<int>();
            _{context.PluralizedParamName}Service.GetById(Arg.Any<int>()).Returns(Task.FromResult<{context.ResultDtoName}?>(null));

            // Act
            var result = await _controller.GetById(id);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Value);
            var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
            Assert.NotNull(notFoundResult);
            Assert.Equal(404, notFoundResult.StatusCode);
            await _{context.PluralizedParamName}Service.Received(1).GetById(id);
        }}

        [Fact]
        public async Task Create_ReturnsCreatedResult_WithNewItem()
        {{
            // Arrange
            var id = _fixture.Create<int>();
            var createDto = _fixture.Create<{context.CreateDtoName}>();
            var expectedResult = _fixture.Create<{context.ResultDtoName}>();
            var internalCreateResult = new InternalCreateResult<{context.ResultDtoName}>
            {{
                Id = id,
                ResultDto = expectedResult
            }};
            _{context.PluralizedParamName}Service.Create(Arg.Any<{context.CreateDtoName}>()).Returns(Task.FromResult(internalCreateResult));

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.NotNull(createdResult);
            Assert.Equal(201, createdResult.StatusCode);
            var resultData = Assert.IsType<{context.ResultDtoName}>(createdResult.Value);
            Assert.Equal(expectedResult, resultData);
            await _{context.PluralizedParamName}Service.Received(1).Create(createDto);
        }}

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_True_ReturnsOkResult_WhenUpdateSuccessful()
        {{
            // Arrange
            var ignoreMissingOrNullFields = true;
            var id = _fixture.Create<int>();
            var updateDto = _fixture.Create<{context.UpdateDtoName}>();
            var expectedResult = _fixture.Create<{context.ResultDtoName}>();
            _{context.PluralizedParamName}Service.Update(Arg.Any<int>(), Arg.Any<{context.UpdateDtoName}>(), Arg.Any<bool>()).Returns(Task.FromResult<{context.ResultDtoName}?>(expectedResult));

            // Act
            var result = await _controller.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            var resultData = Assert.IsType<{context.ResultDtoName}>(okResult.Value);
            Assert.Equal(expectedResult, resultData);
            await _{context.PluralizedParamName}Service.Received(1).Update(id, updateDto, ignoreMissingOrNullFields);
        }}

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_True_ReturnsNotFound_WhenItemDoesNotExist()
        {{
            // Arrange
            var ignoreMissingOrNullFields = true;
            var id = _fixture.Create<int>();
            var updateDto = _fixture.Create<{context.UpdateDtoName}>();
            _{context.PluralizedParamName}Service.Update(Arg.Any<int>(), Arg.Any<{context.UpdateDtoName}>(), Arg.Any<bool>()).Returns(Task.FromResult<{context.ResultDtoName}?>(null));
         
            // Act
            var result = await _controller.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Value);
            var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
            Assert.NotNull(notFoundResult);
            Assert.Equal(404, notFoundResult.StatusCode);
            await _{context.PluralizedParamName}Service.Received(1).Update(id, updateDto, ignoreMissingOrNullFields);
        }}

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_False_ReturnsOkResult_WhenUpdateSuccessful()
        {{
            // Arrange
            var ignoreMissingOrNullFields = false;
            var id = _fixture.Create<int>();
            var updateDto = _fixture.Create<{context.UpdateDtoName}>();
            var expectedResult = _fixture.Create<{context.ResultDtoName}>();
            _{context.PluralizedParamName}Service.Update(Arg.Any<int>(), Arg.Any<{context.UpdateDtoName}>(), Arg.Any<bool>()).Returns(Task.FromResult<{context.ResultDtoName}?>(expectedResult));

            // Act
            var result = await _controller.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            var resultData = Assert.IsType<{context.ResultDtoName}>(okResult.Value);
            Assert.Equal(expectedResult, resultData);
            await _{context.PluralizedParamName}Service.Received(1).Update(id, updateDto, ignoreMissingOrNullFields);
        }}

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_False_ReturnsNotFound_WhenItemDoesNotExist()
        {{
            // Arrange
            var ignoreMissingOrNullFields = false;
            var id = _fixture.Create<int>();
            var updateDto = _fixture.Create<{context.UpdateDtoName}>();
            _{context.PluralizedParamName}Service.Update(Arg.Any<int>(), Arg.Any<{context.UpdateDtoName}>(), Arg.Any<bool>()).Returns(Task.FromResult<{context.ResultDtoName}?>(null));
         
            // Act
            var result = await _controller.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Value);
            var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
            Assert.NotNull(notFoundResult);
            Assert.Equal(404, notFoundResult.StatusCode);
            await _{context.PluralizedParamName}Service.Received(1).Update(id, updateDto, ignoreMissingOrNullFields);
        }}

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenDeleteSuccessful()
        {{
            // Arrange
            var id = _fixture.Create<int>();
            _{context.PluralizedParamName}Service.Delete(Arg.Any<int>()).Returns(Task.FromResult(true));

            // Act
            var result = await _controller.Delete(id);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.NotNull(noContentResult);
            Assert.Equal(204, noContentResult.StatusCode);
            await _{context.PluralizedParamName}Service.Received(1).Delete(id);
        }}

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenItemDoesNotExist()
        {{
            // Arrange
            var id = _fixture.Create<int>();
            _{context.PluralizedParamName}Service.Delete(Arg.Any<int>()).Returns(Task.FromResult(false));

            // Act
            var result = await _controller.Delete(id);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
            Assert.NotNull(notFoundResult);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.IsType<NotFoundResult>(result);
            await _{context.PluralizedParamName}Service.Received(1).Delete(id);
        }}

        [Fact]
        public async Task GetPaginated_ReturnsOkResult_WithPaginatedItems()
        {{
            // Arrange
            var request = _fixture.Create<UnsafePaginationRequest>();
            var dtos = _fixture.CreateMany<{context.ResultDtoName}>(3).ToList();
            var expectedResult = new DataWithOptionalError<PaginatedResult<{context.ResultDtoName}>>
            {{
                Data = new PaginatedResult<{context.ResultDtoName}>()
                {{
                    Items = dtos,
                    TotalCount = dtos.Count()
                }}
            }};
            _{context.PluralizedParamName}Service.GetPaginated(Arg.Any<UnsafePaginationRequest>()).Returns(Task.FromResult(expectedResult));

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
            var resultData = Assert.IsType<PaginatedResult<{context.ResultDtoName}>>(okResult.Value);
            Assert.Equal(expectedResult.Data.Items.Count(), resultData.Items.Count());
            Assert.Equal(expectedResult.Data.TotalCount, resultData.TotalCount);
            var receivedCalls = _{context.PluralizedParamName}Service.ReceivedCalls();
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
        }}

        [Fact]
        public async Task GetPaginated_ReturnsParametersError()
        {{
            // Arrange
            var request = _fixture.Create<UnsafePaginationRequest>();
            var dtos = _fixture.CreateMany<{context.ResultDtoName}>(3).ToList();
            var expectedResult = new DataWithOptionalError<PaginatedResult<{context.ResultDtoName}>>
            {{
                Error = ""Blabla""
            }};
            _{context.PluralizedParamName}Service.GetPaginated(Arg.Any<UnsafePaginationRequest>()).Returns(Task.FromResult(expectedResult));

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
        }}
    }}
}}";
        }

        private static string GetServiceTestContent(ModuleGenerationContext context)
        {
            return $@"using NSubstitute;
using Xunit;
using NestNet.Infra.Query;
using NestNet.Infra.BaseClasses;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using System.Text.Json;
using {context.ProjectName}.Modules.{context.PluralizedModuleName}.Services;
using {context.ProjectName}.Modules.{context.PluralizedModuleName}.Dtos;
using {context.ProjectName}.Modules.{context.PluralizedModuleName}.Daos;

namespace {context.ProjectName}.Modules.{context.PluralizedModuleName}.Tests.Services
{{
    public class {context.PluralizedModuleName}ServiceTests
    {{
        private readonly IFixture _fixture;
        private readonly I{context.ModuleName}Dao _{context.ParamName}Dao;
        private readonly {context.PluralizedModuleName}Service _service;

        public {context.PluralizedModuleName}ServiceTests()
        {{
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            _{context.ParamName}Dao = _fixture.Freeze<I{context.ModuleName}Dao>();
            _service = new {context.PluralizedModuleName}Service(_{context.ParamName}Dao);
        }}

        [Fact]
        public async Task GetAll_ReturnsAllItems()
        {{
            // Arrange
            var srcEntities = _fixture.CreateMany<{context.EntityFullName}>(3).ToList();
            var expectedResult = _service.ToResultDtos(srcEntities);
            _{context.ParamName}Dao.GetAll().Returns(Task.FromResult<IEnumerable<{context.EntityFullName}>>(srcEntities));

            // Act
            var result = await _service.GetAll();
  
            // Assert
            Assert.Equal(expectedResult.Count(), result.Count());
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult),
                JsonSerializer.Serialize(result));
            await _{context.ParamName}Dao.Received(1).GetAll();
        }}

        [Fact]
        public async Task GetById_ReturnsItem_WhenExists()
        {{
            // Arrange
            var id = _fixture.Create<int>();
            var entity = _fixture.Create<{context.EntityFullName}>();
            var expectedResult = _service.ToResultDto(entity);
            _{context.ParamName}Dao.GetById(Arg.Any<int>()).Returns(Task.FromResult<{context.EntityFullName}?>(entity));

            // Act
            var result = await _service.GetById(id);

            // Assert
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult),
                JsonSerializer.Serialize(result));
            await _{context.ParamName}Dao.Received(1).GetById(id);
        }}

        [Fact]
        public async Task GetById_ReturnsNull_WhenNotExists()
        {{
            // Arrange
            var id = _fixture.Create<int>();
            _{context.ParamName}Dao.GetById(Arg.Any<int>()).Returns(Task.FromResult<{context.EntityFullName}?>(null));

            // Act
            var result = await _service.GetById(id);

            // Assert
            Assert.Null(result);
            await _{context.ParamName}Dao.Received(1).GetById(id);
        }}

        [Fact]
        public async Task Create_ReturnsCreatedItem()
        {{
            // Arrange
            var createDto = _fixture.Create<{context.CreateDtoName}>();
            var createdEntity = _service.ToEntity(createDto);
            var expectedResult = _service.ToResultDto(createdEntity);
            _{context.ParamName}Dao.Create(Arg.Any<{context.EntityFullName}>()).Returns(Task.FromResult(createdEntity));

            // Act
            var result = await _service.Create(createDto);

            // Assert
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult),
                JsonSerializer.Serialize(result.ResultDto));
            await _{context.ParamName}Dao.Received(1).Create(Arg.Any<{context.EntityFullName}>());
        }}

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_True_ReturnsUpdatedItem_WhenExists()
        {{
            // Arrange
            var ignoreMissingOrNullFields = true;
            var id = _fixture.Create<int>();
            var updateDto = _fixture.Create<{context.UpdateDtoName}>();
            var updatedEntity = _service.ToEntity(updateDto);
            var expectedResult = _service.ToResultDto(updatedEntity);           
            _{context.ParamName}Dao.Update(Arg.Any<int>(), Arg.Any<{context.UpdateDtoName}>(), Arg.Any<bool>()).Returns(Task.FromResult<{context.EntityFullName}?>(updatedEntity));
            
            // Act
            var result = await _service.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult),
                JsonSerializer.Serialize(result));
            await _{context.ParamName}Dao.Received(1).Update(id, updateDto, ignoreMissingOrNullFields);
        }}

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_True_ReturnsNull_WhenNotExists()
        {{
            // Arrange
            var ignoreMissingOrNullFields = true;
            var id = _fixture.Create<int>();
            var updateDto = _fixture.Create<{context.UpdateDtoName}>();
            _{context.ParamName}Dao.Update(Arg.Any<int>(), Arg.Any<{context.UpdateDtoName}>(), Arg.Any<bool>()).Returns(Task.FromResult<{context.EntityFullName}?>(null));

            // Act
            var result = await _service.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.Null(result);
            await _{context.ParamName}Dao.Received(1).Update(id, updateDto, ignoreMissingOrNullFields);
        }}

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_False_ReturnsUpdatedItem_WhenExists()
        {{
            // Arrange
            var ignoreMissingOrNullFields = false;
            var id = _fixture.Create<int>();
            var updateDto = _fixture.Create<{context.UpdateDtoName}>();
            var updatedEntity = _service.ToEntity(updateDto);
            var expectedResult = _service.ToResultDto(updatedEntity);           
            _{context.ParamName}Dao.Update(Arg.Any<int>(), Arg.Any<{context.UpdateDtoName}>(), Arg.Any<bool>()).Returns(Task.FromResult<{context.EntityFullName}?>(updatedEntity));
            
            // Act
            var result = await _service.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult),
                JsonSerializer.Serialize(result));
            await _{context.ParamName}Dao.Received(1).Update(id, updateDto, ignoreMissingOrNullFields);
        }}

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_False_ReturnsNull_WhenNotExists()
        {{
            // Arrange
            var ignoreMissingOrNullFields = false;
            var id = _fixture.Create<int>();
            var updateDto = _fixture.Create<{context.UpdateDtoName}>();
            _{context.ParamName}Dao.Update(Arg.Any<int>(), Arg.Any<{context.UpdateDtoName}>(), Arg.Any<bool>()).Returns(Task.FromResult<{context.EntityFullName}?>(null));

            // Act
            var result = await _service.Update(id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.Null(result);
            await _{context.ParamName}Dao.Received(1).Update(id, updateDto, ignoreMissingOrNullFields);
        }}

        [Fact]
        public async Task Delete_ReturnsTrue_WhenExists()
        {{
            // Arrange
            var id = _fixture.Create<int>();
            var entity = _fixture.Create<{context.EntityFullName}>();
            _{context.ParamName}Dao.Delete(Arg.Any<int>()).Returns(Task.FromResult(true));

            // Act
            var found = await _service.Delete(id);

            // Assert
            Assert.True(found);
            await _{context.ParamName}Dao.Received(1).Delete(id);
        }}

        [Fact]
        public async Task Delete_ReturnsFalse_WhenNotExists()
        {{
            // Arrange
            var id = _fixture.Create<int>();
            _{context.ParamName}Dao.Delete(Arg.Any<int>()).Returns(Task.FromResult(false));

            // Act
             var found = await _service.Delete(id);

            // Assert
            Assert.False(found);
            await _{context.ParamName}Dao.Received(1).Delete(id);
        }}

        [Fact]
        public async Task GetPaginated_ReturnsPaginatedItems()
        {{
            // Arrange
            var propertyName = ""{context.ParamName}Id"";
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

            var srcEntities = _fixture.CreateMany<{context.EntityFullName}>(3)
              .Select((entity, index) => {{
                  entity.{context.ModuleName}Id = index;
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

            var daoResult = new PaginatedResult<{context.EntityFullName}>
            {{
                Items = srcEntities,
                TotalCount = srcEntities.Count()
            }};
          
            var expectedResult = _service.ToPaginatedResultDtos(daoResult);
            _{context.ParamName}Dao.GetPaginated(Arg.Any<SafePaginationRequest>()).Returns(Task.FromResult(daoResult));

            // Act
            var result = await _service.GetPaginated(unsafeRequest);

            // Assert
            Assert.NotNull(result.Data);
            Assert.Null(result.Error);
            Assert.Equal(expectedResult.TotalCount, result.Data.TotalCount);
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult.Items),
                JsonSerializer.Serialize(result.Data.Items));
            var receivedCalls = _{context.ParamName}Dao.ReceivedCalls();
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
            var propertyName = ""{context.ParamName}Id"";
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
            var srcEntities = _fixture.CreateMany<{context.EntityFullName}>(3).ToList();
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
            var daoResult = new PaginatedResult<{context.EntityFullName}>
            {{
                Items = srcEntities,
                TotalCount = srcEntities.Count()
            }};
            _{context.ParamName}Dao.GetPaginated(Arg.Any<SafePaginationRequest>()).Returns(Task.FromResult(daoResult));

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
            var propertyName = ""{context.ParamName}Id"";
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
            var srcEntities = _fixture.CreateMany<{context.EntityFullName}>(3).ToList();
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
            var daoResult = new PaginatedResult<{context.EntityFullName}>
            {{
                Items = srcEntities,
                TotalCount = srcEntities.Count()
            }};
            _{context.ParamName}Dao.GetPaginated(Arg.Any<SafePaginationRequest>()).Returns(Task.FromResult(daoResult));

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
            var propertyName = ""{context.ParamName}Id"";
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
            var srcEntities = _fixture.CreateMany<{context.EntityFullName}>(3).ToList();
            var daoResult = new PaginatedResult<{context.EntityFullName}>
            {{
                Items = srcEntities,
                TotalCount = srcEntities.Count()
            }};  
            _{context.ParamName}Dao.GetPaginated(Arg.Any<SafePaginationRequest>()).Returns(Task.FromResult(daoResult));

            // Act
            var result = await _service.GetPaginated(unsafeRequest);

            // Assert
            Assert.Null(result.Data);
            Assert.NotNull(result.Error);
            Assert.Contains(""Invalid sort properties"", result.Error);
            Assert.Contains(""Invalid filter properties"", result.Error);
        }}
    }}
}}";
        }

        private static string GetDaoTestContent(ModuleGenerationContext context)
        {
            return $@"using Xunit;
using NestNet.Infra.Query;
using NestNet.Infra.Helpers;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using System.Text.Json;
using {context.ProjectName}.Modules.{context.PluralizedModuleName}.Dtos;
using {context.ProjectName}.Modules.{context.PluralizedModuleName}.Daos;
using {context.ProjectName}.Data;
using Microsoft.EntityFrameworkCore;

namespace {context.ProjectName}.Modules.{context.PluralizedModuleName}.Tests.Daos
{{
    public class {context.PluralizedModuleName}DaoTests : IDisposable, IAsyncLifetime
    {{
        private readonly IFixture _fixture;
        private readonly ApplicationDbContext _context;
        private readonly {context.ModuleName}Dao _dao;

        public {context.PluralizedModuleName}DaoTests()
        {{
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());

            // Create options for in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $""TestDb_{{Guid.NewGuid()}}"")  // Unique name per test
                .Options;

            // Create real context with in-memory database
            _context = new ApplicationDbContext(options);

            _dao = new {context.ModuleName}Dao(_context);
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
            var srcEntities = _fixture.CreateMany<{context.EntityFullName}>(3).ToList();
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
            var srcEntities = _fixture.CreateMany<{context.EntityFullName}>(3).ToList();
            srcEntities.ForEach(async (entity) => await _dao.Create(entity));

            // Act
            var result = await _dao.GetById(srcEntities[1].{context.ModuleName}Id);

            // Assert
            Assert.Equal(
                JsonSerializer.Serialize(srcEntities[1]),
                JsonSerializer.Serialize(result));
        }}

        [Fact]
        public async Task GetById_ReturnsNull_WhenNotExists()
        {{
            // Arrange
            var id = _fixture.Create<int>();
          
            // Act
            var result = await _dao.GetById(id);

            // Assert
            Assert.Null(result);
        }}

        [Fact]
        public async Task Create_ReturnsCreatedItem()
        {{
            // Arrange
            var srcEntity = _fixture.Create<{context.EntityFullName}>();

            // Act
            await _dao.Create(srcEntity);

            // Assert
            var result = await _dao.GetById(srcEntity.{context.ModuleName}Id);
            Assert.Equal(
              JsonSerializer.Serialize(srcEntity),
              JsonSerializer.Serialize(result));
        }}

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_True_ReturnsUpdatedItem_WhenExists()
        {{
            // Arrange
            var ignoreMissingOrNullFields = true;
            var srcEntities = _fixture.CreateMany<{context.EntityFullName}>(3).ToList();
            srcEntities.ForEach(async (entity) => await _dao.Create(entity));
            var updateDto = _fixture.Create<{context.UpdateDtoName}>();

            // Act
            var result = await _dao.Update(srcEntities[1].{context.ModuleName}Id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.NotNull(result);
            TestsHelper.IsValuesExists(updateDto, result, Assert.Equal);
        }}

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_True_ReturnsNull_WhenNotExists()
        {{
            // Arrange
            var ignoreMissingOrNullFields = true;
            var id = _fixture.Create<int>();
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
            var srcEntities = _fixture.CreateMany<{context.EntityFullName}>(3).ToList();
            srcEntities.ForEach(async (entity) => await _dao.Create(entity));
            var updateDto = _fixture.Create<{context.UpdateDtoName}>();

            // Act
            var result = await _dao.Update(srcEntities[1].{context.ModuleName}Id, updateDto, ignoreMissingOrNullFields);

            // Assert
            Assert.NotNull(result);
            TestsHelper.IsValuesExists(updateDto, result, Assert.Equal);
        }}

        [Fact]
        public async Task Update_WithIgnoreMissingOrNullFields_False_ReturnsNull_WhenNotExists()
        {{
            // Arrange
            var ignoreMissingOrNullFields = false;
            var id = _fixture.Create<int>();
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
            var srcEntities = _fixture.CreateMany<{context.EntityFullName}>(3).ToList();
            srcEntities.ForEach(async (entity) => await _dao.Create(entity));

            // Act
            var found = await _dao.Delete(srcEntities[1].{context.ModuleName}Id);

            // Assert
            Assert.True(found);
        }}

        [Fact]
        public async Task Delete_ReturnsFalse_WhenNotExists()
        {{
            // Arrange
            var id = _fixture.Create<int>();
     
            // Act
            var found = await _dao.Delete(id);

            // Assert
            Assert.False(found);
        }}

        [Fact]
        public async Task GetPaginated_ReturnsPaginatedItems()
        {{
            // Arrange
            var srcEntities = _fixture.CreateMany<{context.EntityFullName}>(3)
               .Select((entity, index) => {{
                   entity.{context.ModuleName}Id = index + 1;
                   return entity;
               }})
               .ToList();
            srcEntities.ForEach(async (entity) => await _dao.Create(entity));
            var value = srcEntities[1].{context.ModuleName}Id;
            var propertyName = ""{context.ParamName}Id"";
            var resultItems = srcEntities
                  .Where(e => (e.{context.ModuleName}Id != value))
                  .OrderByDescending(e => e.{context.ModuleName}Id);
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
            var expectedResult = new PaginatedResult<{context.EntityFullName}>
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
    }}
}}";
        }
    }
}