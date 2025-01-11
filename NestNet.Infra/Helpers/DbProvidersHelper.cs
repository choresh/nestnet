using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace NestNet.Infra.Helpers
{
    internal static class DbProvidersHelper
    {
        public class Quotes
        {
            public string OpenQuote { get; set; }
            public string CloseQuote { get; set; }
        }

        public interface IDbProviderHelper
        {
            Quotes GetQuotes();
            string GetCurrDateFunc();

            Task<TEntity?> UpdateEntity<TEntity, TUpdateDto>(
               DbSet<TEntity> dbSet,
               string idFieldName,
               DbContext context,
               int id,
               TUpdateDto updateDto,
               List<System.Reflection.PropertyInfo> modifiedProperties
            ) where TEntity : class;
        }

        private class InMemoryHelper : IDbProviderHelper
        {
            public string GetCurrDateFunc()
            {
                return "CURRENT_TIMESTAMP()";
            }

            public Quotes GetQuotes()
            {
                return new Quotes()
                {
                    OpenQuote = "[",
                    CloseQuote = "]"
                };
            }

            public async Task<TEntity?> UpdateEntity<TEntity, TUpdateDto>(
                DbSet<TEntity> dbSet,
                string idFieldName,
                DbContext context,
                int id,
                TUpdateDto updateDto,
                List<System.Reflection.PropertyInfo> modifiedProperties
                ) where TEntity : class
            {
                TEntity? entity = await dbSet.FindAsync(id);
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
                    await context.SaveChangesAsync();
                }
                return entity;
            }
        }

        private class MsSqlHelper : IDbProviderHelper
        {
            public string GetCurrDateFunc()
            {
                return "CURRENT_TIMESTAMP()";
            }

            public Quotes GetQuotes()
            {
                return new Quotes()
                {
                    OpenQuote = "[",
                    CloseQuote = "]"
                };
            }

            public Task<TEntity?> UpdateEntity<TEntity, TUpdateDto>(
                DbSet<TEntity> dbSet,
                string idFieldName,
                DbContext context,
                int id,
                TUpdateDto updateDto,
                List<System.Reflection.PropertyInfo> modifiedProperties
                ) where TEntity: class
            {
                var setStatements = string.Join(", ",
                                   modifiedProperties.Select(p => $"{p.Name} = @{p.Name}"))
                                   + ", UpdatedAt = GETUTCDATE()";

                var tableName = dbSet.EntityType.GetTableName();
                var sql = $@"
                    UPDATE {tableName} 
                    SET {setStatements}
                    OUTPUT INSERTED.*
                    WHERE {idFieldName} = @Id;
                ";

                // Modified to always include the parameter, even if null
                var parameters = modifiedProperties
                    .Select(p => new SqlParameter($"@{p.Name}", p.GetValue(updateDto) ?? DBNull.Value))
                    .Concat(new[] { new SqlParameter("@Id", id) })
                    .ToArray();

                var entity = dbSet
                    .FromSqlRaw(sql, parameters)
                    .AsEnumerable()
                    .FirstOrDefault();

                return Task.FromResult(entity);
            }
        }

        private class PostgresHelper : IDbProviderHelper
        {
            public string GetCurrDateFunc()
            {
                return "CURRENT_TIMESTAMP";
            }

            public Quotes GetQuotes()
            {
                return new Quotes()
                {
                    OpenQuote = "\"",
                    CloseQuote = "\""
                };
            }

            public Task<TEntity?> UpdateEntity<TEntity, TUpdateDto>(
                DbSet<TEntity> dbSet,
                string idFieldName,
                DbContext context,
                int id,
                TUpdateDto updateDto,
                List<System.Reflection.PropertyInfo> modifiedProperties
                ) where TEntity : class
            {
                var currDateFunc = GetDbProviderHelper().GetCurrDateFunc();
                var quotes = GetDbProviderHelper().GetQuotes();

                var setStatements = string.Join(", ",
                    modifiedProperties.Select(p => $"{quotes.OpenQuote}{p.Name}{quotes.CloseQuote} = @{p.Name}"))
                     + $", {quotes.OpenQuote}UpdatedAt{quotes.CloseQuote} = {currDateFunc}";

                var tableName = dbSet.EntityType.GetTableName();
                var schema = dbSet.EntityType.GetSchema() ?? "public";

                var sql = $@"
                    UPDATE {$"{quotes.OpenQuote}{schema}{quotes.CloseQuote}.{quotes.OpenQuote}{tableName}{quotes.CloseQuote}"} 
                    SET {setStatements}
                    WHERE {$"{quotes.OpenQuote}{schema}{quotes.CloseQuote}.{quotes.OpenQuote}{tableName}{quotes.CloseQuote}.{quotes.OpenQuote}{idFieldName}{quotes.CloseQuote}"} = @Id
                    RETURNING *;";

                var parameters = modifiedProperties
                    .Select(p => new NpgsqlParameter($"@{p.Name}", p.GetValue(updateDto) ?? DBNull.Value))
                    .Concat(new[] { new NpgsqlParameter("@Id", id) })
                    .ToArray();

                var entity = dbSet
                    .FromSqlRaw(sql, parameters)
                    .AsEnumerable()
                    .FirstOrDefault();

                return Task.FromResult(entity);
            }
        }

        private static IDbProviderHelper? _dbProviderHelper;

        public static void Init(string providerName)
        {
            switch (providerName)
            {
                case "Microsoft.EntityFrameworkCore.SqlServer":
                    _dbProviderHelper = new MsSqlHelper();
                    break;
                case "Npgsql.EntityFrameworkCore.PostgreSQL":
                    _dbProviderHelper = new PostgresHelper();
                    break;
                case "Microsoft.EntityFrameworkCore.InMemory":
                    _dbProviderHelper = new InMemoryHelper();
                    break;
                default:
                    throw new ArgumentException($"DB provider '{providerName}' not soppurted");

            }
        }

        public static IDbProviderHelper GetDbProviderHelper()
        {
            if (_dbProviderHelper == null)
            {
                throw new Exception("DB provider helper not initialized yet");
            }
            return _dbProviderHelper;
        }
    }
}
