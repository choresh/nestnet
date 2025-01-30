#pragma warning disable IDE0290 // Use primary constructor
using Microsoft.EntityFrameworkCore;
using NestNet.Infra.Query;
using System.Linq.Expressions;
using NestNet.Infra.Paginatation;

namespace NestNet.Infra.BaseClasses
{
    public class DaoBase<TEntity, TQueryDto> : IDao<TEntity, TQueryDto> where TEntity : class, IEntity where TQueryDto: class
    {
        protected readonly DbContext _context;
        protected readonly DbSet<TEntity> _dbSet;
        protected readonly string _idFieldName;

        public DaoBase(DbContext context, DbSet<TEntity> dbSet, string idFieldName)
        {
            _context = context;
            _dbSet = dbSet;
            _idFieldName = idFieldName;
        }

        public virtual async Task<IEnumerable<TEntity>> GetAll()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<TEntity?> GetById(long id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task Create(TEntity entity)
        {
            _dbSet.Add(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task<bool> Delete(long id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null)
            {
                return false;
            }

            _context.Entry(entity).State = EntityState.Deleted; // _dbSet.Remove(entity); ???
            await _context.SaveChangesAsync();
            return true;
        }

        public virtual async Task<PaginatedResult<TEntity>> GetPaginated(SafePaginationRequest request)
        {
            IQueryable<TEntity> query = _dbSet;

            // Apply filtering
            if (request.FilterCriteria != null && request.FilterCriteria.Any())
            {
                var parameter = Expression.Parameter(typeof(TEntity), "x");

                foreach (var criterion in request.FilterCriteria)
                {
                    var property = Expression.Property(parameter, criterion.PropertyName);
                    var propertyType = property.Type;

                    // Convert the constant value to match the property type
                    var value = Convert.ChangeType(criterion.Value, propertyType);
                    var constant = Expression.Constant(value, propertyType);

                    Expression comparison = criterion.Operator switch
                    {
                        FilterOperator.Equals => Expression.Equal(property, constant),
                        FilterOperator.NotEquals => Expression.NotEqual(property, constant),
                        FilterOperator.Contains => Expression.Call(property, "Contains", null, constant),
                        FilterOperator.NotContains => Expression.Not(Expression.Call(property, "Contains", null, constant)),
                        FilterOperator.GreaterThan => Expression.GreaterThan(property, constant),
                        FilterOperator.LessThan => Expression.LessThan(property, constant),
                        _ => throw new NotSupportedException($"Operator {criterion.Operator} is not supported.")
                    };

                    var lambda = Expression.Lambda<Func<TEntity, bool>>(comparison, parameter);
                    query = query.Where(lambda);
                }
            }

            // Apply sorting
            if (request.SortCriteria != null && request.SortCriteria.Any())
            {
                var parameter = Expression.Parameter(typeof(TEntity), "x");
                var firstCriterion = request.SortCriteria.First();
                var property = Expression.Property(parameter, firstCriterion.PropertyName);
                var lambda = Expression.Lambda<Func<TEntity, object>>(Expression.Convert(property, typeof(object)), parameter);

                query = firstCriterion.SortDirection == SortDirection.Asc
                    ? query.OrderBy(lambda)
                    : query.OrderByDescending(lambda);

                foreach (var criterion in request.SortCriteria.Skip(1))
                {
                    property = Expression.Property(parameter, criterion.PropertyName);
                    lambda = Expression.Lambda<Func<TEntity, object>>(Expression.Convert(property, typeof(object)), parameter);

                    query = criterion.SortDirection == SortDirection.Asc
                        ? ((IOrderedQueryable<TEntity>)query).ThenBy(lambda)
                        : ((IOrderedQueryable<TEntity>)query).ThenByDescending(lambda);
                }
            }

            PaginatedResult<TEntity> result;

            if (request.IncludeTotalCount)
            {
                // Atomic operation getting both count and items
                var items = await query
                    .Select(x => new 
                    {
                        Data = x,
                        TotalCount = query.Count()
                    })
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                var totalCount = items.FirstOrDefault()?.TotalCount ?? 0;
                var resultItems = items.Select(x => x.Data).ToList();
                var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

                result = new PaginatedResult<TEntity>
                {
                    Items = resultItems,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = totalPages,
                    HasPreviousPage = request.PageNumber > 1,
                    HasNextPage = request.PageNumber < totalPages
                };
            }
            else
            {
                // Just get the items without count
                var items = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                result = new PaginatedResult<TEntity>
                {
                    Items = items,
                    TotalCount = null,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = null,
                    HasPreviousPage = request.PageNumber > 1,
                    HasNextPage = items.Count == request.PageSize // If we got a full page, there might be more
                };
            }

            return result;
        }

        public virtual async Task<TEntity?> Update<TUpdateDto>(long id, TUpdateDto updateDto, bool ignoreMissingOrNullFields)
        {
            // Get all properties if not ignoring missing fields, otherwise only non-null properties
            var modifiedProperties = typeof(TUpdateDto).GetProperties()
                .Where(p => !ignoreMissingOrNullFields || p.GetValue(updateDto) != null)
                .ToList();

            if (modifiedProperties.Count == 0)
            {
                throw new ArgumentException($"Properties for updating of Entity with {_idFieldName} {id} not supplied");
            }

            TEntity? entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                foreach (var dtoProp in modifiedProperties)
                {
                    // Find matching property in entity
                    var entityProp = typeof(TEntity).GetProperty(dtoProp.Name);
                    if (entityProp != null && entityProp.CanWrite)
                    {
                        var value = dtoProp.GetValue(updateDto);
                        entityProp.SetValue(entity, value);
                    }
                }
                await _context.SaveChangesAsync();
            }

            return entity;
        }

        public async Task<IEnumerable<TEntity>> GetMany(FindManyArgs<TEntity, TQueryDto> filter)
        {
            return await _dbSet
                .ApplyWhere(filter.Where)
                .ApplySkip(filter.Skip)
                .ApplyTake(filter.Take)
                .ApplyOrderBy(filter.SortBy)
                .ToListAsync();
        }

        public async Task<MetadataDto> GetMeta(FindManyArgs<TEntity, TQueryDto> filter)
        {
            var count = await _dbSet.ApplyWhere(filter.Where).CountAsync();
            return new MetadataDto { Count = count };
        }
    }
}

#pragma warning restore IDE0290 // Use primary constructor