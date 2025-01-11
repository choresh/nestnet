using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using NestNet.Infra.BaseClasses;

namespace NestNet.Infra.Helpers
{
    public static class DbHelper
	{
        /// <summary>
        /// Initialze/update the DB.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceProvider"></param>
        public static void InitDb<T>(IServiceProvider serviceProvider) where T : ApplicationDbContextBase
        {
            // Add database initialization
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<T>();

            // If the database doesn't exist, create it with tables for all current entities.
            // If database already exists, and new entity was added - add the relevant table.
            UpdateDbIfRequired(context);

            // You can add seed data here if needed
            // DbInitializer.Initialize(context);
        }
		
		/// <summary>
        /// If the database doesn't exist, create it with all current entities.
        /// If database exists, and new entity was added in the code - create the relevant table.
        /// </summary>
        /// <param name="context"></param>
        private static void UpdateDbIfRequired(ApplicationDbContextBase context)
        {
            // Check if the database exists
            if (!context.Database.CanConnect())
            {
                // If the database doesn't exist, create it with all current entities
                context.Database.EnsureCreated();
            }
            else
            {
                var quotes = DbProvidersHelper.GetDbProviderHelper().GetQuotes();

                // If the database exists, use a custom approach to add only new tables
                var dbCreator = (RelationalDatabaseCreator)context.Database.GetService<IDatabaseCreator>();
                var tables = context.Model.GetEntityTypes()
                    .Select(t => t.GetTableName())
                    .ToList();

                foreach (var table in tables)
                {
                    try
                    {
                        var sql = dbCreator.GenerateCreateScript();
                        
                        var createTableSql = sql.Split("GO").FirstOrDefault(s => 
                            s.Contains($"CREATE TABLE {quotes.OpenQuote}{table}{quotes.CloseQuote}")
                        );
                        if (!string.IsNullOrEmpty(createTableSql))
                        {
                            context.Database.ExecuteSqlRaw(createTableSql);
                        }
                    }
                    catch (Exception)
                    {
                        // Table already exists, skip
                    }
                }
            }
        }
    }
}
