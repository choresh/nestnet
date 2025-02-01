#pragma warning disable IDE0290 // Use primary constructor
using SampleApp.Resources.MyResource.Dtos;
using NestNet.Infra.Attributes;

namespace SampleApp.Resources.MyResource.Services
{
    public interface IMyResourceService
    {
        Task<IEnumerable<SampleOutputDto>> SampleOperation(SampleInputDto input);
    }

    [Injectable(LifetimeType.Scoped)]
    public class MyResourceService : IMyResourceService
    {
        public MyResourceService()
        {
        }

   		public async Task<IEnumerable<SampleOutputDto>> SampleOperation(SampleInputDto input)
        {
        	// Replace this sample code with your code.
            return new List<SampleOutputDto>() {
                new SampleOutputDto(),
                new SampleOutputDto()
            };
        }

        // How to customize this class:
		// 1) You can modify the sample method.
        // 2) You can add simmilar methods.
    }
}

#pragma warning restore IDE0290 // Use primary constructor