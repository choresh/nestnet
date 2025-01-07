#pragma warning disable IDE0290 // Use primary constructor
using Microsoft.AspNetCore.Mvc;

namespace NestNet.Infra.Query
{
    [BindProperties(SupportsGet = true)]
    public class FindManyArgs<TEntity, TQueryDto> : FindManyInput<TEntity, TQueryDto> where TQueryDto : class
    {
    }
}

#pragma warning restore IDE0290 // Use primary constructor