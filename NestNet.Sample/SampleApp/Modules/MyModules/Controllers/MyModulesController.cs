#pragma warning disable IDE0290 // Use primary constructor
using Microsoft.AspNetCore.Mvc;
using NestNet.Infra.BaseClasses;
using SampleApp.Modules.MyModules.Dtos;
using SampleApp.Modules.MyModules.Services;

namespace SampleApp.Modules.MyModules.Controllers
{
    [Route("api/my-modules")]
    public class MyModulesController : CrudControllerBase<Entities.MyModule, MyModuleCreateDto, MyModuleUpdateDto, MyModuleResultDto, MyModuleQueryDto>
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

        // How to customize this class:
        // 1) You can modify the auto-generated CRUD methods (e.g. by
        //     adding code before or after the base class method calls).
        // 2) You can add custom methods for operations not covered by
        //    the auto-generated CRUD methods. The base class's '_service'
        //    member is accessible for use in these custom methods.
    }
}

#pragma warning restore IDE0290 // Use primary constructor