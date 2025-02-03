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
            GenerateWorkerService(context);
        }

        private static void GenerateProjectFile(AppGenerationContext context)
        {
            var csprojContent = $@"<Project Sdk=""Microsoft.NET.Sdk.Worker"">

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
        <PackageReference Include=""Microsoft.Extensions.Hosting"" Version=""*"" />
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
using System.Reflection;
using {context.CurrProjectName};

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {{
        // Helper function to construct DB connection string from environment variables
        {GetConnectionStringMethod(context.DbType, "\t\t")}

        var connectionString = CreateConnectionString(args);

        // Configure Entity Framework with database context
        services.AddDbContext<AppDbContext>(options =>
        {{
            {GetDbContextOptionsCode(context.DbType, "connectionString")};
        }});

        // Configure dependency injection for classes with [Injectable] attribute
        // This scans and register all injectable classes from both the Worker and Core assemblies
        DependencyInjectionHelper.RegisterInjetables(services, [
            Assembly.GetExecutingAssembly(),
            Assembly.Load(""{context.BaseProjectName}.Core"")
        ]);

        // Add the worker service
        services.AddHostedService<Worker>();
    }});

// Initialize the application
var host = builder.Build();

// Initialize / update the DB (according to code of our entities)
using (var scope = host.Services.CreateScope())
{{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // dbContext.Database.Migrate();  // This will create the database and apply migration
    dbContext.Database.EnsureCreated();  // Simpler, doesn't support migrations
}}

await host.RunAsync();";

            File.WriteAllText(Path.Combine(context.AppPath, "Program.cs"), programContent);
        }

        private static void GenerateWorkerService(AppGenerationContext context)
        {
            var workerContent = $@"namespace {context.CurrProjectName};

public class Worker : BackgroundService
{{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {{
        _logger = logger;
    }}

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {{
        while (!stoppingToken.IsCancellationRequested)
        {{
            _logger.LogInformation(""Worker running at: {{time}}"", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }}
    }}
}}";

            File.WriteAllText(Path.Combine(context.AppPath, "Worker.cs"), workerContent);
        }
    }
}