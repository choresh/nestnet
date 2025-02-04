using MassTransit;
using NSubstitute;
using Xunit;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using GlobalE.Payments.Manager.Core.Resources.ChargeBack.Services;
using GlobalE.Payments.Manager.Core.Resources.ChargeBack.Dtos;
using GlobalE.Payments.Manager.Worker.Resources.ChargeBack.Consumers;

namespace GlobalE.Payments.Manager.Worker.Resources.ChargeBack.Tests.Consumers
{
    public class ChargeBackConsumerTests
    {
        private readonly IFixture _fixture;
        private readonly IChargeBackService _chargeBackService;
        private readonly ChargeBackConsumer _consumer;

        public ChargeBackConsumerTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            _chargeBackService = _fixture.Freeze<IChargeBackService>();
            _consumer = new ChargeBackConsumer(_chargeBackService);
        }

        [Fact]
        public async Task Consume_ShouldCallService_WithCorrectParameters()
        {
            // Arrange
            var message = _fixture.Create<SampleInputDto>();
            var context = Substitute.For<ConsumeContext<SampleInputDto>>();
            context.Message.Returns(message);

            // Act
            await _consumer.Consume(context);

            // Assert
            await _chargeBackService.Received(1).SampleOperation(Arg.Is<SampleInputDto>(x => x == message));
        }
    }
}