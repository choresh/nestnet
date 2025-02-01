using Microsoft.EntityFrameworkCore;
using System.Reflection;
using SampleApp.Data;
using NestNet.Infra.Helpers;
using NestNet.Infra.Swagger;

var builder = WebApplication.CreateBuilder(args);


// Format connection string
static string CreateConnectionString(string[] args)
{
    var server = ConfigHelper.GetConfigParam(args, "POSTGRES_SERVER");
    var dbName = ConfigHelper.GetConfigParam(args, "POSTGRES_DB_NAME");
    var user = ConfigHelper.GetConfigParam(args, "POSTGRES_USER");
    var password = ConfigHelper.GetConfigParam(args, "POSTGRES_PASSWORD");

    return $"Host={server}; Database={dbName}; Username={user}; Password={password}";
}

// * Add Entity Framework Core.
// * If your entities not located (only) at current assembly - customise via C'tor of 'ApplicationDbContext'.
var connectionString = CreateConnectionString(args);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Support Dependency Injection for all classes with [Injectable] attribute
// (Daos, Services, etc), within the given assembleis.
DependencyInjectionHelper.RegisterInjetables(builder.Services, [Assembly.GetExecutingAssembly()]);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();

    // Add enum descriptions
    c.SchemaFilter<EnumSchemaFilter>();

    // Add QueryDto properties descriptions
    c.SchemaFilter<QueryDtoSchemaFilter>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Initialize / update the DB
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // dbContext.Database.Migrate();  // This will create the database and apply migration
    dbContext.Database.EnsureCreated();  // Simpler, doesn't support migrations
}

app.Run();
