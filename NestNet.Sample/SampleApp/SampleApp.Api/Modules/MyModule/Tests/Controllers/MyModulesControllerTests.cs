using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using NestNet.Infra.Query;
using NestNet.Infra.BaseClasses;
using NestNet.Infra.Paginatation;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using System.Text.Json;
using SampleApp.Api.Modules.MyModules.Services;
using SampleApp.Api.Modules.MyModules.Dtos;

namespace SampleApp.Api.Modules.MyModules.Tests.Controllers
{
    public class MyModulesControllerTests
    {
        private readonly IFixture _fixture;
        private readonly IMyModulesService _myModulesService;
        private readonly MyModulesController _controller;

        public MyModulesControllerTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            _myModulesService = _fixture.Freeze<IMyModulesService>();
            _controller = new MyModulesController(_myModulesService);
        }

        // Test methods...
    }
}