using NSubstitute;
using Xunit;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using System.Text.Json;
using SampleApp.Resources.MyResource.Services;
using SampleApp.Resources.MyResource.Dtos;

namespace SampleApp.Resources.MyResource.Tests.Services
{
    public class MyResourceServiceTests
    {
        private readonly IFixture _fixture;
        private readonly MyResourceService _service;

        public MyResourceServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            _service = new MyResourceService();
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