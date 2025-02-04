#pragma warning disable IDE0290 // Use primary constructor
using NestNet.Infra.BaseClasses;
using NestNet.Infra.Attributes;
using NestNet.Infra.Interfaces;
using GlobalE.Payments.Manager.Core.Modules.DisputeHistories.Dtos;
using GlobalE.Payments.Manager.Core.Modules.DisputeHistories.Entities;
using GlobalE.Payments.Manager.Core.Data;

namespace GlobalE.Payments.Manager.Core.Modules.DisputeHistories.Services
{
    public interface IDisputeHistoriesService: ICrudService<DisputeHistoryEntity, DisputeHistoryCreateDto, DisputeHistoryUpdateDto, DisputeHistoryResultDto, DisputeHistoryQueryDto>
    {
        // If you add methods to derived class - expose them here.
    }

    [Injectable(LifetimeType.Scoped)]
    public class DisputeHistoriesService : CrudServiceBase<DisputeHistoryEntity, DisputeHistoryCreateDto, DisputeHistoryUpdateDto, DisputeHistoryResultDto, DisputeHistoryQueryDto>, IDisputeHistoriesService
    {
        public DisputeHistoriesService(IAppRepository appRepository)
            : base(appRepository)
        {
        }

        // How to customise this class:
        // 1) You can add here 'custom' methods (methods for operations not supported by the base class).
        // 2) You can override here base class methods if needed:
        //    * Add methods with the same name and signature as in the base class.
        //    * For example:
        //      public override async Task<IEnumerable<DisputeHistoryResultDto>> GetAll()
        //      {
        //          // Set your custom implementation here.
        //      }
        // 3) In your methods:
        //    * Base class member '_repository' is accessible.
        //    * Base class methods (e.g. 'await base.GetAll()') are accesible.
    }
}

#pragma warning restore IDE0290 // Use primary constructor