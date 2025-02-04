using NSubstitute;
using Xunit;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using System.Text.Json;
using GlobalE.Payments.Manager.Core.Resources.ChargeBack.Services;
using GlobalE.Payments.Manager.Core.Resources.ChargeBack.Dtos;

namespace GlobalE.Payments.Manager.Core.Resources.ChargeBack.Tests.Services
{
    public class ChargeBackServiceTests
    {
        private readonly IFixture _fixture;
        private readonly ChargeBackService _service;

        public ChargeBackServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            // _service = new ChargeBackService(null); // TODO
        }

        [Fact]
        public async Task SampleOperation_ReturnsAllItems()
        {
            // Arrange
            var input = _fixture.Create<SampleInputDto>();
            var expectedResult = _fixture.CreateMany<SampleOutputDto>(2).ToList();
       
            // Act
            var result = await _service.SampleOperation(input);
  
            // Assert
            Assert.Equal(expectedResult.Count(), result.Count());
            Assert.Equal(
                JsonSerializer.Serialize(expectedResult),
                JsonSerializer.Serialize(result));

        }
    }
}