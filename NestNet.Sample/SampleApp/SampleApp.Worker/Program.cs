using SampleApp.Core.Data;
using Microsoft.EntityFrameworkCore;
using NestNet.Infra.Helpers;
using System.Reflection;
using SampleApp.Worker;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        // Helper function to construct DB connection string from environment variables
        static string CreateConnectionString(string[] args)
		{
			var server = ConfigHelper.GetConfigParam(args, "POSTGRES_SERVER");
			var dbName = ConfigHelper.GetConfigParam(args, "POSTGRES_DB_NAME");
			var user = ConfigHelper.GetConfigParam(args, "POSTGRES_USER");
			var password = ConfigHelper.GetConfigParam(args, "POSTGRES_PASSWORD");

			return $"Host={server}; Database={dbName}; Username={user}; Password={password}";
		}

        var connectionString = CreateConnectionString(args);

        // Configure Entity Framework with database context
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        // Configure dependency injection for classes with [Injectable] attribute
        // This scans and register all injectable classes from both the Worker and Core assemblies
        DependencyInjectionHelper.RegisterInjetables(services, [
            Assembly.GetExecutingAssembly(),
            Assembly.Load("SampleApp.Core")
        ]);

        // Add the worker service
        services.AddHostedService<Worker>();
    });

// Initialize the application
var host = builder.Build();

// Initialize / update the DB (according to code of our entities)
using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // dbContext.Database.Migrate();  // This will create the database and apply migration
    dbContext.Database.EnsureCreated();  // Simpler, doesn't support migrations
}

await host.RunAsync();