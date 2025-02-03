using MassTransit;

namespace SampleApp.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IBus _bus;

    public Worker(ILogger<Worker> logger, IBus bus)
    {
        _logger = logger;
        _bus = bus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            
            // Example of how to publish a message:
            // await _bus.Publish(new YourMessage { Property = "Value" }, stoppingToken);
            
            await Task.Delay(1000, stoppingToken);
        }
    }
}