using MassTransit;
using SampleApp.Core.Resources.MyResource.Dtos;
using SampleApp.Core.Resources.MyResource.Services;

namespace SampleApp.Worker.Resources.MyResource.Consumers;

public class MyResourceConsumer : IConsumer<SampleInputDto>
{
    private readonly IMyResourceService _myResourceService;

    public MyResourceConsumer(IMyResourceService myResourceService)
    {
        _myResourceService = myResourceService;
    }

    public async Task Consume(ConsumeContext<SampleInputDto> context)
    {
        await _myResourceService.SampleOperation(context.Message);
    }
}