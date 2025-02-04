using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using GlobalE.Payments.Manager.Api.Resources.ChargeBack.Controllers;
using GlobalE.Payments.Manager.Core.Resources.ChargeBack.Dtos;
using GlobalE.Payments.Manager.Core.Resources.ChargeBack.Services;
using GlobalE.Payments.Manager.Core.Data;

namespace GlobalE.Payments.Manager.Api.Resources.ChargeBack.Tests.Controllers
{
    public class ChargeBackControllerTests
    {
        private readonly IFixture _fixture;
        private readonly IChargeBackService _chargeBackService;
        private readonly ChargeBackController _controller;

        public ChargeBackControllerTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            _chargeBackService = _fixture.Freeze<IChargeBackService>();
            _controller = new ChargeBackController(_chargeBackService);
        }

        [Fact]
        public async Task SampleOperation_ReturnsOkResult_WithAllItems()
        {
            // Arrange
            var input = _fixture.Create<SampleInputDto>();
            var expectedResult = _fixture.CreateMany<DisputeWithHistory>(2).ToList();
            _chargeBackService.SampleOperation(Arg.Any<SampleInputDto>()).Returns(Task.FromResult<IEnumerable<DisputeWithHistory>>(expectedResult));

            // Act
            var result = await _controller.SampleOperation(input);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            var resultData = Assert.IsAssignableFrom<IEnumerable<SampleOutputDto>>(okResult.Value);
            Assert.Equal(expectedResult.Count, resultData.Count());
            await _chargeBackService.Received(1).SampleOperation(Arg.Any<SampleInputDto>());
        }
    }
}