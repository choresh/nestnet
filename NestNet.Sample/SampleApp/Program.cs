using Microsoft.EntityFrameworkCore;
using System.Reflection;
using SampleApp.Data;
using NestNet.Infra.Helpers;
using NestNet.Infra.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Format connection string
static string CreateConnectionString(string[] args)
{
    var dbServer = ConfigHelper.GetConfigParam(args, "DB_SERVER");
    var dbName = ConfigHelper.GetConfigParam(args, "DB_NAME");
    var dbUser = ConfigHelper.GetConfigParam(args, "DB_USER");
    var dbPassword = ConfigHelper.GetConfigParam(args, "DB_PASSWORD");
    var dbTrustServerCertificate = ConfigHelper.GetConfigParam(args, "DB_TRUST_SERVER_CERTIFICATE", "false");
    var dbTrustedConnection = ConfigHelper.GetConfigParam(args, "DB_TRUSTED_CONNECTION", "false");
    var dbMultipleActiveResultSets = ConfigHelper.GetConfigParam(args, "DB_MULTIPLE_ACTIVE_RESULT_SETS", "false");

    return $"Server={dbServer}; Database={dbName}; User Id={dbUser}; Password={dbPassword}; TrustServerCertificate={dbTrustServerCertificate}; Trusted_Connection={dbTrustedConnection}; MultipleActiveResultSets={dbMultipleActiveResultSets}";
}

// * Add Entity Framework Core.
// * If namespace of your entities is not standard (not ended with '.Entities') - customise via C'tor of 'ApplicationDbContext'.
// * If location of your entities is not at the standard folder ('.\Modules\<module name>\Entities') - customise
//   parametrs for post-build command that execute the 'GenerateDtos.ps1' script.
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

    /*
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API V1");
        SwaggerUICustomization.ConfigureSwaggerUI(c);
    });
    */
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


// Initialize/update the DB
DbHelper.InitDb<ApplicationDbContext>(app.Services);

app.Run();
