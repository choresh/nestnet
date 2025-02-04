using MassTransit;
using GlobalE.Payments.Manager.Core.Resources.ChargeBack.Dtos;
using GlobalE.Payments.Manager.Core.Resources.ChargeBack.Services;

namespace GlobalE.Payments.Manager.Worker.Resources.ChargeBack.Consumers;

public class ChargeBackConsumer : IConsumer<SampleInputDto>
{
    private readonly IChargeBackService _chargeBackService;

    public ChargeBackConsumer(IChargeBackService chargeBackService)
    {
        _chargeBackService = chargeBackService;
    }

    public async Task Consume(ConsumeContext<SampleInputDto> context)
    {
        await _chargeBackService.SampleOperation(context.Message);
    }
}