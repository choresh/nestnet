using NestNet.Cli.Generators.Common;
using NestNet.Cli.Infra;
using Spectre.Console;

namespace NestNet.Cli.Generators.ModuleGenerator
{
    internal static partial class ModuleGenerator
    {
        private class ApiModuleGenerator : MultiProjectsGeneratorBase<ModuleGenerationContext>
        {
            public ApiModuleGenerator()
                : base(ProjectType.Api)
            {
            }

            public override void DoGenerate()
            {
                if (Context.GenerateController)
                {
                    // Ensure API directory exists
                    Directory.CreateDirectory(Context.ProjectContext!.TargetPath);

                    CreateControllerFile();
                    CreateControllerTestFile();
                }
            }

            private void CreateControllerFile()
            {
                string controllerContent = GetControllerContent();
                string controllerPath = Path.Combine(Context.ProjectContext!.TargetPath, "Controllers", $"{Context.PluralizedModuleName}Controller.cs");
                Directory.CreateDirectory(GetDirectoryName(controllerPath));
                File.WriteAllText(controllerPath, controllerContent);
                AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {controllerPath}", "grey"));
            }

            private void CreateControllerTestFile()
            {
                string testContent = GetControllerTestContent();
                string testPath = Path.Combine(Context.ProjectContext!.TargetPath, "Tests", "Controllers", $"{Context.PluralizedModuleName}ControllerTests.cs");
                Directory.CreateDirectory(GetDirectoryName(testPath));
                File.WriteAllText(testPath, testContent);
                AnsiConsole.MarkupLine(Helpers.FormatMessage($"Created: {testPath}", "grey"));
            }

            private string GetControllerContent()
            {
                var srcProjectName = Context.ProjectContext!.ProjectName.Replace(".Api", ".Core");
                return $@"#pragma warning disable IDE0290 // Use primary constructor
using Microsoft.AspNetCore.Mvc;
using NestNet.Infra.BaseClasses;
using {srcProjectName}.Modules.{Context.PluralizedModuleName}.Dtos;
using {srcProjectName}.Modules.{Context.PluralizedModuleName}.Services;
using {srcProjectName}.Modules.{Context.PluralizedModuleName}.Entities;

namespace {Context.ProjectContext!.ProjectName}.Modules.{Context.PluralizedModuleName}.Controllers
{{
    [Route(""api/{Context.KebabCasePluralizedModuleName}"")]
    public class {Context.PluralizedModuleName}Controller : CrudControllerBase<{Context.EntityName}, {Context.CreateDtoName}, {Context.UpdateDtoName}, {Context.ResultDtoName}, {Context.QueryDtoName}>
    {{
        public {Context.PluralizedModuleName}Controller(I{Context.PluralizedModuleName}Service {Context.PluralizedParamName}Service)
            : base({Context.PluralizedParamName}Service, ""{Context.ArtifactName}Id"")
        {{
        }}

        [HttpGet]
        public override async Task<ActionResult<IEnumerable<{Context.ResultDtoName}>>> GetAll()
        {{
            return await base.GetAll();
        }}

        [HttpGet(""{{{Context.ParamName}Id}}"")]
        public override async Task<ActionResult<{Context.ResultDtoName}>> GetById(long {Context.ParamName}Id)
        {{
            return await base.GetById({Context.ParamName}Id);
        }}

        [HttpPost]
        public override async Task<ActionResult<{Context.ResultDtoName}>> Create({Context.CreateDtoName} {Context.ParamName})
        {{
            return await base.Create({Context.ParamName});
        }}

        [HttpPut(""{{{Context.ParamName}Id}}"")]
        public override async Task<ActionResult<{Context.ResultDtoName}>> Update(long {Context.ParamName}Id, {Context.UpdateDtoName} {Context.ParamName}, bool ignoreMissingOrNullFields)
        {{
            return await base.Update({Context.ParamName}Id, {Context.ParamName}, ignoreMissingOrNullFields);
        }}

        [HttpDelete(""{{{Context.ParamName}Id}}"")]
        public override async Task<IActionResult> Delete(long {Context.ParamName}Id)
        {{
            return await base.Delete({Context.ParamName}Id);
        }}
    }}
}}

#pragma warning restore IDE0290 // Use primary constructor";
            }

            private string GetControllerTestContent()
            {
                return $@"using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using NestNet.Infra.Query;
using NestNet.Infra.BaseClasses;
using NestNet.Infra.Paginatation;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using System.Text.Json;
using {Context.ProjectContext!.ProjectName}.Modules.{Context.PluralizedModuleName}.Services;
using {Context.ProjectContext!.ProjectName}.Modules.{Context.PluralizedModuleName}.Dtos;

namespace {Context.ProjectContext!.ProjectName}.Modules.{Context.PluralizedModuleName}.Tests.Controllers
{{
    public class {Context.PluralizedModuleName}ControllerTests
    {{
        private readonly IFixture _fixture;
        private readonly I{Context.PluralizedModuleName}Service _{Context.PluralizedParamName}Service;
        private readonly {Context.PluralizedModuleName}Controller _controller;

        public {Context.PluralizedModuleName}ControllerTests()
        {{
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            _{Context.PluralizedParamName}Service = _fixture.Freeze<I{Context.PluralizedModuleName}Service>();
            _controller = new {Context.PluralizedModuleName}Controller(_{Context.PluralizedParamName}Service);
        }}

        // Test methods...
    }}
}}";
            }
        }
    }
}