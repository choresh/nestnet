using Microsoft.EntityFrameworkCore;
using System.Reflection;
using SampleApp.Data;
using NestNet.Infra.Helpers;
using NestNet.Infra.Swagger;

var builder = WebApplication.CreateBuilder(args);


// Format connection string
static string CreateConnectionString(string[] args)
{
    var erver = ConfigHelper.GetConfigParam(args, "MSSQL_SERVER");
    var dbName = ConfigHelper.GetConfigParam(args, "MSSQL_DB_NAME");
    var user = ConfigHelper.GetConfigParam(args, "MSSQL_USER");
    var password = ConfigHelper.GetConfigParam(args, "MSSQL_PASSWORD");
    var trustServerCertificate = ConfigHelper.GetConfigParam(args, "MSSQL_TRUST_SERVER_CERTIFICATE", "false");
    var trustedConnection = ConfigHelper.GetConfigParam(args, "MSSQL_TRUSTED_CONNECTION", "false");
    var multipleActiveResultSets = ConfigHelper.GetConfigParam(args, "MSSQL_MULTIPLE_ACTIVE_RESULT_SETS", "false");

    return $"Server={erver}; Database={dbName}; User Id={user}; Password={password}; TrustServerCertificate={trustServerCertificate}; Trusted_Connection={trustedConnection}; MultipleActiveResultSets={multipleActiveResultSets}";
}

// * Add Entity Framework Core.
// * If your entities not located (only) at current assembly - customise via C'tor of 'ApplicationDbContext'.
var connectionString = CreateConnectionString(args);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));


// Support Dependency Injection for all classes with [Injectable] attribute
// (Daos, Services, etc), within the given assembleis.
DependencyInjectionHelper.RegisterInjetables(builder.Services, [Assembly.GetExecutingAssembly()]);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

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
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


// Initialize/update the DB
DbHelper.InitDb<ApplicationDbContext>(app.Services);

app.Run();
