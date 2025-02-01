using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using SampleApp.Worker.Data;
using NestNet.Infra.Helpers;

// Format connection string
static string CreateConnectionString(string[] args)
{
    var server = ConfigHelper.GetConfigParam(args, "POSTGRES_SERVER");
    var dbName = ConfigHelper.GetConfigParam(args, "POSTGRES_DB_NAME");
    var user = ConfigHelper.GetConfigParam(args, "POSTGRES_USER");
    var password = ConfigHelper.GetConfigParam(args, "POSTGRES_PASSWORD");

    return $"Host={server}; Database={dbName}; Username={user}; Password={password}";
}

// Create service collection
var services = new ServiceCollection();

// Add DB Context
var connectionString = CreateConnectionString(args);
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Support Dependency Injection for all classes with [Injectable] attribute
// (Daos, Services, etc), within the given assembleis.
DependencyInjectionHelper.RegisterInjetables(services, [Assembly.GetExecutingAssembly()]);

// Build service provider
var serviceProvider = services.BuildServiceProvider();

// Initialize DB
using (var scope = serviceProvider.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // dbContext.Database.Migrate();  // This will create the database and apply migration
    dbContext.Database.EnsureCreated();  // Simpler, doesn't support migrations
}

// Your worker logic goes here
Console.WriteLine("Worker application started...");

Console.WriteLine("Press any key to exit...");
Console.ReadKey();