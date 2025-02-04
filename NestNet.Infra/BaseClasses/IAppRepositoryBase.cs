using NestNet.Infra.Paginatation;
using NestNet.Infra.Query;

namespace NestNet.Infra.BaseClasses
{
    public interface IAppRepositoryBase
    {
        Task Create<TEntity>(TEntity entity) where TEntity : class, IEntity;
        Task<bool> Delete<TEntity>(long id) where TEntity : class, IEntity;
        Task<IEnumerable<TEntity>> GetAll<TEntity>() where TEntity : class, IEntity;
        Task<TEntity?> GetById<TEntity>(long id) where TEntity : class, IEntity;
        Task<IEnumerable<TEntity>> GetMany<TEntity, TQueryDto>(FindManyArgs<TEntity, TQueryDto> filter)
            where TEntity : class, IEntity
            where TQueryDto : class;
        Task<MetadataDto> GetMeta<TEntity, TQueryDto>(FindManyArgs<TEntity, TQueryDto> filter)
            where TEntity : class, IEntity
            where TQueryDto : class;
        Task<PaginatedResult<TEntity>> GetPaginated<TEntity>(SafePaginationRequest request) where TEntity : class, IEntity;
        Task<TEntity?> Update<TUpdateDto, TEntity>(long id, TUpdateDto updateDto, bool ignoreMissingOrNullFields) where TEntity : class, IEntity;
        /* Protected method, not to be exposed,
        Task<IEnumerable<TEntity>> GetEntities<TEntity>(Func<IQueryable<TEntity>, Task<List<TEntity>>> query) where TEntity : class, IEntity;
        Task<TEntity?> GetEntityByCondition<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class, IEntity;
        */
    }
}