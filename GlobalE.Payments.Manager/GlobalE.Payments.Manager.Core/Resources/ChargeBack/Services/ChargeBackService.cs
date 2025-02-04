#pragma warning disable IDE0290 // Use primary constructor
using GlobalE.Payments.Manager.Core.Data;
using GlobalE.Payments.Manager.Core.Resources.ChargeBack.Dtos;
using NestNet.Infra.Attributes;

namespace GlobalE.Payments.Manager.Core.Resources.ChargeBack.Services
{
    public interface IChargeBackService
    {
        Task<IEnumerable<DisputeWithHistory>> SampleOperation(SampleInputDto input);
    }

    [Injectable(LifetimeType.Scoped)]
    public class ChargeBackService : IChargeBackService
    {
        IAppRepository _repository;
        public ChargeBackService(IAppRepository repository)
        {
            _repository = repository;
        }

   		public async Task<IEnumerable<DisputeWithHistory>> SampleOperation(SampleInputDto input)
        {
            var disputesWithHistory = await _repository.GetDisputesWithHistory();

        	// Replace this sample code with your code.
            return disputesWithHistory;
        }

        // How to customize this class:
		// 1) You can modify the sample method.
        // 2) You can add simmilar methods.
    }
}

#pragma warning restore IDE0290 // Use primary constructor