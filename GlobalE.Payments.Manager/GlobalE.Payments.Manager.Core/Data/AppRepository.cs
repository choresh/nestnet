#pragma warning disable IDE0290 // Use primary constructor
using NestNet.Infra.BaseClasses;
using NestNet.Infra.Attributes;
using NestNet.Infra.Interfaces;
using Microsoft.EntityFrameworkCore;
using GlobalE.Payments.Manager.Core.Modules.Disputes.Entities;
using GlobalE.Payments.Manager.Core.Modules.DisputeHistories.Entities;

namespace GlobalE.Payments.Manager.Core.Data
{
    public interface IAppRepository : IAppRepositoryBase
    {
        // If you add methods to derived class - expose them here.

        Task<List<DisputeWithHistory>> GetDisputesWithHistory();
    }

    [Injectable(LifetimeType.Scoped)]
    public class AppRepository : AppRepositoryBase, IAppRepository
    {
        public AppRepository(AppDbContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Gets disputes with their corresponding history entries using DisputeId as the join key
        /// </summary>
        /// <returns>A collection of disputes joined with their history entries</returns>
        public async Task<List<DisputeWithHistory>> GetDisputesWithHistory()
        {
            return await _context.Set<DisputeEntity>()
                .Join(
                    _context.Set<DisputeHistoryEntity>(),
                    dispute => dispute.DisputeId,      // Join key from Disputes
                    history => history.DisputeId,      // Join key from DisputeHistories (FK)
                    (dispute, history) => new DisputeWithHistory 
                    { 
                        Dispute = dispute, 
                        History = history 
                    }
                )
                .ToListAsync();
        }

        // How to customise this class:
        // 1) You can add here 'custom' methods (methods for operations not supported by the base class).
        // 2) You can override here base class methods if needed:
        //    * Add methods with the same name and signature as in the base class.
        //    * For example:
        //      public virtual async Task<IEnumerable<TEntity>> GetAll<TEntity>() where TEntity : class, IEntity
        //      {
        //          // Set your custom implementation here.
        //      }
        // 3) In your methods:
        //    * Base class member '_context' are accessible.
        //    * Base class methods (e.g. 'await base.GetAll()') are accesible.
    }
}

#pragma warning restore IDE0290 // Use primary constructor