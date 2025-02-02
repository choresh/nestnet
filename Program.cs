using System.CommandLine;
using System.CommandLine.Rendering;

public class Program
{
    public enum ApplicationType
    {
        Api,
        Worker,
        Both
    }

    public static async Task<int> Main(string[] args)
    {
        var typeOption = new Option<ApplicationType>(
            name: "--type",
            description: "Type of application to create (Api/Worker/Both)",
            getDefaultValue: () => ApplicationType.Worker);

        var silentOption = new Option<bool>(
            name: "--silent",
            description: "Run in silent mode with provided parameters",
            getDefaultValue: () => false);

        var rootCommand = new RootCommand("MassTransit application creator")
        {
            typeOption,
            silentOption
        };

        rootCommand.SetHandler(async (type, silent) =>
        {
            if (!silent)
            {
                type = await ShowInteractiveMenu();
            }

            await CreateApplication(type);
        }, typeOption, silentOption);

        return await rootCommand.InvokeAsync(args);
    }

    private static async Task<ApplicationType> ShowInteractiveMenu()
    {
        var console = new SystemConsole();
        console.WriteLine("Select application type:");
        console.WriteLine("1. API only");
        console.WriteLine("2. Worker only");
        console.WriteLine("3. Both API and Worker");
        
        while (true)
        {
            console.Write("\nEnter your choice (1-3): ");
            var key = Console.ReadKey();
            console.WriteLine();

            switch (key.KeyChar)
            {
                case '1':
                    return ApplicationType.Api;
                case '2':
                    return ApplicationType.Worker;
                case '3':
                    return ApplicationType.Both;
                default:
                    console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }

    private static async Task CreateApplication(ApplicationType type)
    {
        var services = new ServiceCollection();

        // Common MassTransit configuration
        services.AddMassTransit(x =>
        {
            x.AddConsumer<SampleMessageConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.ReceiveEndpoint("sample-message-queue", e =>
                {
                    e.ConfigureConsumer<SampleMessageConsumer>(context);
                });
            });
        });

        // Configure based on application type
        switch (type)
        {
            case ApplicationType.Api:
                ConfigureApi(services);
                break;
            case ApplicationType.Worker:
                ConfigureWorker(services);
                break;
            case ApplicationType.Both:
                ConfigureApi(services);
                ConfigureWorker(services);
                break;
        }

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(s => services.ForEach(service => s.Add(service)))
            .Build();

        await host.RunAsync();
    }

    private static void ConfigureApi(IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }

    private static void ConfigureWorker(IServiceCollection services)
    {
        services.AddHostedService<Worker>();
    }
} 