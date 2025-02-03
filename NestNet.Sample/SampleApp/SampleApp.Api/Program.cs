using SampleApp.Core.Data;
using Microsoft.EntityFrameworkCore;
using NestNet.Infra.Helpers;
using NestNet.Infra.Swagger;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add MVC controllers to the service container.
builder.Services.AddControllers();

// Add API explorer to enable API endpoint discovery and documentation.
// This is required for Swagger/OpenAPI to discover API endpoints.
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger/OpenAPI documentation.
builder.Services.AddSwaggerGen(c =>
{
    // Enables Swagger annotations (SwaggerOperationAttribute, SwaggerParameterAttribute etc.).
    c.EnableAnnotations();

    // Add descriptions for enum values in Swagger documentation.
    c.SchemaFilter<EnumSchemaFilter>();

    // Add descriptions for query parameters from QueryDto classes.
    c.SchemaFilter<QueryDtoSchemaFilter>();
});

// Helper function to construct DB connection string from environment variables.
static string CreateConnectionString(string[] args)
{
	var server = ConfigHelper.GetConfigParam(args, "POSTGRES_SERVER");
	var dbName = ConfigHelper.GetConfigParam(args, "POSTGRES_DB_NAME");
	var user = ConfigHelper.GetConfigParam(args, "POSTGRES_USER");
	var password = ConfigHelper.GetConfigParam(args, "POSTGRES_PASSWORD");

	return $"Host={server}; Database={dbName}; Username={user}; Password={password}";
}

var connectionString = CreateConnectionString(args);

// Configure Entity Framework with database context.
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

// Configure dependency injection for classes with [Injectable] attribute.
// This scans and register all injectable classes from both the API and Core assemblies.
DependencyInjectionHelper.RegisterInjetables(builder.Services, [
    Assembly.GetExecutingAssembly(),
    Assembly.Load("SampleApp.Core")
]);

// Initialize the application.
var app = builder.Build();

// Enable Swagger UI only in development environment.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure middleware pipeline
app.UseHttpsRedirection();  // Redirect HTTP requests to HTTPS.
app.UseAuthorization();     // Enable authorization.
app.MapControllers();       // Register controller endpoints.

// Initialize / update the DB (accurding code of our entities).
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // dbContext.Database.Migrate();  // This will create the database and apply migration
    dbContext.Database.EnsureCreated();  // Simpler, doesn't support migrations
}

app.Run();