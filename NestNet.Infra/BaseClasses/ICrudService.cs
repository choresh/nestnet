﻿using NestNet.Infra.Query;

namespace NestNet.Infra.BaseClasses
{
    public class InternalCreateResult<TResultDto>
    {
        public required TResultDto ResultDto { get; set; }
        public int Id { get; set; }
    }

    public interface ICrudService<TEntity, TCreateDto, TUpdateDto, TResultDto>
    {
        Task<InternalCreateResult<TResultDto>> Create(TCreateDto createDto);
        Task<bool> Delete(int id);
        Task<IEnumerable<TResultDto>> GetAll();
        Task<TResultDto?> GetById(int id);
        Task<DataWithOptionalError<PaginatedResult<TResultDto>>> GetPaginated(UnsafePaginationRequest request);
        Task<TResultDto?> Update(int id, TUpdateDto updateDto, bool ignoreMissingOrNullFields);
    }
}
