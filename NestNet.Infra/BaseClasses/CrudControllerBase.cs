#pragma warning disable IDE0290 // Use primary constructor
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NestNet.Infra.Query;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Linq.Expressions;
using NestNet.Infra.Attributes;
using NestNet.Infra.Paginatation;
using NestNet.Infra.Swagger;

namespace NestNet.Infra.BaseClasses
{
    [ApiController]
    public class CrudControllerBase<TEntity, TCreateDto, TUpdateDto, TResultDto, TQueryDto> : ControllerBase where TEntity : IEntity where TQueryDto : class
    {
        protected readonly ICrudService<TEntity, TCreateDto, TUpdateDto, TResultDto, TQueryDto> _service;
        protected readonly string _idFieldName;

        public CrudControllerBase(
            ICrudService<TEntity, TCreateDto, TUpdateDto, TResultDto, TQueryDto> service,
            string idFieldName
            )
        {
            _service = service;
            _idFieldName = idFieldName;
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get all items"
        )]
        public virtual async Task<ActionResult<IEnumerable<TResultDto>>> GetAll()
        {
            var resultDtos = await _service.GetAll();
            return Ok(resultDtos);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get single item by ID"
        )]
        public virtual async Task<ActionResult<TResultDto>> GetById(long id)
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
        [SwaggerOperation(
            Summary = "Create item"
        )]
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
        [SwaggerOperation(
            Summary = "Update item"
        )]
        public virtual async Task<ActionResult<TResultDto>> Update(long id, TUpdateDto updateDto, bool ignoreMissingOrNullFields)
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
        [SwaggerOperation(
            Summary = "Delete item"
        )]
        public virtual async Task<IActionResult> Delete(long id)
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
        [SwaggerOperation(
            Summary = "Get paginated results"
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedResult<TResultDto>>> GetPaginated(

            // Pagination parameters

            [SwaggerParameter(Description = "Page number to retrieve")]
            [FromQuery]
            int pageNumber = 1,

            [SwaggerParameter(Description = "Number of items per page")]
            [FromQuery]
            int pageSize = 10,

            [SwaggerParameter(Description = "Whether to include the total count of items")]
            [FromQuery] 
            bool includeTotalCount = false,

            // Sorting parameters

            // [ModelBinder(BinderType = typeof(CommaSeparatedModelBinder))]
            // [QueryDtoMetadata]
            // [SwaggerParameter(Description = "Property names to sort by (multiple values allowed)")]
            [SwaggerParameter(Description = "Property names to sort by (multiple values allowed, allowed values - see item fields below)")]
            [FromQuery]
            string[]? sortBy = null,

            // [ModelBinder(BinderType = typeof(CommaSeparatedModelBinder))]
            // [EnumSchema(typeof(SortDirection))]
            // [SwaggerParameter(Description = "Sort directions for each property (multiple values allowed)")]
            [SwaggerParameter(Description = "Sort directions for each property (multiple values allowed, allowed values: Asc, Desc)")]
            [FromQuery]
            string[]? sortDirection = null, 

            // Filtering parameters

            // [ModelBinder(BinderType = typeof(CommaSeparatedModelBinder))]
            // [QueryDtoMetadata]
            // [SwaggerParameter(Description = "Property names to filter by (multiple values allowed)")]
            [SwaggerParameter(Description = "Property names to filter by (multiple values allowed, allowed values - see item fields below)")]
            [FromQuery]
            string[]? filterBy = null,

            // [ModelBinder(BinderType = typeof(CommaSeparatedModelBinder))]
            // [EnumSchema(typeof(FilterOperator))]
            // [SwaggerParameter(Description = "Filter operators for each property (multiple values allowed)")]
            [SwaggerParameter(Description = "Filter operators for each property (multiple values allowed, allowed values: Equals, NotEquals, Contains, NotContains, GreaterThan, LessThan)")]
            [FromQuery]
            string[]? filterOperator = null,

            [SwaggerParameter(Description = "Filter values for each property (multiple values allowed)")]
            [FromQuery]
            string[]? filterValue = null)
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

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("many")]
        [SwaggerOperation(
            Summary = "Get many items, selected by filter"
        )]
        public virtual async Task<ActionResult<IEnumerable<TResultDto>>> GetMany([FromQuery()] FindManyArgs<TEntity, TQueryDto> filter)
        {
            var resultDtos = await _service.GetMany(filter);
            return Ok(resultDtos);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("meta")]
        [SwaggerOperation(
            Summary = "Get meta data results, for items selected by filter"
        )]
        public virtual async Task<ActionResult<MetadataDto>>GetMeta([FromQuery()] FindManyArgs<TEntity, TQueryDto> filter)
        {
            var metadataDto = await _service.GetMeta(filter);
            return Ok(metadataDto);
        }
    }
}

#pragma warning restore IDE0290 // Use primary constructor
