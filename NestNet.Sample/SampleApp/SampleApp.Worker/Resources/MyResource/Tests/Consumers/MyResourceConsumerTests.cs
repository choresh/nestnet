using MassTransit;
using NSubstitute;
using Xunit;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using SampleApp.Worker.Resources.MyResource.Services;

namespace SampleApp.Worker.Resources.MyResource.Tests.Consumers
{
    public class MyResourceConsumerTests
    {
        private readonly IFixture _fixture;
        private readonly IMyResourceService _myResourceService;
        private readonly MyResourceConsumer _consumer;

        public MyResourceConsumerTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            _myResourceService = _fixture.Freeze<IMyResourceService>();
            _consumer = new MyResourceConsumer(_myResourceService);
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
            await _myResourceService.Received(1).SampleOperation(Arg.Is<SampleInputDto>(x => x == message));
        }
    }
}