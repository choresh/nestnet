﻿using NestNet.Infra.Paginatation;
using NestNet.Infra.Query;

namespace NestNet.Infra.BaseClasses
{
    public interface IDao<TEntity, TQueryDto> where TEntity : IEntity where TQueryDto : class
    {
        Task Create(TEntity entity);
        Task<bool> Delete(long id);
        Task<IEnumerable<TEntity>> GetAll();
        Task<TEntity?> GetById(long id);
        Task<PaginatedResult<TEntity>> GetPaginated(SafePaginationRequest request);
        Task<TEntity?> Update<TUpdateDto>(long id, TUpdateDto updateDto, bool ignoreMissingOrNullFields);
        Task<IEnumerable<TEntity>> GetMany(FindManyArgs<TEntity, TQueryDto> filter);
        Task<MetadataDto> GetMeta(FindManyArgs<TEntity, TQueryDto> filter);
    }
}
