#pragma warning disable IDE0290 // Use primary constructor
using Microsoft.AspNetCore.Mvc;
using SampleApp.Core.Resources.MyResource.Dtos;
using SampleApp.Core.Resources.MyResource.Services;

namespace SampleApp.Api.Resources.MyResource.Controllers
{
    [Route("api/my-resource")]
    public class MyResourceController : ControllerBase
    {
        private IMyResourceService _myResourceService;

        public MyResourceController(IMyResourceService myResourceService)
        {
            _myResourceService = myResourceService;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<SampleOutputDto>>> SampleOperation(SampleInputDto input)
        {
            var result = await _myResourceService.SampleOperation(input);
            return Ok(result);
        }

        // How to customize this class:
		// 1) You can modify the sample method.
        // 2) You can add simmilar methods.
        // 3) Instead 'Ok()' - you can use other returnning options, e.g.:       
        //    * NotFound() (then - decorate your method with '[ProducesResponseType(StatusCodes.Status404NotFound)]')
        //    * BadRequest() (then - decorate your method with '[ProducesResponseType(StatusCodes.Status400BadRequest)]')
        //    * CreatedAtAction(...) (then - decorate your method with '[ProducesResponseType(StatusCodes.Status201Created)]')
    }
}

#pragma warning restore IDE0290 // Use primary constructor