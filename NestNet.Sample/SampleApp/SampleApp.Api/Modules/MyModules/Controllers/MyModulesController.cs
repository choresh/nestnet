#pragma warning disable IDE0290 // Use primary constructor
using Microsoft.AspNetCore.Mvc;
using NestNet.Infra.BaseClasses;
using SampleApp.Core.Modules.MyModules.Dtos;
using SampleApp.Core.Modules.MyModules.Services;
using SampleApp.Core.Modules.MyModules.Entities;

namespace SampleApp.Api.Modules.MyModules.Controllers
{
    [Route("api/my-modules")]
    public class MyModulesController : CrudControllerBase<MyModuleEntity, MyModuleCreateDto, MyModuleUpdateDto, MyModuleResultDto, MyModuleQueryDto>
    {
        public MyModulesController(IMyModulesService myModulesService)
            : base(myModulesService, "MyModuleId")
        {
        }

        [HttpGet]
        public override async Task<ActionResult<IEnumerable<MyModuleResultDto>>> GetAll()
        {
            return await base.GetAll();
        }

        [HttpGet("{myModuleId}")]
        public override async Task<ActionResult<MyModuleResultDto>> GetById(long myModuleId)
        {
            return await base.GetById(myModuleId);
        }

        [HttpPost]
        public override async Task<ActionResult<MyModuleResultDto>> Create(MyModuleCreateDto myModule)
        {
            return await base.Create(myModule);
        }

        [HttpPut("{myModuleId}")]
        public override async Task<ActionResult<MyModuleResultDto>> Update(long myModuleId, MyModuleUpdateDto myModule, bool ignoreMissingOrNullFields)
        {
            return await base.Update(myModuleId, myModule, ignoreMissingOrNullFields);
        }

        [HttpDelete("{myModuleId}")]
        public override async Task<IActionResult> Delete(long myModuleId)
        {
            return await base.Delete(myModuleId);
        }
    }
}

#pragma warning restore IDE0290 // Use primary constructor