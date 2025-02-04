using NestNet.Infra.BaseClasses;
using NestNet.Infra.Paginatation;
using NestNet.Infra.Query;

namespace NestNet.Infra.Interfaces
{
    public interface ICrudService<TEntity, TCreateDto, TUpdateDto, TResultDto, TQueryDto> where TQueryDto : class
    {
        Task<InternalCreateResult<TResultDto>> Create(TCreateDto createDto);
        Task<bool> Delete(long id);
        Task<IEnumerable<TResultDto>> GetAll();
        Task<TResultDto?> GetById(long id);
        Task<DataWithOptionalError<PaginatedResult<TResultDto>>> GetPaginated(UnsafePaginationRequest request);
        Task<TResultDto?> Update(long id, TUpdateDto updateDto, bool ignoreMissingOrNullFields);
        Task<IEnumerable<TResultDto>> GetMany(FindManyArgs<TEntity, TQueryDto> filter);
        Task<MetadataDto> GetMeta(FindManyArgs<TEntity, TQueryDto> filter);
    }
}
