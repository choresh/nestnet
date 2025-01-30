#pragma warning disable IDE0290 // Use primary constructor
using NestNet.Infra.Query;
using AutoMapper;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using NestNet.Infra.Helpers;
using NestNet.Infra.Paginatation;
using Microsoft.EntityFrameworkCore;
using NestNet.Infra.Attributes;

namespace NestNet.Infra.BaseClasses
{
    public abstract class CrudServiceBase<TEntity, TCreateDto, TUpdateDto, TResultDto, TQueryDto> : ICrudService<TEntity, TCreateDto, TUpdateDto, TResultDto, TQueryDto> where TEntity : IEntity where TQueryDto : class
    {
        protected readonly IDao<TEntity, TQueryDto> _dao;
        private readonly IMapper _mapper;
        private readonly List<string> _selectableProps;

        protected class MyProfile : Profile
        {
            public MyProfile()
            {
                // Direct mapping from CreateDto to Entity with null value handling
                CreateMap<TCreateDto, TEntity>()
                    .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

                // Direct mapping from UpdateDto to Entity with null value handling
                CreateMap<TUpdateDto, TEntity>()
                    .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

                // Direct mapping from QueryDto to Entity with null value handling
                CreateMap<TQueryDto, TEntity>()
                    .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

                // Direct mapping from Entity to ResultDto with null value handling
                CreateMap<TEntity, TResultDto>()
                    .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

                // Map the paginated result
                CreateMap<PaginatedResult<TEntity>, PaginatedResult<TResultDto>>();
            }
        }

        public CrudServiceBase(IDao<TEntity, TQueryDto> dao)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MyProfile>();
            });
            _mapper = config.CreateMapper();
            _dao = dao;
           _selectableProps = typeof(TEntity)
                .GetProperties()
                .Where(IsSelectableProp)
                .Select(p => StringHelper.ToCamelCase(p.Name))
                .ToList();
        }

        public virtual async Task<IEnumerable<TResultDto>> GetAll()
        {
            var entities = await _dao.GetAll();
            return ToResultDtos(entities);
        }

        public virtual async Task<TResultDto?> GetById(long id)
        {
            var entity = await _dao.GetById(id);
            return ToResultDto(entity);
        }

        public virtual async Task<InternalCreateResult<TResultDto>> Create(TCreateDto createDto)
        {
            TEntity entity = ToEntity(createDto); 
            await _dao.Create(entity);
            return new InternalCreateResult<TResultDto>()
            {
                ResultDto = ToResultDto(entity),
                Id = entity.Id
            };
        }

        public virtual async Task<TResultDto?> Update(long id, TUpdateDto updateDto, bool ignoreMissingOrNullFields)
        {
            var entity = await _dao.Update(id, updateDto, ignoreMissingOrNullFields);
            if (entity == null)
            {
                return default;
            }
            else
            {
                return ToResultDto(entity);
            }
        }

        public virtual async Task<bool> Delete(long id)
        {
            return await _dao.Delete(id);
        }

        public virtual async Task<DataWithOptionalError<PaginatedResult<TResultDto>>> GetPaginated(UnsafePaginationRequest unsafeRequest)
        {
            var parsedPaginationRequest = ParsePaginationRequest(unsafeRequest);
            if (parsedPaginationRequest.Data == null)
            {
                return new()
                {
                    Error = parsedPaginationRequest.Error,
                };
            }
            var result = await _dao.GetPaginated(parsedPaginationRequest.Data);
            return new()
            {
                Data = ToPaginatedResultDtos(result)
            };
        }

        // Public - for unit tests.
        public PaginatedResult<TResultDto> ToPaginatedResultDtos(PaginatedResult<TEntity> result)
        {
            return _mapper.Map<PaginatedResult<TResultDto>>(result);
        }

        // Public - for unit tests.
        public IEnumerable<TResultDto> ToResultDtos(IEnumerable<TEntity> entities)
        {
            return _mapper.Map<IEnumerable<TResultDto>>(entities);
        }

        // Public - for unit tests.
        public TEntity ToEntity(TCreateDto createDto)
        {
            return _mapper.Map<TEntity>(createDto);
        }

        // Public - for unit tests.
        public TEntity ToEntity(TUpdateDto updateDto)
        {
            return _mapper.Map<TEntity>(updateDto);
        }

        // Public - for unit tests.
        public TResultDto ToResultDto(TEntity? entity)
        {
            return _mapper.Map<TResultDto>(entity);
        }

        // Public - for unit tests.
        public IList<string> GetSelectableProps()
        {
            return _selectableProps;
        }

        private bool IsSelectableProp(PropertyInfo p)
        {
            var propAttribute = p.GetCustomAttribute<PropAttribute>();
            var notMappedAttribute = p.GetCustomAttribute<NotMappedAttribute>();

            // Property must have PropAttribute, must not be marked as NotMapped,
            // and must not be marked with GenOpt.Ignore
            return propAttribute != null 
                && notMappedAttribute == null 
                && propAttribute.Result != GenOpt.Ignore;
        }

        private DataWithOptionalError<SafePaginationRequest> ParsePaginationRequest(UnsafePaginationRequest unsafeRequest)
        {
            IList<string> errors = new List<string>();
            bool existsInvalidSortProperties = false;
            bool existsInvalidFilterProperties = false;

            // Validate sort criteria integrity
            if ((unsafeRequest.SortBy != null && unsafeRequest.SortDirection == null) ||
                (unsafeRequest.SortBy == null && unsafeRequest.SortDirection != null) ||
                (unsafeRequest.SortBy?.Length != unsafeRequest.SortDirection?.Length))
            {
                errors.Add("Sort criteria is incomplete. Both SortBy and SortDirection must be provided with matching lengths.");
            }

            // Validate filter criteria integrity
            if ((unsafeRequest.FilterBy != null && (unsafeRequest.FilterOperator == null || unsafeRequest.FilterValue == null)) ||
                (unsafeRequest.FilterOperator != null && (unsafeRequest.FilterBy == null || unsafeRequest.FilterValue == null)) ||
                (unsafeRequest.FilterValue != null && (unsafeRequest.FilterBy == null || unsafeRequest.FilterOperator == null)) ||
                (unsafeRequest.FilterBy?.Length != unsafeRequest.FilterOperator?.Length ||
                 unsafeRequest.FilterBy?.Length != unsafeRequest.FilterValue?.Length))
            {
                errors.Add("Filter criteria is incomplete. FilterBy, FilterOperator, and FilterValue must all be provided with matching lengths.");
            }

            if (unsafeRequest.SortBy != null)
            {
                var invalidPropertyNames = unsafeRequest.SortBy
                   .Where(p => !_selectableProps.Contains(p))
                   .ToList();

                if (invalidPropertyNames.Any())
                {
                    existsInvalidSortProperties = true;
                    errors.Add($"Invalid sort properties ({string.Join(", ", invalidPropertyNames)}).");
                }

                for (int i = 0; i < unsafeRequest.SortBy.Length; i++)
                {
                    if (!Enum.TryParse(unsafeRequest.SortDirection[i], true, out SortDirection sortDirection))
                    {
                        errors.Add($"Invalid sort direction '{unsafeRequest.SortDirection[i]}'. Valid values are: {string.Join(", ", Enum.GetNames<SortDirection>())}.");
                    }
                }
            }

            if (unsafeRequest.FilterBy != null)
            {
                var invalidPropertyNames = unsafeRequest.FilterBy
                    .Where(p => !_selectableProps.Contains(p))
                    .ToList();

                if (invalidPropertyNames.Any())
                {
                    existsInvalidFilterProperties = true;
                    errors.Add($"Invalid filter properties ({string.Join(", ", invalidPropertyNames)}).");
                }

                for (int i = 0; i < unsafeRequest.FilterBy.Length; i++)
                {
                    if (!Enum.TryParse(unsafeRequest.FilterOperator[i], true, out FilterOperator filterOperator))
                    {
                        errors.Add($"Invalid filter operator '{unsafeRequest.FilterOperator[i]}'. Valid values are: {string.Join(", ", Enum.GetNames<FilterOperator>())}.");
                    }
                }
            }

            if (errors.Count > 0)
            {
                if (existsInvalidSortProperties || existsInvalidFilterProperties)
                {
                    errors.Add($"(Valid sort/filter properties for {typeof(TEntity).Name} are: {string.Join(", ", _selectableProps)}).");
                }

                return new()
                {
                    Error = String.Join(Environment.NewLine, errors)
                };
            }

            var safeRequest = new SafePaginationRequest
            {
                PageNumber = unsafeRequest.PageNumber,
                PageSize = unsafeRequest.PageSize,
                IncludeTotalCount = unsafeRequest.IncludeTotalCount,
                SortCriteria = new List<SortCriteria>(),
                FilterCriteria = new List<FilterCriteria>()
            };

            if (unsafeRequest.SortBy != null)
            {
                for (int i = 0; i < unsafeRequest.SortBy.Length; i++)
                {
                    safeRequest.SortCriteria.Add(new SortCriteria
                    {
                        PropertyName = StringHelper.ToPascalCase(unsafeRequest.SortBy[i]),
                        SortDirection = Enum.Parse<SortDirection>(unsafeRequest.SortDirection[i], true)
                    });
                }
            }

            if (unsafeRequest.FilterBy != null)
            {
                for (int i = 0; i < unsafeRequest.FilterBy.Length; i++)
                {
                    safeRequest.FilterCriteria.Add(new FilterCriteria
                    {
                        PropertyName = StringHelper.ToPascalCase(unsafeRequest.FilterBy[i]),
                        Operator = Enum.Parse<FilterOperator>(unsafeRequest.FilterOperator[i], true),
                        Value = unsafeRequest.FilterValue[i]
                    });
                }
            }

            return new ()
            {
                Data = safeRequest
            };
        }

        public async Task<IEnumerable<TResultDto>> GetMany(FindManyArgs<TEntity, TQueryDto> filter)
        {
            var entities = await _dao.GetMany(filter);
            return ToResultDtos(entities);
        }

        public async Task<MetadataDto> GetMeta(FindManyArgs<TEntity, TQueryDto> filter)
        {
            return await _dao.GetMeta(filter);
        }
    }
}

#pragma warning restore IDE0290 // Use primary constructor
