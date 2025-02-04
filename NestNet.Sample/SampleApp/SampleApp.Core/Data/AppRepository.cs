#pragma warning disable IDE0290 // Use primary constructor
using NestNet.Infra.BaseClasses;
using NestNet.Infra.Attributes;
using NestNet.Infra.Interfaces;

namespace SampleApp.Core.Data
{
    public interface IAppRepository : IAppRepositoryBase
    {
        // If you add methods to derived class - expose them here.
    }

    [Injectable(LifetimeType.Scoped)]
    public class AppRepository : AppRepositoryBase, IAppRepository
    {
        public AppRepository(AppDbContext context)
            : base(context)
        {
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