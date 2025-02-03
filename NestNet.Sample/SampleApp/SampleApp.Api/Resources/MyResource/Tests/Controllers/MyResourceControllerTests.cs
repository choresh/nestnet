using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using SampleApp.Api.Resources.MyResource.Controllers;
using SampleApp.Core.Resources.MyResource.Dtos;
using SampleApp.Core.Resources.MyResource.Services;

namespace SampleApp.Api.Resources.MyResource.Tests.Controllers
{
    public class MyResourceControllerTests
    {
        private readonly IFixture _fixture;
        private readonly IMyResourceService _myResourceService;
        private readonly MyResourceController _controller;

        public MyResourceControllerTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            _myResourceService = _fixture.Freeze<IMyResourceService>();
            _controller = new MyResourceController(_myResourceService);
        }

        [Fact]
        public async Task SampleOperation_ReturnsOkResult_WithAllItems()
        {
            // Arrange
            var input = _fixture.Create<SampleInputDto>();
            var expectedResult = _fixture.CreateMany<SampleOutputDto>(2).ToList();
            _myResourceService.SampleOperation(Arg.Any<SampleInputDto>()).Returns(Task.FromResult<IEnumerable<SampleOutputDto>>(expectedResult));

            // Act
            var result = await _controller.SampleOperation(input);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            var resultData = Assert.IsAssignableFrom<IEnumerable<SampleOutputDto>>(okResult.Value);
            Assert.Equal(expectedResult.Count, resultData.Count());
            await _myResourceService.Received(1).SampleOperation(Arg.Any<SampleInputDto>());
        }
    }
}