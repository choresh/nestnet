using NestNet.Cli.Infra;

namespace NestNet.Cli.Generators.AppGenerator
{
    public class WorkerAppGenerator : AppGeneratorBase
    {
        public WorkerAppGenerator()
           : base(AppType.Worker)
        {
        }

        public override void DoGenerate(AppGenerationContext context)
        {
            GenerateProjectFile(context);
            GenerateProgramFile(context);
        }

        private static void GenerateProjectFile(AppGenerationContext context)
        {
            var csprojContent = $@"<Project Sdk=""Microsoft.NET.Sdk.Web"">

    <PropertyGroup>
        <TargetFramework>net8.0</TargetFramework>
        <Nullable>enable</Nullable>
        <ImplicitUsings>enable</ImplicitUsings>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include=""Microsoft.EntityFrameworkCore.Design"" Version=""*"">
            <PrivateAssets>all</PrivateAssets>
            <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
        </PackageReference>
        <PackageReference Include=""Microsoft.EntityFrameworkCore.Tools"" Version=""*"">
            <PrivateAssets>all</PrivateAssets>
            <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
        </PackageReference>
        <PackageReference Include=""Swashbuckle.AspNetCore"" Version=""*"" />
    </ItemGroup>

    <ItemGroup>
        <ProjectReference Include=""..\{context.BaseProjectName}.Core\{context.BaseProjectName}.Core.csproj"" />
    </ItemGroup>

</Project>";

            File.WriteAllText(Path.Combine(context.AppPath, $"{context.CurrProjectName}.csproj"), csprojContent);
        }

        private static void GenerateProgramFile(AppGenerationContext context)
        {
            var programContent = $@"using {context.BaseProjectName}.Core.Data;
using Microsoft.EntityFrameworkCore;
using NestNet.Infra.Helpers;
using NestNet.Infra.Swagger;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add MVC controllers to the service container.
builder.Services.AddControllers();

// Add API explorer to enable API endpoint discovery and documentation
// This is required for Swagger/OpenAPI to discover API endpoints
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger/OpenAPI documentation.
builder.Services.AddSwaggerGen(c =>
{{
    // Enables Swagger annotations (SwaggerOperationAttribute, SwaggerParameterAttribute etc.).
    c.EnableAnnotations();

    // Add descriptions for enum values in Swagger documentation.
    c.SchemaFilter<EnumSchemaFilter>();

    // Add descriptions for query parameters from QueryDto classes.
    c.SchemaFilter<QueryDtoSchemaFilter>();
}});

// Helper function to construct DB connection string from environment variables
{GetConnectionStringMethod(context.DbType)}

var connectionString = CreateConnectionString(args);

// Configure Entity Framework with database context.
builder.Services.AddDbContext<AppDbContext>(options =>
{{
    {GetDbContextOptionsCode(context.DbType, "connectionString")};
}});

// Configure dependency injection for classes with [Injectable] attribute.
// This scans and register all injectable classes from both the API and Core assemblies.
DependencyInjectionHelper.RegisterInjetables(builder.Services, [
    Assembly.GetExecutingAssembly(),
    Assembly.Load(""{context.BaseProjectName}.Core"")
]);

// Initialize the application
var app = builder.Build();

// Enable Swagger UI only in development environment.
if (app.Environment.IsDevelopment())
{{
    app.UseSwagger();
    app.UseSwaggerUI();
}}

// Configure middleware pipeline
app.UseHttpsRedirection();  // Redirect HTTP requests to HTTPS.
app.UseAuthorization();     // Enable authorization.
app.MapControllers();       // Register controller endpoints.

// Initialize / update the DB (accurding code of our entities).
using (var scope = app.Services.CreateScope())
{{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // dbContext.Database.Migrate();  // This will create the database and apply migration
    dbContext.Database.EnsureCreated();  // Simpler, doesn't support migrations
}}

app.Run();";

            File.WriteAllText(Path.Combine(context.AppPath, "Program.cs"), programContent);
        }

    }
}