#pragma warning disable IDE0290 // Use primary constructor
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NestNet.Infra.Query;
using Swashbuckle.AspNetCore.Annotations;

namespace NestNet.Infra.BaseClasses
{
    [ApiController]
    public class CrudControllerBase<TEntity, TCreateDto, TUpdateDto, TResultDto> : ControllerBase where TEntity : IEntity
    {
        protected readonly ICrudService<TEntity, TCreateDto, TUpdateDto, TResultDto> _service;
        protected readonly string _idFieldName;

        public CrudControllerBase(
            ICrudService<TEntity, TCreateDto, TUpdateDto, TResultDto> service,
            string idFieldName
            )
        {
            _service = service;
            _idFieldName = idFieldName;
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult<IEnumerable<TResultDto>>> GetAll()
        {
            var resultDtos = await _service.GetAll();
            return Ok(resultDtos);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult<TResultDto>> GetById(int id)
        {
            var resultDto = await _service.GetById(id);
            if (resultDto == null)
            {
                return NotFound();
            }
            return Ok(resultDto);
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult<TResultDto>> Create(TCreateDto createDto)
        {
            var result = await _service.Create(createDto);
            var routeValues = new Dictionary<string, object>
            {
                { _idFieldName, result.Id }
            };
            return CreatedAtAction(nameof(GetById), routeValues, result.ResultDto);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult<TResultDto>> Update(int id, TUpdateDto updateDto, bool ignoreMissingOrNullFields)
        {
            var resultDto = await _service.Update(id, updateDto, ignoreMissingOrNullFields);
            if (resultDto == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(resultDto);
            }
        }

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            bool found = await _service.Delete(id);
            if (!found)
            {
                return NotFound();
            }
            return NoContent();
        }

        // * Usage sample: GET /api/entitys/paginated?pageNumber=2&pageSize=15&sortBy=<prop 1>&sortDirection=asc&sortBy=<prop 2>&sortDirection=desc&sortBy=<prop 3>&sortDirection=asc&filterBy=<prop 4>&filterOperator=Equals&filterValue=<value for prop 4>&filterBy=<prop 5>&filterOperator=GreaterThan&filterValue=<value for prop 5>&filterBy=<prop 6>&filterOperator=GreaterThanOrEqual&filterValue=<value for prop 6>
        // * More info see at README file at 'NestNet' package, under title 'About the GetPaginated operation'
        [HttpGet("paginated")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedResult<TResultDto>>> GetPaginated(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool includeTotalCount = false,
            [FromQuery][SwaggerParameter(
                Description = "Property names to sort by")] string[]? sortBy = null,
            [FromQuery][SwaggerParameter(
                Description = "Sort directions for each property (Asc or Desc)")] string[]? sortDirection = null,
            [FromQuery][SwaggerParameter(
                Description = "Property names to filter by")] string[]? filterBy = null,
            [FromQuery][SwaggerParameter(
                Description = "Filter operators (Equals, NotEquals, Contains, NotContains, GreaterThan, LessThan)")] string[]? filterOperator = null,
            [FromQuery][SwaggerParameter(
                Description = "Filter values")] string[]? filterValue = null)
        {
            var request = new UnsafePaginationRequest()
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                IncludeTotalCount = includeTotalCount,
                SortBy = sortBy,
                SortDirection = sortDirection,
                FilterBy = filterBy,
                FilterOperator = filterOperator,
                FilterValue = filterValue
            };
            var result = await _service.GetPaginated(request);
            if (result.Data == null)
            {
                return BadRequest(result.Error);
            }
            return Ok(result.Data);
        }
    }
}

#pragma warning restore IDE0290 // Use primary constructor
