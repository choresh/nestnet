using NestNet.Infra.Query;

namespace NestNet.Infra.BaseClasses
{
    public interface IDao<TEntity> where TEntity : IEntity
    {
        Task Create(TEntity entity);
        Task<bool> Delete(int id);
        Task<IEnumerable<TEntity>> GetAll();
        Task<TEntity?> GetById(int id);
        Task<PaginatedResult<TEntity>> GetPaginated(SafePaginationRequest request);
        Task<TEntity?> Update<TUpdateDto>(int id, TUpdateDto updateDto, bool ignoreMissingOrNullFields);
    }
}
